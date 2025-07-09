using Godot;
using System;

public partial class Menu : Control
{
	[Export] private ServerManager WhereButtonBe{get;set;}
	private Button _host, _join, _start;
	private Polygon2D line;
	public Vector2 WhereToGo = new Vector2(0, 100);
	public override void _Ready()
	{
		line = GetNode<Polygon2D>("Polygon2D");
		_host = GetNode<Button>("Host");
		_host.Pressed += WhereButtonBe.HostButtonDown;
		_host.MouseEntered += () => { line.Position = _host.Position-new Vector2(40, 205); };
		_join = GetNode<Button>("Join");
		_join.Pressed += WhereButtonBe.JoinButtonDown;
		_join.MouseEntered += () => { line. Position = _join.Position-new Vector2(40, 205); };
		_start = GetNode<Button>("Start");
		_start.Pressed += WhereButtonBe.StartButtonDown;
		_start.MouseEntered += () => { line. Position = _start.Position-new Vector2(40, 205); };
	}

	public void ShowConnectionError(string message)
	{
		var errorLabel = GetNode<Label>("ErrorLabel");
		errorLabel.Text = message;
		errorLabel.Visible = true;
    
		Callable.From(() => errorLabel.QueueFree()).CallDeferred(5.0);
	}
	
	public void HostButtonOff()
	{
		_start.Disabled = false;
		_host.Disabled = true;
		_join.Disabled = true;
	}

	public void UserButtonOff()
	{
		_host.Disabled = true;
		_join.Disabled = true;
	}

	public void UserButtonOn()
	{
		_host.Disabled = false;
		_join.Disabled = false;
	}
}
