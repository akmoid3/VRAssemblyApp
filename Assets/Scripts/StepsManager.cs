using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepsManager : MonoBehaviour
{
    public GameObject stepPrefab;
    public Transform stepParent;

    public class StepData
    {
        public int StepNumber { get; set; }
        public int Errors { get; set; }
        public int Hints { get; set; }
        public float TimeSpent { get; set; }

        public StepData(int stepNumber)
        {
            StepNumber = stepNumber;
            Errors = 0;
            Hints = 0;
            TimeSpent = 0f;
        }
    }

    private List<StepData> stepsData = new List<StepData>();
    private StepData currentStepData = null;
    private float stepStartTime;

    private int previousErrorCount = 0;
    private int previousHintCount = 0;

    public IReadOnlyList<StepData> StepsData => stepsData;

    private void Awake()
    {
        SequenceManager.OnStepChanged += OnStepChanged;
        SequenceManager.OnErrorCountChanged += OnErrorCountChanged;
        HintManager.OnHintCountChanged += OnHintCountChanged;
        StateManager.OnStateChanged += PrintStepData;
    }

    private void OnDestroy()
    {
        SequenceManager.OnStepChanged -= OnStepChanged;
        SequenceManager.OnErrorCountChanged -= OnErrorCountChanged;
        HintManager.OnHintCountChanged -= OnHintCountChanged;
        StateManager.OnStateChanged -= PrintStepData;
    }

    private void OnStepChanged(int stepNumber)
    {
        if (currentStepData != null)
        {
            // Add time spent on the previous step
            currentStepData.TimeSpent += Time.time - stepStartTime;
        }

        stepStartTime = Time.time;

        currentStepData = stepsData.Find(s => s.StepNumber == stepNumber);
        if (currentStepData == null)
        {
            currentStepData = new StepData(stepNumber);
            stepsData.Add(currentStepData);
        }
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

    public void CompleteCurrentStep()
    {
        if (currentStepData != null)
        {
            currentStepData.TimeSpent += Time.time - stepStartTime;
            stepStartTime = 0f;
        }
    }

    public void PrintStepData(State state)
    {
        if (state == State.Finish)
        {
            foreach (var step in stepsData)
            {
                if (step.TimeSpent == 0)
                    continue;
                GameObject stepObject = Instantiate(stepPrefab, stepParent);

                TextMeshProUGUI stepText = stepObject.transform.Find("Steps").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI errorText = stepObject.transform.Find("Errors").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI hintText = stepObject.transform.Find("Hint").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI timeText = stepObject.transform.Find("Time").GetComponent<TextMeshProUGUI>();

                Button button = stepObject.GetComponent<Button>();
                button.onClick.AddListener(() => StartCoroutine(ShowPlacement(step.StepNumber)));
                stepText.text = $"Step {step.StepNumber}";
                errorText.text = $"Errors: {step.Errors}";
                hintText.text = $"Hints: {step.Hints}";
                timeText.text = $"Time: {step.TimeSpent:F2}s";
            }
        }
    }

    bool isProcessing = false;
    private IEnumerator ShowPlacement(int step)
    {
        if (isProcessing) yield break;

        isProcessing = true;
        Debug.Log("ciao" + step);
        Manager.Instance.RepositionComponentsOnTable(Manager.Instance.Components);
        Manager.Instance.CurrentStep = 0;
        Manager.Instance.CurrentAssembledSequence.Clear();

        foreach (Transform go in Manager.Instance.Components) { go.GetComponent<ComponentObject>().SetIsPlaced(false); }
        for (int i = 0; i <= step; i++)
        {
            Manager.Instance.CurrentStep = i;
            Manager.Instance.UpdateComponentsPerCurrentStep();

            if (i == step)
            {
                //i--;
                //Manager.Instance.RepositionComponentOnTable(Manager.Instance.ComponentsThatCanSnap[0]);
                Manager.Instance.PlaceCurrentComponent(1.2f);
                yield return new WaitForSeconds(1.3f);
                //isProcessing = false;

            }
            else
            {

                Manager.Instance.PlaceCurrentComponent(0.15f);

                yield return new WaitForSeconds(0.25f);

            }

        }

        isProcessing = false;

    }


}
