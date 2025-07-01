using Godot;
using System;

public partial class Mapcon : Map
{
	[Export]Sprite2D bgSprite;
	public override void _Ready()
	{
		ColorChange();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("tab"))
		{
			ColorChange();
		}
	}

	public void ColorChange()
	{
		var random = new Random();
		if (Material is ShaderMaterial shaderMaterial) {
				
				
			shaderMaterial.SetShaderParameter("squares_count", random.Next(16,192));
			shaderMaterial.SetShaderParameter("square_color", new Color(
				random.NextSingle(),
				random.NextSingle(),
				random.NextSingle()
			));
			shaderMaterial.SetShaderParameter("background_color", new Color(
				random.NextSingle(),
				random.NextSingle(),
				random.NextSingle()
			));
			var min = random.Next(1, 5);
			shaderMaterial.SetShaderParameter("min_size", min/100f);
			shaderMaterial.SetShaderParameter("max_size", random.Next(min, 20)/100f);
			shaderMaterial.SetShaderParameter("pulse_speed", random.NextSingle()*2);
			shaderMaterial.SetShaderParameter("seed", random.Next(0, 10));
		}

		if (bgSprite.Material is ShaderMaterial bgShaderMaterial) {
			bgShaderMaterial.SetShaderParameter("color1", new Color(
				random.NextSingle(),
				random.NextSingle(),
				random.NextSingle()
			));
			bgShaderMaterial.SetShaderParameter("color2", new Color(
				random.NextSingle(),
				random.NextSingle(),
				random.NextSingle()
			));
			bgShaderMaterial.SetShaderParameter("speed", random.NextSingle()*2);
			bgShaderMaterial.SetShaderParameter("wave_frequency", random.NextSingle()*10);
			bgShaderMaterial.SetShaderParameter("wave_amplitude", random.NextSingle()/2);
			bgShaderMaterial.SetShaderParameter("noize_scale", random.NextSingle()/5f);
			bgShaderMaterial.SetShaderParameter("color_mix_speed", random.NextSingle()*2);
		}
	}
}
