using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializeComponentManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown componentDropdown;
    [SerializeField] private TMP_Dropdown groupDropdown;
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
        PopulateComponentDropdown();
        PopulateGroupDropdown();  // Populate the new group dropdown

        // Add listeners to handle dropdown value changes
        componentDropdown.onValueChanged.AddListener(OnComponentDropdownValueChanged);
        groupDropdown.onValueChanged.AddListener(OnGroupDropdownValueChanged);  // New group dropdown listener

        // Update dropdowns to reflect the current component's type and group
        UpdateDropdownsForSelectedComponent();
    }

    private void Update()
    {
        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();
        if (selectedComponent != null)
        {
            componentName.text = selectedComponent.name;
            UpdateDropdownsForSelectedComponent();
        }
    }

    void PopulateComponentDropdown()
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

    void PopulateGroupDropdown()
    {
        // Clear existing options
        groupDropdown.ClearOptions();

        // Create a new list of options
        List<string> options = new List<string>();
        foreach (ComponentObject.Group group in System.Enum.GetValues(typeof(ComponentObject.Group)))
        {
            options.Add(group.ToString());
        }

        // Add options to the dropdown
        groupDropdown.AddOptions(options);
    }

    void OnComponentDropdownValueChanged(int index)
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
            }
        }
    }

    void OnGroupDropdownValueChanged(int index)
    {
        // Update selected component group based on dropdown selection
        ComponentObject.Group selectedGroup = (ComponentObject.Group)index;

        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();
        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                componentObject.SetGroup(selectedGroup);
            }
        }
    }

    void UpdateDropdownsForSelectedComponent()
    {
        GameObject selectedComponent = Manager.Instance.GetCurrentSelectedComponent();

        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                // Update the dropdown values to reflect the current component type and group
                componentDropdown.value = (int)componentObject.GetComponentType();
                groupDropdown.value = (int)componentObject.GetGroup();
            }
        }
    }
}
