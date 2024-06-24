using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ScrewDriver : XRGrabInteractable
{
    [SerializeField] private Transform screwDriver;
    [SerializeField] private float speed = 100.0f;
    private float currentRotationSpeed;

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
                RotateScrewDriver();
        }
    }

    private void RotateScrewDriver()
    {
        if (firstInteractorSelecting is XRBaseControllerInteractor interactor)
        {
            InteractionState activateState = interactor.xrController.activateInteractionState;
            currentRotationSpeed = activateState.value * speed * -1;
            screwDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime);
        }
    }

    public float GetRotationSpeed()
    {
        return currentRotationSpeed;
    }
}
