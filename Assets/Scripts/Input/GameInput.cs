using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    // Кэшируемые компоненты
    private GameInputActions _inputActions;  // Ссылка на сгенерированный класс действий ввода
    private Camera _mainCamera;              // Ссылка на основную камеру


    // Интерфейсы
    public event System.Action OnFireStarted;                 // Когда игрок нажимает кнопку (подготовка к выстрелу)
    public event System.Action OnFireCanceled;                // Когда игрок отпускает кнопку (выстрел)
    public event System.Action<Vector2> OnAimPositionChanged; // Когда игрок изменяет позицию прицеливания



    private void Awake()
    {
        // 1. Создаем экземпляр сгенерированного класса действий
        _inputActions = new GameInputActions(); 

        // 2. Получаем ссылку на главную камеру
        _mainCamera = Camera.main;

        // 3. Объект неразрушается при загрузке новых сцен
        DontDestroyOnLoad(gameObject);
    }



    private void OnEnable()
    {
        // 1. Включаем действия ввода
        _inputActions.Enable();

        // 2. Подписываемся на события ввода
        _inputActions.Gameplay.Fire.started += OnFireStartedInternal;
        _inputActions.Gameplay.Fire.canceled += OnFireCenceledInternal;
        _inputActions.Gameplay.AimPosition.performed += OnAimPositionInternal;
    }


    private void OnDisable()
    {
        // 1. Отключаем действия ввода
        _inputActions.Disable();
        
        // 2. Отписываемся от событий ввода
        _inputActions.Gameplay.Fire.started -= OnFireStartedInternal;
        _inputActions.Gameplay.Fire.canceled -= OnFireCenceledInternal;
        _inputActions.Gameplay.AimPosition.performed -= OnAimPositionInternal;
    }



    private void OnFireStartedInternal(InputAction.CallbackContext context)
    {
        OnFireStarted?.Invoke(); 
    }

   
    private void OnFireCenceledInternal(InputAction.CallbackContext context)
    {
        OnFireCanceled?.Invoke();
    }

    
    private void OnAimPositionInternal(InputAction.CallbackContext context)
    {
        // 1. Получаем позицию курсора в экранных координатах
        Vector2 screenPosition = context.ReadValue<Vector2>();

        // 2. Оповещаем подписчиков о изменении позиции прицеливания
        OnAimPositionChanged?.Invoke(screenPosition);
    }



    // Метод для получения позиции прицеливания в мировых координатах
    public Vector2 GetWorldPosition()
    {
        // 1. Получаем позицию курсора в экранных координатах
        Vector2 screenPos = _inputActions.Gameplay.AimPosition.ReadValue<Vector2>();

        // 2. Если камеры есть, то конвертируем экранные координаты в мировые
        if (_mainCamera != null)
        {
            return _mainCamera.ScreenToWorldPoint(screenPos);
        }

        // 3. Возвращаем мировые координаты
        return screenPos;
    }

    
    // Метод для проверки нажата ли кнопка огня прямо сейчас 
    public bool IsFirePressed()
    {
        return _inputActions.Gameplay.Fire.ReadValue<float>() > 0.5f; 
    }


}
