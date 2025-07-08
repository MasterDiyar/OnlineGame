using Godot;
using System;
using Godot.Collections;

public partial class Card : Sprite2D
{
	private Vector2 _textureSize = Vector2.Zero;
	private bool _check = false;
	private Label label;
	public event Action<string, int> CardSelected;
	public string CardName { get; set; }

	public override void _Ready()
	{
		label = GetNode<Label>("Text");
	}

	public override void _Process(double delta)
	{
		Vector2 worldPos = GetGlobalMousePosition();
		Vector2 localPos = ToLocal(worldPos);
		Scale = (GetRect().HasPoint(localPos)) ? Vector2.One * 1.1f : Vector2.One;
	}

	public void SetUp(string cardName)
	{
		label??= GetNode<Label>("Text");
		label.Text = CardList[cardName];
		Texture = GD.Load<Texture2D>(CardImageList[cardName]);
	}
	
	static Dictionary<string, string> CardList = new() {
		{ "Speed Up", "Speed Up\nHp +20%\nPlayer speed +50%" },
		{ "Slow Down", "Slow Down\nHp +20%\nSpeed -20%\nGravity +20%" },
		{ "Alchemist Arrow", "Alchemist Arrow\nDamage +70%\nSpeed -33%" },
		{ "Bad Breakfast", "Bad Breakfast\nHp -20%\nDamage -20%\nReload speed -5%" },
		{ "Fast Reload", "Fast Reload\nDamage -5%\nReload speed +25%" },
		{ "Bless of Church", "Bless of Church\nHp x2\nDamage x2\nReload speed x2" },
		{ "Bread", "Bread\nHp +20%" },
		{ "Dwarf Forge", "Dwarf Forge\nDamage +50%\nReload speed +10%\nBullet speed +50%\nBullet gravity -20%" },
		{ "Good Breakfast", "Good Breakfast\nHp +35%\nDamage +20%" },
		{ "Irish Beer", "Irish Beer\nJump height +20%" },
		{ "Knowledge of Hunters", "Knowledge of Hunters\nDamage +20%\nReload speed +20%\nSpeed +20%" },
		{ "Mastery of Ages", "Mastery of Ages\nHp -28%\nDamage +20%" },
		{ "More Bullets", "More Bullets\nDamage -25%\n+1 bullet count" },
		{ "Obsidian arrow", "Obsidian arrow\nBullet count -2\nBullet gravity +50%\nDamage x2" },
		{ "Poison arrow", "Poison arrow\nBullet count -3\nDamage +50%\nAdds poison effect" },
		{ "Egyptian Beer", "Egyptian Beer\nJump height +30%\nReload speed +10%" },
		{ "Pulemet", "Pulemet\nDamage -33%\nReload speed +50%\n+10 bullets" },
		{ "Multi Shot", "Multi Shot\n+2 ammo per shot\nBullet speed -17%\nBullet gravity -17%" },
		{ "Glass Cannon", "Glass cannon\nDamage x5\nHp ÷5" },
		{ "Moon Walk", "Moon Walk\nGravity -80%\nDamage +20%" },
		{ "Bottom of sea", "Bottom of sea\nGravity +400%\nBullet speed +40%" },
	};

	static Dictionary<string, string> CardImageList = new() {
		{ "Speed Up", "res://Game/cards/speedup.png" },
		{ "Slow Down", "res://Game/cards/slowdown.png" },
		{ "Alchemist Arrow", "res://Game/cards/alchemistarrow.png" },
		{ "Bad Breakfast", "res://Game/cards/badbreakfast.png" },
		{ "Fast Reload", "res://Game/cards/fastreload.png" },
		{ "Bless of Church", "res://Game/cards/blessofchurch.png" },
		{ "Bread", "res://Game/cards/bread.png" },
		{ "Dwarf Forge", "res://Game/cards/dwarfforge.png" },
		{ "Good Breakfast", "res://Game/cards/goodbreakfast.png" },
		{ "Irish Beer", "res://Game/cards/irishbeer.png" },
		{ "Knowledge of Hunters", "res://Game/cards/knowledgeofhunters.png" },
		{ "Mastery of Ages", "res://Game/cards/masteryofages.png" },
		{ "More Bullets", "res://Game/cards/morebullets.png" },
		{ "Obsidian arrow", "res://Game/cards/obsidianarrow.png" },
		{ "Poison arrow", "res://Game/cards/poisonarrow.png" },
		{ "Egyptian Beer", "res://Game/cards/egyptianbeer.png" },
		{ "Pulemet", "res://Game/cards/pulemet.png" },
		{ "Multi Shot", "res://Game/cards/multishot.png" },
		{ "Glass Cannon", "res://Game/cards/glasscannon.png" },
		{ "Moon Walk", "res://Game/cards/moonwalk.png" },
		{ "Bottom of sea", "res://Game/cards/bottomofsea.png" },
	};
}
