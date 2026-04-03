using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Синглтон для доступа из других классов


    // Поля для настройки в инспекторе
    [SerializeField] private LevelData _currentLevelData; // Текущие данные уровня


    // Внутреннее состояние
    private bool _isGameActive = false;    // Флаг активности игры
    private bool _isLevelEnded = false;    // Флаг окончания уровня
    private int _targetsCount = 0;         // Количество целей на уровне
    private int _targetsRemaining = 0;     // Количество оставшихся целей
    private int _projectailLaunched = 0;   // Количество выпущенных снарядов
    private GameObject _currentCastle;     // Ссылка на текущий замок (для очистки при перезапуске уровня)


    // События
    public static event Action OnVictory;                // Уведомление о победе
    public static event Action OnDefeat;                 // Уведомление о поражении
    public static event Action<LevelData> OnStartLevel;  // Уведомление о старте уровня
    public static event Action OnLevelsOver;

    // Свойства
    public bool IsGameActive => _isGameActive; // Свойство для проверки активности игры




    private void OnEnable()
    {
        Destructible.OnTargetDestroyed += HandleTargetDestroyed;
        Slingshot_New.OnProjectileLaunched += HandleProjectileLaunched;
        ProjectaileBehaviour.OnProjectailDestroyed += HandleProjectileDestroyed;
    }

    private void OnDisable()
    {
        Destructible.OnTargetDestroyed -= HandleTargetDestroyed;
        Slingshot_New.OnProjectileLaunched -= HandleProjectileLaunched;
        ProjectaileBehaviour.OnProjectailDestroyed -= HandleProjectileDestroyed;
    }



    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        StartLevel(_currentLevelData); // Запускаем уровень        
    }


    private void StartLevel(LevelData levelData)
    {
        // 1. Сброс игрового состояния
        _isGameActive = true;
        _isLevelEnded = false;
        _projectailLaunched = 0;      

        // 2. Уведомляем о загрузке уровня
        OnStartLevel?.Invoke(levelData);
        
        // 3. Устанавливаем текущие данные уровня
        _currentLevelData = levelData;

        // 4. Удаляем остатки старого замка и спавним новый замок, если это не первый уровень
        ClearCastle();

        // 5. Cпавним новый замок
        _currentCastle = Instantiate(_currentLevelData.castlePrefab,
                                     _currentLevelData.castleSpawnPosition, 
                                     Quaternion.identity);                    

        // 6. Считаем количество целей на уровне
        CountTarget();        
    }


    // Метод очистки текущего уровня от объектов замка и целей
    private void ClearCastle()
    {
        if (_currentCastle != null)
        {
            Destroy(_currentCastle);
            _currentCastle = null;
        }
    }


    private void CountTarget()
    {
        _targetsCount = _currentCastle.GetComponentsInChildren<Transform>()
                                  .Count(t => t.CompareTag("Target"));

        _targetsRemaining = _targetsCount;

        Debug.Log($"GameManager: Counted {_targetsCount} targets in the current castle instance");
    }


    private void HandleTargetDestroyed(Rigidbody2D targetRb)
    {
        // 1. Если уровень уже завершен - выходим
        if (_isLevelEnded) return;
        
        // 2. Уменьшаем счетчик оставшихся целей
        _targetsRemaining--;  
        
        // 3. Если целей больше нет, то запускаем победу
        if (_targetsRemaining <= 0)
        {
            HandleVictory();
        }
    }


    private void HandleProjectileLaunched(Rigidbody2D projectileRb)
    {
        // 1. Если уровень уже завершен - выходим
        if (_isLevelEnded) return;

        // 2. Увеличиваем счетчик выпущенных снарядов 
        _projectailLaunched++;  
        Debug.Log("GameManager: Projectile launched = " + _projectailLaunched);

        // 3. Останавливаем игру, если достигнут лимит снарядов 
        if (_projectailLaunched == _currentLevelData.projectileCount)
        {
            _isGameActive = false;            
        }
        
    }

    private void HandleProjectileDestroyed()
    {
        // 1. Если уровень уже завершен - выходим
        if (_isLevelEnded) return;

        // 2. Если снарядов больше нет, а цели ещё остались, то засчитываем поражение
        if (_projectailLaunched == _currentLevelData.projectileCount && _targetsRemaining > 0)
        {
            HandleDefeat();
        }   
    }


    private void HandleVictory()
    {
        // 1. Если уровень уже завершен - выходим
        if (_isLevelEnded) return;
        
        // 2. Останавливаем игру
        _isLevelEnded = true;
        _isGameActive = false; 
        Debug.Log("GameManager: Victory! All targets destroyed.");

        // 3. Уведомляем о победе
        OnVictory?.Invoke();        
        
    }


    private void HandleDefeat()
    {
        // 1. Если уровень уже завершен - выходим
        if (_isLevelEnded) return;

        // 2. Останавливаем игру 
        _isLevelEnded = true;
        _isGameActive = false;
        Debug.Log("GameNamager: level is failed");

        // 3. Уведомляем о поражении
        OnDefeat?.Invoke();
    }


    public void LoadNextLevel()
    {
        if(_currentLevelData.nextLevelData != null)
        {
            StartLevel(_currentLevelData.nextLevelData);
        }
        else
        {
            OnLevelsOver?.Invoke();
        }
    }


    public void RestartLevel()
    {
        StartLevel(_currentLevelData);
    }
}
