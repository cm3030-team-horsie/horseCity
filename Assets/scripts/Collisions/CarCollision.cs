using UnityEngine;

public class CarCollision : MonoBehaviour
{
    [Header("Slowdown Settings")]
    public float slowDuration = 2f;     // how long horse must slow down for after hit
    public float slowMultiplier = 0.5f; // 50% slower speed

    public AudioClip carHitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Horse"))
        {
            Debug.Log("Horse collided with a car!");

            // sound when hit
            if (carHitSound != null)
                AudioSource.PlayClipAtPoint(carHitSound, transform.position);

            // get the horse’s spline traveler
            var splineTraveler = other.GetComponent<SplineTraveler>();
            if (splineTraveler != null)
            {
                splineTraveler.StartCoroutine(SlowHorse(splineTraveler));
            }

            // destorys car on impact
            Destroy(gameObject);
        }
    }

    // slows the horse down
    private System.Collections.IEnumerator SlowHorse(SplineTraveler splineTraveler)
    {
        float originalSpeed = splineTraveler.TravelSpeed;
        splineTraveler.TravelSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        splineTraveler.TravelSpeed = originalSpeed;
    }
}