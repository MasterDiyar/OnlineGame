using Godot;
using System;
using Godot.Collections;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 100f;
	[Export] public float Damage = 2f;
	[Export] public float Gravity = 981f;
	[Export] public float JumpForce = -450f;
	[Export] public float CoyoteTime = 0.25f; 
	[Export] public float WallJumpForce = -400;
	[Export] public float WallJumpPush = 200;
	[Export] public float Armor = 0;
	[Export] public int BulletCount = 5;
	[Export] public int Ammo = 1;

	public string VisualName  = "";

	private static readonly Dictionary<string, float> Origin = new Dictionary<string, float>()
	{
		{ "hp", 10f},
		{ "ammo", 1},
		{ "armor", 0},
		{ "damage", 2f},
		{ "speed", 100f },
		{ "gravity", 981f },
		{ "bulletcount", 5 },
		{ "jumpforce", 450f },
		{ "reloadtime", 0.8f },
		{ "coyotetime", 0.25f },
		{ "bulletspeed", 400f },
		{ "walljumppush", 200f },
		{ "bulletgravity", 981f },
		{ "walljumpforce", 250f },
	};

	[Export] public PackedScene BulletScene{get; set;}
	private float _bulletSpeed = 400f;
	private float _bulletGravity = 981f;
	private float coyoteTimer = 0f;
	public float MaxHp = 10f, Hp = 10f;
	public float Angle = 0f;
	public int BulletLeft = 5;
	
    private Vector2 inputDir;
	public Upgrades Upgrades;
	private Line2D HpLine;
	private Node2D Weapon;
	private Timer ReloadTimer;

	public bool CardVibor = true;
	private bool reload = true;
	
	public override void _Ready()
	{
		ReloadTimer = GetNode<Timer>("ReloadTimer");
		Upgrades = GetNode<Upgrades>("upgrades");
		HpLine = GetNode<Line2D>("Line2D");
		Weapon = GetNode<Node2D>("Node2D");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		ReloadTimer.Timeout += Reloading;
	}

	public void RoundStart()
	{
		MaxHp = Origin["hp"] * Upgrades.HpModifier;
        		Hp = MaxHp;
		Damage = Origin["damage"] * Upgrades.DamageModifier;
		Armor = Origin["armor"] * Upgrades.ArmorModifier;
		Speed = Origin["speed"] * Upgrades.SpeedModifier;
		Gravity = Origin["gravity"] * Upgrades.GravityModifier;
		JumpForce = -Origin["jumpforce"] * Upgrades.JumpModifier; 
		CoyoteTime = Origin["coyotetime"];
		_bulletSpeed = Origin["bulletspeed"] * Upgrades.BulletSpeedModifier;
		_bulletGravity = Origin["bulletgravity"] * Upgrades.BulletGravityModifier;
		Ammo = Math.Min(1, (int)Origin["ammo"] + Upgrades.AmmoCountModifier);
		BulletCount = Math.Min(1, (int)Origin["bulletcount"] + Upgrades.BulletCountModifier);
		CardVibor = true;
		ReloadTimer.SetWaitTime((int)Origin["reloadtime"]*Upgrades.ReloadModifier);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId() && CardVibor)
		{
			var linehp = Mathf.Clamp(Hp / MaxHp, 0, 1);
			HpLine.SetPointPosition(1, (new Vector2(-7.5f + 15f * linehp, -10.5f)));
			
			Vector2 velocity = Velocity;

			inputDir = new Vector2(
            Input.GetActionStrength("d") - Input.GetActionStrength("a"),0);

			var speed = Input.IsActionPressed("shift") ? Speed * 1.5f : Speed;
			
			

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

	public void Reloading()
	{
		BulletCount = Math.Min(1, (int)Origin["bulletcount"] + Upgrades.BulletCountModifier);
	}

	private void HandleShooting()
	{
		
		Angle = GetAngleTo(GetGlobalMousePosition());
		Weapon.Position = 12.5f * new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
		Weapon.Rotation = Angle;

		if (Input.IsActionJustPressed("lm"))
		{
			if (BulletCount <= 0 || Ammo <= 0) return;
			var consumed = Math.Min(BulletCount, Ammo);
			BulletCount -= consumed;
			for (int i = 0; i < consumed; i++)
				Rpc(nameof(RequestShoot), Angle);
			if (BulletCount <= 0)
			{
				ReloadTimer.Start();
			}
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
