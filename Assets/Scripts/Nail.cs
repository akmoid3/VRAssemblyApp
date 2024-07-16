using UnityEngine;

public class Nail : Fastener
{
    private SimpleHammer hammerScript; // Assuming SimpleHammer or BaseHammer is used
    [SerializeField] private float forceScalingFactor = 1f;
    [SerializeField] private float minimumImpactForce = 10f;

    protected override void HandleInteraction()
    {
        if (hammerScript != null && isAligned && !isStopped)
        {
            // Get the impact force and direction from the hammer script
            float hammerForce = hammerScript.GetImpactForce() * forceScalingFactor;
            Vector3 impactDirection = hammerScript.GetImpactDirection();

            // Check if the impact force meets the minimum required force
            if (hammerForce >= minimumImpactForce)
            {
                // Calculate potential movement of the nail
                float potentialMovement = hammerForce * Time.deltaTime;

                // Calculate the current distance traveled
                float currentDistance = Vector3.Distance(transform.localPosition, initialZPosition);

                // Calculate the remaining distance to travel
                float remainingDistance = distanceToTravel - currentDistance;

                // Ensure the nail doesn't move beyond the distanceToTravel
                float actualMovement = Mathf.Min(potentialMovement, remainingDistance);

                
                // Check if the nail has reached or exceeded the distanceToTravel
                if (currentDistance + actualMovement >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = alignedColor;
                }
                // Check if the impact direction matches the alignment
                if (Vector3.Dot(impactDirection, transform.forward) >= 0.8f)
                {
                    // Move the nail
                    transform.Translate(Vector3.forward * actualMovement);

                    // Direction is correct
                    Debug.Log("Correct direction!");
                }
                else
                {
                    // Direction is incorrect
                    Debug.Log("Incorrect direction!");
                }
            }
        }
    }

    protected override void OnToolCollisionEnter(Collider other)
    {
        hammerScript = other.GetComponentInParent<SimpleHammer>(); // Adjust as per your hierarchy
    }

    protected override void OnToolCollisionExit(Collider other)
    {
        hammerScript = null;
    }
}
