using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]    
    [SerializeField] private Slider _fpsSlider;                // Ссылка на ползунок выбора FPS
    [SerializeField] private TMP_Text _fpsValueText;           // Ссылка на текст с текущим значением FPS
    [SerializeField] private TMP_Dropdown _resolutionDropdown; // Ссылка на дропдаун разрешения
    [SerializeField] private Toggle _fullscreenToggle;         // Ссылка на переключатель фулскрина
    
    // Поля логики FPS
    private readonly int[] _fpsValues = { 30, 60, 120, -1 };                                   // Массив дискретных значений ползунка
    private readonly string[] _fpsLabels = { "30 FPS", "60 FPS", "120 FPS", "FPS unlocked" };  // Массив подписей для UI

    // Поля логики разрешения
    private Resolution[] _currentResolutions; // Кэшированные разрешения для быстрого доступа



    private void Start()
    {
        // 1. Проверяем, что менеджер настроек существует
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("SettingsUI: SettingsManagers not found!");
            return;
        }

        // 2. Настраиваем параметры переключателей
        SetupFpsSlider();
        SetupResolutionDropdown();
        SetupFullscreenToggle();

        // 3. Синхронизируем UI с текущими настройками
        UpdateUIFromSettings();        
    }

    
    // МЕТОД настраивает параметры ползунка FPS
    private void SetupFpsSlider()
    {
        // 1. Устанавливаем минимальное значение
        _fpsSlider.minValue = 0;
        
        // 2. Устанавливаем максимальное значение
        _fpsSlider.maxValue = _fpsValues.Length - 1;
        
        // 3. Только целые значения
        _fpsSlider.wholeNumbers = true;
        
        // 4. Подписываемся на изменение
        _fpsSlider.onValueChanged.AddListener(OnFpsSliderChanged);
    }


    // МЕТОД настраивает параметры дропдауна разрешения
    private void SetupResolutionDropdown()
    {
        // 1. Получаем доступные разрешения из SettingsManager
        _currentResolutions = SettingsManager.Instance.GetAvailableResolutions();

        // 2. Очищаем дропдаун
        _resolutionDropdown.ClearOptions();

        // 3. Добавляем опции
        List<string> resolutionLabels = new List<string>();
        
        for (int i = 0; i < _currentResolutions.Length; i++)
        {
            resolutionLabels.Add(SettingsManager.FormatResolution(_currentResolutions[i]));
        }

        _resolutionDropdown.AddOptions(resolutionLabels);              
               
        // 4. Подписываемся на изменеия
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);        
    }   


    // МЕТОД настраивает параметры переключателя фуллскрина
    private void SetupFullscreenToggle()
    {
        _fullscreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
    }


    // МЕТОД обрабатывает переключение слайдера FPS   
    private void OnFpsSliderChanged(float value)
    {
        // 1. Оругляем до ближайшего целого индекса
        int index = Mathf.RoundToInt(value);

        // 2. Получаем соответсвующее значение FPS
        int fpsValue = _fpsValues[index];

        // 3. Применяем через менеджер настроек
        SettingsManager.Instance.SetFpsLimit(fpsValue);

        // 4. Обновляем текст в интерфейсе
        _fpsValueText.text = _fpsLabels[index];
    }


    // МЕТОД обрабатывает переключение дропдауна разрешения
    private void OnResolutionDropdownChanged(int index)
    {
        SettingsManager.Instance.SetResolution(index);
    }


    // МЕТОД обрабатывает переключение кнопки фуллскрина
    private void OnFullScreenToggleChanged(bool isFullscreen)
    {
        SettingsManager.Instance.SetFullscreen(isFullscreen);
    }


    // МЕТОД синхронизирует UI с текущими настройками (при открытии панели)
    private void UpdateUIFromSettings()
    {
        // 1. FPS
        // 1.1 Получаем сохраненное значение fps из менеджера настроек
        int currentFPS = SettingsManager.Instance.GetCurrentFpsLimit();

        // 1.2 Находим индекс значения fps в массиве
        int index = System.Array.IndexOf(_fpsValues, currentFPS);

        // 1.3 Если значение найдено - обновляем ползунок и текст
        if (index >= 0)
        {
            _fpsSlider.value = index;
            _fpsValueText.text = _fpsLabels[index];
        }

        // 2. Разрешение
        int resIndex = SettingsManager.Instance.GetCurrentResolutionIndex();
        _resolutionDropdown.SetValueWithoutNotify(resIndex);

        // 3. Полноэкранный режим
        _fullscreenToggle.isOn = SettingsManager.Instance.GetCurrentFullscreen();

    } 
    
}
