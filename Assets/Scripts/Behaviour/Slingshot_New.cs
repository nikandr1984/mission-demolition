using System;
using UnityEngine;

public class Slingshot_New : MonoBehaviour
{
    // Синглтон
    private static Slingshot_New _instance;
    public static Slingshot_New Instance => _instance;


    // Настройки в инспекторе
    [Header("References")]
    [SerializeField] private GameObject _prefabProjectile;    // Префаб снаряда
    [SerializeField] private Transform _launchPoint;          // Точка запуска снаряда
    [SerializeField] private GameInput _gameInput;            // Обработчик ввода
    [SerializeField] private Camera _mainCamera;              // Главная камера

    [Header("Physics Settings")]
    [SerializeField] private float _velocityMultiplier = 8f; // Сила выстрела
    [SerializeField] private float _maxPullDistance = 2f;    // Макс. расстояние натяжения

    [Header("Visuals")]
    [SerializeField] private GameObject _aimIndicator;       // Индикатор прицеливания


    // Внутреннее состояние
    private Vector2 _launchPosition;              // Позиция точки запуска (кэш)
    private CircleCollider2D _slingshotCollider;  // Коллайдер рогатки для определения попадания курсора
    private GameObject _currentProjectile;        // Текущий активный снаряд
    private Rigidbody2D _projectileRb;            // Физика текущего снаряда
    private bool _isCameraViewMode;               // Флаг: находится ли камера в режиме обзора
    private bool _isCursorOverSlingshot;          // Флаг: находится ли курсор на рогатке

    private enum SlingshotState { Idle, Aiming, Flying }       // Состояния рогатки
    private SlingshotState _currentState = SlingshotState.Idle;


    // Публичные API
    public static event Action<Rigidbody2D> OnProjectileLaunched; // Событие запуска снаряда
    public bool IsIdled => _currentState == SlingshotState.Idle; // Состояние простоя для внешнего чтения



    private void Awake()
    {
        // 1. Инициализация синглтона
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // 2. Валидация настроек
        ValidateSetup();

        // 3. Кэширование данных
        _launchPosition = _launchPoint.position;
        if(_mainCamera == null) _mainCamera = Camera.main;
        _slingshotCollider = GetComponent<CircleCollider2D>();

        // 4. Инициализация состояния
        _currentState = SlingshotState.Idle;
        _isCameraViewMode = false;
        if (_aimIndicator != null) _aimIndicator.SetActive(false);
    }

    private void ValidateSetup()
    {
        if (_launchPoint == null) Debug.LogError("Slingshot: Launch Point not assigned.");
        if (_prefabProjectile == null) Debug.LogError("Slingshot: Projectile Prefab not assigned.");
        if (_gameInput == null) Debug.LogError("Slingshot: GameInput not assigned.");
    }


    private void OnEnable()
    {
        if (_gameInput != null)
        {
            _gameInput.OnFireStarted += HandleFireStarted;
            _gameInput.OnFireCanceled += HandleFireCanceled;
            _gameInput.OnAimPositionChanged += HandleAimPositionChanged;
        }

        if (FollowCam.Instance != null)
        {
            FollowCam.Instance.OnViewModeChanged += SetCameraViewMode;
        }
    }

    private void OnDisable()
    {
        if (_gameInput != null)
        {
            _gameInput.OnFireStarted -= HandleFireStarted;
            _gameInput.OnFireCanceled -= HandleFireCanceled;
            _gameInput.OnAimPositionChanged -= HandleAimPositionChanged;
        }

        if (FollowCam.Instance != null)
        {
            FollowCam.Instance.OnViewModeChanged -= SetCameraViewMode;
        }

        ProjectaileBehaviour.OnProjectailDestroyed -= OnProjectileWasDestroyed;
    }

    private void HandleFireStarted()
    {
        // 1. Если камера в режиме обзора - игнорируем ввод
        if (_isCameraViewMode) return;

        // 2. Если снаряд уже летит - игнорируем ввод
        if (_currentState == SlingshotState.Flying) return;

        // 3. Если курсор не над зоной рогатки - игнорируем ввод
        if (!_isCursorOverSlingshot) return;

        
                
        // 4. Начинаем прицеливание
        StartAiming();
    }


    private void HandleFireCanceled()
    {
        // 1. Проверка - работаем только в режиме прицеливания
        if (_currentState != SlingshotState.Aiming) return;

        // 2. Запускаем снаряд
        LaunchProjectile();
    }


    private void HandleAimPositionChanged (Vector2 cursorPositionOnScreen)
    {
        // 1. Находим мировые координаты курсора
        Vector2 cursorPositionOnWorld = GameUtilitys.GetWorldPositionFromScreen(cursorPositionOnScreen, _mainCamera);

        // 2. Проверяем, находится ли курсор над рогаткой (State Change Detection)
        // 2.1 Сохраняем ТЕКУЩЕЕ (старое) состояние в переменную
        bool wasOverSlingshot = _isCursorOverSlingshot;
        // 2.2 Вычисляем НОВОЕ состояние на основе текущей позиции курсора
        _isCursorOverSlingshot = IsPositionOverSlingshot(cursorPositionOnWorld); 

        // 3. Если состояние изменилось - обновляем визуальный индикатор
        if (_isCursorOverSlingshot != wasOverSlingshot) 
        {
            UpdateAimIndicator(_isCursorOverSlingshot);
        }

        // 4. Если в режиме прицеливания - обновляем позицию снаряда
        if (_currentState == SlingshotState.Aiming)
        {
            UpdateAim(cursorPositionOnWorld);
        }

    }


    private void StartAiming()
    {
        // 1. Переключаем состояние в прицеливание
        _currentState = SlingshotState.Aiming;

        // 2. Активируем индикатор прицеливания
        if (_aimIndicator != null) _aimIndicator.SetActive(true);
        _launchPoint.gameObject.SetActive(true);

        // 3. Создаем новый снаряд
        SpawnProjectile();
    }


    private void UpdateAim(Vector2 cursorPositionOnWorld)
    {
        // 1. Если снаряда нет - нечего обновлять - выходим
        if (_currentProjectile == null) return;

        // 2. Если не в режиме прицеливания - выходим
        if (_currentState != SlingshotState.Aiming) return;
        
        // 3. Рассчитываем вектор натяжения (куда растянута резинка)
        Vector2 pullVector = _launchPosition - cursorPositionOnWorld;

        // 4. Ограничиваем максимальное натяжение
        pullVector = Vector2.ClampMagnitude(pullVector, _maxPullDistance);

        // 5. Вычисляем финальную позицию снаряда (противоположная сторона от вектора натяжения)
        Vector2 newProjectilePos = _launchPosition - pullVector;

        // 6. Применяем позицию - обновляем трансформ снаряда
        _currentProjectile.transform.position = newProjectilePos;        
    }


    private void CancelAiming()
    {
        // 1. Переводим в режим ожидания        
        _currentState = SlingshotState.Idle;

        // 2. Отключаем индикатор прицеливания
        if (_aimIndicator != null) _aimIndicator.SetActive(false);

        // 3. Деактивируем точку запуска
        _launchPoint.gameObject.SetActive(false);

        // 4. Уничтожаем текущий снаряд и очищаем ссылки
        if (_currentProjectile != null)
        {
            Destroy(_currentProjectile);
            _currentProjectile = null;
            _projectileRb = null;
        }
    }

    private void UpdateAimIndicator(bool show)
    {
        // 1. Включаем или отключаем индикатор прицеливания
        if (_aimIndicator != null) _aimIndicator.SetActive(show);
    }



    private void SpawnProjectile()
    {
        // 1. Проверяем наличие префаба снаряда
        if (_prefabProjectile == null) return;

        // 2. Создаем новый снаряд в точке запуска
        _currentProjectile = Instantiate(_prefabProjectile, _launchPosition, Quaternion.identity);

        // 3. Получаем компонент Rigidbody2D снаряда
        _projectileRb = _currentProjectile.GetComponent<Rigidbody2D>();

        // 4. Проверяем наличие Rigidbody2D
        if (_projectileRb == null)
        {
            Debug.LogError("Slingshot: Projectile prefab must have Rigidbody2D!");
            CancelAiming();
            return;
        }

        // 5. Делаем снаряд кинематическим на время прицеливания
        _projectileRb.bodyType = RigidbodyType2D.Kinematic;
    }



    private void LaunchProjectile()
    {
        // 1. Переводим в режим полета
        _currentState = SlingshotState.Flying;

        // 2. Отключаем индикатор прицеливания
        if (_aimIndicator != null) _aimIndicator.SetActive(false);

        // 3. Деактивируем точку запуска
        _launchPoint.gameObject.SetActive(false);

        // 4. Проверка: если нет Rb или снаряда, то отменяем прицеливание
        if (_projectileRb == null || _currentProjectile == null)
        {
            CancelAiming();
            return;
        }

        // 5. Вычисляем направление и силу выстрела (Вектор от текущей позиции снаряда к точке запуска)
        Vector2 launchDirection = _launchPosition - (Vector2)_currentProjectile.transform.position;

        // 6. Переключаем Rb в динамический режим
        _projectileRb.bodyType = RigidbodyType2D.Dynamic;

        // 7. Применяем силу к снаряду
        _projectileRb.linearVelocity = launchDirection * _velocityMultiplier;

        // 8. Уведомляем подписчиков о запуске снаряда
        OnProjectileLaunched?.Invoke(_projectileRb);        

        // 9. Подписываемся на событие уничтожения снаряда
        ProjectaileBehaviour.OnProjectailDestroyed += OnProjectileWasDestroyed;

        // 10. Очищаем ссылки
        _currentProjectile = null;
        _projectileRb = null;        
    }


    // === Метод перевода рогатки в состояние ожидания
    private void ResetSlingshot()
    {
        // 1. Переводим в режим ожидания
        _currentState = SlingshotState.Idle;
        
        // 2. Сбрасываем флаг курсора
        _isCursorOverSlingshot = false;

        // 3. Скрываем индикатор прицела
        UpdateAimIndicator(false);

        // 4. Отписка, если снаряд был уничтожен минуя событие
        ProjectaileBehaviour.OnProjectailDestroyed -= OnProjectileWasDestroyed;

        // 5. Если снаряд еще сцществует - уничтожаем
        if (_currentProjectile != null)
        {
            Destroy(_currentProjectile);
            _currentProjectile = null;
            _projectileRb = null;
        }
    }


    private void SetCameraViewMode(bool isViewMode)
    {
        // 1. Устанавливаем флаг режима обзора камеры
        _isCameraViewMode = isViewMode;
    }

    
    private bool IsPositionOverSlingshot(Vector2 cursorPositionOnWorld)
    {
        // 1. Проверяем наличие коллайдера рогатки, если его нет - разрешаем прицеливание в любом месте
        if (_slingshotCollider == null) return true;

        // 2. Возвращаем результат проверки, находится ли мировая позиция внутри коллайдера рогатки
        return _slingshotCollider.OverlapPoint(cursorPositionOnWorld);
    }


    
    // === Обработчик события уничтожения снаряда
    private void OnProjectileWasDestroyed()
    {
        // 1. Отписываемся от события
        ProjectaileBehaviour.OnProjectailDestroyed -= OnProjectileWasDestroyed;

        // 2. Сбрасываем рогатку в состояние ожидания
        ResetSlingshot();
    }


}
