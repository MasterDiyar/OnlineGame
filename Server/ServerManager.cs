using Godot;
using System;
using System.Collections.Generic;
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
	private List<int> DieQueue = [];
	public string CurrentMap = "";
	public Camera2D mainCamera;
	public int UserCount = 0, AliveUserCount = 0;
	
	public override void _Ready()  
	{  
		_menu = SpawnNode.GetNode<Menu>("Menu");
		mainCamera = SpawnNode.GetNode<Camera2D>("Camera2D");
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
		SetNewMap();
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
	
	public void StartButtonDown() { Rpc(nameof(StartGame)); } 
	
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

	private void SetNewMap()
	{
		CurrentMap = maps[rand.Next(maps.Length)];
		
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void ReceiveMapInfo(string map) { CurrentMap = map; }
	
											/* --REQUESTS-- */
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]  
	public void RequestCheckGameOver() { if (Multiplayer.IsServer()) CheckGameOver(); }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]  
	public void CheckGameOver()
	{
		if (!Multiplayer.IsServer()) return;
		AliveUserCount--;
		if (AliveUserCount > 1) return; 
		GD.Print("Game over.");
		foreach(var diea in DieQueue)GD.Print(diea.ToString());
		Rpc(nameof(EndGame), DieQueue.ToArray());
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)] 
	public void RequestAddDied(string id) { if (!DieQueue.Contains(int.Parse(id)))
	{
		GD.Print($"trying to add {id}");
		DieQueue.Add(int.Parse(id));
	} }


	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]  
	public void StartGame()  
	{  
		foreach (var node in SpawnNode.GetChildren())
			if (node is CardPick pick)
				pick.QueueFree();
		AliveUserCount = UserCount;
		
		var scene = GD.Load<PackedScene>(CurrentMap).Instantiate<Map>();  
		Vector2I screenSize = DisplayServer.WindowGetSize();
		mainCamera.Enabled = true;
		mainCamera.Zoom = new Vector2(screenSize.X / 1152f, screenSize.Y / 656f);  
		SpawnNode.AddChild(scene);
		foreach (var player in SpawnNode.GetChildren())
			if (player is Player pl) {
				pl.Position = voids.UsersPosition(1, pl.Name);
				pl.Rpc(nameof(pl.PerformRespawn));
			}
		if (SpawnNode.HasNode("Menu")) SpawnNode.GetNode("Menu").QueueFree();  
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void EndGame(int[] dieQueue)
	{
		mainCamera.Enabled = false;
		var l = dieQueue.ToList();
		foreach (var node in SpawnNode.GetChildren()) { switch (node) {
				case Player pl:
					if (!l.Contains(int.Parse(pl.Name))) {
						l.Add(int.Parse(pl.Name));
						pl.Rpc(nameof(pl.PerformWinnerDie));
						GD.Print($"{pl.Name} has been survived");
					} break;
				case Map map:
					map.QueueFree(); break;
				case Bullet bullet:
					bullet.QueueFree(); break;
			} }
		
		GD.Print("Spawning Card Pick");
		var cardPickScene = GD.Load<PackedScene>("res://Game/card_pick.tscn").Instantiate<CardPick>();
		
		cardPickScene.UserQueue = l;
		SpawnNode.AddChild(cardPickScene);
		if (!Multiplayer.IsServer()) return;
		SetNewMap();
		Rpc(nameof(ReceiveMapInfo), CurrentMap);
	}
}