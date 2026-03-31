using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class UIManager : MonoBehaviour
{
    // Поля для настройки в инспекторе     
    [SerializeField] private Transform _projectaleIconsContainer;    // Контейнер для иконок снарядов
    [SerializeField] private Image _projectileIconPrefab;            // Префаб иконки снаряда
    [SerializeField] private GameObject _victoryPanel;               // Панель победы
    [SerializeField] private GameObject _defeatPanel;                // Панель поражения


    // Внутреннее состояние
    private int _projectilesRemaining = 0;                       // Количество оставшихся снарядов
    private readonly List<Image> _activeProjectileIcons = new(); // Список активных иконок снарядов
                                                                  

    // События
    public static event Action OnViewButtonClicked; // Событие, когда игрок нажимает на кнопку Обзор



    private void OnEnable()
    {
        Slingshot_New.OnProjectileLaunched += HandleProjectileLaunched;
        GameManager.OnVictory += HandleVictory;
        GameManager.OnDefeat += HandleDefeat;
        GameManager.OnStartLevel += HandleStartLevel;
        
    }

    private void OnDisable()
    {
        Slingshot_New.OnProjectileLaunched -= HandleProjectileLaunched;
        GameManager.OnVictory -= HandleVictory;
        GameManager.OnDefeat -= HandleDefeat;
        GameManager.OnStartLevel -= HandleStartLevel;
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

    private void HandleVictory()
    {
        // 1. Показываем панель победы
        if (_victoryPanel != null)
        {
            _victoryPanel.SetActive(true);
        }
    }


    private void HandleDefeat()
    {
        // 1. Показываем панель проигрыша
        if (_defeatPanel != null) _defeatPanel.SetActive(true);        
    }

    private void HandleStartLevel(LevelData levelData)
    {
        // 1. Инициализируем UI
        InitializeUI(levelData);
        
        // 2. Деактивируем панели, если они открыты
        if (_victoryPanel != null) _victoryPanel.SetActive(false);
        if (_defeatPanel != null) _defeatPanel.SetActive(false);
    }

   


    private void ClearAllIcons()
    {
        for (int i = _activeProjectileIcons.Count - 1; i >=0; i--)
        {
            Image icon = _activeProjectileIcons[i];

            if (icon != null) Destroy(icon.gameObject);

            _activeProjectileIcons.RemoveAt(i);
        }
    }
         
    

    public void ViewButtonClicked()
    {
        if(Slingshot_New.Instance.IsIdled)
        {
            OnViewButtonClicked?.Invoke();
            Debug.Log("UIManager: Игрок кликнул на кнопку Обзор (рогатка в режиме ожидания)");
        }        
    }
}
