using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    [Header("Penalty Settings")]
    public float slowDuration = 1.5f;
    public float slowMultiplier = 0.5f;
    public AudioClip hitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Horse hit an obstacle!");

            // minus a point
            if (LivesCounter.Instance != null)
                LivesCounter.Instance.DeductLife();

            // Slow the horse
            var splineTraveler = other.GetComponent<SplineTraveler>();
            if (splineTraveler != null)
                splineTraveler.StartCoroutine(SlowHorse(splineTraveler));

            // Remove the obstacle
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