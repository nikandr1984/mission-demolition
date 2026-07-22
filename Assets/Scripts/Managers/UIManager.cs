using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    // Поля для настройки в инспекторе     
    [SerializeField] private Transform _projectaleIconsContainer;    // Контейнер для иконок снарядов
    [SerializeField] private Image _projectileIconPrefab;            // Префаб иконки снаряда
    [SerializeField] private string _mainMenuSceneName = "MainMenu"; // Название сцены главного меню
    [SerializeField] private GameObject _victoryPanel;               // Панель победы
    [SerializeField] private GameObject _defeatPanel;                // Панель поражения
    [SerializeField] private GameObject _plugPanel;                  // Панель-заглушка
    [SerializeField] private GameObject _exitMenuPanel;              // Панель выхода в главное меню
    [SerializeField] private GameObject _exitGamePanel;              // Панель выхода из игры

    // Внутреннее состояние
    private int _projectilesRemaining = 0;                       // Количество оставшихся снарядов
    private GameObject _panelThatOpenedExitDialog;               // Сылка на панель, которая активировала диалог
    private readonly List<Image> _activeProjectileIcons = new(); // Список активных иконок снарядов

                                                                  

    // События
    public static event Action OnViewButtonClicked;    // Событие, когда игрок нажимает на кнопку Обзор
    public static event Action OnRestartButtonClicked; // Событие, когда игрок нажимает на кнопку Рестарт
    public static event Action OnAnyButtonClicked;     // Событие, когда игрок нажимает на любую кнопку



    private void OnEnable()
    {
        Slingshot_New.OnProjectileLaunched += HandleProjectileLaunched;
        GameManager.OnVictory += HandleVictory;
        GameManager.OnDefeat += HandleDefeat;
        GameManager.OnStartLevel += HandleStartLevel;
        GameManager.OnLevelsOver += HandleLevelsOver;
        
    }

    private void OnDisable()
    {
        Slingshot_New.OnProjectileLaunched -= HandleProjectileLaunched;
        GameManager.OnVictory -= HandleVictory;
        GameManager.OnDefeat -= HandleDefeat;
        GameManager.OnStartLevel -= HandleStartLevel;
        GameManager.OnLevelsOver -= HandleLevelsOver;
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
        if (_victoryPanel != null && !_defeatPanel.activeSelf)
        {
            _victoryPanel.SetActive(true);
        }
    }


    private void HandleDefeat()
    {
        // 1. Показываем панель проигрыша
        if (_defeatPanel != null && !_victoryPanel.activeSelf)
        {
            _defeatPanel.SetActive(true);
        }
    }

    private void HandleStartLevel(LevelData levelData)
    {
        // 1. Инициализируем UI
        InitializeUI(levelData);
        
        // 2. Деактивируем панели, если они открыты
        if (_victoryPanel != null) _victoryPanel.SetActive(false);
        if (_defeatPanel != null) _defeatPanel.SetActive(false);
    }

    private void HandleLevelsOver()
    {
        _plugPanel.SetActive(true);
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



    // === Методы для UI кнопок ===

    public void ViewButtonClicked()
    {
        if(Slingshot_New.Instance.IsIdled)
        {
            OnViewButtonClicked?.Invoke();
            OnAnyButtonClicked?.Invoke();
            Debug.Log("UIManager: Игрок кликнул на кнопку Обзор (рогатка в режиме ожидания)");
        }        
    }


    public void RestartButtonClicked()
    {
        OnRestartButtonClicked?.Invoke();
        OnAnyButtonClicked?.Invoke();
        Debug.Log("UIManager: Игрок кликнул на кнопку Рестарт");
    }

    public void HomeButtonClicked()
    {
        // 1. Проверка ссылок на null
        if (_exitMenuPanel == null) return;

        // 2. Активируем панель подтверждения выхода в главное меню
        _exitMenuPanel.SetActive(true);

        // 3. Оповещаем подписчиков о нажатии кнопки
        OnAnyButtonClicked?.Invoke();
    }


    public void ConfirmHomeButtonClicked()
    {
        // 1. Загружаем сцену главного меню
        SceneManager.LoadScene(_mainMenuSceneName);

        // 2. Оповещаем подписчиков о нажатии кнопки
        OnAnyButtonClicked?.Invoke();
    }

    public void CancelHomeButtonClicked()
    {
        // 1. Проверка ссылок на null
        if (_exitMenuPanel == null) return;

        // 2. Деактивируем панель подтверждения выхода в главное меню
        _exitMenuPanel.SetActive(false);

        // 3. Оповещаем подписчиков о нажатии кнопки
        OnAnyButtonClicked?.Invoke();
    }



    public void ExitButtonClicked()
    {
        // 1. Проверка ссылок на null
        if (_exitGamePanel == null) return;             
                
        // 2. Запоминаем какая панель была активна до нажатия на кнопку
        if (_victoryPanel != null && _victoryPanel.activeSelf)
        {
            _panelThatOpenedExitDialog = _victoryPanel;
        }
        else if (_defeatPanel != null && _defeatPanel.activeSelf)
        {
            _panelThatOpenedExitDialog = _defeatPanel;
        }
        else
        {
            _panelThatOpenedExitDialog = null;
        }

        // 3. Деактивируем панель победы / проигрыша
        _victoryPanel.SetActive(false);
        _defeatPanel.SetActive(false);

        // 4. Активируем панель подтверждения выхода из игры
        _exitGamePanel.SetActive(true);            

        // 5. Оповещаем подписчиков о нажатии кнопки
        OnAnyButtonClicked?.Invoke();
    }



    public void ConfirmExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Остановка Play Mode
#else
        Application.Quit();                              // Закрытие билда
#endif
        Debug.Log("Application is closed");
    }



    // МЕТОД отмены действия выхода из игры 
    public void CancelExitButtonClicked()
    {
        // 1. Проверка на null
        if (_exitGamePanel == null) return;
        
        // 2. Закрываем панель выхода и открываем предыдущую, если она была
        if (_panelThatOpenedExitDialog != null)
        {
            // Скрываем панель подтверждения выхода
            _exitGamePanel.SetActive(false);

            // Восстанавливаем панель, которая открыла диалог
            _panelThatOpenedExitDialog.SetActive(true);

            // Очищаем ссылку
            _panelThatOpenedExitDialog = null;
        }
        else
        {
            _exitGamePanel.SetActive(false);
        }

        // 3. Оповещаем подписчиков о нажатии кнопки
        OnAnyButtonClicked?.Invoke();
    }
          

}
