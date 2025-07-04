using Godot;
using System;

public partial class Gorgoroth : Map
{
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("tab"))
		{
			ColorChange();
		}
	}

	public override void ColorChange()
	{
		var rand = new Random();
		var bg = GetNode<Sprite2D>("Пщкпщкще");
		var stata = GetNode<StaticBody2D>("StaticBody2D");
		if (stata.Material is ShaderMaterial sm)
		{
			sm.SetShaderParameter("offset", new Vector2(rand.Next(30, 50)/100f, rand.Next(20, 40)/100f));
			sm.SetShaderParameter("spin_speed", rand.Next(1, 4));


			sm.SetShaderParameter("colour_1", new Color(rand.NextSingle(),rand.NextSingle(),rand.NextSingle()));
			sm.SetShaderParameter("colour_2", new Color(rand.NextSingle(),rand.NextSingle(),rand.NextSingle()));
			sm.SetShaderParameter("colour_3", new Color(rand.NextSingle(),rand.NextSingle(),rand.NextSingle()));

		}

		if (bg.Material is ShaderMaterial sf)
		{
			sf.SetShaderParameter("contrast", rand.NextSingle()*2);
			sf.SetShaderParameter("polar_repeat", rand.Next(6,8));
			sf.SetShaderParameter("spin_speed", rand.NextSingle());
			sf.SetShaderParameter("pixel_filter", rand.Next(2000,5000));

		}
	}
}
