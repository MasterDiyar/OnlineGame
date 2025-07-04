using Godot;

public partial class Map : Node2D
{
    /*Map types
     1 = Open air
     2 = Space shuttle
     3 = Box battle
     4 = King of hill; Open air
     5 = 
     */
    [Export]public uint MapType = 1;
    public Timer MapTimer;
    [Export] public float DeathMatchWaitTime = 30f; 

    public override void _Ready()
    {
        MapTimer = GetNode<Timer>("DeathMatch");
        MapTimer.SetWaitTime(DeathMatchWaitTime);
        MapTimer.Start();
        MapTimer.Timeout += DeathMatch;
    }

    public void DeathMatch()
    {
        MapTimer.Stop();
    }

    public virtual void ColorChange()
    {
    }
}