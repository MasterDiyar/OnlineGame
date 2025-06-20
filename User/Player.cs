using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 40f;
	[Export] public PackedScene BulletScene{get; set;}
	private float _bulletSpeed = 400f;
	public float Hp = 10f;
	public float Angle = 0f;
	
	private Line2D HpLine;
	private Node2D Weapon;
	
	
	public override void _Ready()
	{
		HpLine = GetNode<Line2D>("Line2D");
		Weapon = GetNode<Node2D>("Node2D");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority()==Multiplayer.GetUniqueId())
		{

			Speed = Input.IsActionPressed("shift") ? 60f : 40f;
			HpLine.RemovePoint(1);
			HpLine.AddPoint(new Vector2(-7.5f + 1.5f * Hp, -10.5f));
			
			Vector2 input = new Vector2(
				Input.GetActionStrength("d") - Input.GetActionStrength("a"),
				Input.GetActionStrength("s") - Input.GetActionStrength("w")
			).Normalized();

			Velocity = input * Speed;
			MoveAndSlide();
			
			HandleShooting();
			
			RpcUnreliablePosition(GlobalPosition);
			
		}
	}

	private void HandleShooting()
	{
		Angle = GetAngleTo(GetGlobalMousePosition());
		Weapon.Position = 12.5f * new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
		Weapon.Rotation = Angle;

		if (Input.IsActionJustPressed("lm"))
		{
			Rpc(nameof(RequestShoot), Angle);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestShoot(float angle)
	{
		// Only server actually spawns bullets
		if (Multiplayer.IsServer())
		{
			Rpc(nameof(PerformShoot), angle, Multiplayer.GetRemoteSenderId());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	private void PerformShoot(float angle, int shooterId)
	{
		var bullet = BulletScene.Instantiate<Bullet>();

		Vector2 spawnOffset = 24f * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		bullet.GlobalPosition = GlobalPosition + spawnOffset;
		bullet.Rotation = angle;
		
		bullet.Speed = _bulletSpeed;
		bullet.ShooterId = shooterId; 
		
		GetTree().Root.AddChild(bullet);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void RpcUnreliablePosition(Vector2 pos)
	{
		if (!IsMultiplayerAuthority())
			GlobalPosition = pos;
	}
	
	[Rpc(MultiplayerApi.RpcMode.Authority)]
	public void TakeDamage(float damage)
	{
		Hp -= damage;
		GD.Print($"{Name} took {damage} damage. HP: {Hp}");
		if (Hp <= 0)
		{
			Rpc(nameof(Die));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void Die()
	{
		GD.Print($"{Name} died!");
	}
}
