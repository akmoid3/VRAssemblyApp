using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ManualScrewDriver : BaseScrewDriver
{
    private Quaternion lastRotation;

    void Start()
    {
        lastRotation = transform.rotation;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                CalculateRotationSpeed();
            }
        }
    }

    private void CalculateRotationSpeed()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(lastRotation);
        float angle;
        Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);

        if (angle > 180)
            angle -= 360;

        // Determine the direction of the rotation
        float rotationDirection = Vector3.Dot(axis, Vector3.forward);

        if (rotationDirection > 0)
        {
            // Clockwise rotation
            currentRotationSpeed = angle / Time.deltaTime * speedMultiplier * -1.0f;
        }
        else
        {
            // Counterclockwise rotation
            currentRotationSpeed = angle / Time.deltaTime * speedMultiplier;
        }

        lastRotation = currentRotation;
    }

    public override void RotateScrewDriver()
    {
        screwDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime);
    }
}
