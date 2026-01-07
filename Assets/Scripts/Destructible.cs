using UnityEngine;

public class Destructible : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private BlockMaterials _material;       // Ссылка на ScriptableObject с характеристиками материала


    [Header("Geometry")]
    [SerializeField] private float _thickness = 0.2f;        // Толщина объекта


    [Header("Behaviour")]
    [SerializeField] private bool _canBeDestroyed = true;    // Флаг, можно ли разрушить объект
    [SerializeField] private LayerMask _damagingLayers;      // Слои, которые могут наносить урон
    [SerializeField] private string _groundTag = "Ground";   // Тег для земли


    private Rigidbody _rb;                // Ссылка на Rigidbody компонента
    private bool _isDestroyed = false;    // Флаг, указывающий, был ли объект уже разрушен
    private AudioSource _audioSource;     // Ссылка на AudioSource для воспроизведения звуков
    private float _calculatedMass;        // Вычисленная масса объекта на основе плотности материала
    
    private Renderer[] _renderers;        // Массив рендереров объекта


    private void Awake()
    {
        // Инициализация компонентов
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();

        // Проверка наличия необходимых компонентов
        if (_rb == null)
        {
            Debug.LogError("Destructible: No Rigidbody found on " + name);
            return;
        }

        // Проверка наличия материала
        if (_material == null)
        {
            Debug.LogWarning("Destructible: No BlockMaterials assigned to " + name);
            return;
        }


        // Вычисление и установка массы объекта
        CalculateMass();
        _rb.mass = _calculatedMass;


        // Кэширование рендереров
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    
    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }


    private void CalculateMass()
    {
        // Вычисление массы на основе плотности материала и размеров объекта
        Vector3 scale = transform.localScale; 
        float width = Mathf.Abs(scale.x);     
        float height = Mathf.Abs(scale.y);    
        float volume = width * height * _thickness;
        _calculatedMass = volume * _material.density;


        // Защита от нуля или отрицательной массы
        if (_calculatedMass <= 0f)
        {
            _calculatedMass = 0.1f;
            Debug.LogWarning("Destructible: Calculated mass <= 0 " + name + ". Using fallback 0.1");
        } 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDestroyed || !_canBeDestroyed) return;     // Если уже разрушен или не может быть разрушен, выходим
        if (collision.impulse == Vector3.zero) return;   // Если нет импульса, выходим

        float impactForce = collision.impulse.magnitude; // Вычисление силы удара
        float damage = 0f;                               // Инициализация переменной урона


        // Определение типа столкновения и расчет урона
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
            damage = impactForce * (other._calculatedMass / _calculatedMass) * otherDamageDealt * selfCollisionMult;
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
        if (_isDestroyed) return;     // Защита от повторного разрушения

        _isDestroyed = true;          // Установка флага разрушения
        _rb.isKinematic = true;       // Отключение физики
        _rb.detectCollisions = false; // Отключение коллизий


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
