using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Information")]
    public int levelNumber = 1;              // Номер уровня    
    public string levelName = "Level 1";     // Название уровня

    [Header("Level Content")]
    public GameObject castlePrefab;                      // Префаб замка для этого уровня
    public Vector3 castleSpawnPosition = Vector3.zero;   // Позиция спавна замка
    public int projectileCount = 3;                      // Количество снарядов, доступных игроку
    public int targetCount = 3;                          // Количество целей на уровне

    [Header("Progression")]
    public LevelData nextLevelData;

    [Header("Visuals")]
    public Sprite backgroundSprite;
}
