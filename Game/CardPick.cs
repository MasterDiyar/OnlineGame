using Godot;
using System;

public partial class CardPick : Control
{
	private string[] CommonCard = new[] { "Speed Up", "Fast Reload", "Pulemet", "Multi Shot", "Glass Cannon"};
	public int CardCount = 5;
	Vector2 Center = new Vector2(800, 500);
	private float distance = 200;
	public override void _Ready()
	{
		var cardScene = GD.Load<PackedScene>("res://Game/card.tscn");
		for (float i = 0; i < CardCount; i++)
		{
			Card card = cardScene.Instantiate<Card>();
			card.GetNode<Label>("Text").Text = CommonCard[(int)i];
			card.Position = Center + distance * new Vector2(Mathf.Cos(Mathf.Pi+4*i/CardCount), Mathf.Sin(Mathf.Pi+ 4*i/CardCount));
			card.Rotation = -Mathf.Pi/4 + i / CardCount;
			AddChild(card);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta)
	//{
		//if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() ==
		    //Multiplayer.GetUniqueId())
		//{
			
	
			
		//}}

	public void CardPicked()
	{
		
	}


	public void CardShuffle(int seed)
	{
		var random = new Random(seed);
		
	}
}
