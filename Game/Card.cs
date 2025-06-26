using Godot;
using System;

public partial class Card : Sprite2D
{
	private Vector2 _textureSize = Vector2.Zero;
	private bool _check = false;
	
	public override void _Ready()
	{
		if (Texture != null)
		{
			_textureSize = Texture.GetSize();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			Vector2 worldPos = GetGlobalMousePosition();
			Vector2 localPos = ToLocal(worldPos);
            
			if (GetRect().HasPoint(localPos) && _check)
			{
				Vector2 uv = GetUvFromClick(localPos);
				BurnCard(uv);
			}
		}
	}

	public override void _Process(double delta)
	{
		Vector2 worldPos = GetGlobalMousePosition();
		Vector2 localPos = ToLocal(worldPos);
            
		if (GetRect().HasPoint(localPos)) {
			Scale = Vector2.One * 1.1f;
			_check = true;
		}else {
			_check = false;
			Scale = Vector2.One;
		}
	}

	private Vector2 GetUvFromClick(Vector2 localClickPos)
	{
		Vector2 topLeftPos = localClickPos + (_textureSize / 2);
		Vector2 uv = topLeftPos / _textureSize;
		return uv;
	}

	private void BurnCard(Vector2 uv)
	{
		if (Material is ShaderMaterial shaderMaterial)
		{
			Tween tween = CreateTween();
			shaderMaterial.SetShaderParameter("position", uv);
			tween.TweenMethod(Callable.From<float>(UpdateRadius), 0.0f, 2.0f, 1.5f);
		}
	}

	private void UpdateRadius(float value)
	{
		if (Material is ShaderMaterial shaderMaterial)
			shaderMaterial.SetShaderParameter("radius", value);
	}
}
