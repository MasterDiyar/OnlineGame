using Godot;
using System;

public partial class boat : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("tab"))
		{
			if (Material is ShaderMaterial shaderMaterial)
			{
				var random = new Random();
            
				// Треугольники
				shaderMaterial.SetShaderParameter("triangles_count", random.Next(5, 30));
				shaderMaterial.SetShaderParameter("triangle_color", new Color(
					(float)random.NextDouble(),
					(float)random.NextDouble(),
					(float)random.NextDouble()
				));
				shaderMaterial.SetShaderParameter("min_size", (float)random.NextDouble() * 0.1f);
				shaderMaterial.SetShaderParameter("max_size", 0.1f + (float)random.NextDouble() * 0.2f);
				shaderMaterial.SetShaderParameter("pulse_speed", (float)random.NextDouble() * 2f);
				shaderMaterial.SetShaderParameter("rotation_speed", (float)random.NextDouble() * 1f);
            
				// Полосы
				shaderMaterial.SetShaderParameter("lines_count", random.Next(5, 20));
				shaderMaterial.SetShaderParameter("stripe_color", new Color(
					(float)random.NextDouble(),
					(float)random.NextDouble(),
					(float)random.NextDouble()
				));
				shaderMaterial.SetShaderParameter("stripe_width", 0.01f + (float)random.NextDouble() * 0.03f);
				shaderMaterial.SetShaderParameter("stripe_speed", (float)random.NextDouble() * 0.5f);
				shaderMaterial.SetShaderParameter("stripe_angle", (float)random.NextDouble());
            
				// Общие параметры
				shaderMaterial.SetShaderParameter("background_color", new Color(
					(float)random.NextDouble() * 0.2f,
					(float)random.NextDouble() * 0.2f,
					(float)random.NextDouble() * 0.3f
				));
				shaderMaterial.SetShaderParameter("seed", (float)random.NextDouble() * 100f);
			}
		}
	}
}
