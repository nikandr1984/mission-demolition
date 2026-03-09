using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Синглтон для доступа из других классов


    // Поля для настройки в инспекторе
    [SerializeField] private LevelData _currentLevelData; // Текущие данные уровня


    // Внутреннее состояние
    private bool _isGameActive = false;    // Флаг активности игры
    private int _targetsCount = 0;         // Количество целей на уровне
    private int _targetsRemaining = 0;     // Количество оставшихся целей
    private int _projectailLaunched = 0;   // Количество выпущенных снарядов
        



    private void OnEnable()
    {
        Destructible.OnTargetDestroyed += HandleTargetDestroyed;
        Slingshot.OnProjectileLaunched += HandleProjectileLaunched;
    }

    private void OnDisable()
    {
        Destructible.OnTargetDestroyed -= HandleTargetDestroyed;
        Slingshot.OnProjectileLaunched -= HandleProjectileLaunched;
    }



    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
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
        // 1. Активируем игру
        _isGameActive = true;

        // 2. Устанавливаем текущие данные уровня
        _currentLevelData = levelData;

        // 3. Удаляем остатки старого замка и спавним новый замок, если это не первый уровень
        if (_currentLevelData.levelNumber > 1)
        {
            ClearCastle();
            Instantiate(_currentLevelData.castlePrefab, _currentLevelData.castleSpawnPosition, Quaternion.identity);
        }        

        // 4. Считаем количество целей на уровне
        CountTarget();

    }


    // Метод очистки текущего уровня от объектов замка и целей
    private void ClearCastle()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

        foreach (var target in targets)
        {
            Destroy(target);
        }

        foreach (var block  in blocks)
        {
            Destroy(block);
        }
    }


    private void CountTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        _targetsCount = targets.Length;    // Сохраняем общее количество целей для текущего уровня
        _targetsRemaining = _targetsCount; // Инициализируем счетчик оставшихся целей        
    }


    private void HandleTargetDestroyed(Rigidbody2D targetRb)
    {
        //if (!_isGameActive) return; // Если игра не активна, не обрабатываем событие

        _targetsRemaining--; // Уменьшаем счетчик оставшихся целей
        Debug.Log("GameManager: Target destroyed, remaining = " + _targetsRemaining);


        // Проверяем, остались ли еще цели
        if (_targetsRemaining <= 0)
        {
            HandleVictory();
        }
    }


    private void HandleProjectileLaunched(Rigidbody2D projectileRb)
    {
        _projectailLaunched++; // Увеличиваем счетчик выпущенных снарядов
        Debug.Log("GameManager: Projectile launched = " + _projectailLaunched);

        if (_projectailLaunched == _currentLevelData.projectileCount)
        {
            _isGameActive = false; // Останавливаем игру, если достигнут лимит снарядов
        }
    }


    private void HandleVictory()
    {
        _isGameActive = false; // Останавливаем игру
        Debug.Log("GameManager: Victory! All targets destroyed.");
    }


    private void HandleDefeat()
    {

    }


    private void LoadNextLevel()
    {

    }


    private void RestartLevel()
    {

    }



}
