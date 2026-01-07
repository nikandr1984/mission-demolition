using UnityEngine;


[CreateAssetMenu(fileName = "New Block Material", menuName = "Game/Block Material")]
public class BlockMaterials : ScriptableObject
{
    [Header("Physics")]
    public float density = 1f;  // Масса для Rigidbody

    [Header("Destruction")]    
    public float damageThreshold = 10f;  // Сколько урона выдерживает, прежде чем сломаться

    [Header("Damage")]
    public float damageDealt = 5f;                      // Сколько урона наносит материал при ударе
    public float selfCollisionDamageMultiplier = 0.5f;  // Глобальный ослабляющий коэффициент для взаимных столкновений
    public float externalImpactDamageMultiplier = 1f;   // Усиливающий коэффициент для ударов об damagingLayers


    [Header("Visual and Sound")]
    public ParticleSystem breakVFX;  // Эффект разрушения
    public AudioClip breakSound;     // Звук разрушения
    public float soundVolume = 1f;   // Громкость звука
}
