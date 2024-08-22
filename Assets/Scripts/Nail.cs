using UnityEngine;

public class Nail : Fastener
{
    private SimpleHammer hammerScript;
    [SerializeField] private float forceScalingFactor = 0.5f;
    [SerializeField] private float minimumImpactForce = 2f;

    [SerializeField] private float moveCooldown = 0.5f;

    private float lastMoveTime = 0f;

    protected override void HandleInteraction()
    {
        // Check if cooldown period has elapsed
        if (Time.time - lastMoveTime < moveCooldown)
            return;

        if (StateManager.Instance.CurrentState == State.PlayBack)
        {
            if (socketTransform != null)
                HandleSocketTransformInteraction();
        }
        else if (hammerScript != null && isAligned)
        {
            HandleNormalInteraction();
        }
    }

    private void HandleSocketTransformInteraction()
    {
        float hammerForce = hammerScript.GetImpactForce() * forceScalingFactor;
        Vector3 impactDirection = hammerScript.GetImpactDirection();

        if (hammerForce >= minimumImpactForce)
        {

            float potentialMovement = hammerForce * Time.fixedDeltaTime * forceScalingFactor;
            float currentDistance = Vector3.Distance(socketTransform.localPosition, initialSocketPosition);
            float remainingDistance = distanceToTravel - currentDistance;

            float direction = Vector3.Dot(impactDirection, transform.forward);

            // Check if the impact direction matches the alignment
            if (direction >= 0.8f)
            {
                float actualMovement = Mathf.Min(potentialMovement, remainingDistance);

                // Move the socketTransform based on the impact
                socketTransform.Translate(Vector3.forward * actualMovement);

                hammerScript.PlayHammerSound();

                // Check if the nail has reached or exceeded the distanceToTravel
                if (currentDistance + actualMovement >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                }

                lastMoveTime = Time.time;
            }
           
        }
    }

    private void HandleNormalInteraction()
    {
        float hammerForce = hammerScript.GetImpactForce() * forceScalingFactor;
        Vector3 impactDirection = hammerScript.GetImpactDirection();

        if (hammerForce >= minimumImpactForce)
        {


            float potentialMovement = hammerForce * Time.fixedDeltaTime * forceScalingFactor;
            float currentDistance = Vector3.Distance(transform.localPosition, initialZPosition);
            float remainingDistance = distanceToTravel - currentDistance;

            float direction = Vector3.Dot(impactDirection, transform.forward);

            // Check if the impact direction matches the alignment
            if (direction >= 0.8f)
            {
                float actualMovement = Mathf.Min(potentialMovement, remainingDistance);

                // Move the transform based on the impact
                transform.Translate(Vector3.forward * actualMovement);

                hammerScript.PlayHammerSound();

                // Check if the nail has reached or exceeded the distanceToTravel
                if (currentDistance + actualMovement >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                }

                lastMoveTime = Time.time;
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
