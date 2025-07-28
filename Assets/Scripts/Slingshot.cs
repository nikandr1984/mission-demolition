using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Класс управления рогаткой с системой прицеливания и запуска снарядов
public class Slingshot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Настройки рогатки (задаются в инспекторе)
    [Header("Set in Inspector")]
    public GameObject prefabProjectile;  // Префаб снаряда при запуске
    public float velocityMult = 8f;      // Множитель скорости снаряда

    // Динамические параметры (устанавливаются во время игры)
    [Header("Set Dynamically")]
    public GameObject launchPoint;   // Объект-метка, откуда вылетает снаряд
    public Vector3 launchPos;        // Координаты метки в мире
    public GameObject projectile;    // Созданный снаряд
    public bool aimingMode = false;  // Режима прицеливания (выкл/вкл)

    private Rigidbody projectileRigidbody;  // Физика снаряда
    private Mouse currentMouse;             // Управление мышью
    private Camera mainCam;                 // Главная камера (для кеша)

   
    // Начальная настройка
    void Awake()
    {
        // Находим и настраиваем точку запуска
        Transform launchPointTrans = transform.Find("LaunchPoint"); 
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);                     // Скрываем метку
        launchPos = launchPointTrans.position;            // Запоминаем позицию
                                                
        currentMouse = Mouse.current; // Подключаем мышь
        mainCam = Camera.main;        // Кешируем камеру
    }
    
    
    // Ищем мировые координаты мыши
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos2D = currentMouse.position.ReadValue();
        mousePos2D.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(mousePos2D);
    }

    
    // При наведении курсора на рогатку
    public void OnPointerEnter(PointerEventData eventData)
    {        
        launchPoint.SetActive(true);  // Показываем точку запуска   
    }

    // При выходе курсора за пределы рогатки
    public void OnPointerExit(PointerEventData eventData)
    {        
        launchPoint.SetActive(false);  // Скрываем точку запуска
    }

    // При нажатии левой кнопки мыши на рогатке
    public void OnPointerDown(PointerEventData eventData)
    {
        // Проверяем нажата ли левая кнопку мыши
        if (eventData.button == PointerEventData.InputButton.Left)
        {            
            aimingMode = true;  // Активируем режим прицеливания

            projectile = Instantiate(prefabProjectile); // Создаем новый снаряд
            projectile.transform.position = launchPos;  // Ставим его на метку

            // Выключаем физику снаряда (чтобы не падал)
            projectileRigidbody = projectile.GetComponent<Rigidbody>();
            projectileRigidbody.isKinematic = true; 
        }         
    }

    
    // При отпускании левой кнопки мыши
    public void OnPointerUp(PointerEventData eventData)
    {
        // Если были в режиме прицеливания
        if (aimingMode)
        {
            LaunchProjectile(); // Запускаем снаряд
        }
            
    }

    // Основной игровой цикл
    public void Update()
    {
        // Если не в режиме прицеливания - выходим
        if (!aimingMode) return;
                       
        // Получаем текущую позицию мыши в мировых координатах
        Vector3 mousePos3D = GetMouseWorldPos();

        // Считаем, насколько далеко оттянули снаряд
        Vector3 mouseDelta = mousePos3D - launchPos;
        
        // Ограничиваем максимальное расстояние оттягивания радиусом коллайдера
        float maxMagnitude = GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // Перемещаем снаряд согласно позиции мыши
        projectile.transform.position = launchPos + mouseDelta;
        
    }

    // Метод запуска снаряда
    private void LaunchProjectile()
    {
        // Выходим из режима прицеливания
        aimingMode = false;

        // Получаем финальную позицию мыши в мировых координатах        
        Vector3 mousePos3D = GetMouseWorldPos();

        // Рассчитываем вектор силы (от мыши к точке запуска)
        Vector3 launchVector = launchPos - mousePos3D;

        // Применяем физику снаряда
        projectileRigidbody.isKinematic = false;
        projectileRigidbody.linearVelocity = launchVector * velocityMult;

        // Камера следит за снарядом
        FollowCam.POI = projectile;
        
        // Сбрасываем ссылку на снаряд (он теперь управляется физикой)
        projectile = null;            
    }
}
