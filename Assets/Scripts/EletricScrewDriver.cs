using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectricScrewDriver : BaseScrewDriver
{
    private AudioSource audioSource;
    private bool isPlayingSound = false;
    private float triggerValue;

    public AudioSource AudioSource { get => audioSource; set => audioSource = value; }
    public bool IsPlayingSound { get => isPlayingSound; set => isPlayingSound = value; }
    public float TriggerValue { get => triggerValue; set => triggerValue = value; }

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

                if (firstInteractorSelecting is XRBaseControllerInteractor interactor)
                {
                    TriggerValue = interactor.xrController.activateInteractionState.value;
                    ActivateAudio(TriggerValue);
                    RotateScrewDriver();
                }
            }
            else
            {
                StopScrewDriverSound();
            }
        }
    }

    public void ActivateAudio(float triggerValue)
    {
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

    public override void RotateScrewDriver()
    {
        currentRotationSpeed = TriggerValue * SpeedMultiplier;
        ScrewDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime * -1.0f);
    }

    public void StopScrewDriverSound()
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
