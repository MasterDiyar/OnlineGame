using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	public float Damage = 2;
	public float Speed = 100f;
	public override void _Ready()
	{
		GetNode<Timer>("Timer").Timeout += TimeOut;
		BodyEntered += BodyEnter;
	}

	private void BodyEnter(Node body)
	{
		GD.Print("BodyEnter");
		if (body is Player pl)
		{
			pl.Hp-=Damage;
		}
		QueueFree();
	}

	private void TimeOut()
	{
		QueueFree();
	}
	

	
	public override void _PhysicsProcess(double delta)
	{
		LinearVelocity = Speed * new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
	}
}
