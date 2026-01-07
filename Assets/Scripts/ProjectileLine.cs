using UnityEngine;
using System.Collections.Generic;

public class ProjectileLine : MonoBehaviour
{
    public static ProjectileLine Instance; // Синглтон 

    [Header("Set in Inspector")]
    [SerializeField] private float _minDist = 0.1f;    // Минимальное расстояние между точками

    private LineRenderer  _line;    // Ссылка на компонент LineRenderer
    private GameObject    _pointOfInterest;     // Ссылка на объект интереса
    private List<Vector3> _points;  // Список точек линии



    // Свойство для доступа к объекту интереса
    public GameObject PointOfInterest                       
    {
        get 
        { 
            return _pointOfInterest;     // Возвращаем ссылку на объект интереса
        }
        
        set
        {
            _pointOfInterest = value;    // Устанавливаем ссылку на объект интереса

            if (_pointOfInterest != null) 
            {
                _line.enabled = false;          // Отключаем LineRenderer
                _points = new List<Vector3>();  // Сброс списка точек
                AddPoint();                     // Добавляем первую точку
            }
        }
    }


    public Vector3 LastPoint                    // Свойство, возвращающее местоположение последней добавленной точки
    {
        get
        {
            if (_points == null)                // Если точек нет, вернуть нулевой вектор
            {
                return (Vector3.zero);
            }
            return (_points[_points.Count - 1]); // Возвращаем последнюю точку в списке
        }
    }


    private void Awake()
    {
        Instance = this;                      // Инициализация синглтона
        _line = GetComponent<LineRenderer>(); // Получаем компонент LineRenderer
        _line.enabled = false;                // Отключаем LineRenderer пока не понадобится
        _points = new List<Vector3>();        // Инициализация списка точек
    }


    private void FixedUpdate()
    {
        if (PointOfInterest == null) // Если объект интереса не задан...
        {
            if (FollowCam.Instance?.TrackingObject != null) // ...попытаться получить его из камеры слежения...
            {
                if (FollowCam.Instance.TrackingObject.CompareTag("Projectile")) // ...если это снаряд...
                {
                    PointOfInterest = FollowCam.Instance.TrackingObject; // ...установить его как объект интереса
                }
                else
                {
                     return; // ...иначе выйти
                }
            }
            else
            {
                return; // Выйти, если нет объекта интереса
            }
        }

        // Если объект интереса найден, добавить точку с его координатами в каждом FixedUpdate
        AddPoint();

        if (FollowCam.Instance?.TrackingObject == null)
        {
            PointOfInterest = null; // Сбросить объект интереса, если камера слежения больше не имеет его
        }
    }



    // Метод для очистки линии
    public void Clear() 
    {
        _pointOfInterest = null;
        _line.enabled = false;
        _points = new List<Vector3>();
    }


    // Метод для добавления точки к линии
    public void AddPoint() 
    {
        Vector3 pt = _pointOfInterest.transform.position;  // Получаем позицию объекта интереса

        
        if (_points.Count > 0 && (pt - LastPoint).magnitude < _minDist) // Если точка недостаточно далека от предыдущей, то выйти
        {
            return;
        }

        if (_points.Count == 0) // Если это точка запуска...
        {
            // ...добавляем дополнительный фрагмент линиии
            Vector3 launchPosDiff = pt - Slingshot.LaunchPos; // Вычисляем смещение от позиции запуска
            _points.Add(pt + launchPosDiff);                  // Добавляем первую точку с учётом смещения
            _points.Add(pt);                                  // Добавляем вторую точку (текущую позицию)
            _line.positionCount = 2;                          // Устанавливаем количество точек в LineRenderer равным 2

            // Устанавливаем первые две точки линии
            _line.SetPosition(0, _points[0]);
            _line.SetPosition(1, _points[1]);

            // Включаем LineRenderer
            _line.enabled = true;
        }
        else // Если бычные точки
        {
            _points.Add(pt);                                 // Добавляем точку в список
            _line.positionCount = _points.Count;             // Обновляем количество точек в LineRenderer
            _line.SetPosition(_points.Count - 1, pt);        // Устанавливаем позицию новой точки
            _line.enabled = true;                            // Включаем LineRenderer
        }
    }
}