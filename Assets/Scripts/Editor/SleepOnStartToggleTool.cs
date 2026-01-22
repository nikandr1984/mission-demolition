using UnityEngine;
using UnityEditor;

public static class SleepOnStartToggleTool
{
    private static bool _isEnabled = true;


    [MenuItem("Tools/Toggle Sleep On Start for PhysicalProperties")]
    private static void ToggleSleepOnStart()
    {
        // Находим все объекты с компонентом PhysicalProperties в сцене
        var physicalProps = Object.FindObjectsByType<PhysicalProperties>(FindObjectsSortMode.None);


        // Проверяем, есть ли такие объекты
        if (physicalProps == null || physicalProps.Length == 0)
        {
            Debug.LogWarning("SleepOnStartToggleTool: No PhysicalProperties components found in the scene");
            return;
        }

        // Перключаем глобальное состояние
        _isEnabled = !_isEnabled;

        // Счетчик измененных компонентов
        int modifiedCount = 0;

        // Обновляем свойство EditorSleepOnStart для каждого компонента
        foreach (var pp in physicalProps)
        {
            if (pp != null) // Проверяем, что компонент не равен null
            {
                pp.EditorSleepOnStart = _isEnabled; // Устанавливаем новое значение
                EditorUtility.SetDirty(pp);         // Необходимо для сохранения изменений в сцене
                modifiedCount++;                    // Увеличиваем счетчик
            }
        }

        string action = _isEnabled ? "enabled" : "disabled";
        Debug.Log($"SleepOnStartToggleTool: SleepOnStart has been {action} for {modifiedCount} PhysicalProperties components.");

    }
}
