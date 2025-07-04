using Godot;
using System;
using System.Collections.Generic;

public partial class CardPick : Control
{
	private string[] CommonCard = new[] { "Speed Up", "Fast Reload", "Pulemet", "Multi Shot", "Glass Cannon", "Moon Walk", "Bottom of sea"};
	private string[] RareCard = new[] { "Rare Cannon", "Mise Dice" };
	private string[] EpicCard = new[] { "Epic Cannon", "Prosperity" };
	private string[] LegendaryCard = new[] { "Legendary Cannon", "Egg" };

    public List<int> UserQueue = new List<int>();
    public int CurrentUser = 0;
	public int CardCount = 5;
	Vector2 Center = new Vector2(800, 500);
	private float distance = 80;
	private Dictionary<int, string> playerChoices = new Dictionary<int, string>();
	private List<Card> spawnedCards = new List<Card>();
	private bool hasChosen = false;
    private string[] _serverCards;
    int nowChoose = 0;
    private Line2D PickLine;
	public override void _Ready()
    {
        foreach(var user in UserQueue)GD.Print(user.ToString());
        SetMultiplayerAuthority(Multiplayer.GetUniqueId());
        GD.Print(Multiplayer.GetUniqueId(), " MPS: ", GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority());
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
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
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
            card.Position = Center + distance * new Vector2(1.85f*Mathf.Cos(Mathf.Pi+4*i/CardCount),Mathf.Sin(Mathf.Pi+ 4*i/CardCount));
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
        GetParent().GetNode<ServerManager>("ServerManager").Rpc("StartGame");
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
        var sync = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        if (sync.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
        if (UserQueue.Count <= CurrentUser || UserQueue[CurrentUser] != Multiplayer.GetUniqueId()) return;
        
        
        if (Input.IsActionJustPressed("ui_right"))
            nowChoose = (nowChoose + 1) % CardCount;
        if (Input.IsActionJustPressed("ui_left"))
            nowChoose = (nowChoose - 1 + CardCount) % CardCount;
        
        if (Input.IsActionJustPressed("ui_accept")) {
            GD.Print("Choosed ",_serverCards[nowChoose]," card.");
            CurrentUser++;
            Rpc(nameof(SetUserCount), CurrentUser);
            foreach (var node in GetParent().GetChildren()) {
                if (node is Player pl)
                    pl.Rpc(nameof(pl.AddUpgrade), _serverCards[nowChoose]);
            }}
        PickLine.Position =  Center + (distance+5) * 
            new Vector2(1.5f*Mathf.Cos(Mathf.Pi+ 4f *nowChoose/CardCount),
                Mathf.Sin(Mathf.Pi+ 4f *nowChoose/CardCount));
        PickLine.Rotation = -Mathf.Pi/4 + Mathf.Pi/2*nowChoose / CardCount;
        Rpc(nameof(PickMeLine), PickLine.GlobalPosition, PickLine.GlobalRotation);
        if(CurrentUser == UserQueue.Count) Rpc(nameof(AllPlayersChosen));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SetUserCount(int count)
    {
        CurrentUser = count;
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PickMeLine(Vector2 position, float rotation)
    {
        //if (!IsMultiplayerAuthority()) {
            PickLine.GlobalPosition = position;
            PickLine.GlobalRotation = rotation;
        //}
    }
}
