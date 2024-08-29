using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class Manager : MonoBehaviour
{
    [SerializeField] private StateManager stateManager;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private SequenceManager sequenceManager;
    [SerializeField] private HintManager hintManager;
    [SerializeField] private AutomaticPlacementManager automaticPlacementManager;
    [SerializeField] private ToolManager toolManager;
    [SerializeField] private PdfLoader pdfLoader;


    [SerializeField] private GameObject model;
    [SerializeField] private List<Transform> components = new List<Transform>();


    SnapToPosition interactor;


    // Singleton
    public static Manager Instance { get; private set; }


    public GameObject Model { get => model; set => model = value; }
    public List<Transform> Components { get => components; set => components = value; }

    public List<ComponentData> AssemblySequence { get => sequenceManager.AssemblySequence; set => sequenceManager.AssemblySequence = value; }
    public int CurrentStep { get => sequenceManager.CurrentStep; set => sequenceManager.CurrentStep = value; }
    public string FinishTime { get => sequenceManager.FinishTime; set => sequenceManager.FinishTime = value; }
    public int HintCount { get => hintManager.HintCount; set => hintManager.HintCount = value; }

    public int ErrorCount { get => sequenceManager.ErrorCount; set => sequenceManager.ErrorCount = value; }
    public GameObject CurrentSelectedComponent { get => interactionManager.GetCurrentSelectedComponent(); set => interactionManager.SetCurrentSelectedComponent(value); }

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
    }

    List<Fastener> fasteners = new List<Fastener>();
    private void Update()
    {
        if (stateManager.CurrentState == State.PlayBack)
        {
            hintManager.HighlightComponentToPlace(AssemblySequence, CurrentStep, components);

            ComponentData componentData = AssemblySequence[CurrentStep];
            if (componentData != null)
            {
                foreach (var component in components)
                {
                    ComponentObject componentObject = component.GetComponent<ComponentObject>();
                    if (component.name == componentData.componentName || (componentObject.GetGroup() != ComponentObject.Group.None))
                    {
                        Fastener fastener = component.GetComponent<Fastener>();
                        if (fasteners.Contains(fastener))
                            fastener = null;
                        if (fastener != null && componentData.toolName != "null")
                        {
                            fastener.CorrectToolName = componentData.toolName;
                        }
                        if (fastener != null && fastener.IsStopped)
                        {
                            fasteners.Add(fastener);
                            if (interactor != null)
                            {
                                // Find the snappoint with the same name as the current component
                                Transform correctSnappoint = interactor.transform.GetChild(CurrentStep);

                                if (correctSnappoint != null)
                                {
                                    correctSnappoint.GetComponent<MeshRenderer>().enabled = false;
                                }
                            }
                            AudioManager.Instance.PlayPopSound();
                            ValidateComponent(component.gameObject);
                            IncrementCurrentStep();
                        }
                    }
                }
            }
        }
    }


    private void IncrementCurrentStep()
    {
        if (sequenceManager)

            sequenceManager.IncrementCurrentStep();
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
            sequenceManager.SaveBuildingSequence(CurrentSelectedComponent, model.name);
    }

    public void ModifyBuildingSequence()
    {
        if (sequenceManager != null)
            sequenceManager.ModifyBuildingSequence(CurrentSelectedComponent, model.name);
    }

    public void RemoveComponentFromSequence()
    {
        sequenceManager.RemoveComponentFromSequence(CurrentSelectedComponent, model.name);
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
                MakeComponentsGrabbable();
                interactor = FindObjectOfType<SnapToPosition>();
                CopyComponentObjectToInteractor();
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
        pdfLoader.LoadPDF(model.name);
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
        if (interactor == null)
        {
            Debug.LogError("Interactor is not assigned.");
            return;
        }

        foreach (var component in components)
        {
            // Find the corresponding child in the interactor
            Transform interactorChild = interactor.transform.Find(component.name);
            if (interactorChild != null)
            {
                // Copy the ComponentObject from the original component
                ComponentObject sourceComponentObject = component.GetComponent<ComponentObject>();
                if (sourceComponentObject != null)
                {
                    ComponentObject targetComponentObject = interactorChild.GetComponent<ComponentObject>();
                    if (targetComponentObject == null)
                    {
                        targetComponentObject = interactorChild.gameObject.AddComponent<ComponentObject>();
                    }
                    // Copy properties
                    targetComponentObject.SetComponentType(sourceComponentObject.GetComponentType());
                    targetComponentObject.SetGroup(sourceComponentObject.GetGroup());
                    targetComponentObject.SetIsPlaced(sourceComponentObject.GetIsPlaced());
                    targetComponentObject.IsReleased = sourceComponentObject.IsReleased;
                }
                else
                {
                    Debug.LogWarning($"ComponentObject not found on source component: {component.name}");
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

    private void PlaceInitialComponent()
    {
        automaticPlacementManager.PlaceInitialComponent(AssemblySequence, components, interactor);
    }


    public void PlaceAllComponentsGradually(float delayBetweenComponents)
    {
        automaticPlacementManager.PlaceAllComponentsGradually(delayBetweenComponents, interactor, AssemblySequence, components, toolManager);
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

    public void ShowHint()
    {
        hintManager.ShowHint(AssemblySequence, CurrentStep, components, interactor);
    }

    public void CloseApp()
    {
        ApplicationQuit applicationQuit = new ApplicationQuit();
        applicationQuit.QuitApplication();
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