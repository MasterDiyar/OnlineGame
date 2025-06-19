using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	public float Damage = 2;
	public float Speed = 100f;
	public int ShooterId { get; set; } 

	public override void _Ready()
	{
		Timer timer = GetNode<Timer>("Timer");
		timer.Timeout += QueueFree;
		if (Multiplayer.IsServer())
			BodyEntered += BodyEnter;
	}
	
	private void BodyEnter(Node body)
	{
		GD.Print("BodyEnter");
		if (!Multiplayer.IsServer()) return;
		if (body is Player player && player.GetMultiplayerAuthority() != ShooterId)
			player.RpcId(player.GetMultiplayerAuthority(), nameof(Player.TakeDamage), Damage);
		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		var linearVelocity =(float)delta* Speed * new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
		MoveAndCollide(linearVelocity);
	}
}
