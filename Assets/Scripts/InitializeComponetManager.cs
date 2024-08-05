using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializeComponentManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown componentDropdown;
    [SerializeField] private GameObject canvasInit;
    [SerializeField] private Button finishedButton;
    [SerializeField] private TextMeshProUGUI componentName;

    private void Awake()
    {
        Manager.OnStateChanged += SetPanelActive;
        finishedButton.onClick.AddListener(OnFinishedButtonClick);
    }

    private void OnFinishedButtonClick()
    {
        Manager.Instance.UpdateState(State.SelectingMode);
    }

    private void OnDestroy()
    {
        Manager.OnStateChanged -= SetPanelActive;
        finishedButton.onClick.RemoveListener(OnFinishedButtonClick);
    }
    private void SetPanelActive(State state)
    {
        canvasInit.SetActive(state == State.Initialize);
    }

    void Start()
    {
        // Populate the dropdown options
        PopulateDropdown();

        // Add listener to handle dropdown value changes
        componentDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        // Update dropdown to reflect the current component type
        UpdateDropdownForSelectedComponent();
    }

    private void Update()
    {

        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();
        if (selectedComponent != null)
        {
            componentName.text = selectedComponent.name;
            UpdateDropdownForSelectedComponent();
        }

    }

    void PopulateDropdown()
    {
        // Clear existing options
        componentDropdown.ClearOptions();

        // Create a new list of options
        List<string> options = new List<string>();
        foreach (ComponentObject.ComponentType type in System.Enum.GetValues(typeof(ComponentObject.ComponentType)))
        {
            options.Add(type.ToString());
        }

        // Add options to the dropdown
        componentDropdown.AddOptions(options);
    }

    void OnDropdownValueChanged(int index)
    {
        // Update selected component type based on dropdown selection
        ComponentObject.ComponentType selectedType = (ComponentObject.ComponentType)index;

        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();
        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                componentObject.SetComponentType(selectedType);
                InitializeSelectedComponent(selectedComponent);
            }
        }
    }

    public void InitializeSelectedComponent(GameObject selectedComponent)
    {
        if (selectedComponent == null)
        {
            Debug.LogError("No component selected.");
            return;
        }

        // Get the ComponentObject script
        ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

        // Remove existing components of type Screw or Nail
        RemoveExistingScripts<Screw>(selectedComponent);
        RemoveExistingScripts<Nail>(selectedComponent);
        selectedComponent.tag = "Untagged";
        // Add the selected component script
        switch (componentObject.GetComponentType())
        {
            case ComponentObject.ComponentType.Screw:
                selectedComponent.AddComponent<Screw>();
                break;
            case ComponentObject.ComponentType.Nail:
                selectedComponent.AddComponent<Nail>();
                break;
            case ComponentObject.ComponentType.None:
                selectedComponent.tag = "Component";
                break;
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

    void UpdateDropdownForSelectedComponent()
    {
        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();

        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                // Update the dropdown value to reflect the current component type
                componentDropdown.value = (int)componentObject.GetComponentType();
            }
        }
    }
}
