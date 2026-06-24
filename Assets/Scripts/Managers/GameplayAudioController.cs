using UnityEngine;
using System.Collections;
using System;

public class GameplayAudioController : MonoBehaviour
{
    // Поля для логики фонового бормотания
    [Header("Mumble Sounds")]
    [SerializeField] private AudioSource _mumbleSource;                  // Компонент для фоновых фраз
    [SerializeField] private AudioClip[] _mumbleClips;                   // Массив клипов для фоновых фраз
    [SerializeField] private float _minInterval = 3f;                    // Мин. интервал между фразами
    [SerializeField] private float _maxInterval = 8f;                    // Макс. интервал между фразами
    [SerializeField, Range(0f, 1f)] private float _defaultVolume = 0.6f; // Стандартная громкость для звуков
    private Coroutine _mumbleRoutine;                                    // Ссылка на корутину для управления циклом фраз    
    private bool _isProjectileInFlight = false;


    // Поля для логики звуков натяжения жгута
    [Header("Harness Sounds")]
    [SerializeField] private AudioSource _stretchSource;            // Источник для зацикленного звука натяжения
    [SerializeField] private AudioClip _stretchClip;                // Клип натяжения                             
    [SerializeField] private float _sharpPullThreshhold = 0.1f;     // Порог для резкого натяжения, при котором будет проигрываться звук
    [SerializeField] private float _minStretchToTrigger = 0.15f;    // Минимальное натяжение для проигрывания звука
    [SerializeField] private float _minStretchSoundInterval = 0.2f; // Минимальный интервал между звуками
    private float _lastProcessedStretch = 0f;                       // Последнее обработанное значение натяжения для предотвращения повторов
    private float _lastStretchSoundTime;                            // Время последнего звука натяжения
    private bool _hasPlayedThisPull;                                // Флаг был ли уже проигран звук для текущего натяжения


    // Поля для логики поведения снаряда
    [Header("Launch Siren Sounds")]
    [SerializeField] private AudioClip[] _projectileSirenClips;      // Массив клипов для звуков сирены при выстреле
    [SerializeField] private AudioClip _projectileDestroyClip;       // Клип уничтожения снаряда
    [SerializeField] private AudioSource _projectileSfxSource;       // Источник для звуков снаряда
    [SerializeField] private float _projectileSirenVolume = 0.7f;    // Громкость сирены
    [SerializeField] private float _projectileDestroyVolume = 0.8f;  // Громкость уничтожения снаряда 


    // Поля для логики ударов и разрушения блоков и целей
    [Header("Block Sounds")]
    [SerializeField] private AudioSource _blocksSfxSource;        // Источник для звуков ударов и разрушения блоков
    [SerializeField] private float _minImpactForceForSound = 2f;  // Минимальная сила удара для проигрывания звука


    // Поля для логики интерфейса
    [Header("Interface Sounds")]
    [SerializeField] AudioSource _interfaceSfxSource;
    [SerializeField] AudioClip _clickClip;
    [SerializeField, Range(0f, 1f)] private float _clickVolume = 0.9f;
       



    // --- ЖИЗНЕННЫЙ ЦИКЛ ---

    private void OnEnable()
    {
        // 1. Подписываемся на события
        Slingshot_New.OnStretchChanged += HandleStretchChanged;
        Slingshot_New.OnProjectileLaunched += HandleProjectileLaunched;
        Destructible.OnBlockHit += HandleBlockHit;
        Destructible.OnBlockDestroyed += HandleBlockDestroyed;
        ProjectaileBehaviour.OnProjectailDestroyed += HandleProjectileDestroyed;        
        UIManager.OnAnyButtonClicked += HahdleInterfaceButtonPressed;

    }


    private void OnDisable()
    {
        // 1. Отписываемся от событий
        Slingshot_New.OnStretchChanged -= HandleStretchChanged;
        Slingshot_New.OnProjectileLaunched -= HandleProjectileLaunched;
        Destructible.OnBlockHit -= HandleBlockHit;
        Destructible.OnBlockDestroyed -= HandleBlockDestroyed;
        ProjectaileBehaviour.OnProjectailDestroyed -= HandleProjectileDestroyed;        
        UIManager.OnAnyButtonClicked -= HahdleInterfaceButtonPressed;

        // 2. Сброс флагов при отключении
        _hasPlayedThisPull = false;
        _isProjectileInFlight = false;


        // 2. Безопасная остановка корутины на случай дублирования
        if (_mumbleRoutine != null)
        {
            StopCoroutine(_mumbleRoutine);
            _mumbleRoutine = null;
        }
    }



    private void Start()
    {
        // 1. Запускаем корутину управления циклом произношения фраз
        _mumbleRoutine = StartCoroutine(MumbleRoutine());
    }
          


    // === ЛОГИКА БОРМОТАНИЯ ===    


    // Корутина: управление таймингом
    private IEnumerator MumbleRoutine()
    {
        while (true)
        {
            // 1. Выбираем случайный интервал для следующей фразы
            float waitTime = UnityEngine.Random.Range(_minInterval, _maxInterval);

            // 2. Ждем выбранный интервал
            yield return new WaitForSeconds(waitTime);

            // 3. Если снаряд в полете, то пропускаем этот цикл
            if (_isProjectileInFlight)
            {
                continue;
            }

            // 4. Проигрываем случайную фразу
            PlayRandomMumble();
        }       
    }


    // Выбор клипа
    private void PlayRandomMumble()
    {
        // 1. Предохранитель от null и пустого массива
        if (_mumbleClips == null || _mumbleClips.Length == 0 || _mumbleSource == null) return;

        // 2. Выбираем случайный клип из массива
        int randomIndex = UnityEngine.Random.Range(0, _mumbleClips.Length);

        // 3. Проигрываем выбранный клип
        PlayMumbleClip(_mumbleClips[randomIndex]);
    }


    // Воспроизведение клипа
    private void PlayMumbleClip(AudioClip clip, float volumeOverride = -1f)
    {
        // 1. Предохранитель от null
        if (clip == null || _mumbleSource == null) return;

        // 2. Определяем громкость звучания
        float finalVolume = (volumeOverride == -1f) ? _defaultVolume : volumeOverride;

        // 3. Настраиваем и запускаем аудио
        _mumbleSource.clip = clip;
        _mumbleSource.volume = finalVolume;
        _mumbleSource.Play();
    }
    
    


    // === ЛОГИКА НАТЯЖЕНИЯ ЖГУТА ===    

    private void HandleStretchChanged(float stretchAmount, bool isIncreasing)
    {
        
        // 1. Если не прошло достаточно времени, то выходим
        if (Time.time - _lastStretchSoundTime < _minStretchSoundInterval)
        {
            _lastProcessedStretch = stretchAmount;
            return;
        }  
                
        // 2. Если натяжение меньше минимального порога - игнорируем
        if (stretchAmount < _minStretchToTrigger)
        {
            _hasPlayedThisPull = false;
            _lastProcessedStretch = stretchAmount;
            return;
        }

        // 3. Если натяжение не растет, а уменьшается - игнорируем
        if (!isIncreasing)
        {
            _hasPlayedThisPull = false;
            _lastProcessedStretch = stretchAmount;
            return;
        }                
        
        // 4. Если натяжение выросло незначительно - игнорируем
        if (stretchAmount - _lastProcessedStretch < _sharpPullThreshhold)
        {
            _lastProcessedStretch = stretchAmount;
            return;
        }

        // 5. Защита от повторного срабатывания на одном рывке
        if (_hasPlayedThisPull)
        {
            _lastProcessedStretch = stretchAmount;
            return;
        }

        // 6. Если все условия выполнены - проигрываем звук натяжения
        PlayStretchSound();

        // 7. Запоминаем время воспроизведения
        _lastStretchSoundTime = Time.time;

        // 8. Блокируем повторное проигрывание до следующего рывка
        _hasPlayedThisPull = true;

        // 9. Сохраняем текущее значение натяжения для сравнения в следующем вызове
        _lastProcessedStretch = stretchAmount;

    }

    private void PlayStretchSound()
    {
        // 1. Проыерка на null
        if (_stretchClip == null || _stretchSource == null) return;

        // 2. Пройгрываем звук натяжения
        _stretchSource.PlayOneShot(_stretchClip);
    }       



    // === ЛОГИКА ЗВУКОВ ПОВЕДЕНИЯ СНАРЯДА ===    

    private void PlayLaunchSiren()
    {
        // 1. Проверяем массис: не null и не пустой
        if (_projectileSirenClips == null || _projectileSirenClips.Length == 0) return;

        // 2. Проверяем источник на null
        if (_projectileSfxSource == null) return;

        // 3. Генерируем случайный индекс в диапозоне [0, длина_массива}
        int randomIndex = UnityEngine.Random.Range(0, _projectileSirenClips.Length);

        // 4. Получаем ссылку на выбранный клип
        AudioClip selectedClip = _projectileSirenClips[randomIndex];

        // 5. Предохранитель, если клип вдруг оказался null
        if (selectedClip == null) return;

        // 6. Проигрываем выбранный клип
        _projectileSfxSource.PlayOneShot(selectedClip, _projectileSirenVolume);
    }


    // Обработчик события запуска снаряда
    private void HandleProjectileLaunched(Rigidbody2D projectileRB)
    {
        // 1. Устанавливаем флаг, что снаряд в полете
        _isProjectileInFlight = true;
        
        // 2. Проигрываем звук сирены
        PlayLaunchSiren();
    }


    // Обработчик события уничтожения снаряда
    private void HandleProjectileDestroyed()
    {
        // 1. Проверка ссылок на null
        if (_projectileDestroyClip == null || _projectileSfxSource == null) return;

        // 2. Устанавливаем флаг, что снаряд не в полете
        _isProjectileInFlight = false;

        // 3. Проигрываем звук уничтожения снаряда
        _projectileSfxSource.PlayOneShot(_projectileDestroyClip, _projectileDestroyVolume);
    }


    // === ЛОГИКА ЗВУКА УДАРА И РАЗРУШЕНИЯ ===    

    private void HandleBlockHit(DestructibleMaterial material, float impactForce)
    {
        // 1. Проверка на null
        if (material == null) return;

        // 2. Если сила удара меньше минимального порога - выходим
        if (impactForce < _minImpactForceForSound) return;

        // 3. Получаем случайный клип для удара
        AudioClip hitClip = material.GetRandomHitClip();

        // 4. Если клипов нет - выходим
        if (hitClip == null) return;

        // 5. Проигрываем клип
        _blocksSfxSource.PlayOneShot(hitClip, material.HitVolume);
    }


    private void HandleBlockDestroyed(DestructibleMaterial material)
    {
        // 1. Проверка на null
        if (material == null) return;

        // 2. Получаем случайный клип для разрушения
        AudioClip destroyClip = material.GetRandomDestroyClip();

        // 3. Есо=ли клипов нет - выходим
        if (destroyClip == null) return;

        // 4. Проигрываем клип
        _blocksSfxSource.PlayOneShot(destroyClip, material.DestroyVolume);
    }



    // === ЛОГИКА ЗВУКОВ ИНТЕРФЕЙСА ===   
       
    private void HahdleInterfaceButtonPressed()
    {
        PlayClick();
    }

    private void PlayClick()
    {
        // 1. Проверка на null
        if (_interfaceSfxSource == null || _clickClip == null) return;

        // 2. Проигрываем звук клика
        _interfaceSfxSource.PlayOneShot(_clickClip, _clickVolume);
    }
}
