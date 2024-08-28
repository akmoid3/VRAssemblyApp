using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectricScrewDriver : BaseScrewDriver
{
    private AudioSource audioSource;
    private bool isPlayingSound = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.pitch = 1f; // Set initial pitch value
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                RotateScrewDriver();

                if (firstInteractorSelecting is XRBaseControllerInteractor interactor)
                {
                    float triggerValue = interactor.xrController.activateInteractionState.value;

                    // Adjust the audio volume and pitch based on the trigger value
                    if (audioSource != null)
                    {
                        if (triggerValue > 0f)
                        {
                            if (!isPlayingSound)
                            {
                                audioSource.Play();
                                isPlayingSound = true;
                            }

                            audioSource.volume = Mathf.Clamp(triggerValue, 0f, 1f); // Fade in based on trigger value

                            // Adjust pitch based on trigger value
                            audioSource.pitch = Mathf.Lerp(1f, 3f, triggerValue); // Adjust the range as needed
                        }
                        else if (triggerValue == 0f && isPlayingSound)
                        {
                            StopScrewDriverSound();
                        }
                    }
                }
            }
            else
            {
                StopScrewDriverSound();
            }
        }
    }

    public override void RotateScrewDriver()
    {
        if (firstInteractorSelecting is XRBaseControllerInteractor interactor)
        {
            InteractionState activateState = interactor.xrController.activateInteractionState;
            currentRotationSpeed = activateState.value * SpeedMultiplier;
            screwDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime * -1.0f);
        }
    }

    private void StopScrewDriverSound()
    {
        if (isPlayingSound && audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
            audioSource.pitch = 1f; // Reset pitch
            isPlayingSound = false;
        }
    }
}
