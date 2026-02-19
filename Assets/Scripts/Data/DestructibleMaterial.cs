using UnityEngine;


[CreateAssetMenu(fileName = "New Block Material", menuName = "Game/Block Material")]
public class DestructibleMaterial : ScriptableObject
{
    [Header("Physics")]
    public float density = 1f;  // ѕлотность дл€ рассчета массы

    [Header("Destruction")]    
    public float damageThreshold = 10f;  // —колько урона выдерживает, прежде чем сломатьс€

    [Header("Damage")]
    public float damageDealt = 5f;                      // —колько урона наносит материал при ударе
    public float selfCollisionDamageMultiplier = 0.5f;  // √лобальный ослабл€ющий коэффициент дл€ взаимных столкновений
    public float externalImpactDamageMultiplier = 1f;   // ”силивающий коэффициент дл€ ударов об damagingLayers


    [Header("Visual and Sound")]
    public ParticleSystem breakVFX;  // Ёффект разрушени€
    public AudioClip breakSound;     // «вук разрушени€
    public float soundVolume = 1f;   // √ромкость звука
}
