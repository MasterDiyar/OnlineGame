using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 100f;
	[Export] public float Gravity = 981f;
	[Export] public float JumpForce = -450f;
	[Export] public float CoyoteTime = 0.5f; 
	[Export] public int WallJumpForce = -400;
	[Export] public int WallJumpPush = 200;

	public string VisualName  = "";

	[Export] public PackedScene BulletScene{get; set;}
	private float _bulletSpeed = 400f;
	public float MaxHp = 10f, Hp = 10f;
	public float Angle = 0f;
	private float coyoteTimer = 0f;
    private Vector2 inputDir;

	
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
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Vector2 velocity = Velocity;

			inputDir = new Vector2(
            Input.GetActionStrength("d") - Input.GetActionStrength("a"),0);

			var speed = Input.IsActionPressed("shift") ? Speed * 1.5f : Speed;
			HpLine.RemovePoint(1);
			HpLine.AddPoint(new Vector2(-7.5f + 1.5f * Hp, -10.5f));

			if (IsOnFloor() || IsOnWall())
				coyoteTimer = CoyoteTime;
			else
				coyoteTimer -= (float)delta;
			
			if (!IsOnFloor() && !IsOnWall())
				velocity.Y += Gravity * (float)delta;

			if (Input.IsActionJustPressed("jump") && coyoteTimer > 0){
				if (IsOnWall() && !IsOnFloor()){
					int wallDir = GetWallNormal().X > 0 ? -1 : 1; 
					velocity.X = wallDir * WallJumpPush;
					velocity.Y = WallJumpForce;
				}else
					velocity.Y = JumpForce;
				coyoteTimer = 0; 
			}

			velocity.X = inputDir.X * speed;

			Velocity = velocity;
			MoveAndSlide();

			HandleShooting();

			RpcUnreliablePosition(GlobalPosition);

			FallCheck();
		}
	}

	public void FallCheck()
	{
		if (Position.Y >= 700) Position += Vector2.Up * 700;
		if (Position.X >= 1200) Position += Vector2.Left * 1200;
		if (Position.X <= -50) Position += Vector2.Right * 1200;

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

	private new Vector2 GetWallNormal()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            if (Mathf.Abs(collision.GetNormal().X) > 0.9f)
                return collision.GetNormal();
        }
        return Vector2.Zero;
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
			Rpc(nameof(Die));
		
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void Die()
	{
		GD.Print($"{Name} died!");
	}
}
