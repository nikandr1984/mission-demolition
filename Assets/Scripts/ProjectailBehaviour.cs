using UnityEngine;

public class ProjectailBehaviour : MonoBehaviour
{
    public static event System.Action<Rigidbody2D> OnProjectailCollision;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnProjectailCollision?.Invoke(GetComponent<Rigidbody2D>());
        Debug.Log("ProjectileBehaviour: Collision detected with " + collision.gameObject.name);
    }
}
