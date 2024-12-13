using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
                if(step.TimeSpent == 0)
                    continue;
                GameObject stepObject = Instantiate(stepPrefab, stepParent);

                TextMeshProUGUI stepText = stepObject.transform.Find("Steps").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI errorText = stepObject.transform.Find("Errors").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI hintText = stepObject.transform.Find("Hint").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI timeText = stepObject.transform.Find("Time").GetComponent<TextMeshProUGUI>();

                stepText.text = $"Step {step.StepNumber}";
                errorText.text = $"Errors: {step.Errors}";
                hintText.text = $"Hints: {step.Hints}";
                timeText.text = $"Time: {step.TimeSpent:F2}s";
            }
        }
    }
}
