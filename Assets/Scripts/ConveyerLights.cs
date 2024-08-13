using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConveyerLights : MonoBehaviour
{
    [SerializeField] private Material materialRed;
    [SerializeField] private Material materialGreen;
    [SerializeField] private float blinkInterval = 0.3f;

    private Coroutine blinkCoroutine;

    [SerializeField] private InputActionReference toggleLightsInputReference;

    private void OnEnable()
    {
        toggleLightsInputReference.action.started += OnToggleLightsStarted;
        toggleLightsInputReference.action.canceled += OnToggleLightsCanceled;
        toggleLightsInputReference.action.Enable();
    }

    private void OnDisable()
    {
        toggleLightsInputReference.action.started -= OnToggleLightsStarted;
        toggleLightsInputReference.action.canceled -= OnToggleLightsCanceled;
        toggleLightsInputReference.action.Disable();
    }

    private void OnToggleLightsStarted(InputAction.CallbackContext context)
    {
        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkMaterials());
        }
    }

    private void OnToggleLightsCanceled(InputAction.CallbackContext context)
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            TurnOffEmission(materialRed);
            TurnOffEmission(materialGreen);
        }
    }

    private IEnumerator BlinkMaterials()
    {
        while (true)
        {
            ToggleEmission(materialRed, true);
            yield return new WaitForSeconds(blinkInterval);

            ToggleEmission(materialRed, false);
            ToggleEmission(materialGreen, true);

            yield return new WaitForSeconds(blinkInterval);
            ToggleEmission(materialGreen, false);
        }
    }

    private void ToggleEmission(Material material, bool state)
    {
        if (state)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.red); 
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }

    private void TurnOffEmission(Material material)
    {
        material.DisableKeyword("_EMISSION");
    }
}
