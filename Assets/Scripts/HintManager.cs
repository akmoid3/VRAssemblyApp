using Codice.CM.Client.Differences.Graphic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private float hintCooldown = 1.0f;
    [SerializeField] private int hintCount;
    [SerializeField] private Material highlightMaterial;

    private Dictionary<int, bool> hintShownForStep = new Dictionary<int, bool>();
    private Dictionary<int, bool> secondHintShownForStep = new Dictionary<int, bool>();
    private bool isWaiting = false;
    public static event Action<int> OnHintCountChanged;
  

    public void ShowHint(List<ComponentData> assemblySequence, int currentStep, List<Transform> components, SnapToPosition interactor)
    {
        if (isWaiting)
        {
            // If we are waiting, do nothing and return immediately
            return;
        }

        var currentComponentName = assemblySequence[currentStep].componentName;

        // Check if the hint for the current step has already been shown
        if (hintShownForStep.ContainsKey(currentStep) && hintShownForStep[currentStep])
        {
            // Check if the second type of hint has been shown for this step
            if (!secondHintShownForStep.ContainsKey(currentStep) || !secondHintShownForStep[currentStep])
            {
                // Increment the hint count for the second hint and mark it as shown
                hintCount++;
                secondHintShownForStep[currentStep] = true;
                OnHintCountChanged?.Invoke(hintCount);
            }

            foreach (var component in components)
            {
                if (component.name == currentComponentName)
                {
                    StartCoroutine(HandleHintCooldown(component.gameObject));
                }
            }

            return;
        }

        // Increment the hint count and mark the hint as shown for the current step
        hintCount++;
        hintShownForStep[currentStep] = true;
        OnHintCountChanged?.Invoke(hintCount);

        if (interactor != null)
        {
            // Find the snappoint with the same name as the current component
            Transform correctSnappoint = interactor.transform.GetChild(currentStep);

            if (correctSnappoint != null)
            {
                correctSnappoint.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    private IEnumerator HandleHintCooldown(GameObject component)
    {
        isWaiting = true;

        // Start the color change sequence
        yield return StartCoroutine(ChangeColorTemporarily(component));

        yield return new WaitForSeconds(hintCooldown);


        isWaiting = false;
    }

    private IEnumerator ChangeColorTemporarily(GameObject component)
    {
        MeshRenderer meshRenderer = component.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Store the original materials
            Material[] originalMaterials = meshRenderer.materials;

            // Create a temporary array with the highlight material replacing the original
            Material[] tempMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < tempMaterials.Length; i++)
            {
                tempMaterials[i] = highlightMaterial;
            }

            // Set the highlight materials
            meshRenderer.materials = tempMaterials;

            // Wait for 0.5 seconds
            yield return new WaitForSeconds(0.5f);

            // Revert back to the original materials
            meshRenderer.materials = originalMaterials;

            // Wait for 0.25 seconds
            yield return new WaitForSeconds(0.25f);

            // Set the highlight materials again
            meshRenderer.materials = tempMaterials;

            // Wait for another 0.5 seconds
            yield return new WaitForSeconds(0.5f);

            // Revert back to the original materials again
            meshRenderer.materials = originalMaterials;
        }
    }

    public int HintCount { get => hintCount; set => hintCount = value; }

}
