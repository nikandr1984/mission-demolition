using UnityEngine;

public class CloudCrafter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public int numClouds = 40;                                // ���������� �������
    public GameObject cloudPrefab;                            // ������ ��� �������
    public Vector3 cloudPosMin = new Vector3(-50, -5, 10);    // ����������� ������� �������
    public Vector3 cloudPosMax = new Vector3(150, 100, 10);   // ������������ ������� �������
    public float cloudScaleMin = 1f;                          // ����������� ������� ������� ������
    public float cloudScaleMax = 3f;                          // ������������ ������� ������� ������
    public float cloudSpeedMult = 0.5f;                       // ��������� �������� �������

    private GameObject[] cloudInstatces;  // ������ ��� �������� �������

    private void Awake()  // ����� ������� ��� ������ � ��������� �� �� �����
    {
        // ������� ������ ��� �������� ���� ����������� �������
        cloudInstatces = new GameObject[numClouds];

        // ������� ������������ ������� ������ CloudAnchor
        GameObject anchor = GameObject.Find("CloudAnchor");

        // ������� � ����� �������� ���������� �������
        GameObject cloud;
        for (int i = 0; i < numClouds; i++)
        {
            // ������� ��������� ������ �� �������
            cloud = Instantiate(cloudPrefab);

            // �������� ��������� �������������� ��� ������
            Vector3 cPos = Vector3.zero;
            cPos.x = Random.Range(cloudPosMin.x, cloudPosMax.x);
            cPos.y = Random.Range(cloudPosMin.y, cloudPosMax.y);

            // ������������ ������ ��������� �������
            float scaleU = Random.value;
            float scaleVal = Mathf.Lerp(cloudScaleMin, cloudScaleMax, scaleU);

            // ������ ������������ ������ ������� �� ��� ��������
            cPos.y = Mathf.Lerp(cloudPosMin.y, cPos.y, scaleU); 

            // ������� ������ ������ ���� ������ � �����
            cPos.z = 100 - 90 * scaleU; 
            
            // ��������� ���������� �������� ��������� � �������� � ������
            cloud.transform.position = cPos;
            cloud.transform.localScale = Vector3.one * scaleVal;

            // ������� ������ �������� �� ��������� � ������� CloudAnchor
            cloud.transform.SetParent(anchor.transform);

            // ��������� ������ � ������ cloudInstatces
            cloudInstatces[i] = cloud;
        }
    }

    void Update()   // ����� ������� ������ ������ ���� ����� � ������ �����
    {
        // ������� ��� ������ � �������
        foreach (GameObject cloud in cloudInstatces)
        {
            // ������� ������� � ���������� ������
            float scaleVal = cloud.transform.localScale.x;
            Vector3 cPos = cloud.transform.position;

            // ����������� �������� ��� ������� �������
            cPos.x -= scaleVal * Time.deltaTime * cloudSpeedMult;

            // ���� ������ ����� �� ����� �������, ���������� ��� ������ (������ ������������ ��������)
            if (cPos.x <= cloudPosMin.x)
            {
                cPos.x = cloudPosMax.x;                
            }

            // ��������� ����������� ���������� � ������
            cloud.transform.position = cPos;
        }                
    }
}
