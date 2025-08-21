using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [Header("Penalty Settings")]
    public float slowDuration = 1.5f;
    public float slowMultiplier = 0.5f;
    public AudioClip hitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Horse"))
        {
            Debug.Log("Horse hit an obstacle!");

            // play sound
            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position);

            // slow the horse
            var splineTraveler = other.GetComponent<SplineTraveler>();
            if (splineTraveler != null)
                splineTraveler.StartCoroutine(SlowHorse(splineTraveler));

            // remove the obstacle if colllides
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