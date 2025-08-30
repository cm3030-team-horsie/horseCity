using UnityEngine;

public class AppleCollision : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        // compares tag
        if (other.CompareTag("Horse"))
        {
            Debug.Log("Horse collected an apple");
            EventManager.RaiseAppleCollected();
            Destroy(gameObject);
        }
    }
}
