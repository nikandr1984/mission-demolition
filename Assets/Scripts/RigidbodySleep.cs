using UnityEngine;

public class RigidbodySleep : MonoBehaviour
{
    
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>(); // Получаем компонент Rigidbody

        if (rb != null)
        {
            rb.Sleep(); // Переводим Rigidbody в состояние сна
        }
    }    
}
