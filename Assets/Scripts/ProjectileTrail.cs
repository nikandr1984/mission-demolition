using UnityEngine;
using System.Collections.Generic;

public class ProjectileTrail : MonoBehaviour
{
    [SerializeField] private float _minDistance = 0.1f;    // Минимальное расстояние между точками    

    private LineRenderer _lineRenderer;                    // Ссылка на компонент LineRenderer
    private List<Vector3> _points = new List<Vector3>();   // Список точек линии
    private Rigidbody2D _targetRigidbody;                  // Цель, к которой будет привязан след 



    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();  // Кэшируем компонент LineRenderer
        if (_lineRenderer == null)      // Проверочка
        {
            Debug.LogError("ProjectileTrail: LineRenderer not found");
            enabled = false;            // Отключаем скрипт, если LineRenderer не найден
            return;
        }
        _lineRenderer.enabled = false;  // Изначально отключаем LineRenderer
    }


    private void OnEnable()
    {
        Slingshot2D.OnProjectileLaunched += OnProjectileLaunched;            // Подписываемся на событие запуска снаряда
        ProjectailBehaviour.OnProjectailCollision += OnProjectailCollision;  // Подписываемся на событие столкновения снаряда
    }

    private void OnDisable()
    {
        Slingshot2D.OnProjectileLaunched -= OnProjectileLaunched;            // Отписываемся от события
        ProjectailBehaviour.OnProjectailCollision -= OnProjectailCollision;  // Отписываемся от события

    }


    private void OnProjectileLaunched(Rigidbody2D projectileRb)
    {
        if (_targetRigidbody != null) // Если уже есть активный снаряд, то очищаем след
        {
            ClearTrail();
        }          

        StartTrail(projectileRb);
    }


    private void OnProjectailCollision(Rigidbody2D projectileRb)
    {
        if (_targetRigidbody == projectileRb) // Проверяем, что столкновение произошло с нашим снарядом
        {
            FinishTrail(); // Завершаем след при столкновении
        }
    }


    private void FixedUpdate()
    {
        if (_targetRigidbody == null || _targetRigidbody.gameObject == null) // Если снаряда нет, то выходим
        {
            return;
        }

        AddPoint(_targetRigidbody.position); // Добавляем текущую позицию снаряда в след
    }




    private void StartTrail(Rigidbody2D projectileRb) // Метод для начала отслеживания снаряда
    {
        _targetRigidbody = projectileRb; // Сохраняем ссылку на Rigidbody2D снаряда
        if (_targetRigidbody == null)    // Проверочка на null
        {
            Debug.LogError("ProjectileTrail: Received null Rigidbody2D in StartTraking");
            return;
        }
        _lineRenderer.enabled = true;        // Включаем LineRenderer для отображения следа
        AddPoint(_targetRigidbody.position); // Добавляем первую точку в след
    }


    private void AddPoint(Vector2 position) // Метод для добавления новой точки в след
    {
        Vector3 worldPos = new Vector3(position.x, position.y, transform.position.z);

        if (_points.Count > 0 && Vector3.Distance(worldPos, _points[^1]) < _minDistance)
        {
            return; // Если новая точка слишком близко к последней, не добавляем её
        }

        _points.Add(worldPos); // Добавляем новую точку в список

        _lineRenderer.positionCount = _points.Count;            // Обновляем количество точек в LineRenderer
        _lineRenderer.SetPosition(_points.Count - 1, worldPos); // Устанавливаем позицию новой точки в LineRenderer
    }


    private void FinishTrail()
    {
        enabled = false; // Отключаем скрипт, чтобы прекратить обновление следа        
    }


    private void ClearTrail()
    {
        _points.Clear();                  // Очищаем список точек
        _lineRenderer.positionCount = 0;  // Сбрасываем количество точек в LineRenderer
        _lineRenderer.enabled = false;    // Отключаем LineRenderer
        _targetRigidbody = null;          // Сбрасываем ссылку на Rigidbody2D
    }   

}
