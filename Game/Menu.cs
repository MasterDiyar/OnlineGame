using Godot;
using System;

public partial class Menu : Control
{
	[Export] private ServerManager WhereButtonBe{get;set;}
	private Button _host, _join, _start; 
	public override void _Ready()
	{
		base._Ready();
		_host = GetNode<Button>("Host");
		_host.Pressed += WhereButtonBe.HostButtonDown;
		_join = GetNode<Button>("Join");
		_join.Pressed += WhereButtonBe.JoinButtonDown;
		_start = GetNode<Button>("Start");
		_start.Pressed += WhereButtonBe.StartButtonDown;
	}
}
