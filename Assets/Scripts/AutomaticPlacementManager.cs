using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class AutomaticPlacementManager : MonoBehaviour
{
    [SerializeField] private float timeForFirstPlacement = 1.0f;
    [SerializeField] private Transform showSolutionPosition;
    private GameObject interactorClone;
    private Dictionary<string, GameObject> instantiatedComponents = new Dictionary<string, GameObject>();

    private bool isPlacingComponent = false;

    public float TimeForFirstPlacement { get => timeForFirstPlacement; set => timeForFirstPlacement = value; }
    public Dictionary<string, GameObject> InstantiatedComponents { get => instantiatedComponents; set => instantiatedComponents = value; }

    public virtual void PlaceInitialComponent(List<ComponentData> assemblySequence, List<Transform> components, SnapToPosition interactor)
    {
        if (assemblySequence != null && assemblySequence.Count > 0)
        {
            var firstComponentName = assemblySequence[0].componentName;
            var firstComponent = components.Find(c => c.name == firstComponentName);
            if (firstComponent != null && interactor != null)
            {
                Transform correctSnappoint = interactor.transform.GetChild(0);

                if (correctSnappoint != null)
                {
                    StartCoroutine(SmoothMoveComponent(firstComponent, correctSnappoint, timeForFirstPlacement));
                }
            }
        }
    }

    private Dictionary<string, int> componentPlacementCounts = new Dictionary<string, int>();

    public void PlaceCurrentStepComponent(int stepIndex, List<ComponentData> assemblySequence, List<Transform> components, SnapToPosition interactor)
    {
        if (isPlacingComponent) return;  // Prevent spamming if placement is ongoing

        // Ensure the step index is within bounds
        if (assemblySequence == null || stepIndex < 0 || stepIndex >= assemblySequence.Count)
        {
            Debug.LogWarning("Invalid step index or empty assembly sequence.");
            return;
        }

        isPlacingComponent = true;

        var componentData = assemblySequence[stepIndex];
        int stepID = assemblySequence[stepIndex].stepId;
        string componentName = componentData.componentName;
        var componentGroup = componentData.group;


        if (!componentPlacementCounts.ContainsKey(componentName))
        {
            componentPlacementCounts[componentName] = 0;
        }
        componentPlacementCounts[componentName]++;



        Transform componentToPlace;

        if (!Manager.Instance.CurrentAssembledSequence.TryGetValue(stepID, out var currentComponent))
        {
            componentToPlace = components.FirstOrDefault(component => component.name == componentName || component.GetComponent<ComponentObject>().GetGroup() == componentGroup);
        }
        else
        {
            componentToPlace = currentComponent.transform;
        }

        if (componentToPlace == null)
        {
            Debug.LogWarning($"Component {componentName} not found in the components list.");
            isPlacingComponent = false;  // Reset flag if component is not found
            return;
        }

        if (componentPlacementCounts[componentName] > 1)
        {
            Debug.Log($"Placing component '{componentName}' for the {componentPlacementCounts[componentName]} time.");
            componentToPlace.GetComponent<Fastener>().IsStopped = true;
        }

        Transform correctSnappoint = interactor.transform.GetChild(stepIndex);
        if (correctSnappoint == null)
        {
            Debug.LogWarning($"Snap point for step index {stepIndex} not found in the interactor.");
            isPlacingComponent = false;  // Reset flag if snap point is not found
            return;
        }

        // Start coroutine to move the component and reset the flag afterward
        StartCoroutine(SmoothMoveAndResetFlag(componentToPlace, correctSnappoint, timeForFirstPlacement));
    }

    private IEnumerator SmoothMoveAndResetFlag(Transform component, Transform correctSnappoint, float duration)
    {
        yield return StartCoroutine(SmoothMoveComponent(component, correctSnappoint, duration));
        isPlacingComponent = false;  // Reset flag after placement is complete
    }

    public virtual void PlaceAllComponentsGradually(float delayBetweenComponents, SnapToPosition interactor, List<ComponentData> assemblySequence, List<Transform> components, ToolManager toolManager)
    {
        CleanupPreviousClones();
        interactorClone = new GameObject(interactor.name);
        interactorClone.transform.position = showSolutionPosition.position;
        interactorClone.transform.rotation = showSolutionPosition.rotation;

        foreach (Transform child in interactor.transform)
        {
            GameObject childClone = Instantiate(child.gameObject);
            childClone.transform.SetParent(interactorClone.transform, false);

            MeshRenderer meshRenderer = childClone.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Destroy(meshRenderer);
            }

            foreach (Transform grandchild in childClone.transform)
            {
                Destroy(grandchild.gameObject);
            }
        }

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
                    var toolInstance = toolManager.AttachToolToComponent(componentClone, componentData.toolName);

                    if (isFirstComponent)
                    {
                        componentClone.transform.position = correctSnappoint.position;
                        componentClone.transform.rotation = correctSnappoint.rotation;
                        isFirstComponent = false;
                    }
                    else
                    {
                        yield return StartCoroutine(SmoothMoveComponent(componentClone.transform, correctSnappoint, timeForFirstPlacement));
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
        toolManager.HideAllTools();
        CleanupPreviousClones();
    }

    public void CleanupPreviousClones()
    {
        if (interactorClone != null)
        {
            Destroy(interactorClone);
        }

        foreach (var componentClone in instantiatedComponents.Values)
        {
            if (componentClone != null)
            {
                Destroy(componentClone);
            }
        }

        instantiatedComponents.Clear();
    }

    public IEnumerator SmoothMoveComponent(Transform component, Transform correctSnappoint, float duration)
    {
        Vector3 initialPosition = component.position;
        Quaternion initialRotation = component.rotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Continuously update target position and rotation to follow snap point
            Vector3 targetPosition = correctSnappoint.position;
            Quaternion targetRotation = correctSnappoint.rotation;

            float t = elapsedTime / duration;
            component.position = Vector3.Lerp(initialPosition, targetPosition, t);
            component.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final alignment with the snap point after the movement completes
        component.position = correctSnappoint.position;
        component.rotation = correctSnappoint.rotation;
    }
}
