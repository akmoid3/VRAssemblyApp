using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticPlacementManager : MonoBehaviour
{
    [SerializeField] private float timeForFirstPlacement = 1.0f;
    [SerializeField] private Transform showSolutionPosition;
    private GameObject interactorClone;
    private Dictionary<string, GameObject> instantiatedComponents = new Dictionary<string, GameObject>();

    public void PlaceInitialComponent(List<ComponentData> assemblySequence, List<Transform> components, SnapToPosition interactor)
    {
        if (assemblySequence != null && assemblySequence.Count > 0)
        {
            // Get the name of the first component to be placed
            var firstComponentName = assemblySequence[0].componentName;

            // Find the first component in the list of components
            var firstComponent = components.Find(c => c.name == firstComponentName);
            if (firstComponent != null && interactor != null)
            {
                // Find the snappoint with the same name as the first component
                Transform correctSnappoint = interactor.transform.GetChild(0);

                if (correctSnappoint != null)
                {
                    // Start the coroutine to smoothly move the component into place
                    StartCoroutine(SmoothMoveComponent(firstComponent, correctSnappoint.position, correctSnappoint.rotation, timeForFirstPlacement));
                }
            }
        }
    }

    public void PlaceAllComponentsGradually(float delayBetweenComponents, SnapToPosition interactor, List<ComponentData> assemblySequence, List<Transform> components, ToolManager toolManager)
    {
        // Clean up any previous clones before starting again
        CleanupPreviousClones();

        // Create a new GameObject to serve as the interactor clone
        interactorClone = new GameObject(interactor.name);
        interactorClone.transform.position = showSolutionPosition.position;
        interactorClone.transform.rotation = showSolutionPosition.rotation;

        // Manually clone only the first-level children of the original interactor
        foreach (Transform child in interactor.transform)
        {
            GameObject childClone = Instantiate(child.gameObject);
            childClone.transform.SetParent(interactorClone.transform, false);

            // Remove the MeshRenderer component from the cloned child
            MeshRenderer meshRenderer = childClone.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Destroy(meshRenderer);
            }

            // Ensure childClone does not carry over any deeper children
            foreach (Transform grandchild in childClone.transform)
            {
                Destroy(grandchild.gameObject); // Remove nested children
            }
        }

        // Start the coroutine to place the components gradually
        StartCoroutine(PlaceAllComponentsGraduallyCoroutine(delayBetweenComponents, interactorClone, assemblySequence, components, toolManager));
    }

    public IEnumerator PlaceAllComponentsGraduallyCoroutine(float delayBetweenComponents, GameObject interactorClone, List<ComponentData> assemblySequence, List<Transform> components, ToolManager toolManager)
    {
        if (assemblySequence == null || assemblySequence.Count == 0)
            yield break;

        bool isFirstComponent = true;

        foreach (var componentData in assemblySequence)
        {
            var originalComponent = components.Find(c => c.name == componentData.componentName);
            if (originalComponent != null)
            {
                GameObject componentClone;

                if (instantiatedComponents.ContainsKey(componentData.componentName))
                {
                    componentClone = instantiatedComponents[componentData.componentName];
                }
                else
                {
                    componentClone = Instantiate(originalComponent.gameObject);
                    instantiatedComponents[componentData.componentName] = componentClone;
                }

                Transform correctSnappoint = interactorClone.transform.GetChild(assemblySequence.IndexOf(componentData));

                if (correctSnappoint != null)
                {
                    // Attach the tool to the component and get the tool instance
                    var toolInstance = toolManager.AttachToolToComponent(componentClone, componentData.toolName);

                    if (isFirstComponent)
                    {
                        componentClone.transform.position = correctSnappoint.position;
                        componentClone.transform.rotation = correctSnappoint.rotation;
                        isFirstComponent = false;
                    }
                    else
                    {
                        yield return StartCoroutine(SmoothMoveComponent(componentClone.transform, correctSnappoint.position, correctSnappoint.rotation, timeForFirstPlacement));
                        yield return new WaitForSeconds(delayBetweenComponents);
                    }

                    if (toolInstance != null)
                    {
                        yield return new WaitForSeconds(1.0f);
                        toolManager.HideToolOnComponent(componentClone);
                    }
                }
            }
        }

        yield return new WaitForSeconds(3.0f);

        // Hide all remaining tools
        toolManager.HideAllTools();

        CleanupPreviousClones();
    }

    private void CleanupPreviousClones()
    {
        // Destroy the interactor clone if it exists
        if (interactorClone != null)
        {
            Destroy(interactorClone);
        }

        // Destroy all component clones stored in the dictionary
        foreach (var componentClone in instantiatedComponents.Values)
        {
            if (componentClone != null)
            {
                Destroy(componentClone);
            }
        }

        // Clear the dictionary for the next run
        instantiatedComponents.Clear();
    }

    private IEnumerator SmoothMoveComponent(Transform component, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 initialPosition = component.position;
        Quaternion initialRotation = component.rotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Calculate the percentage of time passed
            float t = elapsedTime / duration;

            // Smoothly interpolate position and rotation
            component.position = Vector3.Lerp(initialPosition, targetPosition, t);
            component.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait until the next frame
            yield return null;
        }

        // Ensure the final position and rotation are set correctly
        component.position = targetPosition;
        component.rotation = targetRotation;
    }
}
