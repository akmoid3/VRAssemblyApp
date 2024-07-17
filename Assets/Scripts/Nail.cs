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
            Debug.Log(hammerScript.GetImpactForce());
            float hammerForce = hammerScript.GetImpactForce() * forceScalingFactor;
            Vector3 impactDirection = hammerScript.GetImpactDirection();

            if (hammerForce >= minimumImpactForce)
            {
                float potentialMovement = hammerForce * Time.deltaTime;

                float currentDistance = Vector3.Distance(transform.localPosition, initialZPosition);

                float remainingDistance = distanceToTravel - currentDistance;

                // Ensure the nail doesn't move beyond the distanceToTravel
                float actualMovement = Mathf.Min(potentialMovement, remainingDistance);


                float direction = Vector3.Dot(impactDirection, transform.forward);

                // Check if the impact direction matches the alignment
                if (direction >= 0.9f)
                {
                    transform.Translate(Vector3.forward * actualMovement);

                    // Check if the nail has reached or exceeded the distanceToTravel
                    if (currentDistance + actualMovement >= distanceToTravel)
                    {
                        isStopped = true;
                        fastenerRenderer.material.color = alignedColor;
                    }

                    Debug.Log("Correct direction!" + direction);
                }
                else
                {
                    Debug.Log("Incorrect direction!" + direction);
                }
            }
        }
    }

    protected override void OnToolCollisionEnter(Collider other)
    {
        hammerScript = other.GetComponentInParent<SimpleHammer>();
    }

    protected override void OnToolCollisionExit(Collider other)
    {
        hammerScript = null;
    }
}
