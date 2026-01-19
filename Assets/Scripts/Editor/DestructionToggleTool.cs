using UnityEngine;
using UnityEditor;


// Скрипт отключения/включения всех Destructible компонентов в сцене
public class DestructionToggleTool : ScriptableObject 
{
    private static bool _destructiblesEnabled = true;   // Флаг состояния Destructible компонентов

    [MenuItem("Tools/Toggle Destructible Components")]  // Добавление пункта в меню Unity Editor


    // Метод для переключения состояния всех Destructible компонентов
    private static void ToggleAllDestructibles()
    {
        // Поиск всех Destructible компонентов в сцене
        var destructibles = Object.FindObjectsByType<Destructible>(FindObjectsSortMode.None);


        // Проверка наличия Destructible компонентов
        if (destructibles == null || destructibles.Length == 0)
        {
            Debug.LogWarning("DestructionToggleTool: No Destructible components found in the scene");
            return;
        }

        // Переключаем состояние: если сейчас включены - выключим, и наоборот
        _destructiblesEnabled = !_destructiblesEnabled;


        // Применяем новое состояние ко всем Destructible компонентам
        foreach (var d in destructibles)
        {
            if (d != null)
            {
                d.enabled = _destructiblesEnabled;
            }
        }

        string action = _destructiblesEnabled ? "enabled" : "disabled";

        Debug.Log($"Toggled {destructibles.Length} Destructible components: {action}"); 
    }
}