using UnityEngine;

public class Nail : Fastener
{
    private SimpleHammer hammerScript;
    [SerializeField] private float forceScalingFactor = 0.5f;
    [SerializeField] private float minimumImpactForce = 2f;

    [SerializeField] private float moveCooldown = 0.5f; // Cooldown duration in seconds

    private float lastMoveTime = 0f;


    protected override void HandleInteraction()
    {
        // Check if cooldown period has elapsed
        if (Time.time - lastMoveTime < moveCooldown)
            return;


        if ((hammerScript != null && isAligned) || (hammerScript != null && socketTransform))
        {
            float hammerForce = hammerScript.GetImpactForce() * forceScalingFactor;
            Vector3 impactDirection = hammerScript.GetImpactDirection();

            if (hammerForce >= minimumImpactForce)
            {

                float potentialMovement = hammerForce * Time.fixedDeltaTime * forceScalingFactor;
                float currentDistance;
                if (socketTransform)
                {
                    currentDistance = Vector3.Distance(socketTransform.localPosition, initialSocketPosition);
                }
                else
                {
                    currentDistance = Vector3.Distance(transform.localPosition, initialZPosition);
                }


                float remainingDistance = distanceToTravel - currentDistance;

                float actualMovement = potentialMovement;



                float direction = Vector3.Dot(impactDirection, transform.forward);

                // Check if the impact direction matches the alignment
                if (direction >= 0.8f)
                {
                    // Ensure the nail doesn't move beyond the distanceToTravel
                    if (potentialMovement > remainingDistance)
                    {
                        actualMovement = Mathf.Min(potentialMovement, remainingDistance);
                        isStopped = true;
                    }
                    if(socketTransform)
                        socketTransform.Translate(Vector3.forward * actualMovement);
                    else
                        transform.Translate(Vector3.forward * actualMovement);

                    // Check if the nail has reached or exceeded the distanceToTravel
                    if (currentDistance + actualMovement >= distanceToTravel)
                    {
                        isStopped = true;
                        fastenerRenderer.material.color = defaultColor;
                    }

                    lastMoveTime = Time.time;

                    Debug.Log("Correct direction!" + direction + " remaining " + remainingDistance);
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
