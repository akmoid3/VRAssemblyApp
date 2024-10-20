using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleHammer : BaseHammer
{
    private Vector3 lastPosition;
    private Vector3 impactDirection;

    public void Start()
    {
        lastPosition = transform.position;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                CalculateImpactForce();
            }
        }
    }

    public void CalculateImpactForce()
    {
        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - lastPosition;

        // Calculate the speed of the hammer's movement
        float movementSpeed = deltaPosition.magnitude / Time.deltaTime;

        // Determine the impact force based on movement speed and force multiplier
        float rawImpactForce = movementSpeed * forceMultiplier;

        // Clamp the impact force to the range between minImpactForce and maxImpactForce
        currentImpactForce = Mathf.Clamp(rawImpactForce, minImpactForce, maxImpactForce);

        // Calculate the direction of the impact force
        impactDirection = deltaPosition.normalized;

        lastPosition = currentPosition;
    }


    public Vector3 GetImpactDirection()
    {
        return impactDirection;
    }

}
