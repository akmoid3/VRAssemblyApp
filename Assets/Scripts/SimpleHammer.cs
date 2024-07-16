using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleHammer : BaseHammer
{
    private Vector3 lastPosition;
    private Vector3 impactDirection;

    void Start()
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

    private void CalculateImpactForce()
    {
        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - lastPosition;

        // Calculate the speed of the hammer's movement
        float movementSpeed = deltaPosition.magnitude / Time.deltaTime;

        // Determine the impact force based on movement speed and force multiplier
        currentImpactForce = movementSpeed * forceMultiplier;

        // Calculate the direction of the impact force
        impactDirection = deltaPosition.normalized;

        lastPosition = currentPosition;
    }

    public Vector3 GetImpactDirection()
    {
        return impactDirection;
    }
}
