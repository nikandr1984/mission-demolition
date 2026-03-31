using UnityEngine;

public class GameUtilitys
{
    public static Vector2 GetWorldPositionFromScreen(Vector2 cursorPositionOnScreen, Camera camera = null)
    {
        // 1. Если камера не передана, используем главную камеру
        if (camera == null) camera = Camera.main;

        // 2. Зфщита от NullReferenceException
        if (camera == null)
        {
            Debug.LogError("GameUtilitys: No camera available.");
            return Vector2.zero;
        }

        // 3. Получаем абсолютное значение z-координаты камеры
        float distanceFromCamera = Mathf.Abs(camera.transform.position.z);

        // 4. Создаем переменную Vector3 для преобразования
        Vector3 screenPointV3 = new Vector3(cursorPositionOnScreen.x, cursorPositionOnScreen.y, distanceFromCamera);

        // 5. Преобразуем экранные координаты в мировые
        Vector3 cursorPositionOnWorld = camera.ScreenToWorldPoint(screenPointV3);

        // 6. Возвращаем только x и y координаты в виде Vector2
        return new Vector2(cursorPositionOnWorld.x, cursorPositionOnWorld.y);
    }
}
