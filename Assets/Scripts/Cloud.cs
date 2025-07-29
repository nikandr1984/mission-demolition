using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Cloud : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject cloudSphere;
    public int numSpheresMin = 6;
    public int numSpheresMax = 10;
    public Vector3 spherOffsetScale = new Vector3(5, 2, 1);
    public Vector2 sphereScaleRangeX = new Vector2(4, 8);
    public Vector2 sphereScaleRangeY = new Vector2(3, 4);
    public Vector2 sphereScaleRangeZ = new Vector2(2, 4);
    public float scaleYMin = 2f;

    private List<GameObject> spheres;
    
    private void Start()
    {
        spheres = new List<GameObject>();

        int num = Random.Range(numSpheresMin, numSpheresMax);

        for (int i=0;  i<num; i++)
        {
            GameObject sp = Instantiate (cloudSphere);
            spheres.Add(sp);
            Transform spTrans = sp.transform;
            spTrans.SetParent(transform);

            // Выбрать случайное местоположение
            Vector3 offset = Random.insideUnitSphere;
            offset.x *= spherOffsetScale.x;
            offset.y *= spherOffsetScale.y;
            offset.z *= spherOffsetScale.z;
            spTrans.localPosition = offset;

            // Выбрать случайный масштаб
            Vector3 scale = Vector3.one;
            scale.x = Random.Range(sphereScaleRangeX.x, sphereScaleRangeX.y);
            scale.y = Random.Range(sphereScaleRangeY.x, sphereScaleRangeY.y);
            scale.z = Random.Range(sphereScaleRangeZ.x, sphereScaleRangeZ.y);

            // Скорректировать масштаб y по расстоянию x от центра
            scale.y *= 1 - (Mathf.Abs(offset.x) / spherOffsetScale.x);
            scale.y = Mathf.Max(scale.y, scaleYMin);

            spTrans.localScale = scale;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Restart();
        }

        void Restart()
        {
            // Удалить старые сферы, составляющие облако
            foreach (GameObject sp in spheres)
            {
                Destroy(sp);
            }
            Start();
        }
    }
}
