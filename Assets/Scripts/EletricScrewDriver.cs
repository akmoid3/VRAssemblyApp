using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class EletricScrewDriver : BaseScrewDriver
{
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
                RotateScrewDriver();
        }
    }

    public override void RotateScrewDriver()
    {
        if (firstInteractorSelecting is XRBaseControllerInteractor interactor)
        {
            InteractionState activateState = interactor.xrController.activateInteractionState;
            currentRotationSpeed = activateState.value * speedMultiplier;
            screwDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime * -1.0f);
        }
    }
}
