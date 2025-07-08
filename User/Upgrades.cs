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
	public string[] BulletAttributes = []; //attribute name: damage: times: timer

	public void ConsumeUpgrade(string upgradeName)
	{
		UpgradeNames.Append(upgradeName);
		switch (upgradeName)
		{
			case "Speed Up":
				HpModifier *= 1.2f;
				SpeedModifier *= 1.5f;
			break;
			case "Slow Down":
				HpModifier *= 1.2f;
				SpeedModifier /= 1.25f;
				GravityModifier *= 1.2f;
				break;
			case "Alchemist Arrow":
				DamageModifier *= 1.7f;
				SpeedModifier /= 1.5f;
				break;
			case "Bad Breakfast":
				HpModifier /= 1.2f;
				DamageModifier /= 1.2f;
				ReloadModifier /= 1.05f;
				break;
			case "Fast Reload":
				DamageModifier /= 1.05f;
				ReloadModifier /= 1.25f;
				break;
			case "Bless of Church":
				HpModifier *= 2f;
				DamageModifier *= 2f;
				ReloadModifier *= 2f;
				break;
			case "Bread":
				HpModifier *= 1.2f;
				break;
			case "Dwarf Forge":
				DamageModifier *= 1.5f;
				ReloadModifier /= 1.1f;
				BulletSpeedModifier *= 1.5f;
				BulletGravityModifier /= 1.2f;
				break;
			case "Good Breakfast":
				HpModifier *= 1.35f;
				DamageModifier *= 1.2f;
				break;
			case "Irish Beer":
				JumpModifier *= 1.2f;
				break;
			case "Knowledge of Hunters":
				DamageModifier *= 1.2f;
				ReloadModifier *= 1.2f;
				SpeedModifier *= 1.2f;
				break;
			case "Mastery of Ages":
				HpModifier /= 1.4f;
				DamageModifier *= 1.2f;
				break;
			case "More Bullets":
				DamageModifier /= 1.33f;
				BulletCountModifier *= 2;
				break;
			case "Obsidian arrow":
				BulletCountModifier -=2;
				BulletGravityModifier *= 1.5f;
				DamageModifier *= 2f;
				break;
			case "Poison arrow":
				BulletCountModifier -=3;
				DamageModifier *= 1.5f;
				var enumerable = BulletAttributes.Append("poison:1:1:2");
				BulletAttributes = enumerable.ToArray();
				break;
			case "Egyptian Beer":
				JumpModifier *= 1.3f;
				ReloadModifier /= 1.1f;
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
			case "Glass Cannon": 
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
