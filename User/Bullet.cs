using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	public float Damage = 2;
	public float Speed = 100f;
	public int ShooterId { get; set; } 

	public override void _Ready()
	{
		SetupDestructionTimer();
		
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
		
		if (area != null) {
			area.AreaEntered += OnAreaEntered;
		} else
			GD.PushWarning("Bullet Area2D node not found - area detection won't work");
	}
	
	private void OnAreaEntered(Area2D area)
	{
		HandleHit(area.GetParent());
	}

	private void OnBodyEntered(Node body)
	{
		HandleHit(body);
	}

	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void RpcUnreliablePosition(Vector2 pos)
	{
		if (!IsMultiplayerAuthority())
			GlobalPosition = pos;
	}
	
	private void HandleHit(Node hitNode)
	{
		if (!Multiplayer.IsServer()) return;

		GD.Print("Bullet hit something");

		// Check if we hit a player and it's not the shooter
		if (hitNode is Player player && player.GetMultiplayerAuthority() != ShooterId)
		{
			GD.Print($"Hit player {player.Name}");
			// Call the TakeDamage method on the player's instance
			player.Rpc(nameof(Player.TakeDamage), Damage);
		}

		// Destroy the bullet on all clients
		Rpc(nameof(Destroy));
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.IsServer())
		{
			var linearVelocity = (float)delta * Speed * new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
			MoveAndCollide(linearVelocity);
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void Destroy()
	{
		QueueFree();
	}
}
