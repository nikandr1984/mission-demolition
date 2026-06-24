using System;
using System.Collections;
using UnityEngine;

public class MainMenuAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _bgmSource;         // Компонент воспроизведения фоновой музыки
    [SerializeField] private AudioSource _startVoiceSource;  // Компонент воспроизведения стартовой фразы
    [SerializeField] private AudioSource _clickSource;       // Компонент воспроизведения звука кликов   

    [SerializeField] private AudioClip _bgmClip;         // Аудиоклип фоновой музыки
    [SerializeField] private AudioClip _startVoiceClip;  // Аудиоклип стартовой фразы
    [SerializeField] private AudioClip _clickClip;       // Аудиоклип звука кликов

    [SerializeField, Range(0f, 1f)] private float _defaultVolume = 0.7f;  // Базовая громкость для всех звуков    
    [SerializeField] private float _clickVolumeMultiplier = 1.5f;         // Множитель громкости для кликов
    [SerializeField] private float _bgmVolumeMultiplier = 0.4f;           // Множитель громкости для фоновой музыки                                                                // 

    public event Action OnVoiceFinished; // Событие окончания звучания стартовой фразы
       

    private void Awake()
    {
        // 1. Инициализируем все аудиоисточники
        SetupSource(_bgmSource, _bgmClip, loop: true);
        SetupSource(_startVoiceSource, _startVoiceClip, loop: false);
        SetupSource(_clickSource, _clickClip, loop: false);


        // 2. Настраиваем громкость фоновой музыки отдельно
        if (_bgmSource != null)
        {
            _bgmSource.volume = _defaultVolume * _bgmVolumeMultiplier;
        }
        
        
        // 3. Настраиваем громкость клика отдельно
        if (_clickSource != null)
        {
            _clickSource.volume = _defaultVolume * _clickVolumeMultiplier;
        }
    }

    private void Start()
    {
       // 1. Запускаем фоновую музыку при загрузке меню
       if (_bgmSource != null && _bgmClip != null)
        {
            _bgmSource.Play();
        }
       
    }

    
    private void SetupSource(AudioSource source, AudioClip clip, bool loop = false)
    {
        // 1. Проверяем, что компоненты и клипы назначены
        if (source == null || clip == null) return;

        // 2. Настраиваем источник
        source.clip = clip;
        source.volume = _defaultVolume;
        source.loop = loop;
        source.playOnAwake = false;
    }


    private void PlayClick()
    {
        // 1. После проверки воспроизводим звук клика
        if (_clickClip != null && _clickSource != null)
        {
            _clickSource.PlayOneShot(_clickClip, _defaultVolume * _clickVolumeMultiplier);
        }
    }

    private void NotifyVoiceFinished()
    {    
        // 1. Вызываем событие окончания звучания стартовой фразы
        OnVoiceFinished?.Invoke();
    }


    public void OnMenuButtonPressed()
    {
        PlayClick();
    }


    public void OnStartButtonPressed()
    {
        // 1. Останавливаем предыдущие корутины, чтобы не было наложения фейдов
        StopAllCoroutines();

        // 2. Проигрываем звук клика для тактильного отклика
        PlayClick();

        // 3. Проигрываем стартовую фразу, планируем переход после её окончания, затухаем фоновую музыку 
        if (_startVoiceClip != null && _startVoiceSource != null)
        {       
            _startVoiceSource.Play();
            StartCoroutine(FadeOutBGM(_startVoiceClip.length));
        }
        else
        {
            StartCoroutine(FadeOutBGM(0.5f));
        }    
    }


    private IEnumerator FadeOutBGM(float duration)
    {
        // 1. Защита отnull
        if (_bgmSource == null) yield break;

        // 2. Сохраняем начальную громкость
        float startVolume = _bgmSource.volume;

        // 3. Вводим таймер для затухания
        float timer = 0f;

        // 4. Цикл затухания
        while (timer < duration)
        {
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }

        // 5. Гарантируем, что громкость равна нулю
        _bgmSource.volume = 0f;

        // 6. Останавливаем источник после затухания
        _bgmSource.Stop();

        // 7. Уведомляем подписчиков, что можно грузить сцену
        NotifyVoiceFinished();
    }
    
}
