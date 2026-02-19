using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicalProperties : MonoBehaviour
{   

    
    [SerializeField] private DestructibleMaterial _material;  // Ссылка на ScriptableObject с характеристиками материала
    [SerializeField] private bool _rigidbodySleep = true;     // Флаг для сна Rigidbody при старте

    private Rigidbody2D _rigidBody2D;        // Ссылка на Rigidbody компонента
    private SpriteRenderer _spriteRenderer;  // Ссылка на SpriteRenderer компонента

    public float Mass => _rigidBody2D.mass;    // Публичное свойство для доступа к массе

    private void Awake()
    {
        // 1. Кзширование компонентов        
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Проверка наличия BlockMaterials
        if (_material == null)
        {
            Debug.LogWarning($"PhysicalProperties: No material assigned to {name}");
        }

        // 3. Вычисление и установка массы 
        CalculateAndApplyMass();

        // 4. Установка состояния сна Rigidbody
        if (_rigidbodySleep && _rigidBody2D != null)
        {
            _rigidBody2D.sleepMode = RigidbodySleepMode2D.StartAsleep;
        }
    }


    private void CalculateAndApplyMass()
    {
        float width, height; // Размеры объекта

        // Используем реальный размер спрайта, если есть
        if (_spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
            width = spriteSize.x;
            height = spriteSize.y;
        }
        // Подстраховка: используем масштаб трансформы
        else
        {
            Vector3 scale = transform.localScale;
            width = Mathf.Abs(scale.x);
            height = Mathf.Abs(scale.y);
            Debug.Log($"PhysicalProperties: Using transform scale for {name} (no valid SpriteRenderer)");            
        }

        float area = width * height;              // Вычисляем объем
        float mass = area * _material.density;    // Вычисляем массу

        // Защита от нулевой или отрицательной массы
        if (mass <= 0f)
        {
            mass = 0.1f; 
            Debug.LogWarning($"PhysicalProperties: Invalid mass for {name}, using fallback 0.1");
        }

        _rigidBody2D.mass = mass; // Применяем массу к Rigidbody
    }

#if UNITY_EDITOR
    // 
    public bool EditorSleepOnStart
    {
        get => _rigidbodySleep;
        set => _rigidbodySleep = value;
    }
#endif

}
