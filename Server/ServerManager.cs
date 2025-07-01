using Godot;
using System;
using System.IO;
using System.Linq;
using AgeOfEmpires.Server;

public partial class ServerManager : Control
{
	[Export] private Node SpawnNode { get; set; } //must be main
	[Export] private PackedScene PlayerScene{get;set;}
	
	private ENetMultiplayerPeer peer;
	public string Address = "127.0.0.1"; 
	public int port = 8901;
	
	private Random rand = new Random();
	private VoidControll voids= new ();
	private Menu _menu;
	
	private string[] maps;
	private int[] DieQueue = [];
	public string CurrentMap = "";
	public int UserCount = 0;
	public int AliveUserCount = 0;
	
	public override void _Ready()  
	{  
		_menu = SpawnNode.GetNode<Menu>("Menu");
		maps = File.ReadAllLines("Server/mapnames.txt");  
         
		Multiplayer.PeerConnected += OnPeerConnected;  
		Multiplayer.PeerDisconnected += OnPeerDisconnected;  
		Multiplayer.ConnectedToServer +=  () => GD.Print("Connected to server"); 
		Multiplayer.ConnectionFailed += () => GD.Print("Connection failed");  
	}

	public void OnPeerConnected(long id)
	{
		GD.Print($"User {id} connected.");
		if (Multiplayer.IsServer()) {
			AddPlayer(id); UserCount++;
			
			foreach (var player in GameManager.Players.Where(player => player.Id != id)) {
				RpcId(id, nameof(RemoteAddPlayer), player.Id);
				RpcId(id, nameof(ReceivePlayerInfo), player.Name, player.Id);
				RpcId(id, nameof(ReceiveMapInfo), CurrentMap);
			}
			var newPlayerInfo = GameManager.Players.FirstOrDefault(p => p.Id == id);
			if (newPlayerInfo == null) return;
			
			foreach (var existingPlayer in GameManager.Players.Where(existingPlayer => existingPlayer.Id != id))
				RpcId(existingPlayer.Id, nameof(ReceivePlayerInfo), newPlayerInfo.Name, newPlayerInfo.Id);
		}
	}
	private void OnPeerDisconnected(long id)  
	{  
		GD.Print($"Client {id} disconnected.");  
		if (SpawnNode.HasNode($"{id}"))  
			SpawnNode.GetNode($"{id}").QueueFree();  
	} 
	
	public void AddPlayer(long id)
	{ if (!Multiplayer.IsServer()) return;
		
		voids.AddNewPlayer(PlayerScene, SpawnNode, id);
		Rpc(nameof(RemoteAddPlayer), id);
	}  
	
	
	public void HostButtonDown()  
	{  
		peer = new ENetMultiplayerPeer();  
		var error = peer.CreateServer(port, 3);  
		if (error != Error.Ok) GD.PrintErr("Failed to create server" + error.ToString()); 
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);  
		Multiplayer.MultiplayerPeer = peer;  
		UserCount++;  
		GD.Print("Server started.");  
		AddPlayer(1);  
		SendPlayerInfo("Al Capone", 1);
		
		_menu.HostButtonOff();
	}
	
	public void JoinButtonDown()  
	{  
		GD.Print("Joining server");  
		peer = new ENetMultiplayerPeer();  
		var address = SpawnNode.GetNode<Menu>("Menu").GetNode<TextEdit>("TextEdit").Text;  
		if (address != "") 
			Address = address;  
		
		var err =peer.CreateClient(Address, port);  
		if (err != Error.Ok) 
			GD.PrintErr("Failed to create client"+err.ToString());  
		
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);  
		Multiplayer.MultiplayerPeer = peer;  
		GD.Print("User joined."); 
		
		_menu.UserButtonOff();
	}  
	
	public void StartButtonDown() { Rpc("startGame"); } 
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]  
	private void SendPlayerInfo(string name, int id)  
	{  
		var info = new PlayerInfo() { 
			Name = name,  
			Id = id };  
		if (!GameManager.Players.Contains(info))  
			GameManager.Players.Add(info);
		
		if (!Multiplayer.IsServer()) return;
		
		foreach (var player in GameManager.Players)  
			Rpc(nameof(SendPlayerInfo), player.Name, player.Id);
	}
	
											/* --Recieves-- */
	
	[Rpc(MultiplayerApi.RpcMode.Authority)]  
    private void RemoteAddPlayer(long id)  
    { if (Multiplayer.IsServer()) return;  
      	voids.AddNewPlayer(PlayerScene, SpawnNode, id);
    }  
	
	[Rpc(MultiplayerApi.RpcMode.Authority)]  
	private void ReceivePlayerInfo(string name, int id)
	{  
		var info = new PlayerInfo { Name = name, Id = id };  
		if (!GameManager.Players.Contains(info))  
			GameManager.Players.Add(info);  
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void ReceiveMapInfo(string map) { CurrentMap = map; }
	
											/* --REQUESTS-- */
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]  
	public void RequestCheckGameOver() { if (Multiplayer.IsServer()) CheckGameOver(); }

	public void CheckGameOver()
	{
		if (AliveUserCount > 1) return;
		
		Rpc(nameof(EndGame));
	}
	
	
	
	
	
	
	
	
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]  
	public void StartGame()  
	{  
		foreach (var node in SpawnNode.GetChildren())
			if (node is CardPick pick)
				pick.QueueFree();
		AliveUserCount = UserCount;
		
		var scene = GD.Load<PackedScene>(CurrentMap).Instantiate<Map>();  
		GetParent().GetNode<Camera2D>("Camera2D").Zoom = new Vector2(1920f / 1152, 1080f / 656);  
		GetTree().Root.AddChild(scene);  
		foreach (var player in SpawnNode.GetChildren())  
			if (player is Player pl)
				pl.Position = voids.UsersPosition(1, pl.Name);
		
		SpawnNode.GetNode("Menu").QueueFree();  
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void EndGame()
	{
		foreach (var node in SpawnNode.GetChildren()) { switch (node) {
				case Player { Died: false } pl:
					pl.Died = false; break;
				case Map map:
					map.QueueFree(); break;
				case Bullet bullet:
					bullet.QueueFree(); break;
			} }
		var cardPickScene = GD.Load<PackedScene>("res://Game/card_pick.tscn").Instantiate<CardPick>();
		cardPickScene.UserQueue = DieQueue;
		SpawnNode.AddChild(cardPickScene);
	}
}