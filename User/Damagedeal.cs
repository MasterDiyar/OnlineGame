using Godot;
using System;

public partial class Damagedeal : Timer
{
	private Player Player;
	public int Times;
	public float Damage;
	public int shooterId;
	public string type;
	public override void _Ready()
	{
		Player = GetParent<Player>();
		Timeout += Timeo;
		switch (type)
		{
			case "Poison":
				break;
		}
	}

	private void Timeo()
	{
		Times--;
		Player.Rpc(nameof(Player.RequestTakeDamage), Damage, shooterId);
		if (Times <= 0) QueueFree();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
