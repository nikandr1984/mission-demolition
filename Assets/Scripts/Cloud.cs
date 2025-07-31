using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// ����� Cloud ������� ������, ��������� �� ���������� ���� � ���������� �����������
public class Cloud : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject cloudSphere;                            // ������ �����, �� ������� ����� �������� ������
    public int numSpheresMin = 6;                             // ����������� ���������� ���� � ������
    public int numSpheresMax = 10;                            // ������������ ���������� ���� � ������
    public Vector3 spherOffsetScale = new Vector3(5, 2, 1);   // ������� ���������� �������� ���� � ������
    public Vector2 sphereScaleRangeX = new Vector2(4, 8);     // �������� ���������� �������� ���� �� ��� X
    public Vector2 sphereScaleRangeY = new Vector2(3, 4);     // �������� ���������� �������� ���� �� ��� Y
    public Vector2 sphereScaleRangeZ = new Vector2(2, 4);     // �������� ���������� �������� ���� �� ��� Z
    public float scaleYMin = 2f;                              // ����������� ������� ����� �� ��� Y

    private List<GameObject> spheres;      // ������ ��� �������� ���� ���� ������

    private void Start()
    {
        spheres = new List<GameObject>(); // ������������� ������ ����

        // ���������� ��������� ���������� ���� � ������ (� �������� ���������)
        int num = Random.Range(numSpheresMin, numSpheresMax);

        // ������� ������ ����� � ������
        for (int i=0;  i<num; i++)
        {
            
            GameObject sp = Instantiate (cloudSphere);  // ������� ��������� ����� �� �������
            spheres.Add(sp);                            // ��������� ����� � ������
            Transform spTrans = sp.transform;           // �������� ���������� ����� ��� ��������
            spTrans.SetParent(transform);               // ������������� ������������ ������ ��� ����� (������)

            // ���������� ��������� �������� ��� ����� ������ ������   
            Vector3 offset = Random.insideUnitSphere;   // �������� ��������� ����� ������ ����� �������� 1
            // ������������ �������� �� ����
            offset.x *= spherOffsetScale.x;             // ������ ������� �� � (������ ������)
            offset.y *= spherOffsetScale.y;             // ��������� ������� �� Y (������ ������)
            offset.z *= spherOffsetScale.z;             // ��������� ������� �� Z (������� ������)
            spTrans.localPosition = offset;             // ������������� ������� ����� � ������

            // ���������� ��������� ������� ��� �����
            Vector3 scale = Vector3.one; // ��������� ������� ����� (1, 1, 1)
            // ������������� ��������� ������� ��� ������ ��� � �������� ����������
            scale.x = Random.Range(sphereScaleRangeX.x, sphereScaleRangeX.y);
            scale.y = Random.Range(sphereScaleRangeY.x, sphereScaleRangeY.y);
            scale.z = Random.Range(sphereScaleRangeZ.x, sphereScaleRangeZ.y);

            // ������������ ������� �� ��� Y � ����������� �� ���������� �� ������ �� X
            // ��� ������ �� ������, ��� ������ ������� �� Y (������ ������ ����� ������� � �����
            scale.y *= 1 - (Mathf.Abs(offset.x) / spherOffsetScale.x);
            // �����������, ����� ������� �� Y �� ��� ������ ������������ ��������
            scale.y = Mathf.Max(scale.y, scaleYMin);

            // ��������� �������� ������� � �����
            spTrans.localScale = scale;
        }
    }

    private void Update()
    {
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Restart();
        }
        */

        // ������������� ����� ��� ����������� ���������� �������� � ���������� 
        void Restart()
        {
            // ������� ������ �����, ������������ ������
            foreach (GameObject sp in spheres)
            {
                Destroy(sp);
            }
            Start();
        }
    }
}
