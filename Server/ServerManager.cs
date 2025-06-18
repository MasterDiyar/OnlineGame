using Godot;
using System;
using System.Linq;

public partial class ServerManager : Control
{
	private ENetMultiplayerPeer peer;
	public int port = 8901;
	[Export] private Node SpawnNode { get; set; }
	[Export] private PackedScene PlayerScene{get;set;}
	
	public string Address = "127.0.0.1";

	public override void _Ready()
	{
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServeri;
		Multiplayer.ConnectionFailed += () => GD.Print("Connection failed");
	}

	/// <summary>
	/// runs when the connection is successful and only runs on the clients
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	private void ConnectedToServeri()
	{
		GD.Print("Connected to server");
	}

	private void OnPeerConnected(long id)
	{
		GD.Print($"Client {id} connected.");
		if (Multiplayer.IsServer())
		{
			// First create the new player
			AddPlayer(id);
        
			// Send existing player info to the new client
			foreach (var player in GameManager.Players)
			{
				if (player.Id != id) // Don't send the new player their own info again
				{
					// Tell the new client to add existing players
					RpcId(id, nameof(RemoteAddPlayer), player.Id);
                
					// Send player info to the new client
					RpcId(id, nameof(ReceivePlayerInfo), player.Name, player.Id);
				}
			}
        
			// Send the new player's info to all existing clients
			var newPlayerInfo = GameManager.Players.FirstOrDefault(p => p.Id == id);
			if (newPlayerInfo != null)
			{
				foreach (var existingPlayer in GameManager.Players)
				{
					if (existingPlayer.Id != id) // Don't send to self
					{
						RpcId(existingPlayer.Id, nameof(ReceivePlayerInfo), newPlayerInfo.Name, newPlayerInfo.Id);
					}
				}
			}
		}
	}
	private void OnPeerDisconnected(long id)
	{
		GD.Print($"Client {id} disconnected.");
		if (SpawnNode.HasNode($"{id}"))
		{
			SpawnNode.GetNode($"{id}").QueueFree();
		}
	}
	public void HostButtonDown()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 3);
		if (error != Error.Ok)
		{
			GD.PrintErr("Failed to create server");
			GD.PrintErr(error.ToString());
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server started.");
		AddPlayer(1);
		sendPlayerInfo("Al Capone", 1);
	}

	public void JoinButtonDown()
	{
		GD.Print("Joining server");
		peer = new ENetMultiplayerPeer();
		var address = SpawnNode.GetNode<Menu>("Menu").GetNode<TextEdit>("TextEdit").Text;
		if (address != "")
		{
			GD.Print("Joining server on address: ", address);
			Address = address;
		}
		var err =peer.CreateClient(Address, port);
		if (err != Error.Ok)
		{
			GD.PrintErr("Failed to create client");
			GD.PrintErr(err.ToString());
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("User joined.");
	}

	
	public void StartButtonDown()
	{
		Rpc("startGame");
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void startGame()
	{
		foreach (var players in GameManager.Players)
		{
			GD.Print($"Starting player {players.Id}");
		}
		
		var scene = GD.Load<PackedScene>("res://Game/map.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
		foreach (var player in SpawnNode.GetChildren())
		{
			if (player is Player pl)
			{
				pl.Position = new Vector2(int.Parse(pl.Name)%10 * 100+100, 100);
			}
		}
	}

	public void AddPlayer(long id)
	{
		if (Multiplayer.IsServer())
		{
			GD.Print($"Server creating player {id}");
			var player = PlayerScene.Instantiate<Player>();
			player.Name = $"{id}";
        
			// Add to spawn node or root
			if (SpawnNode != null)
				SpawnNode.AddChild(player);
			else
				GetTree().Root.AddChild(player);
            
			// Tell all clients to add this player
			Rpc(nameof(RemoteAddPlayer), id);
		}
	}
	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RemoteAddPlayer(long id)
	{
		// Skip if we're the server (already handled)
		if (Multiplayer.IsServer()) return;
    
		GD.Print($"Client adding player {id}");
		var player = PlayerScene.Instantiate<Player>();
		player.Name = $"{id}";
    
		if (SpawnNode != null)
			SpawnNode.AddChild(player);
		else
			GetTree().Root.AddChild(player);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInfo(string name, int id)
	{
		PlayerInfo info = new PlayerInfo()
		{
			Name = name,
			Id = id
		};
		if (!GameManager.Players.Contains(info))
		{
			GameManager.Players.Add(info);
		}

		if (Multiplayer.IsServer())
		{
			foreach (var player in GameManager.Players)
			{
				Rpc("sendPlayerInfo", player.Name, player.Id);
			}
		}
	}
	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void ReceivePlayerInfo(string name, int id)
	{
		// Update or add player info
		var info = new PlayerInfo { Name = name, Id = id };
		if (!GameManager.Players.Contains(info))
		{
			GameManager.Players.Add(info);
		}
	}
}
