using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{
    // Поля для настройки в инспекторе     
    [SerializeField] private LevelData _currentLevelData;            // Текущие данные уровня
    [SerializeField] private Transform _projectaleIconsContainer;    // Контейнер для иконок снарядов
    [SerializeField] private Image _projectileIconPrefab;            // Префаб иконки снаряда


    // Внутреннее состояние
    private int _projectilesRemaining = 0;                       // Количество оставшихся снарядов
    private readonly List<Image> _activeProjectileIcons = new(); // Список активных иконок снарядов    



    private void OnEnable()
    {
        Slingshot.OnProjectileLaunched += HandleProjectileLaunched;             
    }

    private void OnDisable()
    {
        Slingshot.OnProjectileLaunched -= HandleProjectileLaunched;                
    }


    private void Start()
    {
        InitializeUI(_currentLevelData); // Инициализируем UI для текущего уровня
    }



    // Инициализация UI элементов для нового уровня
    private void InitializeUI(LevelData levelData)
    {
        // 1. Проверка наличия данных уровня
        if (levelData == null)
        {
            Debug.LogError("UIManager: LevelData is not assigned.");
            return;
        }

        // 2. Очищаем старые иконки снарядов
        ClearAllIcons();

        // 3. Берем количество снарядов из данных уровня
        _projectilesRemaining = levelData.projectileCount;

        // 4. Создаем иконки снарядов
        SpawnProjectileIcons(_projectilesRemaining);
    }


    private void  SpawnProjectileIcons (int count)
    {
        // 1. Проверка
        if (_projectileIconPrefab == null || _projectaleIconsContainer == null)
        {
            Debug.LogError("UIManager: Projectile icon prefab or container is not assigned.");
            return;
        }

        // 2. Создание иконок снарядов
        for (int i = 0; i < count; i++)
        {
            Image newIcon = Instantiate(_projectileIconPrefab, _projectaleIconsContainer);
            newIcon.gameObject.SetActive(true);
            _activeProjectileIcons.Add(newIcon);
        }
    }



    // Обработчик запуска снарядов
    private void HandleProjectileLaunched(Rigidbody2D projectileRb)
    {
        // 1. Проверка, что есть активные иконки снарядов для удаления
        if (_activeProjectileIcons.Count == 0) return;

        // 2. Уменьшаем счетчик оставшихся снарядов
        _projectilesRemaining--;        

        // 3. Получаем последнюю иконку из списка активных иконок
        Image iconToRemove = _activeProjectileIcons[_activeProjectileIcons.Count - 1];

        // 4. Удаляем эту иконку из списка
        _activeProjectileIcons.Remove(iconToRemove);

        // 5. Удаляем иконку из UI
        Destroy(iconToRemove.gameObject);       

    }


    private void ClearAllIcons()
    {
        foreach (var icon in _activeProjectileIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
            _activeProjectileIcons.Clear();

        }
    }


    private void InitializeUINextLevel(LevelData levelData)
    {
        
    }

}
