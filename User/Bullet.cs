using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	public float Damage = 2;
	public float Speed = 100f;
	public float Acceletation = 0, acceleration = 0;
	public float GravityForce = 981f, LifeSteal = 0;
	public string[] Attributes { get; set; }
	public int ShooterId { get; set; } 

	public override void _Ready()
	{
		SetupDestructionTimer();
		GravityScale = GravityForce / 981;

		if (Multiplayer.IsServer())
			SetupCollisionDetection();	
	}

	private void SetupDestructionTimer()
	{
		var timer = GetNodeOrNull<Timer>("Timer");
		if (timer != null) {
			timer.Timeout += () => {
				if (IsInstanceValid(this))
					QueueFree();
			};
		} else
			GD.PushWarning("Bullet timer not found - bullet won't auto-destroy");
	}

	private void SetupCollisionDetection()
	{
		BodyEntered += OnBodyEntered;
		var area = GetNodeOrNull<Area2D>("Area2D");
		
		if (area != null) area.AreaEntered += OnAreaEntered;
		else GD.PushWarning("Bullet Area2D node not found - area detection won't work");
	}
	
	private void OnAreaEntered(Area2D area) { HandleHit(area.GetParent()); }

	private void OnBodyEntered(Node body) { HandleHit(body); }

	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcUnreliablePosition(Vector2 pos)
	{
		if (!IsMultiplayerAuthority())
			GlobalPosition = pos;
	}
	
	private void HandleHit(Node hitNode)
	{
		if (!Multiplayer.IsServer()) return;
		
		GD.Print($"Bullet from {ShooterId} hit {hitNode.Name}");
		
		if (hitNode is Player player)
		{
			var targetId = player.GetMultiplayerAuthority();
			GD.Print($"Shooter: {ShooterId}, Target: {player.GetMultiplayerAuthority()}, Target Name: {player.Name}");
			var axe = Mathf.Clamp(acceleration / 2, 0, 50);
			
	        if (targetId != ShooterId)
	            player.Rpc(nameof(Player.RequestTakeDamage), Damage * (1+axe), ShooterId);
	        if (LifeSteal != 0)
	            player.GetParent().GetNode<Player>($"{ShooterId}").Rpc(
	            nameof(Player.RequestTakeDamage), Damage * LifeSteal/100, ShooterId);
			if (Attributes != null)
	        foreach (var attribute in Attributes)
		        switch (attribute.Split(":")[0])
		        {
			        case "poison":
				        var chld = GD.Load<PackedScene>("res://User/damagedeal.tscn").Instantiate<Damagedeal>();
				        chld.Damage = float.Parse(attribute.Split(":")[1]);
				        chld.Times = int.Parse(attribute.Split(":")[2]);
				        chld.WaitTime = float.Parse(attribute.Split(":")[3]);
				        player.AddChild(chld);
				        break;
		        }
		}
		Rpc(nameof(Destroy));
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.IsServer()){
			var linearVelocity = Vector2.Zero;
			acceleration += Acceletation / 100;
			if (Acceletation == 0)
				linearVelocity = (float)delta * Speed * new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
			else
				linearVelocity = acceleration * (float)delta * Speed * new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
			MoveAndCollide(linearVelocity);
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void Destroy() 
	{
		if (IsInstanceValid(this))
			QueueFree();
	}
}
