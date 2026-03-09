using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectailBehaviour : MonoBehaviour
{
    // Поля для настройки в инспекторе
    [SerializeField] private float _stopVelocityThreshold = 0.15f; // Пороговая скорость для определения остановки снаряда
    [SerializeField] private float _timeToDestroyAfterStop = 3.0f; // Время до уничтожения снаряда после его остановки

    // Внутреннее состояние
    private bool _isStopped = false;   // Флаг, указывающий, остановился ли снаряд
    private Coroutine _stopCoroutine;  // Ссылка на корутину для отслеживания остановки снаряда

    // Кэшируемые компоненты
    private Rigidbody2D _projectileRb;

    // Публичные API
    public static event System.Action<Rigidbody2D> OnProjectailCollision;
    public static event System.Action OnProjectailDestroyed;




    private void Awake()
    {
        _projectileRb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        // 1. Если флаг остановки уже установлен, ничего не делаем
        if (_isStopped) return;

        // 2. Находим текущую скорость снаряда
        float currrentSpeed = _projectileRb.linearVelocity.magnitude;

        // 3. Проверяем скорость снаряда и запускаем или останавливаем таймер уничтожения
        if (currrentSpeed < _stopVelocityThreshold)
        {
            if (_stopCoroutine == null)
            {
                _stopCoroutine = StartCoroutine(StopTimer());
            }
        }
        else
        {
            if (_stopCoroutine != null)
            {
                StopCoroutine(_stopCoroutine);
                _stopCoroutine = null;
            }
        }
    }


    // Метод для обработки столкновений с другими объектами
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnProjectailCollision?.Invoke(GetComponent<Rigidbody2D>());        
    }



    // Метод для обработки выхода за границы уровня
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("LevelBoundary"))
        {
            DestroyProjectile();            
        }
    }



    // Корутина для уничтожения снаряда после его остановки
    private IEnumerator StopTimer()
    {
        yield return new WaitForSeconds(_timeToDestroyAfterStop);

        if (_projectileRb.linearVelocity.magnitude < _stopVelocityThreshold)
        {
            _isStopped = true;            
            DestroyProjectile();
        }
        else
        {
            _stopCoroutine = null;
        }
    }



    // Метод для уничтожения снаряда и вызова события
    private void DestroyProjectile()
    {
        Destroy(gameObject);
        OnProjectailDestroyed?.Invoke();
    }


    // Метод для очистки корутины при уничтожении объекта
    private void OnDestroy()
    {
        if (_stopCoroutine != null)
        {
            StopCoroutine(_stopCoroutine);
        }
    }
}
