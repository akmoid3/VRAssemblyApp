using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Object = UnityEngine.Object;

public class StepsManager : MonoBehaviour
{
    public GameObject stepPrefab;
    public Transform stepParent;

    private Dictionary<int, Transform> stepComponentsByStep;
    private List<Transform> componentToHighlight;
    private List<Transform> componentToRemoveHighlight;

    [SerializeField] private Transform InteractorPosition;

    public class StepData
    {
        public int StepNumber { get; set; }
        public int Errors { get; set; }
        public int Hints { get; set; }
        public float TimeSpent { get; set; }
        public float Accuracy { get; set; }

        public StepData(int stepNumber)
        {
            StepNumber = stepNumber;
            Errors = 0;
            Hints = 0;
            TimeSpent = 0f;
            Accuracy = 0f;
        }
    }

    private List<StepData> stepsData = new List<StepData>();
    private StepData currentStepData = null;
    private float stepStartTime;
    private int previousErrorCount = 0;
    private int previousHintCount = 0;
    private bool processOneTimeComponentsPerSteps = false;
    public IReadOnlyList<StepData> StepsData => stepsData;

    [SerializeField] private Material highlightMaterial;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        SequenceManager.OnStepChanged += OnStepChanged;
        SequenceManager.OnErrorCountChanged += OnErrorCountChanged;
        HintManager.OnHintCountChanged += OnHintCountChanged;
        StateManager.OnStateChanged += PrintStepData;
        processOneTimeComponentsPerSteps = false;
        componentToHighlight = new List<Transform>();
        componentToRemoveHighlight = new List<Transform>();
        stepComponentsByStep = new Dictionary<int, Transform>();
    }

    private void OnDestroy()
    {
        SequenceManager.OnStepChanged -= OnStepChanged;
        SequenceManager.OnErrorCountChanged -= OnErrorCountChanged;
        HintManager.OnHintCountChanged -= OnHintCountChanged;
        StateManager.OnStateChanged -= PrintStepData;
    }

    private void Update()
    {
        if (StateManager.Instance.CurrentState == State.Finish)
        {
            foreach (Transform comp in componentToHighlight)
            {
                HighlightComponent(comp);
            }
            foreach (Transform comp in componentToRemoveHighlight)
            {
                ClearHighlight(comp);
            }
        }
        else
        {
            foreach (Transform comp in componentToHighlight)
            {
                ClearHighlight(comp);
            }
            foreach (Transform comp in componentToRemoveHighlight)
            {
                ClearHighlight(comp);
            }
        }
    }

    private void OnStepChanged(int stepNumber)
    {
        if (currentStepData != null)
            currentStepData.TimeSpent += Time.time - stepStartTime;
        stepStartTime = Time.time;
        currentStepData = new StepData(stepNumber + 1);
        stepsData.Add(currentStepData);
    }

    private void OnErrorCountChanged(int errorCount)
    {
        if (currentStepData != null)
        {
            int errorDifference = errorCount - previousErrorCount;
            currentStepData.Errors += errorDifference;
            previousErrorCount = errorCount;
        }
    }

    private void OnHintCountChanged(int hintCount)
    {
        if (currentStepData != null)
        {
            currentStepData.Hints = hintCount - previousHintCount;
            previousHintCount = hintCount;
        }
    }

    public void PrintStepData(State state)
    {
        if (state == State.Finish)
        {
            if (currentStepData != null)
                currentStepData.TimeSpent += Time.time - stepStartTime;
            
            foreach (var step in stepsData)
            {
                if (step.StepNumber > Manager.Instance.PerformaceForEachStep.Count)
                    return;
                step.Accuracy = Manager.Instance.PerformaceForEachStep[step.StepNumber - 1] * 100;
              
                GameObject stepObject = Instantiate(stepPrefab, stepParent);
                TextMeshProUGUI stepText = stepObject.transform.Find("Steps").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI errorText = stepObject.transform.Find("Errors").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI hintText = stepObject.transform.Find("Hint").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI timeText = stepObject.transform.Find("Time").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI accuracyText = stepObject.transform.Find("Accuracy").GetComponent<TextMeshProUGUI>();

                Button button = stepObject.GetComponent<Button>();
                button.onClick.AddListener(() => ShowPlacement(step.StepNumber));
                stepText.text = $"{step.StepNumber - 1}";
                errorText.text = $"{step.Errors}";
                hintText.text = $"{step.Hints}";
                timeText.text = $"{step.TimeSpent:F2}s";
                accuracyText.text = $"{step.Accuracy:F2}%";
            }
        }
    }

    private void ShowPlacement(int step)
    {
        Manager.Instance.RepositionComponentsOnTable(Manager.Instance.Components);
        Manager.Instance.CurrentStep = 0;
        Manager.Instance.CurrentAssembledSequence.Clear();
        //Manager.Instance.Interactor.transform.SetPositionAndRotation(InteractorPosition.position, InteractorPosition.rotation);

        foreach (Transform go in Manager.Instance.Components)
        {
            go.GetComponent<ComponentObject>().SetIsPlaced(false);
            ClearHighlight(go);
        }

        if (!processOneTimeComponentsPerSteps)
        {
            for (int i = 0; i < Manager.Instance.AssemblySequence.Count; i++)
                UpdateComponentsForStepManager(i);
            processOneTimeComponentsPerSteps = true;
        }

        // Pulizia delle liste
        componentToHighlight.Clear();
        componentToRemoveHighlight.Clear();

        for (int i = 0; i < step; i++)
        {
            var stepID = Manager.Instance.AssemblySequence[i].stepId;
            if (!stepComponentsByStep.TryGetValue(stepID, out var componentForStep))
            {
                Debug.LogWarning($"No components to snap for step {i}");
                continue;
            }

            Manager.Instance.PlaceComponent(i, componentForStep);
            componentForStep.SetParent(Manager.Instance.Interactor.transform.GetChild(i));

            // If it is last step highlight it
            if (i == step - 1)
            {
                if (componentToRemoveHighlight.Contains(componentForStep.transform))
                    componentToRemoveHighlight.Remove(componentForStep.transform);
                if (!componentToHighlight.Contains(componentForStep.transform))
                    componentToHighlight.Add(componentForStep.transform);
            }
            else
            {
                if (componentToHighlight.Contains(componentForStep.transform))
                    componentToHighlight.Remove(componentForStep.transform);
                if (!componentToRemoveHighlight.Contains(componentForStep.transform))
                    componentToRemoveHighlight.Add(componentForStep.transform);
            }
        }
    }

    public void UpdateComponentsForStepManager(int currentStep)
    {
        if (currentStep >= Manager.Instance.AssemblySequence.Count) return;

        int stepID = Manager.Instance.AssemblySequence[currentStep].stepId;
        string componentToPlaceName = Manager.Instance.AssemblySequence[currentStep].componentName;
        string componentToPlaceGroup = Manager.Instance.AssemblySequence[currentStep].group;

        foreach (Transform component in Manager.Instance.Components)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            if ((component.name == componentToPlaceName && !componentObject.GetIsPlaced() &&
                 !stepComponentsByStep.ContainsValue(component)) ||
                (!componentObject.GetIsPlaced() && componentObject.GetGroup() != "None" &&
                 componentToPlaceGroup == componentObject.GetGroup() && !stepComponentsByStep.ContainsValue(component)))
            {
                if (!stepComponentsByStep.ContainsValue(component))
                    stepComponentsByStep[stepID] = component;
            }
        }
    }

    private void HighlightComponent(Transform component)
    {
        Renderer[] renderers = component.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!originalMaterials.ContainsKey(rend))
                originalMaterials[rend] = rend.materials;
            Material[] newMats = new Material[rend.materials.Length];
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = highlightMaterial;
            rend.materials = newMats;
        }
    }

    private void ClearHighlight(Transform component)
    {
        Renderer[] renderers = component.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (originalMaterials.TryGetValue(rend, out Material[] origMats))
                rend.materials = origMats;
        }
    }
}
