using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class Manager : MonoBehaviour
{
    [SerializeField] private SaveSequence saveSequence;
    [SerializeField] private GameObject currentSelectedComponent;
    [SerializeField] private GameObject model;

    [SerializeField] private bool isRealeased = false;
    [SerializeField] private bool canHover = true;
    [SerializeField] private List<GameObject> sequenceOrderList = new List<GameObject>();
    [SerializeField] private List<Transform> components = new List<Transform>();


    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1.0f;


    [SerializeField] private State state;
    public static event Action<State> OnStateChanged;


    [SerializeField] private List<ComponentData> assemblySequence;
    [SerializeField] private int currentStep;
    [SerializeField] private int errorCount;
    [SerializeField] private bool canIncrement = false;
    [SerializeField] private int hintCount;
    [SerializeField] private string finishTime;

    public static event Action<int> OnErrorCountChanged;
    public static event Action<int> OnHintCountChanged;
    public static event Action<int> OnStepChanged;

    private Dictionary<int, bool> hintShownForStep = new Dictionary<int, bool>();

    SnapToPosition interactor;
    [SerializeField] private float timeForFirstPlacement = 1.0f;
    [SerializeField] private Transform showSolutionPosition;

    [SerializeField] private ToolManager toolManager; // Assign in Inspector


    // Singleton
    public static Manager Instance { get; private set; }

    public bool IsRealeased { get => isRealeased; set => isRealeased = value; }

    public GameObject Model { get => model; set => model = value; }
    public List<Transform> Components { get => components; set => components = value; }

    public State State { get => state; private set => state = value; }
    public List<ComponentData> AssemblySequence { get => assemblySequence; set => assemblySequence = value; }
    public int CurrentStep { get => currentStep; set => currentStep = value; }
    public string FinishTime { get => finishTime; set => finishTime = value; }
    public int HintCount { get => hintCount; set => hintCount = value; }
    public int ErrorCount { get => errorCount; set => errorCount = value; }
    public GameObject CurrentSelectedComponent { get => currentSelectedComponent; set => currentSelectedComponent = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }

        SnapToPosition.OnComponentPlaced += IncrementCurrentStep;
    }

    private void OnDestroy()
    {
        SnapToPosition.OnComponentPlaced -= IncrementCurrentStep;
    }
    private void IncrementCurrentStep()
    {
        currentStep++;
        OnStepChanged?.Invoke(currentStep);
    }

    public void IncrementCurrentError()
    {
        errorCount++;
        OnErrorCountChanged?.Invoke(errorCount);
    }

    private void Start()
    {
        UpdateState(State.ChoosingModel);
    }

    private void Update()
    {
        if (state == State.PlayBack)
        {
            ComponentData componentData = assemblySequence[currentStep];
            if (componentData != null)
            {
                foreach (var component in components)
                {
                    if (component.name == componentData.componentName)
                    {
                        Fastener fastener = component.GetComponent<Fastener>();
                        if (componentData.toolName != "null")
                        {
                            fastener.CorrectToolName = componentData.toolName;
                        }
                        if (fastener != null && fastener.IsStopped)
                        {
                            if (interactor != null)
                            {
                                // Find the snappoint with the same name as the current component
                                Transform correctSnappoint = interactor.transform.GetChild(CurrentStep);

                                if (correctSnappoint != null)
                                {
                                    correctSnappoint.GetComponent<MeshRenderer>().enabled = false;
                                }
                            }
                            ValidateComponent(component.gameObject);
                            IncrementCurrentStep();
                        }
                    }
                }
            }

        }
    }



    public void InitializeSequence(List<ComponentData> sequence)
    {
        assemblySequence = sequence;
        currentStep = 0;
        errorCount = 0;
    }

    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        if (args.interactorObject as XRBaseControllerInteractor)
        {
            currentSelectedComponent = args.interactableObject.transform.gameObject;
            SetComponentReleasedState(currentSelectedComponent, false);
            if (state != State.Initialize)
                ResetParentIfNotGroup(currentSelectedComponent);
            canHover = false;

            if (currentSelectedComponent.GetComponent<Fastener>())
            {
                currentSelectedComponent.GetComponent<Fastener>().IsAligned = false;
                currentSelectedComponent.GetComponent<Fastener>().CanStop = false;
                currentSelectedComponent.GetComponent<Fastener>().IsStopped = false;
            }
        }
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {
        if (args.interactorObject as XRBaseControllerInteractor)
        {
            if (state != State.Initialize && state != State.PlayBack)
                ResetParentIfNotGroup(currentSelectedComponent);
            SetComponentReleasedState(currentSelectedComponent, true);
            canHover = true;

            if (state == State.PlayBack)
                ValidateComponent(currentSelectedComponent);
        }
    }

    private void ResetParentIfNotGroup(GameObject component)
    {
        Transform parent = component.transform.parent;
        if (parent != null && parent.name != "Group")
        {
            component.transform.SetParent(null);
        }
    }

    private void SetComponentReleasedState(GameObject component, bool isReleased)
    {
        ComponentObject componentObject = component.GetComponent<ComponentObject>();
        if (componentObject != null)
        {
            componentObject.IsReleased = isReleased;
        }
    }


    private void ConfigureOutline(GameObject gameObject, bool enable)
    {
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
        }
        outline.enabled = enable;
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (canHover)
            ConfigureOutline(args.interactableObject.transform.gameObject, true);
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        ConfigureOutline(args.interactableObject.transform.gameObject, false);
    }



    public GameObject GetCurrentSelectedComponent()
    {
        return currentSelectedComponent;
    }

    public void SetCurrentSelectedComponent(GameObject gameObject)
    {
        currentSelectedComponent = gameObject;
    }

    public void SaveBuildingSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.SaveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(model.name);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    public void ModifyBuildingSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.ModifyComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(model.name);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    public void RemoveComponentFromSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.RemoveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(model.name);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }


    public void UpdateState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.ChoosingModel:
                break;
            case State.Initialize:
                MakeComponentsNonGrabbable();
                break;
            case State.Record:
                InitializeComponentsType();
                MakeComponentsGrabbable();
                break;
            case State.PlayBack:
                InitializeComponentsType();
                MakeComponentsGrabbable();
                interactor = FindObjectOfType<SnapToPosition>();
                PlaceInitialComponent();
                break;
            case State.Finish:
                break;
            case State.SelectingMode:
                break;
            default:
                break;
        }
        OnStateChanged?.Invoke(newState);
    }

    private void MakeComponentsGrabbable()
    {
        foreach (Transform component in components)
        {
            MakeGrabbable makeGrabbable = component.GetComponent<MakeGrabbable>();
            makeGrabbable.MakeObjectGrabbable();
        }
    }


    private void MakeComponentsNonGrabbable()
    {
        foreach (Transform component in components)
        {
            MakeGrabbable makeGrabbable = component.GetComponent<MakeGrabbable>();
            makeGrabbable.MakeObjectNonGrabbable();
        }
    }

    private void InitializeComponentsType()
    {
        foreach (Transform component in components)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            // Remove existing components of type Screw or Nail
            RemoveExistingScripts<Screw>(component.gameObject);
            RemoveExistingScripts<Nail>(component.gameObject);
            component.tag = "Untagged";
            // Add the selected component script
            switch (componentObject.GetComponentType())
            {
                case ComponentObject.ComponentType.Screw:
                    component.gameObject.AddComponent<Screw>();
                    break;
                case ComponentObject.ComponentType.Nail:
                    component.gameObject.AddComponent<Nail>();
                    break;
                case ComponentObject.ComponentType.None:
                    component.tag = "Component";
                    break;
            }
        }
    }

    void RemoveExistingScripts<T>(GameObject target) where T : Component
    {
        T[] existingScripts = target.GetComponents<T>();
        foreach (T script in existingScripts)
        {
            Destroy(script);
        }
    }

    private void ValidateComponent(GameObject component)
    {
        var nextStep = currentStep + 1;
        if (nextStep >= assemblySequence.Count)
        {
            UpdateState(State.Finish);
            return;
        }

        ComponentData expectedComponent = assemblySequence[currentStep];
        if (component.name != expectedComponent.componentName)
        {
            IncrementCurrentError();
        }
    }

    private void PlaceInitialComponent()
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

    private Dictionary<string, GameObject> instantiatedComponents = new Dictionary<string, GameObject>();
    private GameObject interactorClone; // Store a reference to the interactor clone
    [SerializeField] private Material highlightMaterial;
    private Dictionary<int, bool> secondHintShownForStep = new Dictionary<int, bool>();

    public void PlaceAllComponentsGradually(float delayBetweenComponents)
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
        StartCoroutine(PlaceAllComponentsGraduallyCoroutine(delayBetweenComponents, interactorClone));
    }

    private void RemoveAllComponentsExceptTransform(GameObject gameObject)
    {
        // Get all components attached to the GameObject
        var components = gameObject.GetComponents<Component>();

        // Loop through each component and destroy it, except for the Transform component
        foreach (var component in components)
        {
            if (!(component is Transform))
            {
                Destroy(component);
            }
        }
    }

    private IEnumerator PlaceAllComponentsGraduallyCoroutine(float delayBetweenComponents, GameObject interactorClone)
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


    private bool isWaiting;

    public void ShowHint()
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

        yield return new WaitForSeconds(1f);

        
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
}

    public enum State
{
    ChoosingModel,
    SelectingMode,
    Initialize,
    Record,
    PlayBack,
    Finish
}