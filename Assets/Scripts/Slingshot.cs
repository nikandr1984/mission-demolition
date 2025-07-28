using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// ����� ���������� �������� � �������� ������������ � ������� ��������
public class Slingshot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // ��������� ������� (�������� � ����������)
    [Header("Set in Inspector")]
    public GameObject prefabProjectile;  // ������ ������� ��� �������
    public float velocityMult = 8f;      // ��������� �������� �������

    // ������������ ��������� (��������������� �� ����� ����)
    [Header("Set Dynamically")]
    public GameObject launchPoint;   // ������-�����, ������ �������� ������
    public Vector3 launchPos;        // ���������� ����� � ����
    public GameObject projectile;    // ��������� ������
    public bool aimingMode = false;  // ������ ������������ (����/���)

    private Rigidbody projectileRigidbody;  // ������ �������
    private Mouse currentMouse;             // ���������� �����
    private Camera mainCam;                 // ������� ������ (��� ����)

   
    // ��������� ���������
    void Awake()
    {
        // ������� � ����������� ����� �������
        Transform launchPointTrans = transform.Find("LaunchPoint"); 
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);                     // �������� �����
        launchPos = launchPointTrans.position;            // ���������� �������
                                                
        currentMouse = Mouse.current; // ���������� ����
        mainCam = Camera.main;        // �������� ������
    }
    
    
    // ���� ������� ���������� ����
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos2D = currentMouse.position.ReadValue();
        mousePos2D.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(mousePos2D);
    }

    
    // ��� ��������� ������� �� �������
    public void OnPointerEnter(PointerEventData eventData)
    {        
        launchPoint.SetActive(true);  // ���������� ����� �������   
    }

    // ��� ������ ������� �� ������� �������
    public void OnPointerExit(PointerEventData eventData)
    {        
        launchPoint.SetActive(false);  // �������� ����� �������
    }

    // ��� ������� ����� ������ ���� �� �������
    public void OnPointerDown(PointerEventData eventData)
    {
        // ��������� ������ �� ����� ������ ����
        if (eventData.button == PointerEventData.InputButton.Left)
        {            
            aimingMode = true;  // ���������� ����� ������������

            projectile = Instantiate(prefabProjectile); // ������� ����� ������
            projectile.transform.position = launchPos;  // ������ ��� �� �����

            // ��������� ������ ������� (����� �� �����)
            projectileRigidbody = projectile.GetComponent<Rigidbody>();
            projectileRigidbody.isKinematic = true; 
        }         
    }

    
    // ��� ���������� ����� ������ ����
    public void OnPointerUp(PointerEventData eventData)
    {
        // ���� ���� � ������ ������������
        if (aimingMode)
        {
            LaunchProjectile(); // ��������� ������
        }
            
    }

    // �������� ������� ����
    public void Update()
    {
        // ���� �� � ������ ������������ - �������
        if (!aimingMode) return;
                       
        // �������� ������� ������� ���� � ������� �����������
        Vector3 mousePos3D = GetMouseWorldPos();

        // �������, ��������� ������ �������� ������
        Vector3 mouseDelta = mousePos3D - launchPos;
        
        // ������������ ������������ ���������� ����������� �������� ����������
        float maxMagnitude = GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // ���������� ������ �������� ������� ����
        projectile.transform.position = launchPos + mouseDelta;
        
    }

    // ����� ������� �������
    private void LaunchProjectile()
    {
        // ������� �� ������ ������������
        aimingMode = false;

        // �������� ��������� ������� ���� � ������� �����������        
        Vector3 mousePos3D = GetMouseWorldPos();

        // ������������ ������ ���� (�� ���� � ����� �������)
        Vector3 launchVector = launchPos - mousePos3D;

        // ��������� ������ �������
        projectileRigidbody.isKinematic = false;
        projectileRigidbody.linearVelocity = launchVector * velocityMult;

        // ������ ������ �� ��������
        FollowCam.POI = projectile;
        
        // ���������� ������ �� ������ (�� ������ ����������� �������)
        projectile = null;            
    }
}
