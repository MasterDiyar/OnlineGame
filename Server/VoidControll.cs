using Godot;

namespace AgeOfEmpires.Server;

public class VoidControll
{
    public void AddNewPlayer(PackedScene playerScene, Node where, long id) {
        var player = playerScene.Instantiate<Player>();  
        player.Name = $"{id}";
        where.AddChild(player);  
    }

    public Vector2 UsersPosition(int type, StringName name) => type switch
        {
            1 => new Vector2(int.Parse(name) % 10 * 100 + 100, 100),
            _ => Vector2.Zero
        }; 

    public Vector2 EV(float x, float y) => new Vector2(x, y);//easy vector

    public Vector2I EVI(int x, int y) => new Vector2I(x, y);  // easy vector integer
    
}