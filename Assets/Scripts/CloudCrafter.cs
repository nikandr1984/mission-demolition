using UnityEngine;

public class CloudCrafter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public int numClouds = 40;                                // Количество облаков
    public GameObject cloudPrefab;                            // Шаблон для облаков
    public Vector3 cloudPosMin = new Vector3(-50, -5, 10);    // Минимальная позиция облаков
    public Vector3 cloudPosMax = new Vector3(150, 100, 10);   // Максимальная позиция облаков
    public float cloudScaleMin = 1f;                          // Минимальный масштаб каждого облака
    public float cloudScaleMax = 3f;                          // Максимальный масштаб каждого облака
    public float cloudSpeedMult = 0.5f;                       // Множитель скорости облаков

    private GameObject[] cloudInstatces;  // Массив для хранения облаков

    private void Awake()  // Метод создает все облака и размещает их на сцене
    {
        // Создаем массив для хранения всех экземпляров облаков
        cloudInstatces = new GameObject[numClouds];

        // Находим родительский игровой объект CloudAnchor
        GameObject anchor = GameObject.Find("CloudAnchor");

        // Создаем в цикле заданное количество облаков
        GameObject cloud;
        for (int i = 0; i < numClouds; i++)
        {
            // Создаем экземпляр облака из префаба
            cloud = Instantiate(cloudPrefab);

            // Выбираем рандомное местоположение для облака
            Vector3 cPos = Vector3.zero;
            cPos.x = Random.Range(cloudPosMin.x, cloudPosMax.x);
            cPos.y = Random.Range(cloudPosMin.y, cloudPosMax.y);

            // Масштабируем облако случайным образом
            float scaleU = Random.value;
            float scaleVal = Mathf.Lerp(cloudScaleMin, cloudScaleMax, scaleU);

            // Высота расположения облака зависит от его масштаба
            cPos.y = Mathf.Lerp(cloudPosMin.y, cPos.y, scaleU); 

            // Меньшие облака должны быть дальше в глубь
            cPos.z = 100 - 90 * scaleU; 
            
            // Применяем полученные значения координат и масштаба к облаку
            cloud.transform.position = cPos;
            cloud.transform.localScale = Vector3.one * scaleVal;

            // Сделать облако дочерним по отношению к объекту CloudAnchor
            cloud.transform.SetParent(anchor.transform);

            // Добавляем облако в массив cloudInstatces
            cloudInstatces[i] = cloud;
        }
    }

    void Update()   // Метод смещает каждое облако чуть влево в каждом кадре
    {
        // Обходим все облака в массиве
        foreach (GameObject cloud in cloudInstatces)
        {
            // Полуаем масштаб и координаты облака
            float scaleVal = cloud.transform.localScale.x;
            Vector3 cPos = cloud.transform.position;

            // Увеличиваем скорость для ближних облаков
            cPos.x -= scaleVal * Time.deltaTime * cloudSpeedMult;

            // Если облако вышло за левую границу, перемещаем его вправо (эффект бесконечного движения)
            if (cPos.x <= cloudPosMin.x)
            {
                cPos.x = cloudPosMax.x;                
            }

            // Применяем обновленные координаты к облаку
            cloud.transform.position = cPos;
        }                
    }
}
