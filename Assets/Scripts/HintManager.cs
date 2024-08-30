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


    public virtual void ShowHint(List<ComponentData> assemblySequence, int currentStep, List<Transform> components, SnapToPosition interactor)
    {
        var currentComponentName = assemblySequence[currentStep].componentName;

        // Check if the hint for the current step has already been shown
        if (hintShownForStep.ContainsKey(currentStep) && hintShownForStep[currentStep])
        {
            return;
        }

        // Increment the hint count and mark the hint as shown for the current step
        hintCount++;
        hintShownForStep[currentStep] = true;
        OnHintCountChanged?.Invoke(hintCount);

        if (interactor != null)
        {
            // Show the snappoint where the component should be snapped
            ShowSnapPoint(interactor, currentStep);
        }
    }

    private void ShowSnapPoint(SnapToPosition interactor, int currentStep)
    {
        // Find the snappoint with the same name as the current component
        Transform correctSnappoint = interactor.transform.GetChild(currentStep);

        if (correctSnappoint != null)
        {
            correctSnappoint.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public virtual void HighlightComponentToPlace(List<ComponentData> assemblySequence, int currentStep, List<Transform> components)
    {
        if(isWaiting)
            return;

        var componentToPlaceName = assemblySequence[currentStep].componentName;
        var componentToPlaceGroup = assemblySequence[currentStep].group;

        foreach (var component in components)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            if (component.name == componentToPlaceName || (componentObject.GetGroup() != ComponentObject.Group.None && componentToPlaceGroup == componentObject.GetGroup()))
            {
                StartCoroutine(HandleHintCooldown(component.gameObject));
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

            yield return new WaitForSeconds(hintCooldown);

            // Revert back to the original materials
            meshRenderer.materials = originalMaterials;


        }
    }

    public int HintCount { get => hintCount; set => hintCount = value; }

}
