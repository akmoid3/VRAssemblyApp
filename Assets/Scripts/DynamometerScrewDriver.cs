using System.Collections;
using TMPro;
using UnityEngine;
using Unity.VRTemplate;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamometerScrewDriver : BaseScrewDriver
{
    private bool isPlayingSound = false;
    private float triggerValue;
    private AudioSource audioSource;
    [SerializeField] private int force = 0;
    [SerializeField] private TextMeshProUGUI forceText;
    public bool IsPlayingSound { get => isPlayingSound; set => isPlayingSound = value; }
    public float TriggerValue { get => triggerValue; set => triggerValue = value; }
    public int Force { get => force; set => force = value; }
    private Coroutine showCorrectForceCoroutine;
    public void Start()
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

    public void SetForce(XRKnob knob)
    {
        if(knob != null)
        {
            Force = Mathf.RoundToInt(Mathf.Lerp(0, 120, knob.value));
        }

        forceText.text = $"{Force}";
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
    
    public void ShowCorrectForceTemporarily(int correctForce, float duration = 2f)
    {
        // Se una coroutine era già in esecuzione, la fermiamo per evitare conflitti
        if (showCorrectForceCoroutine != null)
        {
            StopCoroutine(showCorrectForceCoroutine);
        }
        showCorrectForceCoroutine = StartCoroutine(ShowCorrectForceCoroutine(correctForce, duration));
    }

    private IEnumerator ShowCorrectForceCoroutine(int correctForce, float duration)
    {
        // Salva lo stato iniziale del testo (colore e contenuto)
        Color originalColor = forceText.color;
        string originalText = forceText.text;

        // Imposta il testo con il valore corretto e lo colore rosso
        forceText.text = correctForce.ToString();
        forceText.color = Color.red;

        // Attende per il tempo specificato
        yield return new WaitForSeconds(duration);

        // Ripristina il testo originale e il colore iniziale
        forceText.text = originalText;
        forceText.color = originalColor;
        showCorrectForceCoroutine = null;
    }
    
}
