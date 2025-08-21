using UnityEngine;

public class CarCollision : MonoBehaviour
{
    [Header("Slowdown Settings")]
    public float slowDuration = 2f;     // seconds slowed
    public float slowMultiplier = 0.5f; // horse runs at 50% speed

    public AudioClip carHitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Horse"))
        {
            Debug.Log("Horse collided with a car!");

            // play sound if assigned
            if (carHitSound != null)
                AudioSource.PlayClipAtPoint(carHitSound, transform.position);

            // get the horse’s spline traveler
            var splineTraveler = other.GetComponent<SplineTraveler>();
            if (splineTraveler != null)
            {
                splineTraveler.StartCoroutine(SlowHorse(splineTraveler));
            }

            // remove the car
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator SlowHorse(SplineTraveler splineTraveler)
    {
        float originalSpeed = splineTraveler.TravelSpeed;
        splineTraveler.TravelSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        splineTraveler.TravelSpeed = originalSpeed;
    }
}