using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance { get; private set; } // Синглтон


    [Header("Set in Inspector")]
    [SerializeField] private float _easing = 0.05f;                // Коэффициент сглаживания камеры
    [SerializeField] private float _orthoSizeOffset = 10f;         // Смещение ортографического размера камеры
    [SerializeField] private Vector2 _minXY = Vector2.zero;        // Минимальные координаты X и Y камеры
    [SerializeField] private Vector2 _homePosition = Vector2.zero; // Стартовая позиция камеры

    [Header("Set Dynamically")]
    [SerializeField] private float _camZ;                         // Желаемая координата Z камеры (глубина)


    private bool _shouldRealiseProjectile = false;        // Флаг состояния снаряда
    
    private Rigidbody _trackedRigidbody;                  // Физика объекта, за которым следует камера
    private Camera _mainCamera;                           // Ссылка на главную камеру

    private GameObject _trackingObject;                   // Объект, за которым следует камера
    public GameObject TrackingObject => _trackingObject;  // Свойство для доступа к объекту интереса



    private void Awake()
    {
        // Инициализация синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _mainCamera = Camera.main;   // Кешируем главную камеру

        _camZ = transform.position.z; // Сохранение исходной Z-координаты камеры
    }

    private void FixedUpdate()
    {
        _shouldRealiseProjectile = false; // Сбрасываем флаг в начале - на случай, если снаряд снова запущен

        // Если объект интереса - снаряд и он остановился, устанавливаем флаг
        if (_trackingObject != null &&
            _trackingObject.CompareTag("Projectile") &&
            _trackedRigidbody != null &&
            _trackedRigidbody.IsSleeping())
        {            
            _shouldRealiseProjectile = true;
        }
    }


    private void LateUpdate()
    {
        // Если флаг установлен - отпускаем снаряд
        if (_shouldRealiseProjectile)
        {
            _trackingObject = null;
            _trackedRigidbody = null;
            _shouldRealiseProjectile = false;
        }


        // Обычная логика слежения камеры
        Vector3 destination;          // Желаемая позиция камеры

        if (_trackingObject == null)  // Если нет объекта интереса
        {
            destination = new Vector3(_homePosition.x, _homePosition.y, _camZ); // Переходим к стартовой позиции
        }
        else
        {
            destination = _trackingObject.transform.position;               // Иначе - к позиции объекта интереса
        }


        // Ограничения по minXY
        destination.x = Mathf.Max(_minXY.x, destination.x);
        destination.y = Mathf.Max(_minXY.y, destination.y);


        // Плавное перемещение камеры
        destination = Vector3.Lerp(transform.position, destination, _easing);
        destination.z = _camZ; // Сохраняем Z-координату камеры

        transform.position = destination;

        // Обновление размера камеры
        _mainCamera.orthographicSize = destination.y + _orthoSizeOffset;
    }



    // Метод для установки объекта интереса
    public void SetPointOfInterest(GameObject target)
    {
        _trackingObject = target;
        _trackedRigidbody = target != null ? target.GetComponent<Rigidbody>() : null;
    }


    // Метод для очистки объекта интереса
    public void ClearPointOfInterest()
    {
        _trackingObject = null;
        _trackedRigidbody = null;
    }
}
