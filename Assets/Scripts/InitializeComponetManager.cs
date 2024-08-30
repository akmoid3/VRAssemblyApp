using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializeComponentManager : MonoBehaviour
{
    public TMP_Dropdown componentDropdown;
    public TMP_Dropdown groupDropdown;
    public GameObject canvasInit;
    public Button finishedButton;
    public Button loadInstructionPDF;
    public TextMeshProUGUI componentName;
    [SerializeField] private FileBrowserManager fileBrowserManager;

    public FileBrowserManager FileBrowserManager { get => fileBrowserManager; set => fileBrowserManager = value; }

    public void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        if (finishedButton != null)
            finishedButton.onClick.AddListener(OnFinishedButtonClick);
        if (loadInstructionPDF != null)

            loadInstructionPDF.onClick.AddListener(OpenFileBrowser);
    }

    public void OpenFileBrowser()
    {
        fileBrowserManager.ShowDialog("Instructions", ".pdf");
    }

    public void OnFinishedButtonClick()
    {
        StateManager.Instance.UpdateState(State.SelectingMode);
    }

    private void OnDestroy()
    {
        StateManager.OnStateChanged -= SetPanelActive;
        if (finishedButton != null)

            finishedButton.onClick.RemoveListener(OnFinishedButtonClick);
        if (loadInstructionPDF != null)

        loadInstructionPDF.onClick.RemoveListener(OpenFileBrowser);

    }

    private void SetPanelActive(State state)
    {
        canvasInit.SetActive(state == State.Initialize);
    }

    public void Start()
    {
        // Populate the dropdown options
        PopulateComponentDropdown();
        PopulateGroupDropdown();  // Populate the new group dropdown

        // Add listeners to handle dropdown value changes
        componentDropdown.onValueChanged.AddListener(OnComponentDropdownValueChanged);
        groupDropdown.onValueChanged.AddListener(OnGroupDropdownValueChanged);  // New group dropdown listener

        // Update dropdowns to reflect the current component's type and group
        UpdateDropdownsForSelectedComponent(Manager.Instance.CurrentSelectedComponent);
    }

    public void Update()
    {
        GameObject selectedComponent = Manager.Instance.CurrentSelectedComponent;
        if (selectedComponent != null)
        {
            componentName.text = selectedComponent.name;
            UpdateDropdownsForSelectedComponent(selectedComponent);
        }
    }

    public void PopulateComponentDropdown()
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

    public void PopulateGroupDropdown()
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

    public void OnComponentDropdownValueChanged(int index)
    {
        // Update selected component type based on dropdown selection
        ComponentObject.ComponentType selectedType = (ComponentObject.ComponentType)index;

        GameObject selectedComponent = Manager.Instance.CurrentSelectedComponent;
        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                componentObject.SetComponentType(selectedType);
            }
        }
    }

    public void OnGroupDropdownValueChanged(int index)
    {
        // Update selected component group based on dropdown selection
        ComponentObject.Group selectedGroup = (ComponentObject.Group)index;

        GameObject selectedComponent = Manager.Instance.CurrentSelectedComponent;
        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                componentObject.SetGroup(selectedGroup);
            }
        }
    }

    public void UpdateDropdownsForSelectedComponent(GameObject selectedComponent)
    {

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
