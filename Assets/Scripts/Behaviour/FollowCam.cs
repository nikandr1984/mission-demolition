using Unity.VisualScripting;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance { get; private set; } // Синглтон для доступа из других классов

    // Поля для настройки в инспекторе
    [SerializeField] private float _easing = 0.05f;                         // Коэффициент сглаживания камеры
    [SerializeField] private float _orthoSizeOffset = 10f;                  // Базовый размер ортографической камеры
    [SerializeField] private float _viewOrthoSize = 15f;                    // Размер камеры при обзоре уровня
    [SerializeField] private float _camZ = -10f;                            // Желаемая координата Z камеры (глубина)

    [SerializeField] private float _leftBound = 5f;                         // Левая граница уровня
    [SerializeField] private float _rightBound = 80f;                       // Правая граница уровня
    [SerializeField] private float _bottomBound = 0f;                       // Нижняя граница уровня
    [SerializeField] private float _topBound = 50f;                         // Верхняя граница уровня

    [SerializeField] private Vector2 _homePosition = Vector2.zero;          // Стартовая позиция камеры
    [SerializeField] private Vector2 _viewPosition = new Vector2(25f, 25f); // Позиция камеры для обзора уровня

    
    // Внутреннее состояние    
    private bool _shouldRealiseProjectile = false;   // Флаг состояния снаряда
    private bool _isViewingLevel = false;            // Флаг для отслеживания, находится ли камера в обзорной позиции


    private Camera _mainCamera;                      // Ссылка на основную камеру
    private GameObject _trackingObject;              // Объект, за которым следует камера
    private Rigidbody2D _trackedRigidbody2D;         // Физика объекта, за которым следует камера




    private void OnEnable()
    {
        Slingshot.OnProjectileLaunched += HandleProjectailLaunched; // Подписываемся на событие запуска снаряда        
    }



    private void OnDisable()
    {
        Slingshot.OnProjectileLaunched -= HandleProjectailLaunched; // Отписываемся от события при отключении
    }



    private void Awake()
    {
        // Реализация синглтона для обеспечения единственного экземпляра камеры
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _mainCamera = Camera.main; // Получаем ссылку на основную камеру

    }


    private void HandleProjectailLaunched(Rigidbody2D projectileRb)
    {
        _trackingObject = projectileRb.gameObject; // Сохраняем ссылку на объект, за которым нужно следить
        _trackedRigidbody2D = projectileRb;        // Сохраняем ссылку на Rigidbody2D для проверки его состояния
    }


    private void FixedUpdate()
    {
        _shouldRealiseProjectile = false; // Сбрасываем флаг в начале каждого кадра

        // Если объект интереса - снаряд, и он остановился, устанавливаем флаг для последующей обработки в LateUpdate
        if (_trackingObject != null &&
            _trackingObject.CompareTag("Projectile") &&
            _trackedRigidbody2D != null &&
            _trackedRigidbody2D.IsSleeping())
        {
            _shouldRealiseProjectile = true; // Устанавливаем флаг, если снаряд остановился
        }
    }


    private void LateUpdate()
    {
        // 1. Проверяем не остановился ли снаряд, если да - отпускаем его и сбрасываем флаг        
        if (_shouldRealiseProjectile)
        {
            _trackingObject = null;
            _trackedRigidbody2D = null;
            _shouldRealiseProjectile = false; 
        }


        // 2. Движение камеры за снарядом, к точке осмотра или стартовой позиции        
        
        Vector3 destination; // Хранит следующую позицию камеры
        
        
        if (_trackingObject != null) // Если снаряд есть - двигаем камеру за ним
        {
            destination = _trackingObject.transform.position;  
            
             
        }
        
        else if (_trackingObject == null && _isViewingLevel) // Если осматриваем уровень
        {
            destination = new Vector3(_viewPosition.x, _viewPosition.y, _camZ);            
        }

        else // Если снаряда нет - двигаем камеру к стартовой позиции
        {            
            destination = new Vector3(_homePosition.x, _homePosition.y, _camZ);
        }

        // 3. Ограничение камеры границами уровня (Clamping)
        destination.x = Mathf.Clamp(destination.x, _leftBound, _rightBound);
        destination.y = Mathf.Clamp(destination.y, _bottomBound, _topBound);                         

        // 4. Плавное перемещение камеры (Lerp)
        destination = Vector3.Lerp(transform.position, destination, _easing);

        // 5. Принудительное сохранение Z-координаты камеры, чтобы она не смещалась по глубине
        destination.z = _camZ;

        // 7. Применение новой позиции к камере
        transform.position = destination;

        // 6. Динамический зум камеры (только если не в режиме обзора уровня)     
        _mainCamera.orthographicSize = destination.y + _orthoSizeOffset;
    }


    public void MoveToViewPosition() // Метод для перемещения камеры к обзорной позиции уровня
    {        
        _isViewingLevel = !_isViewingLevel;
    }


   
}
