using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// Класс Cloud создает облако, состоящее из нескольких сфер с случайными параметрами
public class Cloud : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject cloudSphere;                            // Префаб сферы, из которой будет состоять облако
    public int numSpheresMin = 6;                             // Минимальное количество сфер в облаке
    public int numSpheresMax = 10;                            // Максимальное количество сфер в облаке
    public Vector3 spherOffsetScale = new Vector3(5, 2, 1);   // Масштаб случайного смещения сфер в облаке
    public Vector2 sphereScaleRangeX = new Vector2(4, 8);     // Диапазон случайного масштаба сфер по оси X
    public Vector2 sphereScaleRangeY = new Vector2(3, 4);     // Диапазон случайного масштаба сфер по оси Y
    public Vector2 sphereScaleRangeZ = new Vector2(2, 4);     // Диапазон случайного масштаба сфер по оси Z
    public float scaleYMin = 2f;                              // Минимальный масштаб сферы по оси Y

    private List<GameObject> spheres;      // Список для хранения всех сфер облака

    private void Start()
    {
        spheres = new List<GameObject>(); // Инициализация списка сфер

        // Генерируем случайное количество сфер в облаке (в заданном диапазоне)
        int num = Random.Range(numSpheresMin, numSpheresMax);

        // Создаем каждую сферу в облаке
        for (int i=0;  i<num; i++)
        {
            
            GameObject sp = Instantiate (cloudSphere);  // Создаем экземпляр сферы из префаба
            spheres.Add(sp);                            // Добавляем сферу в список
            Transform spTrans = sp.transform;           // Получаем трансформу сферы для удобства
            spTrans.SetParent(transform);               // Устанавливаем родительский объект для сферы (облако)

            // Генерируем случайное смещение для сферы внутри облака   
            Vector3 offset = Random.insideUnitSphere;   // Получаем случайную точку внутри сферы радиусом 1
            // Масштабируем смещение по осям
            offset.x *= spherOffsetScale.x;             // Больше разброс по Х (ширина облака)
            offset.y *= spherOffsetScale.y;             // Умеренный разброс по Y (высота облака)
            offset.z *= spherOffsetScale.z;             // Наиеньший разброс по Z (глубина облака)
            spTrans.localPosition = offset;             // Устанавливаем позицию сферы в облаке

            // Генерируем случайный масштаб для сферы
            Vector3 scale = Vector3.one; // Начальный масштаб сферы (1, 1, 1)
            // Устанавливаем случайный масштаб для каждой оси в заданных диапазонах
            scale.x = Random.Range(sphereScaleRangeX.x, sphereScaleRangeX.y);
            scale.y = Random.Range(sphereScaleRangeY.x, sphereScaleRangeY.y);
            scale.z = Random.Range(sphereScaleRangeZ.x, sphereScaleRangeZ.y);

            // Корректируем масштаб по оси Y в зависимости от расстояния от центра по X
            // Чем дальше от центра, тем меньше масштаб по Y (делаем облако более плоским к краям
            scale.y *= 1 - (Mathf.Abs(offset.x) / spherOffsetScale.x);
            // Обепечиваем, чтобы масштаб по Y не был меньше минимального значения
            scale.y = Mathf.Max(scale.y, scaleYMin);

            // Применяем итоговый масштаб к сфере
            spTrans.localScale = scale;
        }
    }

    private void Update()
    {
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Restart();
        }
        */

        // Колибровочный метод для определения приемлемых значений в инспекторе 
        void Restart()
        {
            // Удалить старые сферы, составляющие облако
            foreach (GameObject sp in spheres)
            {
                Destroy(sp);
            }
            Start();
        }
    }
}
