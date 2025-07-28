using UnityEngine;

public class FollowCam : MonoBehaviour
{
    static public GameObject POI;  // "Point of interest" - c����� �� ������������ ������

    [Header("Set in Inspector")]
    public float easing = 0.05f;
    public Vector2 minXY = Vector2.zero;
    
    [Header("Set Dynamically")]
    public float camZ;            // �������� ���������� Z ������ (�������)

    private void Awake()
    {
        camZ = transform.position.z;
    }

    private void FixedUpdate()
    {
        if (POI == null) return; // ���� ��� ������� �������� - �����

        // �������� ������� ������� ��������
        Vector3 destination = POI.transform.position;

        // ���������� X � Y ������������ ����������
        destination.x = Mathf.Max(minXY.x, destination.x);
        destination.y = Mathf.Max(minXY.y, destination.y);

        // ������ ���������� ������ �� ������� ������� � ������� ��������
        destination = Vector3.Lerp(transform.position, destination, easing);

        // ��������� �������� Z-���������� ������ (����� ������ �� ������ �������)
        destination.z = camZ;

        // ������������� ����� ������� ������
        transform.position = destination;

        // �������� ������ ������, ����� ����� ���������� �����
        Camera.main.orthographicSize = destination.y + 10;
    }
}
