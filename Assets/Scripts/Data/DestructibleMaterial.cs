using UnityEngine;


[CreateAssetMenu(fileName = "New Block Material", menuName = "Game/Block Material")]
public class DestructibleMaterial : ScriptableObject
{
    [Header("Physics")]
    public float density = 1f;  // Плотность для рассчета массы

    [Header("Destruction")]    
    public float damageThreshold = 10f;  // Сколько урона выдерживает, прежде чем сломаться

    [Header("Damage")]
    public float damageDealt = 5f;                      // Сколько урона наносит материал при ударе
    public float selfCollisionDamageMultiplier = 0.5f;  // Глобальный ослабляющий коэффициент для взаимных столкновений
    public float externalImpactDamageMultiplier = 1f;   // Усиливающий коэффициент для ударов об damagingLayers    


    [Header("Audio")]
    [SerializeField] private AudioClip[] _hitClips;                       // Массив звуков ударов
    [SerializeField] private AudioClip[] _destroyClips;                   // Массив звуков разрушения
    [SerializeField, Range(0f, 1f)] private float _hitVolume = 0.7f;      // Громкость звуков ударов 
    [SerializeField, Range(0f, 1f)] private float _destroyVolume = 0.9f;  // Громкость звуков разрушения

    public float HitVolume => _hitVolume;
    public float DestroyVolume => _destroyVolume;


    // === ЛОГИКА ДЛЯ АУДИО ===
    public AudioClip GetRandomHitClip()
    {
        // 1. Проверка ссылки на null и длины массива
        if (_hitClips == null || _hitClips.Length == 0) return null;

        // 2. Выбираем рандомный индекс от 0 до длины массива
        int randomIndex = UnityEngine.Random.Range(0, _hitClips.Length);

        // 3. Возвращаем клип по выбранному индексу
        return _hitClips[randomIndex];
    }

    public AudioClip GetRandomDestroyClip()
    {
        // 1. Проверка ссылки на null и длины массива
        if (_destroyClips == null || _destroyClips.Length == 0) return null;

        // 2. Выбираем рандомный индекс от 0 до длины массива
        int randomIndex = UnityEngine.Random.Range(0, _destroyClips.Length);

        // 3. Возвращаем клип по выбранному индексу
        return _destroyClips[randomIndex];
    }

}
