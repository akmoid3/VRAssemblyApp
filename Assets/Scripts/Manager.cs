using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections.Generic;


public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject currentSelectedPrefabName;
    [SerializeField] private SaveSequence saveSequence;
    [SerializeField] private GameObject currentSelectedComponent;
    [SerializeField] private bool isRecording = false;
    [SerializeField] private bool isPlayBacking = false;
    [SerializeField] private bool isModelConfirmed = false;
    [SerializeField] private bool finishedRecording = false;
    [SerializeField] private float increment = 0.1f;

    [SerializeField] private TMP_Text componentNameText;
    [SerializeField] private TMP_Text positionXText;
    [SerializeField] private TMP_Text positionYText;
    [SerializeField] private TMP_Text positionZText;
    [SerializeField] private TMP_Text rotationXText;
    [SerializeField] private TMP_Text rotationYText;
    [SerializeField] private TMP_Text rotationZText;

    [SerializeField] private Button addPosXButton;
    [SerializeField] private Button addPosYButton;
    [SerializeField] private Button addPosZButton;
    [SerializeField] private Button addRotXButton;
    [SerializeField] private Button addRotYButton;
    [SerializeField] private Button addRotZButton;

    [SerializeField] private Button reducePosXButton;
    [SerializeField] private Button reducePosYButton;
    [SerializeField] private Button reducePosZButton;
    [SerializeField] private Button reduceRotXButton;
    [SerializeField] private Button reduceRotYButton;
    [SerializeField] private Button reduceRotZButton;

    [SerializeField] private List<GameObject> sequenceOrderList = new List<GameObject>();

    private void Start()
    {
        addPosXButton.onClick.AddListener(() => AddToPosition(Vector3.right * increment));
        addPosYButton.onClick.AddListener(() => AddToPosition(Vector3.up * increment));
        addPosZButton.onClick.AddListener(() => AddToPosition(Vector3.forward * increment));
        addRotXButton.onClick.AddListener(() => AddToRotation(Vector3.right * increment));
        addRotYButton.onClick.AddListener(() => AddToRotation(Vector3.up * increment));
        addRotZButton.onClick.AddListener(() => AddToRotation(Vector3.forward * increment));

        reducePosXButton.onClick.AddListener(() => AddToPosition(Vector3.left * increment));
        reducePosYButton.onClick.AddListener(() => AddToPosition(Vector3.down * increment));
        reducePosZButton.onClick.AddListener(() => AddToPosition(Vector3.back * increment));
        reduceRotXButton.onClick.AddListener(() => AddToRotation(Vector3.left * increment));
        reduceRotYButton.onClick.AddListener(() => AddToRotation(Vector3.down * increment));
        reduceRotZButton.onClick.AddListener(() => AddToRotation(Vector3.back * increment));
    }

    void Update()
    {
        if (currentSelectedComponent != null)
        {
            Vector3 position = currentSelectedComponent.transform.position;
            Vector3 rotation = currentSelectedComponent.transform.eulerAngles;

            componentNameText.text = currentSelectedComponent.name;
            positionXText.text = $"{position.x:F1}";
            positionYText.text = $"{position.y:F1}";
            positionZText.text = $"{position.z:F1}";
            rotationXText.text = $"{rotation.x:F1}";
            rotationYText.text = $"{rotation.y:F1}";
            rotationZText.text = $"{rotation.z:F1}";
        }
        else
        {
            positionXText.text = "N/A";
            positionYText.text = "N/A";
            positionZText.text = "N/A";
            rotationXText.text = "N/A";
            rotationYText.text = "N/A";
            rotationZText.text = "N/A";
        }

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
        Debug.Log("Selected: " + currentSelectedComponent.name);

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

    private void AddToPosition(Vector3 increment)
    {
        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.position += increment;
        }
    }

    private void AddToRotation(Vector3 increment)
    {
        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.eulerAngles += increment;
        }
    }

    public void SetCurrentSelectedPrefabName(GameObject name)
    {
        currentSelectedPrefabName = name;
    }

    public GameObject GetCurrentSelectedPrefabName()
    {
        return currentSelectedPrefabName;
    }


    public void SaveBuildingSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.SaveComponentsSequence(sequenceOrderList);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }
}
