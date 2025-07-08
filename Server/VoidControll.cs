using Godot;

namespace AgeOfEmpires.Server;

public class VoidControll
{
    public void AddNewPlayer(PackedScene playerScene, Node where, long id) {
        var player = playerScene.Instantiate<Player>();  
        player.Name = $"{id}";
        player.SetMultiplayerAuthority((int)id);
        player.Position = EV(625, 225);
        where.AddChild(player);  
    }

    public Vector2 UsersPosition(int type, StringName name) => type switch
        {
            1 => new Vector2(int.Parse(name) % 10 * 100 + 100, 100),
            _ => Vector2.Zero
        }; 

    public Vector2 EV(float x, float y) => new Vector2(x, y);//easy vector

    public Vector2I EVI(int x, int y) => new Vector2I(x, y);  // easy vector integer
    
    public Color[] Colours = new Color[]
    {
        new Color(0, 0, 0),          // 0. Чёрный
        new Color(0.25f, 0, 0),       // 1. Тёмно-красный
        new Color(0.5f, 0, 0),        // 2. Красный
        new Color(0.75f, 0, 0),       // 3. Ярко-красный
        new Color(1f, 0, 0),          // 4. Полностью красный
        new Color(0, 0.25f, 0),       // 5. Тёмно-зелёный
        new Color(0, 0.5f, 0),        // 6. Зелёный
        new Color(0, 0.75f, 0),       // 7. Ярко-зелёный
        new Color(0, 1f, 0),          // 8. Полностью зелёный
        new Color(0, 0, 0.25f),       // 9. Тёмно-синий
        new Color(0, 0, 0.5f),        // 10. Синий
        new Color(0, 0, 0.75f),       // 11. Ярко-синий
        new Color(0, 0, 1f),          // 12. Полностью синий
        new Color(0.5f, 0.5f, 0),     // 13. Жёлтый
        new Color(0.5f, 0, 0.5f),     // 14. Пурпурный
        new Color(0, 0.5f, 0.5f),     // 15. Бирюзовый
        new Color(0.5f, 0.5f, 0.5f),  // 16. Серый
        new Color(0.75f, 0.25f, 0),   // 17. Оранжево-красный
        new Color(0.75f, 0, 0.25f),    // 18. Розово-пурпурный
        new Color(0, 0.75f, 0.25f),    // 19. Зелёно-бирюзовый
        new Color(0.25f, 0.75f, 0),    // 20. Салатовый
        new Color(0, 0.25f, 0.75f),    // 21. Сине-бирюзовый
        new Color(0.25f, 0, 0.75f),    // 22. Фиолетовый
        new Color(0.75f, 0.75f, 0),    // 23. Ярко-жёлтый
        new Color(0.75f, 0, 0.75f),    // 24. Ярко-пурпурный
        new Color(0, 0.75f, 0.75f),    // 25. Ярко-бирюзовый
        new Color(0.75f, 0.75f, 0.25f),// 26. Светло-жёлтый
        new Color(0.75f, 0.25f, 0.75f),// 27. Светло-пурпурный
        new Color(0.25f, 0.75f, 0.75f),// 28. Светло-бирюзовый
        new Color(0.8f, 0.8f, 0.8f),   // 29. Светло-серый
        new Color(0.9f, 0.9f, 0.9f),   // 30. Почти белый
        new Color(1f, 1f, 1f)          // 31. Белый
    };
}