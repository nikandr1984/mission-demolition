using UnityEngine;

public class FollowCam : MonoBehaviour
{
    static public GameObject POI;  // "Point of interest" - cсылка на интересующий объект

    [Header("Set in Inspector")]
    public float easing = 0.05f;
    public Vector2 minXY = Vector2.zero;
    
    [Header("Set Dynamically")]
    public float camZ;            // Желаемая координата Z камеры (глубина)

    private void Awake()
    {
        camZ = transform.position.z;
    }

    private void FixedUpdate()
    {
        if (POI == null) return; // Если нет объекта интереса - выйти

        // Получить позицию объекта интереса
        Vector3 destination = POI.transform.position;

        // Ограничить X и Y минимальными значениями
        destination.x = Mathf.Max(minXY.x, destination.x);
        destination.y = Mathf.Max(minXY.y, destination.y);

        // Плавно перемещаем камеры от текущей позиции к объекту интереса
        destination = Vector3.Lerp(transform.position, destination, easing);

        // Сохраняем исходную Z-координату камеры (чтобы камера не меняла глубину)
        destination.z = camZ;

        // Устанавливаем новую позицию камеры
        transform.position = destination;

        // Изменяем размер камеры, чтобы земля оставалась видна
        Camera.main.orthographicSize = destination.y + 10;
    }
}
