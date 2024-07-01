using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections.Generic;


public class Manager : MonoBehaviour
{
    [SerializeField] private string currentPrefabName;
    [SerializeField] private SaveSequence saveSequence;
    [SerializeField] private GameObject currentSelectedComponent;
    [SerializeField] private bool isRecording = false;
    [SerializeField] private bool isPlayBacking = false;
    [SerializeField] private bool isModelConfirmed = false;
    [SerializeField] private bool finishedRecording = false;
 

    [SerializeField] private List<GameObject> sequenceOrderList = new List<GameObject>();

    void Update()
    {
        if (isRecording)
        {
        }

        if (isPlayBacking)
        {
        }
    }

    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        currentSelectedComponent = args.interactableObject.transform.gameObject;
        // Add current selected component to the list
        if (isRecording && !sequenceOrderList.Contains(currentSelectedComponent))
        {
            sequenceOrderList.Add(currentSelectedComponent);
        }
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {
        //if (currentSelectedComponent == args.interactableObject.transform.gameObject)
        //{
        //    currentSelectedComponent = null;
        //    Debug.Log("Deselected: " + args.interactableObject.transform.gameObject.name);
        //}
    }

    public void SetRecording(bool recording)
    {
        isRecording = recording;
    }

    public bool GetRecording()
    {
        return isRecording;
    }

    public void SetPlayBacking(bool playBacking)
    {
        isPlayBacking = playBacking;
    }

    public bool GetPlayBacking()
    {
        return isPlayBacking;
    }

    public void setModelConfirmed(bool modelConfermation)
    {
        isModelConfirmed = modelConfermation;
    }

    public bool GetModelConfirmed()
    {
        return isModelConfirmed;
    }

    public void SetCurrentSelectedPrefabName(string name)
    {
        currentPrefabName = name;
    }

    public string GetCurrentSelectedPrefabName()
    {
        return currentPrefabName;
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
            saveSequence.SaveSequenceToJSON(currentPrefabName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }
}
