using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PolygonCollider2D))]

public class TriangleBlockCollider : MonoBehaviour
{
    public TriangleType triangleType = TriangleType.Isosceles;
    public float baseWidtf = 1f;
    public float height = 1f;

    private PolygonCollider2D _collider;


    private void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        Vector2[] points = CreateTrianglePoints();
        _collider.points = points;
    }

    private Vector2[] CreateTrianglePoints()
    {
        switch (triangleType)
        {
            case TriangleType.Isosceles:
                return CreateIsoscelesTrianglePoints(baseWidtf, height);
            case TriangleType.Right:
                return CreateRightTrianglePoints(baseWidtf, height);
            default:
                return CreateIsoscelesTrianglePoints(baseWidtf, height);

        }
    }


    // –авнобедренный треугольник: центр (0,0)
    private Vector2[] CreateIsoscelesTrianglePoints(float w, float h)
    {
        float buttom = -h / 2f;  // Y нижней границы
        float top = h / 2f;      // Y верхней границы

        return new Vector2[]
        {
            new Vector2(-w / 2f, buttom), // левый низ
            new Vector2(w / 2f, buttom),  // правый низ
            new Vector2(0f, top)          // вершина
        };
    }

    // ѕр€моугольный треугольник: пр€мой угол в (0,0), основание по ’, высота по Y
    private Vector2[] CreateRightTrianglePoints(float w, float h)
    {
        // ÷ентрируем относительно (0,0):
        // ј = (-w/2, -h/2) - пр€мой угол
        // B = (w/2, -h/2) - конец основани€
        // C = (-w/2, h/2) - вершина по высоте

        float left = -w / 2f;    // X левой границы
        float bottom = -h / 2f;  // Y нижней границы
        float right = w / 2f;    // X правой границы
        float top = h / 2f;      // Y верхней границы

        return new Vector2[]
        {
            new Vector2(left, bottom),   // A - пр€мой угол
            new Vector2(right, bottom),  // B - конец основани€
            new Vector2(left, top)       // C - вершина по высоте
        };

    }
}

public enum TriangleType
{
    Isosceles,
    Right
}
