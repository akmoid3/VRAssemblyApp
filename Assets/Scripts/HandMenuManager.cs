using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField] private Manager manager;
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

    [SerializeField] private TMP_Dropdown incrementDropdown;

    private void Start()
    {
        // Set up button listeners
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

        // Set up dropdown listener
        incrementDropdown.onValueChanged.AddListener(UpdateIncrement);
        UpdateIncrement(incrementDropdown.value); // Set initial increment based on dropdown value
    }

    // Update is called once per frame
    void Update()
    {
        GameObject currentSelectedComponent = manager.GetCurrentSelectedComponent();
        if (currentSelectedComponent != null)
        {
            Vector3 position = currentSelectedComponent.transform.position;
            Vector3 rotation = currentSelectedComponent.transform.eulerAngles;

            componentNameText.text = currentSelectedComponent.name;
            positionXText.text = $"{position.x:F2}";
            positionYText.text = $"{position.y:F2}";
            positionZText.text = $"{position.z:F2}";
            rotationXText.text = $"{rotation.x:F2}";
            rotationYText.text = $"{rotation.y:F2}";
            rotationZText.text = $"{rotation.z:F2}";
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
    }

    private void AddToPosition(Vector3 increment)
    {
        if (manager.GetCurrentSelectedComponent() != null)
        {
            manager.GetCurrentSelectedComponent().transform.position += increment;
        }
    }

    private void AddToRotation(Vector3 increment)
    {
        if (manager.GetCurrentSelectedComponent() != null)
        {
            manager.GetCurrentSelectedComponent().transform.eulerAngles += increment;
        }
    }

    private void UpdateIncrement(int index)
    {
        switch (index)
        {
            case 0:
                increment = 0.1f;
                break;
            case 1:
                increment = 0.05f;
                break;
            case 2:
                increment = 0.01f;
                break;
            case 3:
                increment = 1f;
                break;
            default:
                increment = 0.1f;
                break;
        }
    }
}
