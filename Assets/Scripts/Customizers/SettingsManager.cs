using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }  // Синглтон

    // Ключи для PlayerPrefs
    private const string FPS_PREF_KEY = "Settings_FPSMode";       
    private const string RES_WIDTH_PREF_KEY = "Settings_ScreenWidth";
    private const string RES_HEIGHT_PREF_KEY = "Settings_ScreenHeight";
    private const string FULLSCREEN_PREF_KEY = "Settings_Fullscreen";

    // Текущие значения
    private int _currentFpsLimit = 60;                            
    private int _currentWidth = 1280;
    private int _currentHeight = 720;
    private bool _currentFullscreen = false;

    // Массив доступных разрешений (16:9)
    private Resolution[] _availableResolutions;
           


    private void Awake()
    {
        // 1. Инициализация синглтона и загрузка данных
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            CacheAvailableResolutions();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    // МЕТОД кэширует доступные разрешения экрана, фильтруя только 16:9
    private void CacheAvailableResolutions()
    {
        // 1. Получаем все поддерживаемые монитором разрешения в массив 
        Resolution[] allResolutions = Screen.resolutions;

        // 2. Фильтруем только 16:9 и добавляем в список
        List<Resolution> filteredList = new List<Resolution>();
        
        float targetAspect = 16f / 9f;        
        float tolerance = 0.01f;

        foreach (var res in allResolutions)
        {
            float aspect = (float)res.width / res.height;
            
            if (Mathf.Abs(aspect - targetAspect) < tolerance)
            {
                // Убираем дубликаты (одинаковые разрешения с разной частотой)
                if (!filteredList.Any(r => r.width == res.width && r.height == res.height))
                {
                    filteredList.Add(res);
                }
            }
        }

        // 3. Соритируем по возрастанию 
        _availableResolutions = filteredList
             .OrderBy(r => r.width)
             .ToArray();

        // 4. Лог - количество доступных разрешения
        Debug.Log($"Settings: Found {_availableResolutions.Length} resolutions (16:9)");

        // 5. Лог - список доступных разрешений
        for (int i = 0; i < _availableResolutions.Length; i++)
        {
            Debug.Log($" {i}: {_availableResolutions[i].width}x{_availableResolutions[i].height}");
        }
    }


    // МЕТОД устанавливает разрешение экрана
    public void SetResolution(int index)
    {
        // 1. Проверяем границы, выходим в случае несовпадения
        if (index < 0 || index >= _availableResolutions.Length)
        {
            Debug.LogWarning($"SettingsManager: Invalid rsolution index {index}");
            return;
        }

        // 2. Получаем выбранное разрешение
        Resolution selectedRes = _availableResolutions[index];

        // 3. Применяем разрешение
        Screen.SetResolution(selectedRes.width, selectedRes.height, _currentFullscreen);

        // 4. Сохраняем настройки
        PlayerPrefs.SetInt(RES_WIDTH_PREF_KEY, selectedRes.width);
        PlayerPrefs.SetInt(RES_HEIGHT_PREF_KEY, selectedRes.height);
        PlayerPrefs.Save();

        // 5. Обновляем локальные поля
        _currentWidth = selectedRes.width;
        _currentHeight = selectedRes.height;

        // 6. Лог - выводим текущее разрешение
        Debug.Log($"SettingsManager: Resolution set to {selectedRes.width}x{selectedRes.height}");
    }


    // МЕТОД вкл-выкл полноэкранный режим и сохраняет настройку
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_PREF_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        _currentFullscreen = fullscreen;

        Debug.Log($"SettingsManager: Fullscreen set to {fullscreen}");
    }


    // МЕТОД возвращает текущий индекс разрешения
    public int FindResolutionIndex(int width, int height)
    {
        // 1. Проходим по массиву, сравниваем ширину/высоту, возвращаем индекс
        for (int i = 0; i < _availableResolutions.Length; i++)
        {
            if (_availableResolutions[i].width == width &&
                _availableResolutions[i].height == height)
            {
                return i;
            }
        }

        // 2. Если не нашли, то возвращаем -1
        return -1;
    }



    // МЕТОД устанавливает FPS
    public void SetFpsLimit(int fpsValue)
    {
        // 1. Применяем лимит fps к игре
        Application.targetFrameRate = fpsValue;

        // 2. Отключаем Vsync, чтобы лимит работал корректно
        QualitySettings.vSyncCount = 0;

        // 3. Сохраняем настройки в памяти устройства
        PlayerPrefs.SetInt(FPS_PREF_KEY, fpsValue);
        PlayerPrefs.Save();

        // 4. Обновляем локальное поле
        _currentFpsLimit = fpsValue;

        Debug.Log($"Settings: FPS limit set to {GetFpsLabel(fpsValue)}");
    }



    // МЕТОД загружает настройки
    private void LoadSettings()
    {
        // 1. Загружаем и применяем Fullscreen
        int savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_PREF_KEY, 0);
        SetFullscreen(savedFullscreen == 1);          
                
        // 2. Загружаем и применяем разрешение
        int saveWidth = PlayerPrefs.GetInt(RES_WIDTH_PREF_KEY, 1280);
        int saveHeight = PlayerPrefs.GetInt(RES_HEIGHT_PREF_KEY, 720);        

        int resolutionIndex = FindResolutionIndex(saveWidth, saveHeight);

        if (resolutionIndex >= 0)
        {
            SetResolution(resolutionIndex);
            Debug.Log($"SettingsManager: Loaded resolution {saveWidth}x{saveHeight}");
        }
        else
        {
            SetResolution(0);
            Debug.Log($"SettingsManager: Saved resolution {saveWidth}x{saveHeight} not available.");
        }       

        // 3. Загружаем и применяем FPS
        int savedFps = PlayerPrefs.GetInt(FPS_PREF_KEY, 60);
        SetFpsLimit(savedFps);
    }
           
        
    // МЕТОД форматирует значение FPS в строку
    public static string GetFpsLabel(int fpsValue)
    {
        return fpsValue switch
        {
            -1 => "Unlocked",
            30 => "30 FPS",
            60 => "60 FPS",
            120 => "120 FPS",
            _ => $"{fpsValue} FPS"
        };
    }
    

    // МЕТОД форматирует значение разрешения в строку
    public static string FormatResolution(Resolution res)
    {
        return $"{res.width}x{res.height}";
    }


    // ГЕТТЕРЫ (для синхронизации UI)  

    public int GetCurrentFpsLimit() => _currentFpsLimit;
    public Resolution[] GetAvailableResolutions() => _availableResolutions;
    public bool GetCurrentFullscreen() => _currentFullscreen;

    public int GetCurrentResolutionIndex()
    {
        return FindResolutionIndex(_currentWidth, _currentHeight);
    }

}
