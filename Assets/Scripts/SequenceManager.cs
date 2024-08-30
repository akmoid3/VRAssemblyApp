using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{

    [SerializeField] private SaveSequence saveSequence;
    [SerializeField] private List<ComponentData> assemblySequence;
    [SerializeField] private int currentStep;
    [SerializeField] private int errorCount;
    [SerializeField] private string finishTime;

    public static event Action<int> OnErrorCountChanged;
    public static event Action<int> OnStepChanged;

    // Initializes the sequence with the given list
    public virtual void InitializeSequence(List<ComponentData> sequence)
    {
        assemblySequence = sequence;
        currentStep = 0;
        errorCount = 0;
    }

    // Moves to the next step in the sequence
    public virtual void IncrementCurrentStep()
    {
        currentStep++;
        OnStepChanged?.Invoke(currentStep);
    }

    // Increments the error count and triggers the related event
    public virtual void IncrementCurrentError()
    {
        errorCount++;
        OnErrorCountChanged?.Invoke(errorCount);
    }

    // Saves the current sequence and component data
    public virtual void SaveBuildingSequence(GameObject currentSelectedComponent, string modelName)
    {
        if (saveSequence != null)
        {
            saveSequence.SaveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(modelName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    // Modifies the current sequence and component data
    public virtual void ModifyBuildingSequence(GameObject currentSelectedComponent, string modelName)
    {
        if (saveSequence != null)
        {
            saveSequence.ModifyComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(modelName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    // Removes a component from the sequence
    public virtual void RemoveComponentFromSequence(GameObject currentSelectedComponent, string modelName)
    {
        if (saveSequence != null)
        {
            saveSequence.RemoveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(modelName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    // Checks the validity of the current component
    public virtual void ValidateComponent(GameObject component)
    {
        ComponentData expectedComponent = assemblySequence[currentStep];
        if (component.name != expectedComponent.componentName && component.GetComponent<ComponentObject>().GetGroup() != expectedComponent.group)
        {
            IncrementCurrentError();
        }
    }

    public List<ComponentData> AssemblySequence { get => assemblySequence; set => assemblySequence = value; }
    public int CurrentStep { get => currentStep; set => currentStep = value; }
    public string FinishTime { get => finishTime; set => finishTime = value; }
    public int ErrorCount { get => errorCount; set => errorCount = value; }
}

