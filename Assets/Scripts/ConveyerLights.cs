using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConveyerLights : MonoBehaviour
{
    [SerializeField] private Material materialRed;
    [SerializeField] private Material materialRed2;
    [SerializeField] private float blinkInterval = 0.3f;

    private Coroutine blinkCoroutine;

    public float BlinkInterval { get => blinkInterval; set => blinkInterval = value; }

    public void OnToggleLightsStarted()
    {
        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkMaterials());
        }
    }

    public void OnToggleLightsCanceled()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            TurnOffEmission(materialRed);
            TurnOffEmission(materialRed2);
        }
    }

    private IEnumerator BlinkMaterials()
    {
        while (true)
        {
            ToggleEmission(materialRed, true);
            yield return new WaitForSeconds(blinkInterval);

            ToggleEmission(materialRed, false);
            ToggleEmission(materialRed2, true);

            yield return new WaitForSeconds(blinkInterval);
            ToggleEmission(materialRed2, false);
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
