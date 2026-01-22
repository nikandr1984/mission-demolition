using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalProperties : MonoBehaviour
{
    [Header("Geometry")]
    [SerializeField] private float _thickness = 0.2f;    // Толщина объекта

    [Header("Material")]
    [SerializeField] private DestructibleMaterial _material;   // Ссылка на ScriptableObject с характеристиками материала

    [Header("Physics Initialization")]
    [SerializeField] private bool _sleepOnStart = true;  // Флаг для установки Rigidbody в состояние сна при старте

    private Rigidbody _rigidBody;            // Ссылка на Rigidbody компонента
    private SpriteRenderer _spriteRenderer;  // Ссылка на SpriteRenderer компонента

    public float Mass => _rigidBody.mass;    // Публичное свойство для доступа к массе

    private void Awake()
    {
        // 1. Кзширование компонентов        
        _rigidBody = GetComponent<Rigidbody>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Проверка наличия BlockMaterials
        if (_material == null)
        {
            Debug.LogWarning($"PhysicalProperties: No material assigned to {name}");
        }

        // 3. Вычисление и установка массы
        CalculateAndApplyMass(); 

        // 4. Установка Rigidbody в состояние сна, если флаг установлен
        if (_sleepOnStart && _rigidBody != null)
        {
            _rigidBody.Sleep();
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

        float volume = width * height * _thickness; // Вычисляем объем
        float mass = volume * _material.density;    // Вычисляем массу

        // Защита от нулевой или отрицательной массы
        if (mass <= 0f)
        {
            mass = 0.1f; 
            Debug.LogWarning($"PhysicalProperties: Invalid mass for {name}, using fallback 0.1");
        }

        _rigidBody.mass = mass; // Применяем массу к Rigidbody
    }


#if UNITY_EDITOR
    // Позволяет Editor-скриптам читать и изменять флаг _sleepOnStart
    public bool EditorSleepOnStart
    {
        get => _sleepOnStart;
        set => _sleepOnStart = value;
    }
#endif


}
