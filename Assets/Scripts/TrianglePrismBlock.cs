using UnityEngine;


// Этот скрипт требует наличие компонентов SpriteRenderer, MeshCollider и Rigidbody
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]


public class TrianglePrismBlock : MonoBehaviour
{
    public TriangleType triangleType = TriangleType.Isosceles; // Тип треугольника (по умолчанию равнобедренный)

    [Tooltip("Width base of triangle")]
    [SerializeField] private float baseWidth = 1f;

    [Tooltip("Height of triangle")]
    [SerializeField] private float height = 1f;

    [Tooltip("Depth of prism")]
    [SerializeField] private float depth = 1f;

    private MeshCollider _meshCollider;

    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>(); // Кэшируем MeshCollider


        Mesh collisionMesh = CreatePrismMesh();      // Генерируем меш в зависимости от типа треугольника

        _meshCollider.sharedMesh = collisionMesh;    // Присваиваем сгенерированный меш MeshCollider
        
        _meshCollider.convex = true;                 // Делаем меш выпуклым для корректной работы с Rigidbody


        // Улучшаем плавность движения
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; 

    }

    // Метод создания меша призмы в зависимости от типа треугольника
    private Mesh CreatePrismMesh()
    {
        switch (triangleType)
        {
            case TriangleType.Isosceles:
                return CreateIsoscelesPrism(baseWidth, height, depth);
            case TriangleType.Right:
                return CreateRightTrianglePrism(baseWidth, height, depth);
            default:
                return CreateIsoscelesPrism(baseWidth, height, depth);
        }
    }


    // Равнобедренный треугольник: основание по центру, вершина сверху
    private Mesh CreateIsoscelesPrism(float w, float h, float d)
    {        
        Vector3[] vertices = {
            new Vector3(-w/2, -h/2, -d/2),   // А - точка основания слева (front)
            new Vector3(w/2, -h/2, -d/2),    // B - точка основания справа (front)
            new Vector3(0, h/2, -d/2),       // C - вершина треугольника (front)
            new Vector3(-w/2, -h/2, d/2),    // A1 - точка основания слева (back)
            new Vector3(w/2, -h/2, d/2),     // B1 - точка основания справа (back)
            new Vector3(0, h/2, d/2)         // C1 - вершина треугольника (back)
        };

        int[] triangles = {
            // Передняя грань
            0, 1, 2,
            // Задняя грань
            3, 5, 4,

            // Нижняя грань
            0, 3, 1, 1, 3, 4,
            // Левая грань
            0, 2, 3, 3, 2, 5,
            // Правая грань
            1, 4, 2, 2, 4, 5
        };

        return BuildMesh(vertices, triangles);
    }


    // Прямоугольный треугольник: прямой угол внизу слева
    private Mesh CreateRightTrianglePrism (float w, float h, float d)
    {
        Vector3[] vertices = {
            new Vector3(-w/2, -h/2, -d/2),    // A - точка прямого угла (front)
            new Vector3(w/2, -h/2, -d/2),     // B - точка по основанию (front)
            new Vector3(-w/2, h/2, -d/2),     // C - точка по высоте (front)
            new Vector3(-w/2, -h/2, d/2),     // A1 - точка прямого угла (back)
            new Vector3(w/2, -h/2, d/2),      // B1 - точка по основанию (back)
            new Vector3(-w/2, h/2, d/2)       // C1 - точка по высоте (back)
        };

        int[] triangles = {
            // Перед
            0, 1, 2,
            // Зад
            3, 5, 4,

            // Низ
            0, 3, 1, 1, 3, 4,
            // Вертикаль (Y-Z плоскость)
            0, 2, 3, 3, 2, 5,
            // Гипотенуза
        };

        return BuildMesh(vertices, triangles);
    }

    // Вспомогательный метод, чтобы не дублировать код создания меша
    private Mesh BuildMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


    // Освобождение памяти при уничтожении объекта
    private void OnDestroy()
    {
        if (_meshCollider != null && _meshCollider.sharedMesh != null)
        {
            Destroy(_meshCollider.sharedMesh); // Освобождаем память от созданного меша
        }

        Debug.Log("TrianglePrismBlock: Triangle block destroyed — mesh cleaned up", this);
    }
}

// Перечисление для типов треугольников
public enum TriangleType
{
    Isosceles,
    Right
}
