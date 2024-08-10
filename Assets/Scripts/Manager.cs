using Codice.Client.Common.GameUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
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
    private Dictionary<int, bool> hintShownForStep = new Dictionary<int, bool>();

    //XRSnapPointSocketInteractor interactor;
    SnapToPosition interactor;
    [SerializeField] private float timeForFirstPlacement = 1.0f;

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
    }

    private void Start()
    {
        UpdateState(State.ChoosingModel);
    }

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
        if (component.name == expectedComponent.componentName)
        {
        }
        else
        {
            errorCount++;
            OnErrorCountChanged?.Invoke(errorCount);
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
                Transform correctSnappoint = null;
                foreach (Transform child in interactor.transform)
                {
                    if (child.name == firstComponentName)
                    {
                        correctSnappoint = child;
                        break;
                    }
                }

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

    public void ShowHint()
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
            // Find the snappoint with the same name as the current component
            Transform correctSnappoint = null;
            foreach (Transform child in interactor.transform)
            {
                if (child.name == currentComponentName)
                {
                    correctSnappoint = child;
                    break;
                }
            }

            if (correctSnappoint != null)
            {
                correctSnappoint.GetComponent<MeshRenderer>().enabled = true;
            }
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