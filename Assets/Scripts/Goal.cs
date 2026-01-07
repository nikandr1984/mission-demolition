using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Material _defaultMarerial; // Материал по умолчанию для цели
    [SerializeField] private Material _hitMaterial;     // Материал при попадании в цель

    private Renderer _renderer;                        // Ссылка на компонент Renderer цели


    private bool _goalMet = false;    // Флаг поражения цели
    public bool GoalMet => _goalMet;  // Свойство для доступа к флагу поражения цели


    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_defaultMarerial == null)
        {
            _defaultMarerial = _renderer.sharedMaterial;
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile")  && !_goalMet) // Если в цель попал снаряд и цель ещё не поражена...
        {
            _goalMet = true; // ...устанавливаем флаг поражения цели
            
            _renderer.sharedMaterial = _hitMaterial; // Меняем материал цели на материал при попадании

        }
    }


    public void ResetGoal() // Метод для сброса состояния цели
    {
        _goalMet = false;                            // Сбрасываем флаг поражения цели
        _renderer.sharedMaterial = _defaultMarerial; // Возвращаем материал по умолчанию
    }

}
