using Godot;
using System;
using System.IO;
using AgeOfEmpires.Server;
using Godot.Collections;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 100f;
	[Export] public float Damage = 0;
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
		{ "reloadtime", 2f },
		{ "coyotetime", 0.25f },
		{ "bulletspeed", 400f },
		{ "walljumppush", 200f },
		{ "bulletgravity", 981f },
		{ "walljumpforce", 250f },
		{ "bulletacceleration", 0},
		{ "lifesteal", 1},
	};

	[Export] public PackedScene BulletScene{get; set;}
	private float _bulletSpeed = 400f;
	private float _bulletGravity = 981f;
	private float coyoteTimer = 0f;
	private string[] skins;
	private int currentSkin = 0;
	private int currentColor = 0;
	public float MaxHp = 10f, Hp = 10f;
	public float Angle = 0f;
	public int BulletLeft = 5;
	
    private Vector2 inputDir;
	public Upgrades Upgrades;
	private Line2D HpLine;
	private Node2D Weapon;
	private Timer ReloadTimer;
	private CpuParticles2D FallParticle;
	private PackedScene FallParticleScene;
	private Sprite2D Icon;
	private VoidControll _controll;

	public bool OnMenu = true;
	public bool CardVibor = true;
	public bool Died = false;
	private bool reload = true, lifesteal = false;
	
	public override void _Ready()
	{
		_controll = new VoidControll();
		skins = File.ReadAllLines("User/skins.txt");
		FallParticleScene = GD.Load<PackedScene>("res://User/fallpart.tscn");
		GD.Print(Multiplayer.GetUniqueId(), " Player MPS: ", GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority());
		ReloadTimer = GetNode<Timer>("ReloadTimer");
		Upgrades = GetNode<Upgrades>("upgrades");
		Icon = GetNode<Sprite2D>("Icon");
		HpLine = GetNode<Line2D>("Line2D");
		Weapon = GetNode<Node2D>("Node2D");
		FallParticle = GetNode<CpuParticles2D>("CPUParticles2D");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		ReloadTimer.Timeout += Reloading;
	}

	public void RoundStart()
	{
		MaxHp = Origin["hp"] * Upgrades.HpModifier;
        		Hp = MaxHp;
		Damage = Origin["damage"] * Upgrades.DamageModifier;
		Armor = Origin["armor"] + Upgrades.ArmorModifier;
		Speed = Origin["speed"] * Upgrades.SpeedModifier;
		Gravity = Origin["gravity"] * Upgrades.GravityModifier;
		JumpForce = -Origin["jumpforce"] * Upgrades.JumpModifier; 
		CoyoteTime = Origin["coyotetime"]* Upgrades.CoyoteTimeModifier;
		_bulletSpeed = Origin["bulletspeed"] * Upgrades.BulletSpeedModifier;
		_bulletGravity = Origin["bulletgravity"] * Upgrades.BulletGravityModifier;
		Ammo = Math.Min(1, (int)Origin["ammo"] + Upgrades.AmmoCountModifier);
		BulletCount = Math.Min(1, (int)Origin["bulletcount"] + Upgrades.BulletCountModifier);
		CardVibor = true;
		ReloadTimer.SetWaitTime(Origin["reloadtime"]*Upgrades.ReloadModifier);
	}

	public override void _PhysicsProcess(double delta)
	{
		var linehp = Mathf.Clamp(Hp / MaxHp, 0, 1);
		HpLine.SetPointPosition(1, new Vector2(-7.5f + 15f * linehp, -10.5f));
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId() && CardVibor)
		{
			if (OnMenu)
				Wardrobe();
			
			Vector2 velocity = Velocity;

			inputDir = new Vector2(
			Input.GetActionStrength("d") - Input.GetActionStrength("a"), 0);

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
				} else
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
		if (Position.X >= 1200) {
			var oar = FallParticleScene.Instantiate<CpuParticles2D>();
			oar.Finished += QueueFree;
			GetParent().AddChild(oar);
			Position += Vector2.Left * 1200;
		}
		if (Position.X <= -50) Position += Vector2.Right * 1200;
	}

	public void Reloading()
	{
		BulletCount = Math.Min(1, (int)Origin["bulletcount"] + Upgrades.BulletCountModifier);
	}

	private void HandleShooting(){	
		Angle = GetAngleTo(GetGlobalMousePosition());
		Weapon.Position = 12.5f * new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
		Weapon.Rotation = Angle;

		if (Input.IsActionJustPressed("lm")){
			if (BulletCount <= 0 || Ammo <= 0) return;
			var consumed = Math.Min(BulletCount, Ammo);
			BulletCount -= consumed;
			for (int i = 0; i < consumed; i++)
				Rpc(nameof(RequestShoot), Angle);
			if (BulletCount <= 0)
				ReloadTimer.Start();
		}
	}

	private new Vector2 GetWallNormal()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++) {
            var collision = GetSlideCollision(i);
            if (Mathf.Abs(collision.GetNormal().X) > 0.9f)
                return collision.GetNormal();
        }
        return Vector2.Zero;
    }

	public void Wardrobe()
	{
		bool cliked = false;
		if (Input.IsActionJustPressed("down")) { cliked = true;
			currentSkin = (1 + currentSkin) % skins.Length;
			Icon.Texture = GD.Load<Texture2D>(skins[currentSkin]);
		} if (Input.IsActionJustPressed("up")) {cliked = true;
			currentSkin = (-1 + currentSkin+skins.Length) % skins.Length;
			Icon.Texture = GD.Load<Texture2D>(skins[currentSkin]);
		}

		if (Input.IsActionJustPressed("right")) {cliked = true;
			currentColor = (1 + currentColor) % _controll.Colours.Length;
			Icon.Modulate = _controll.Colours[currentColor];
		} if (Input.IsActionJustPressed("left")) {cliked = true;
			currentColor = (-1 + currentColor+ _controll.Colours.Length) % _controll.Colours.Length;
			Icon.Modulate = _controll.Colours[currentColor];
		}
		if (cliked)
			Rpc(nameof(UpdatePlayerAppearance), currentSkin, currentColor);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void UpdatePlayerAppearance(int skinIndex, int colorIndex)
	{
		// Применяем изменения внешности
		Icon.Texture = GD.Load<Texture2D>(skins[skinIndex]);
		Icon.Modulate = _controll.Colours[colorIndex];
    
		// Обновляем текущие значения (если нужно для локального игрока)
		currentSkin = skinIndex;
		currentColor = colorIndex;
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestShoot(float angle)
	{
		if (Multiplayer.IsServer())
			Rpc(nameof(PerformShoot), angle, Multiplayer.GetRemoteSenderId());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void PerformShoot(float angle, int shooterId)
	{
		var bullet = BulletScene.Instantiate<Bullet>();

		Vector2 spawnOffset = 24f * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		bullet.GlobalPosition = GlobalPosition + spawnOffset;
		bullet.Rotation = angle;
		bullet.Damage = Damage;
		bullet.Speed = _bulletSpeed;
		bullet.ShooterId = shooterId;
		bullet.Attributes = Upgrades.BulletAttributes;
		GetTree().Root.AddChild(bullet);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void RpcUnreliablePosition(Vector2 pos)
	{
		if (!IsMultiplayerAuthority())
			GlobalPosition = pos;
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RequestTakeDamage(float damage, int attackerId)
	{
		// Only server processes damage
		if (!Multiplayer.IsServer()) return;
		
		Hp -= damage;
		GD.Print($"{Name} took {damage} damage from {attackerId}. HP: {Hp}");
		Rpc(nameof(UpdateHealth), Hp);
        
		if (Hp <= 0)
			RpcId(GetMultiplayerAuthority() ,nameof(PerformDie), attackerId);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void UpdateHealth(float newHp) {
		Hp = newHp;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void PerformDie(int killerId)
	{
		Position = Vector2.One * 100000;
		if (Died) return;
		Died = true; Visible = false;
		var server = GetParent().GetNode<ServerManager>("ServerManager");
		SetProcess(false);
		SetPhysicsProcess(false);
		SetCollisionMaskValue(1, false);
		server.RpcId(1, nameof(ServerManager.RequestAddDied), Name);
		
		server.RpcId(1,nameof(ServerManager.CheckGameOver));
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void PerformWinnerDie()
	{
		SetCollisionMaskValue(1, true);
		Position = Vector2.One * 100000;
		if (Died) return;
		Died = true; Visible = false;
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void PerformRespawn()
	{
		Died = false;
		Visible = true;
		SetCollisionMaskValue(1, true);		
		SetProcess(true);
		SetPhysicsProcess(true);
		Hp = MaxHp;
		RoundStart();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void AddUpgrade(string upgradeName)
	{
		Upgrades.ConsumeUpgrade(upgradeName);
	}
}
//King of hill
//Save the crown
//