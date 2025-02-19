using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;


public class Manager : MonoBehaviour
{
    [SerializeField] private StateManager stateManager;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private SequenceManager sequenceManager;
    [SerializeField] private HintManager hintManager;
    [SerializeField] private AutomaticPlacementManager automaticPlacementManager;
    [FormerlySerializedAs("pdfLoader")] [SerializeField] private PdfLoader pdfLoaderPdf;


    [SerializeField] private GameObject model;
    [SerializeField] private string modelName;
    [SerializeField] private List<Transform> components = new List<Transform>();
    [SerializeField] private List<Transform> removedComponents = new List<Transform>();
    [SerializeField] private ComponentPositioner componentPositioner;
    [SerializeField] private List<float> performaceForEachStep = new List<float>();


    private SnapToPosition interactor;
    private List<Transform> componentsThatCanSnap;

    private static Dictionary<int, GameObject> currentAssembledSequence;

    // Singleton
    public static Manager Instance { get; private set; }


    public GameObject Model
    {
        get => model;
        set => model = value;
    }

    public List<Transform> Components
    {
        get => components;
        set => components = value;
    }

    public List<Transform> RemovedComponents
    {
        get => removedComponents;
        set => removedComponents = value;
    }


    public List<ComponentData> AssemblySequence
    {
        get => sequenceManager?.AssemblySequence;
        set => sequenceManager.AssemblySequence = value;
    }

    public int CurrentStep
    {
        get => sequenceManager.CurrentStep;
        set => sequenceManager.CurrentStep = value;
    }

    public string FinishTime
    {
        get => sequenceManager.FinishTime;
        set => sequenceManager.FinishTime = value;
    }

    public int HintCount
    {
        get => hintManager.HintCount;
        set => hintManager.HintCount = value;
    }

    public int ErrorCount
    {
        get => sequenceManager.ErrorCount;
        set => sequenceManager.ErrorCount = value;
    }

    public GameObject CurrentSelectedComponent
    {
        get => interactionManager?.GetCurrentSelectedComponent();
        set => interactionManager?.SetCurrentSelectedComponent(value);
    }

    public Dictionary<int, GameObject> CurrentAssembledSequence
    {
        get => currentAssembledSequence;
        set => currentAssembledSequence = value;
    }

    public List<Transform> ComponentsThatCanSnap
    {
        get => componentsThatCanSnap;
        set => componentsThatCanSnap = value;
    }

    public SnapToPosition Interactor
    {
        get => interactor;
        set => interactor = value;
    }

    public List<float> PerformaceForEachStep
    {
        get => performaceForEachStep;
        set => performaceForEachStep = value;
    }

    public PdfLoader LoaderPDF
    {
        get => pdfLoaderPdf;
        set => pdfLoaderPdf = value;
    }

    public string ModelName
    {
        get => modelName;
        set => modelName = value;
    }

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
        StateManager.OnStateChanged += HandleStateChange;
    }

    private void OnDestroy()
    {
        SnapToPosition.OnComponentPlaced -= IncrementCurrentStep;
        StateManager.OnStateChanged -= HandleStateChange;
    }

    private void Start()
    {
        stateManager = StateManager.Instance;
        stateManager.UpdateState(State.ChoosingModel);
        currentAssembledSequence = new Dictionary<int, GameObject>();
        componentsThatCanSnap = new List<Transform>();
    }


    private void Update()
    {
        if (stateManager.CurrentState == State.PlayBack)
            HighlightComponentToPlace(componentsThatCanSnap);
    }


    public void HighlightComponentToPlace(List<Transform> componentsToHighlight)
    {
        hintManager.HighlightComponentToPlace(componentsToHighlight);
    }


    private void HandleComponentFastener(Transform component, ComponentData componentData)
    {
        Fastener fastener = component.GetComponent<Fastener>();
        if (fastener == null)
            return;

        if (componentData == null)
            return;


        fastener.CorrectToolName = componentData.toolName;
        fastener.CorrectToolForce = componentData.toolForce;
        fastener.InitialPosition = fastener.transform.localPosition;
    }


    private void HandleCurrentStepPlayBack()
    {
        if (CurrentStep >= AssemblySequence.Count) return;

        foreach (Transform component in components)
        {
            MakeGrabbable makeGrabbable = component.GetComponent<MakeGrabbable>();
            if (componentsThatCanSnap.Contains(component))
            {
                makeGrabbable.MakeObjectGrabbable();
                HandleComponentFastener(component, AssemblySequence[CurrentStep]);
            }
            else if (!component.GetComponent<ComponentObject>().GetIsPlaced())
            {
                makeGrabbable.MakeObjectNonGrabbable();
            }
        }
    }

    public void UpdateComponentsPerCurrentStep()
    {
        if (CurrentStep >= AssemblySequence.Count) return;
        int stepID = AssemblySequence[CurrentStep].stepId;
        string componentToPlaceName = AssemblySequence[CurrentStep].componentName;
        string componentToPlaceGroup = AssemblySequence[CurrentStep].group;


        if (!CurrentAssembledSequence.TryGetValue(stepID, out var currentComponent))
        {
            foreach (Transform component in components)
            {
                ComponentObject componentObject = component.GetComponent<ComponentObject>();


                if ((component.name == componentToPlaceName && !componentObject.GetIsPlaced()) ||
                    (!componentObject.GetIsPlaced() && componentObject.GetGroup() != "None" &&
                     componentToPlaceGroup == componentObject.GetGroup()))
                {
                    if (!componentsThatCanSnap.Contains(component))
                        ComponentsThatCanSnap.Add(component);
                    continue;
                }

                if (componentsThatCanSnap.Contains(component))
                    ComponentsThatCanSnap.Remove(component);
            }
        }
        else
        {
            foreach (Transform component in components)
            {
                if (component.gameObject == currentComponent)
                {
                    if (!componentsThatCanSnap.Contains(component))
                        ComponentsThatCanSnap.Add(component);
                    continue;
                }

                if (componentsThatCanSnap.Contains(component))
                    ComponentsThatCanSnap.Remove(component);
            }
        }
    }

    private void IncrementCurrentStep()
    {
        if (!sequenceManager)
            return;

        if (AssemblySequence != null && CurrentStep >= AssemblySequence.Count - 1 &&
            StateManager.Instance.CurrentState == State.PlayBack)
        {
            sequenceManager.IncrementCurrentStep();
            StateManager.Instance.UpdateState(State.Finish);
            return;
        }

        if (LoaderPDF.CanActivePanel && CurrentStep + 1 < AssemblySequence.Count)
        {
            var nextStep = AssemblySequence[CurrentStep + 1];
            if (nextStep != null)
            {
                LoaderPDF.ShowPage(nextStep.pdfIndex);
            }
        }
        sequenceManager.IncrementCurrentStep();

        UpdateComponentsPerCurrentStep();
        HandleCurrentStepPlayBack();
    }


    public void IncrementCurrentError()
    {
        if (sequenceManager)
            sequenceManager.IncrementCurrentError();
    }


    public virtual void OnSelectEnter(SelectEnterEventArgs args)
    {
        interactionManager.OnSelectEnter(args);
        if (stateManager.CurrentState == State.PlayBack)
            ValidateComponent(CurrentSelectedComponent);
    }

    public virtual void OnSelectExit(SelectExitEventArgs args)
    {
        interactionManager.OnSelectExit(args);
    }


    public virtual void OnHoverEnter(HoverEnterEventArgs args)
    {
        interactionManager.OnHoverEnter(args);
    }

    public virtual void OnHoverExit(HoverExitEventArgs args)
    {
        interactionManager.OnHoverExit(args);
    }

    public void InitializeSequence(List<ComponentData> sequence)
    {
        if (sequenceManager != null)
            sequenceManager.InitializeSequence(sequence);
    }

    public void SaveBuildingSequence()
    {
        if (sequenceManager != null)
            sequenceManager.SaveBuildingSequence(CurrentSelectedComponent, modelName);
    }

    public void ModifyBuildingSequence()
    {
        if (sequenceManager != null)
            sequenceManager.ModifyBuildingSequence(CurrentSelectedComponent, modelName);
    }

    public void RemoveComponentFromSequence()
    {
        if (sequenceManager != null)
            sequenceManager.RemoveComponentFromSequence(CurrentSelectedComponent, modelName);
    }

    public void RepositionComponentsOnTable(List<Transform> components)
    {
        componentPositioner.RepositionComponentsOnTable(components);
    }

    public void PlaybackSpawnComponents()
    {
        // Create a dictionary to track the count of each component in the components list
        Dictionary<string, int> componentsCount = new Dictionary<string, int>();

        // Iterate through the components list and count occurrences of each component
        foreach (Transform component in components)
        {
            if (componentsCount.ContainsKey(component.name))
            {
                componentsCount[component.name]++;
            }
            else
            {
                componentsCount[component.name] = 1;
            }
        }

        // Group components by stepId to handle each step individually
        var groupedByStep = AssemblySequence
            .GroupBy(c => c.stepId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(c => c.componentName).Select(grp => grp.First()).ToList()
            );


        // Iterate through each stepId to spawn the components
        foreach (var stepGroup in groupedByStep)
        {
            int stepId = stepGroup.Key;
            List<ComponentData> stepComponents = stepGroup.Value;

            // Create a dictionary to track the components required for this step
            Dictionary<string, int> stepComponentCounts = new Dictionary<string, int>();

            // Count the required components for this step based on AssemblySequence
            foreach (var componentData in stepComponents)
            {
                string componentName = componentData.componentName;

                // Count how many times each unique component appears for the current stepId
                if (stepComponentCounts.ContainsKey(componentName))
                {
                    stepComponentCounts[componentName]++;
                }
                else
                {
                    stepComponentCounts[componentName] = 1;
                }
            }

            // Iterate through the unique components for this step
            foreach (var componentData in stepComponents)
            {
                string componentName = componentData.componentName;
                int requiredCount =
                    stepComponentCounts[componentName]; // How many times this component is required in this step
                int componentCount =
                    componentsCount.ContainsKey(componentName)
                        ? componentsCount[componentName]
                        : 0; // How many of this component exist in the scene

                // Calculate the number of missing components for this step
                int missingCount = requiredCount - componentCount;

                // Spawn the missing components
                for (int i = 0; i < missingCount; i++)
                {
                    // Try to load the prefab from Resources/TableUIComponents
                    GameObject prefab = Resources.Load<GameObject>("TemplateComponentsPrefab/" + componentName);
                    if (prefab != null)
                    {
                        // Instantiate the prefab at the specified position and rotation from the AssemblySequence
                        GameObject instantiatedObject = Instantiate(prefab, componentData.position,
                            componentData.rotation, Interactor.transform);
                        instantiatedObject.name = prefab.name; // Ensure name matches the prefab

                        // Add the MakeGrabbable script to the instantiated object
                        instantiatedObject.AddComponent<MakeGrabbable>().MakeObjectGrabbable();

                        // Attach the same ComponentObject script from the interactor's child to the new object
                        Transform
                            firstChild =
                                Interactor.transform
                                    .Find(componentName); // Find the first matching child with the same name
                        if (firstChild != null)
                        {
                            ComponentObject originalComponentObject = firstChild.GetComponent<ComponentObject>();
                            if (originalComponentObject != null)
                            {
                                ComponentObject newComponentObject = instantiatedObject.AddComponent<ComponentObject>();
                                newComponentObject.CopyFrom(
                                    originalComponentObject); // Copy data from the original object
                            }
                        }

                        InitializeSingleComponentType(instantiatedObject.transform);

                        // Add the newly instantiated object to the components list
                        components.Add(instantiatedObject.transform);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Prefab for missing component {componentName} not found in Resources/TableUIComponents.");
                    }
                }
            }
        }

        // Reposition components after ensuring all are present
        componentPositioner.RepositionComponentsOnTable(components);
    }


    public void HandleStateChange(State newState)
    {
        switch (newState)
        {
            case State.ChoosingModel:
                break;
            case State.Initialize:
                MakeComponentsNonGrabbable();
                break;
            case State.Record:
                LoadPDF();
                InitializeComponentsType();
                MakeComponentsGrabbable();
                break;
            case State.PlayBack:
                LoadPDF();
                InitializeComponentsType();
                //MakeComponentsGrabbable();
                Interactor = FindObjectOfType<SnapToPosition>();
                PlaybackSpawnComponents();
                CopyComponentObjectToInteractor();
                UpdateComponentsPerCurrentStep();
                HandleCurrentStepPlayBack();
                PlaceInitialComponent();
                break;
            case State.Finish:
                break;
            case State.SelectingMode:
                break;
            default:
                break;
        }
    }

    private void LoadPDF()
    {
        if (LoaderPDF != null)
            LoaderPDF.LoadPDF(modelName);
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

    public void InitializeComponentsType()
    {
        foreach (Transform component in components)
        {
            InitializeSingleComponentType(component);
        }
    }

    private void InitializeSingleComponentType(Transform component)
    {
        ComponentObject componentObject = component.GetComponent<ComponentObject>();

        if (componentObject != null)
        {
            if (componentObject.IsDestroyed)
            {
                Destroy(componentObject.gameObject);
                return;
            }

            // Remove existing components of type Screw or Nail
            RemoveExistingScripts<Screw>(component.gameObject);
            RemoveExistingScripts<Nail>(component.gameObject);
            RemoveExistingScripts<WoodenPin>(component.gameObject);

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
                case ComponentObject.ComponentType.WoodenPin:
                    component.gameObject.AddComponent<WoodenPin>();
                    break;
                case ComponentObject.ComponentType.None:
                    component.tag = "Component";
                    break;
            }
        }
    }

    private void CopyComponentObjectToInteractor()
    {
        if (Interactor == null)
        {
            Debug.LogError("Interactor is not assigned.");
            return;
        }

        foreach (var component in components)
        {
            // Trova tutti i figli in Interactor che hanno lo stesso nome del componente
            var matchingChildren = Interactor.GetComponentsInChildren<Transform>()
                .Where(child => child.name == component.name);

            if (!matchingChildren.Any())
            {
                Debug.LogWarning($"No childs named: {component.name}");
                continue;
            }

            foreach (var interactorChild in matchingChildren)
            {
                // Copia il ComponentObject dal componente originale
                ComponentObject sourceComponentObject = component.GetComponent<ComponentObject>();
                if (sourceComponentObject != null)
                {
                    ComponentObject targetComponentObject = interactorChild.GetComponent<ComponentObject>();
                    if (targetComponentObject == null)
                    {
                        targetComponentObject = interactorChild.gameObject.AddComponent<ComponentObject>();
                    }

                    // Copia le proprietà
                    targetComponentObject.SetComponentType(sourceComponentObject.GetComponentType());
                    targetComponentObject.SetGroup(sourceComponentObject.GetGroup());
                    targetComponentObject.SetIsPlaced(sourceComponentObject.GetIsPlaced());
                    targetComponentObject.IsReleased = sourceComponentObject.IsReleased;
                }
                else
                {
                    Debug.LogWarning($"Component object missing: {component.name}");
                }
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
        sequenceManager.ValidateComponent(component);
    }

    public void PlaceInitialComponent()
    {
        if (automaticPlacementManager != null)
            automaticPlacementManager.PlaceCurrentStepComponent(CurrentStep, componentsThatCanSnap[0], Interactor,
                1.0f);
    }

    public void PlaceCurrentComponent(float timePlacement)
    {
        if (automaticPlacementManager != null)
        {
            componentsThatCanSnap[0].GetComponent<ComponentObject>().IsAutomaticSnap = true;
            automaticPlacementManager.PlaceCurrentStepComponent(CurrentStep, componentsThatCanSnap[0], Interactor,
                timePlacement);
        }
            
        hintManager.HideHints(Interactor);
    }

    public void PlaceComponent(int step, Transform component)
    {
        if (automaticPlacementManager != null)
            automaticPlacementManager.PlaceStepComponent(step, component, Interactor);
        hintManager.HideHints(Interactor);
    }

    public void ShowHint()
    {
        hintManager.ShowHint(CurrentStep, componentsThatCanSnap[0], Interactor);
    }

    public void HideHint()
    {
        hintManager.HideHints(Interactor);
    }

    public void CloseApp()
    {
        ApplicationQuit applicationQuit = new ApplicationQuit();
        applicationQuit.QuitApplication();
    }
    
    
}