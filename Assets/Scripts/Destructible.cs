using UnityEngine;

public class Destructible : MonoBehaviour
{
    // Поля для настройки в инспекторе
    [SerializeField] private DestructibleMaterial _material;     // Ссылка на ScriptableObject с характеристиками материала    
    [SerializeField] private PhysicalProperties _physicalProps;  // Ссылка на компонент с физическими свойствами    
    [SerializeField] private LayerMask _damagingLayers;          // Слои, которые могут наносить урон
    [SerializeField] private string _groundTag = "Ground";       // Тег для земли
    [SerializeField] private bool _canBeDestroyed = true;        // Флаг, можно ли разрушить объект


    private Rigidbody2D _rigidBody2D;        // Ссылка на Rigidbody компонента    
    private AudioSource _audioSource;        // Ссылка на AudioSource для воспроизведения звуков

    private bool _isDestroyed = false;       // Флаг, указывающий, был ли объект уже разрушен     
    
    private Renderer[] _renderers;           // Массив рендереров объекта



    private void Awake()
    {
        // Инициализация компонентов
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();        

        // Проверка наличия Rigidbody
        if (_rigidBody2D == null)
        {
            Debug.LogError("Destructible: No Rigidbody2D found on " + name);
            return;
        }

        // Проверка наличия PhysicalProperties
        if (_physicalProps == null)
        {
            _physicalProps = GetComponent<PhysicalProperties>();
            if (_physicalProps == null )
            {
                Debug.LogError("Destructible: No PhysicalProperties found on " + name);
            }
        }


        // Проверка наличия материала
        if (_material == null)
        {
            Debug.LogWarning("Destructible: No BlockMaterials assigned to " + name);
            return;
        }        


        // Кэширование рендереров
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    
    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDestroyed || !_canBeDestroyed) return;     // Если уже разрушен или не может быть разрушен, выходим

        // Получаем величину относительной скорости столкновения
        float relativeVelocity = collision.relativeVelocity.magnitude;
        // Рассчитываем силу удара
        float impactForce = _physicalProps.Mass * relativeVelocity;
        // Если сила удара нулевая или отрицательная, выходим
        if (impactForce <= 0f) return;
        // Инициализаируем переменную урона
        float damage = 0f;                               


        // Определяем тип столкновения и рассчитываем урон
        // 1. Удар о землю
        if (collision.gameObject.CompareTag(_groundTag))
        {
            float groundMult = _material?.externalImpactDamageMultiplier ?? 1f;
            damage = impactForce * groundMult;
        }
        // 2. Удар о другой разрушаемый объект
        else if (collision.gameObject.TryGetComponent<Destructible>(out Destructible other))
        {
            // Получение урона, наносимого другим объектом
            float otherDamageDealt = other._material?.damageDealt ?? 1f;

            // Расчет урона с учетом массы обоих объектов
            float selfCollisionMult = _material?.selfCollisionDamageMultiplier ?? 0.5f;        
            damage = impactForce * (other._physicalProps.Mass / _physicalProps.Mass) * otherDamageDealt * selfCollisionMult;
        }
        // 3. Удар о объект из слоев, наносящих урон
        else if (IsInLayerMask(collision.gameObject, _damagingLayers))
        {
            float externalMult = _material?.externalImpactDamageMultiplier ?? 1f;
            damage = impactForce * externalMult;
        }


        // Применение урона
        if (damage >= (_material?.damageThreshold ?? 10f))
        {
            DestroyObject();
        }
    }


    private void DestroyObject() 
    {
        if (_isDestroyed) return; // Защита от повторного разрушения

        _isDestroyed = true;      // Установка флага разрушения

        _rigidBody2D.bodyType = RigidbodyType2D.Kinematic; // Отключение физики
            

        // Отключение рендереров
        foreach (Renderer renderer in _renderers)
        {
            renderer.enabled = false;
        }


        // Воспроизведение визуального эффекта разрушения
        if (_material?.breakVFX != null) 
        {
            var vfx = Instantiate(_material.breakVFX, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, 2f);
        }

        // Воспроизведение звука разрушения
        if (_material?.breakSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_material.breakSound, _material.soundVolume);
        }


        Invoke(nameof(Deactivate), 0.1f); // Отключение объекта через короткое время
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
