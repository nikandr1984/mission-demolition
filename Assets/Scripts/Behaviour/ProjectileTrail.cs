using UnityEngine;
using System.Collections.Generic;

public class ProjectileTrail : MonoBehaviour
{
    // Поля для настройки в инспекторе
    [SerializeField] private float _minDistance = 0.1f;       // Мин. расстояние между точками
    [SerializeField] private float _minTravelDistance = 0.3f; // Мин. расстояние, после которго коллиззии считаются валидными
    [SerializeField] private float _launchGraceTime = 0.15f;  // Время невидимости для коллизии после запуска снаряда 

    // Внутреннее состояние    
    private List<Vector3> _points = new List<Vector3>();   // Список точек линии
    private bool _isTracking = false;                      // Фдаг для отслеживания состояния следа
    private float _launchTimestamp = 0f;                   // Время запуска снаряда
    private Vector3 _launchPosition;                       // Позиция запуска снаряда


    // Кэшируемые компоненты и ссылки
    private LineRenderer _lineRenderer;                    // Ссылка на компонент LineRenderer
    private Rigidbody2D _targetRigidbody;                  // Цель, к которой будет привязан след





    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();  // Кэшируем компонент LineRenderer
        if (_lineRenderer == null)      // Проверочка
        {
            Debug.LogError("ProjectileTrail: LineRenderer not found");
            enabled = false;            // Отключаем скрипт, если LineRenderer не найден
            return;
        }
        _lineRenderer.enabled = false;  // Изначально отключаем LineRenderer
    }


    private void OnEnable()
    {
        Slingshot_New.OnProjectileLaunched += HeandlerProjectileLaunched;           // Подписываемся на событие запуска снаряда
        ProjectaileBehaviour.OnProjectailCollision += HeandlerProjectailCollision;  // Подписываемся на событие столкновения снаряда
        GameManager.OnStartLevel += HeandlerStartLevel;                       // Подписываемся на событие старта уровня 
    }

    private void OnDisable()
    {
        Slingshot_New.OnProjectileLaunched -= HeandlerProjectileLaunched;               
        ProjectaileBehaviour.OnProjectailCollision -= HeandlerProjectailCollision;
        GameManager.OnStartLevel -= HeandlerStartLevel;

    }


    private void HeandlerProjectileLaunched(Rigidbody2D projectileRb)
    {
        // 1. Очищаем предыдущий след
        ClearTrail();
        
        // 2. Включаем режим отслеживания
        _isTracking = true;

        // 3. Начинаем отслеживать новый снаряд
        StartTrail(projectileRb);
    }


    private void HeandlerProjectailCollision(Rigidbody2D projectileRb)
    {
        // 1. Если коллизия не нашего снаряда - игнорируем 
        if (_targetRigidbody != projectileRb) return;

        // 2. Игнорируем коллизию, если прошло недостаточно времени со старта
        float timeSinceLaunch = Time.time - _launchTimestamp;
        if (timeSinceLaunch < _launchGraceTime) return;

        // 3. Игнорируем коллизию, если снаряд пролетел недостаточное расстояние со старта
        float travelDistance = Vector3.Distance(_launchPosition, _targetRigidbody.position);
        if (travelDistance < _minTravelDistance) return;

        // 4. Оканчиваем след, так как коллизия считается валидной
        FinishTrail();
    }


    private void FixedUpdate()
    {
        // 1. Если мы не в режиме отслеживания или цель для следа не установлена - выходим
        if (!_isTracking || _targetRigidbody == null) return;

        // 2. Добавляем текущую позицию снаряда в след
        AddPoint(_targetRigidbody.position); 
    }




    private void StartTrail(Rigidbody2D projectileRb) // Метод для начала отслеживания снаряда
    {
        // 1. Присваиваем ссылку - поле тепер смотри на новый объект
        _targetRigidbody = projectileRb;

        // 2. Проверяем, что ссылка не null, иначе выводим ошибку и выходим
        if (_targetRigidbody == null)   
        {
            Debug.LogError("ProjectileTrail: Received null Rigidbody2D in StartTraking");
            return;
        }

        // 3. Записываем момент запуска и стартовую позицию для грейс-периода
        _launchTimestamp = Time.time;
        _launchPosition = _targetRigidbody.position;

        // 4. Включаем LineRenderer для отображения следа
        _lineRenderer.enabled = true;

        // 5. Добавляем первую точку в след
        AddPoint(_targetRigidbody.position); 
    }


    private void AddPoint(Vector2 position) // Метод для добавления новой точки в след
    {
        Vector3 worldPos = new Vector3(position.x, position.y, transform.position.z);

        if (_points.Count > 0 && Vector3.Distance(worldPos, _points[^1]) < _minDistance)
        {
            return; // Если новая точка слишком близко к последней, не добавляем её
        }

        _points.Add(worldPos); // Добавляем новую точку в список

        _lineRenderer.positionCount = _points.Count;            // Обновляем количество точек в LineRenderer
        _lineRenderer.SetPosition(_points.Count - 1, worldPos); // Устанавливаем позицию новой точки в LineRenderer
    }


    private void FinishTrail()
    {
        // 1. Выключаем режим отслеживания
        _isTracking = false;

        // 2. Обнуляем ссылке на отслеживаемый Rigidbody2D
        _targetRigidbody = null;
    }


    private void ClearTrail()
    {
        _points.Clear();                  // Очищаем список точек
        _lineRenderer.positionCount = 0;  // Сбрасываем количество точек в LineRenderer
        _lineRenderer.enabled = false;    // Отключаем LineRenderer
        _targetRigidbody = null;          // Сбрасываем ссылку на Rigidbody2D
        _isTracking = false;              // Сбрасываем флаг отслеживания
        _launchTimestamp = 0f;            // Сбрасываем время запуска
        _launchPosition = Vector3.zero;   // Сбрасываем позицию запуска
    }
    

    private void HeandlerStartLevel(LevelData levelData)
    {
        ClearTrail();
    }

}
