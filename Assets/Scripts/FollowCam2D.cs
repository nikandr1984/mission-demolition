using UnityEngine;

public class FollowCam2D : MonoBehaviour
{
    public static FollowCam2D Instance { get; private set; } // Синглтон для доступа из других классов

    // Поля для настройки в инспекторе
    [SerializeField] private float _easing = 0.05f;                   // Коэффициент сглаживания камеры
    [SerializeField] private float _orthoSizeOffset = 10f;            // Базовый размер ортографической камеры
    [SerializeField] private float _camZ = -10f;                      // Желаемая координата Z камеры (глубина)
    [SerializeField] private Vector2 _minXY = Vector2.zero;           // Минимальные координаты X и Y камеры
    [SerializeField] private Vector2 _maxXY = new Vector2(80f, 50f);  // Максимальные координаты X и Y камеры
    [SerializeField] private Vector2 _homePosition = Vector2.zero;    // Стартовая позиция камеры

    // Внутреннее состояние    
    private bool _shouldRealiseProjectile = false;   // Флаг состояния снаряда
    
    
    private Camera _mainCamera;                      // Ссылка на основную камеру
    private GameObject _trackingObject;              // Объект, за которым следует камера
    private Rigidbody2D _trackedRigidbody2D;         // Физика объекта, за которым следует камера




    private void OnEnable()
    {
        Slingshot2D.OnProjectileLaunched += HandleProjectailLaunched; // Подписываемся на событие запуска снаряда
        Debug.Log("FolowCam2D: Subscribed to Slingshot2D.OnProjectileLaunched event.");
    }



    private void OnDisable()
    {
        Slingshot2D.OnProjectileLaunched -= HandleProjectailLaunched; // Отписываемся от события при отключении
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


        // 2. Логика движения камеры к объекту интереса или к стартовой позиции, если объекта нет        
        
        Vector3 destination; // Хранит следующую позицию камеры

        // Если снаряд есть - двигаем камеру за ним
        if (_trackingObject != null)
        {
            destination = _trackingObject.transform.position;            
        }
        // Если снаряда нет - двигаем камеру к стартовой позиции
        else
        {
            destination = new Vector3(_homePosition.x, _homePosition.y, _camZ);
        }

        // 3. Ограничение камеры границами уровня (Clamping)
        destination.x = Mathf.Clamp(destination.x, _minXY.x, _maxXY.x);
        destination.y = Mathf.Clamp(destination.y, _minXY.y, _maxXY.y);                         

        // 4. Плавное перемещение камеры (Lerp)
        destination = Vector3.Lerp(transform.position, destination, _easing);

        // 5. Принудительное сохранение Z-координаты камеры, чтобы она не смещалась по глубине
        destination.z = _camZ;

        // 7. Применение новой позиции к камере
        transform.position = destination;

        // 6. Динамический зум камеры
        _mainCamera.orthographicSize = destination.y + _orthoSizeOffset; // Увеличиваем размер камеры по мере подъема

    }   
}
