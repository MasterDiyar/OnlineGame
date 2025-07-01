using Godot;
using System;
using System.Linq;

public partial class Upgrades : Node
{
	public float HpModifier = 1f; //
	public float DamageModifier = 1f; //
	public float ArmorModifier = 1f; //
	public float SpeedModifier = 1f; //
	public float BulletSpeedModifier = 1f; //
	public float BulletGravityModifier = 1f; //
	public float JumpModifier = 1f; //
	public float ReloadModifier = 1f; //
	public float CoyoteTimeModifier = 1f;//
	public float LifeStealModifier = 1f;
	public float CooldownReductionModifier = 1f;
	public float GravityModifier = 1f; //
	public int BulletCountModifier = 0; //
	public int AmmoCountModifier = 0; //

	public string[] UpgradeNames = ["Bits"];

	public void ConsumeUpgrade(string upgradeName)
	{
		UpgradeNames.Append(upgradeName);
		switch (upgradeName)
		{
			case "Speed Up":
				HpModifier *= 1.2f;
				SpeedModifier *= 1.5f;
			break;
			case "Fast Reload":
				ReloadModifier /= 1.2f;
				break;
			case "Pulemet":
				DamageModifier /= 1.5f;
				ReloadModifier /= 1.5f;
				BulletCountModifier += 10;
				break;
			case "Multi Shot":
				AmmoCountModifier += 2;
				BulletSpeedModifier /= 1.2f;
				BulletGravityModifier /= 1.2f;
				break;
			case "Glass cannon":
				DamageModifier *= 5;
				HpModifier /= 5;
				break;
			case "Moon Walk":
				GravityModifier /= 5.2f;
				DamageModifier *= 1.2f;
				break;
			case "Bottom of sea":
				GravityModifier *= 5f;
				BulletSpeedModifier *= 1.4f;
				break;
		}
	}
}
