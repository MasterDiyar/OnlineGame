using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 40f;
	[Export] public PackedScene BulletScene{get; set;}
	
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
			HpLine.Points[1] = new Vector2(-10.5f, -7.5f + 1.5f * Hp);
			
			Vector2 input = new Vector2(
				Input.GetActionStrength("d") - Input.GetActionStrength("a"),
				Input.GetActionStrength("s") - Input.GetActionStrength("w")
			).Normalized();

			Velocity = input * Speed;
			MoveAndSlide();
			
			Angle = GetAngleTo(GetGlobalMousePosition());
			Weapon.Position = 12.5f * new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
			Weapon.Rotation = Angle;

			if (Input.IsActionJustPressed("lm"))
			{
				Rpc("Vistrel");
			}
			
			RpcUnreliablePosition(GlobalPosition);
			
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Vistrel()
	{
		Bullet bullet = BulletScene.Instantiate<Bullet>();
		bullet.GlobalPosition = GlobalPosition +  20f * new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
		bullet.Rotation = Angle;
		GetTree().Root.AddChild(bullet);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void RpcUnreliablePosition(Vector2 pos)
	{
		if (!IsMultiplayerAuthority())
			GlobalPosition = pos;
	}
}
