using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Slingshot2D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Синглтон
    static private Slingshot2D _instance;            
    static public Slingshot2D Instance => _instance; 


    // Поля для настройки в инспекторе
    [SerializeField] private GameObject _prefabProjectile; // Поле для префаба снаряда
    [SerializeField] private GameObject _launchPoint;      // Поле для точки запуска снаряда
    [SerializeField] private float _velocityMult = 8f;     // Множитель скорости снаряда    


    // Внутреннее состояние
    private Vector2 _launchPos;                  // Позиция точки запуска                                            
    private GameObject _projectile;              // Созданный снаряд
    private Rigidbody2D _projectileRigidbody2D;  // Физика снаряда
    private bool _aimingMode = false;            // Режим прицеливания (выкл/вкл)


    // Кэшируемые компоненты    
    private CircleCollider2D _slingCollider;     // Коллайдер рогатки
    private Mouse _currentMouse;                 // Управление мышью
    private Camera _mainCam;                     // Главная камера (для кеша) 


    // Публичные API
    public Vector2 LaunchPos => _launchPos;    // Текущая позиция точки запуска
    public bool AimingMode => _aimingMode;     // Находимся ли в режиме прицеливания


    // События
    public static event Action<Rigidbody2D> OnProjectileLaunched; // Событие запуска снаряда




    private void Awake()
    {
        // Проверяем нет ли уже синглтона в сцене
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject); // Если есть, удаляем его
            return;
        }

        // Инициализируем синглтон
        _instance = this;


        // Проверяем назначение точки запуска в инспекторе
        if (_launchPoint == null)
        {
            Debug.LogError("Slingshot2D: Launch Point is not assigned in the inspector!");
            return;
        }

        _launchPoint.SetActive(false);                // Скрываем точку запуска
        _launchPos = _launchPoint.transform.position; // Запоминаем позицию точки запуска

        // Кешируем компоненты
        _slingCollider = GetComponent<CircleCollider2D>();
        _currentMouse = Mouse.current;         
        _mainCam = Camera.main;

        if (_slingCollider == null) // Если нет коллайдера на рогатке
        {
            Debug.LogError("Slingshot2D: Missing CircleCollider2D component!");
            enabled = false; // Отключаем компонент
            return;
        }

    }
        

    private void Update()
    {
        if (!_aimingMode || _projectile == null) return; // Если не в режиме прицеливания или снаряд не создан, выходим

        Vector2 mousePos = GetMouseWorldPos();           // Получаем позицию мыши в мировых координатах
        
        Vector2 pullVector = _launchPos - mousePos;      // Вычисляем вектор натяжения                

        float maxPullDistance = _slingCollider.radius;   // Устанавливаем максимально возможную длину натяжения

        pullVector = Vector2.ClampMagnitude(pullVector, maxPullDistance); // Ограничиваем длину вектора натяжения

        // Позиция снаряда следует за мышью с учетом ограничений
        Vector2 projectilePos = _launchPos - pullVector; // Вычисляем новую позицию снаряда
        _projectile.transform.position = projectilePos;  // Обновляем позицию снаряда
    }


    // Метод для получения позиции мыши в мировых координатах 
    private Vector2 GetMouseWorldPos()
    {
        // Ищем позицию мыши в экранных координатах
        Vector2 mousePos = _currentMouse.position.ReadValue();

        // Конвертируем в экранные координаты с учетом ближней плоскости камеры
        Vector3 screenPoint = new Vector3(mousePos.x, mousePos.y, _mainCam.nearClipPlane);

        // Конвертируем в мировые координаты
        Vector3 worldPoint = _mainCam.ScreenToWorldPoint(screenPoint);

        // Возвращаем позицию мыши в мировых координатах
        return new Vector2(worldPoint.x, worldPoint.y);
    }


    // Обработчики событий курсора и нажатий
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Slingshot2D: Cursor ENTERED slingshot collider");

        _launchPoint.SetActive(true); // Показать точку запуска
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Slingshot2D: Cursor LEFT slingshot collider");

        _launchPoint.SetActive(false); // Скрыть точку запуска
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Slingshot2D: LMB is PRESSED");

        if (eventData.button != PointerEventData.InputButton.Left) return; // Если не левая кнопка, выходим

        _aimingMode = true;           // Включаем режим прицеливания  
        _launchPoint.SetActive(true); // Показываем точку запуска

        _projectile = Instantiate(_prefabProjectile, _launchPos, Quaternion.identity); // Создаем снаряд в точке запуска

        _projectileRigidbody2D = _projectile.GetComponent<Rigidbody2D>();   // Получаем компонент Rigidbody2D снаряда

        if (_projectileRigidbody2D == null) // Проверяем наличие Rigidbody2D
        {
            Debug.LogError("Slingshot2D: Projectile prefab must have a Rigidbody2D!");
            CancelAim();
            return;
        }

        _projectileRigidbody2D.bodyType = RigidbodyType2D.Kinematic; // Делаем снаряд кинематическим на время прицеливания
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Slingshot2D: LMB is RELEASED");

        if (_aimingMode)        // Если в режиме прицеливания
        {
            LaunchProjectile(); // Запускаем снаряд
        }
    }

    // Обработчик запуска снаряда
    private void LaunchProjectile()
    {
        _aimingMode = false;           // Выключаем режим прицеливания
        _launchPoint.SetActive(false); // Скрываем точку запуска
                
        if (_projectile == null || _projectileRigidbody2D == null) // Проверяем наличие снаряда и его Rigidbody2D
        {
            CancelAim(); // Если снаряд не создан, отменяем прицеливание
            return;
        }

        Vector2 mousePos = GetMouseWorldPos();            // Получаем позицию мыши в мировых координатах
        Vector2 launchDirection = _launchPos - mousePos;  // Вычисляем направление запуска

        _projectileRigidbody2D.bodyType = RigidbodyType2D.Dynamic;               // Делаем снаряд динамическим для физики
        _projectileRigidbody2D.linearVelocity = launchDirection * _velocityMult; // Задаем скорость снаряду
               

        OnProjectileLaunched?.Invoke(_projectileRigidbody2D); // Оповещаем о запуске снаряда

        _projectile = null;             // Сбрасываем ссылку на снаряд
        _projectileRigidbody2D = null;  // Сбрасываем ссылку на Rigidbody2D
    }


    // Обработчик отмены прицеливания
    private void CancelAim()
    {
        _aimingMode = false;           // Выключаем режим прицеливания
        _launchPoint.SetActive(false); // Скрываем точку запуска

        if (_projectile != null)
        {
            Destroy(_projectile);          // Удаляем снаряд, если он был создан
            _projectile = null;            // Сбрасываем ссылку на снаряд
            _projectileRigidbody2D = null; // Сбрасываем ссылку на Rigidbody2D
        }
    }
}
