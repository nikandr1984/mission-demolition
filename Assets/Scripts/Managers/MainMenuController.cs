using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _creditsPanel;      // Панель "Создатели"
    [SerializeField] private GameObject _settingsPanel;     // Панель "Настройки"

    [Header("Buttons")]
    [SerializeField] private Button _playButton;           // Кнопка "Играть"
    [SerializeField] private Button _settingsButton;       // Кнопка "Настройки"
    [SerializeField] private Button _closeSettingsButton;  // Кнопка закрытия панели настроек
    [SerializeField] private Button _creditsButton;        // Кнопка "Создатели"
    [SerializeField] private Button _closeCreditsButton;   // Кнопка закрытия панели создателей    
    [SerializeField] private Button _exitButton;           // Кнопка 

    [Header("Settings")]
    [SerializeField] private string _gameplaySceneName = "Gameplay";     // Название сцены с игровым процессом
    [SerializeField] private MainMenuAudioController _audioController;   // Ссылка на компонент аудио 

    private bool _isInitialized = false; // Предотвращает повторную подписку при множественных вызовах


    private void Awake()
    {
        SetupButtonListeners();
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }


    private void OnEnable()
    {
        // 1. Подписываемся на событие окончания звучания стартовой фразы
        if (_audioController != null)
        {
            _audioController.OnVoiceFinished += LoadGameplayScene; 
        }
    }


    private void OnDisable()
    {
        // 1. Отписываемся от события окончания звучания стартовой фразы
        if (_audioController != null)
        {
            _audioController.OnVoiceFinished -= LoadGameplayScene;
        }
    }

    

    // -- МЕТОД вспомогательный: безопасная подписка на кнопку
    private void SubscribeButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null && action != null)
        {
            button.onClick.AddListener(action);
        }
        else
        {
            Debug.LogWarning($"MainMenu: Button or action is null not assignment");
        }
    }

    private void UnsubscribeButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null && action != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
     

    private void SetupButtonListeners()
    {
        // 1. Защита от повторной инициализации
        if (_isInitialized) return;

        // 2. Подписываемся на события кнопок
        SubscribeButton(_playButton, LoadGameplayScene);
        SubscribeButton(_creditsButton, OpenCreditsPanel);
        SubscribeButton(_closeCreditsButton, CloseCreditsPanel);
        SubscribeButton(_settingsButton, OpenSettingsPanel);
        SubscribeButton(_closeSettingsButton, CloseSettingsPanel);
        SubscribeButton(_exitButton, ExitApplication);

        // 3. Переключаем флаг инициализации
        _isInitialized = true;

        // 4. Добавляем лог
        Debug.Log("MainMenuController: Button listeners subscribed");
    }

    private void RemoveButtonListeners()
    {
        // 1. Отписываемся на события кнопок
        UnsubscribeButton(_playButton, LoadGameplayScene);
        UnsubscribeButton(_creditsButton, OpenCreditsPanel);
        UnsubscribeButton(_closeCreditsButton, CloseCreditsPanel);
        UnsubscribeButton(_settingsButton, OpenSettingsPanel);
        UnsubscribeButton(_closeSettingsButton, CloseSettingsPanel);
        UnsubscribeButton(_exitButton, ExitApplication);

        // 2. Переключаем флаг инициализации
        _isInitialized = false;

        // 3. Добавляем лог
        Debug.Log("MainMenuController: Button listeners unsubscribed");
    }


    private void LoadGameplayScene()
    {        
        SceneManager.LoadScene(_gameplaySceneName);
    }


    private void OpenCreditsPanel()
    {
        _creditsPanel?.SetActive(true);
    }


    private void CloseCreditsPanel()
    {
        _creditsPanel?.SetActive(false);
    }


    private void OpenSettingsPanel()
    {
        _settingsPanel?.SetActive(true);
    }


    private void CloseSettingsPanel()
    {
        _settingsPanel?.SetActive(false);
    }


    private void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Остановка Play Mode
#else
        Application.Quit();                              // Закрытие билда
#endif
        Debug.Log("Application is closed");
    }

}
