using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectricScrewDriver : BaseScrewDriver
{
    private bool isPlayingSound = false;
    private float triggerValue;
    private AudioSource audioSource;

    public bool IsPlayingSound { get => isPlayingSound; set => isPlayingSound = value; }
    public float TriggerValue { get => triggerValue; set => triggerValue = value; }

    private void Start()
    {
        // Ensure AudioManager is initialized
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is not found!");
        }

        // Reference the existing AudioSource or add one if missing
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
        if (triggerValue > 0f)
        {
            if (!isPlayingSound)
            {
                AudioManager.Instance.PlaySound(audioSource, "Drill", true, Mathf.Clamp(triggerValue, 0f, 1f));
                isPlayingSound = true;
            }

            AudioManager.Instance.SetVolume(audioSource, Mathf.Lerp(0f, 1f, triggerValue));

            // Adjust pitch based on trigger value
            AudioManager.Instance.SetPitch(audioSource, Mathf.Lerp(1f, 3f, triggerValue));
        }
        else if (triggerValue == 0f && isPlayingSound)
        {
            StopScrewDriverSound();
        }
    }

    public override void RotateScrewDriver()
    {
        currentRotationSpeed = TriggerValue * SpeedMultiplier;
        ScrewDriver.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime * -1.0f);
    }

    public void StopScrewDriverSound()
    {
        if (isPlayingSound)
        {
            AudioManager.Instance.StopSound(audioSource);
            isPlayingSound = false;
        }
    }
}
