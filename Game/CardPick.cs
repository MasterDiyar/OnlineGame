using Godot;
using System;
using System.Collections.Generic;

public partial class CardPick : Control
{
	private string[] CommonCard = new[] { "Speed Up", "Fast Reload", "Pulemet", "Multi Shot", "Glass Cannon", "Moon Walk", "Bottom of sea"};
	private string[] RareCard = new[] { "Rare Cannon", "Mise Dice" };
	private string[] EpicCard = new[] { "Epic Cannon", "Prosperity" };
	private string[] LegendaryCard = new[] { "Legendary Cannon", "Egg" };

    public int[] UserQueue = [];
	public int CardCount = 5;
	Vector2 Center = new Vector2(800, 500);
	private float distance = 80;
	private Dictionary<int, string> playerChoices = new Dictionary<int, string>();
	private List<Card> spawnedCards = new List<Card>();
	private bool hasChosen = false;
    private string[] _serverCards;

    private Line2D PickLine;
	public override void _Ready()
    {
        PickLine = GetNode<Line2D>("PickLine");
        if (Multiplayer.IsServer())
        {
            _serverCards = new string[CardCount];
            var rand = new Random((int)GD.Randi());
            
            for (int i = 0; i < CardCount; i++)
                _serverCards[i] = GetRandomCard(rand);
            
            Rpc(nameof(SetCards), _serverCards);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SetCards(string[] cards)
    {
        _serverCards = cards;  /*--------------------*/  SpawnCards();
    }
    
    private void SpawnCards()
    {
        var cardScene = GD.Load<PackedScene>("res://Game/card.tscn");
        
        for (float i = 0; i < _serverCards.Length; i++) {
            Card card = cardScene.Instantiate<Card>();
            card.GetNode<Label>("Text").Text = _serverCards[(int)i];
            card.Position = Center + distance * new Vector2(1.5f*Mathf.Cos(Mathf.Pi+4*i/CardCount),Mathf.Sin(Mathf.Pi+ 4*i/CardCount));
            card.Rotation = -Mathf.Pi/4 + Mathf.Pi/2*i / CardCount;
            AddChild(card);
            spawnedCards.Add(card);
        }
    }

    private string GetRandomCard(Random rand)
    {
        // Вероятности: Common 70%, Rare 20%, Epic 7%, Legendary 3%
        float rarity = rand.NextSingle();
        
        if (rarity < 0.7f) return CommonCard[rand.Next(0, CommonCard.Length)];
        if (rarity < 0.9f) return RareCard[rand.Next(0, RareCard.Length)];
        if (rarity < 0.97f) return EpicCard[rand.Next(0, EpicCard.Length)];
        return LegendaryCard[rand.Next(0, LegendaryCard.Length)];
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RegisterChoice(string cardName, int playerId)
    {
        if (playerChoices.ContainsKey(playerId)) return;
        
        playerChoices.Add(playerId, cardName);
        
        if (playerChoices.Count == Multiplayer.GetPeers().Length + 1) 
            Rpc(nameof(AllPlayersChosen));
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void AllPlayersChosen()
    {
        // Здесь можно обработать завершение выбора карт
        // Например, скрыть интерфейс или начать следующий раунд
        foreach (int playerId in playerChoices.Keys) {
            foreach (var player in GetParent().GetChildren()) {
                if (player is Player pl && pl.Name == playerId.ToString()) {
                    pl.AddUpgrade(playerChoices[playerId]);
                    pl.CardVibor = true;
                    pl.Position = new Vector2(int.Parse(pl.Name)%10 * 100+100, 100);
                }
            }
        }
        GetTree().CreateTimer(2.0).Timeout += () => {
            foreach (var card in spawnedCards)
                card.QueueFree();
            this.QueueFree();
        };
    }

	public override void _Process(double delta)
	{
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() ==
		    Multiplayer.GetUniqueId() && UserQueue[0] == Multiplayer.GetUniqueId())
        {
            int nowChoose = 0;
            if (Input.IsActionPressed("ui_right")) {
                nowChoose += (nowChoose +1>= CardCount) ?  1-CardCount: 1;
            } if (Input.IsActionPressed("ui_left")) {
                nowChoose += (nowChoose -1<= 0) ? 1-CardCount : -1;
            } if (Input.IsActionPressed("ui_accept")) {
                foreach (var node in GetParent().GetChildren()) {
                    if (node is Player pl)
                        pl.Rpc(nameof(pl.AddUpgrade), _serverCards[nowChoose]);
                }}
            PickLine.Position =  Center + (distance+5) * 
                new Vector2(1.5f*Mathf.Cos(Mathf.Pi+ 4f *nowChoose/CardCount),
                                 Mathf.Sin(Mathf.Pi+ 4f *nowChoose/CardCount));
            PickLine.Rotation = -Mathf.Pi/4 + Mathf.Pi/2*nowChoose / CardCount;
        }
	}

}
