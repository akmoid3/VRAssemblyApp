using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializeComponentManager : MonoBehaviour
{
    public TMP_Dropdown componentDropdown;
    public TMP_Dropdown groupDropdown;
    public Button plusGroupButton; // Button to automatically create new group
    public GameObject canvasInit;
    public Button finishedButton;
    public Button loadInstructionPDF;
    public TextMeshProUGUI componentName;
    [SerializeField] private FileBrowserManager fileBrowserManager;

    private List<string> groups = new List<string> { "None" }; // Initial group list

    public FileBrowserManager FileBrowserManager { get => fileBrowserManager; set => fileBrowserManager = value; }

    public void Awake()
    {
        StateManager.OnStateChanged += SetPanelActive;
        StateManager.OnStateChanged += OnStateChanged; // Subscribe to state changes
        if (finishedButton != null)
            finishedButton.onClick.AddListener(OnFinishedButtonClick);
        if (loadInstructionPDF != null)
            loadInstructionPDF.onClick.AddListener(OpenFileBrowser);
        if (plusGroupButton != null)
            plusGroupButton.onClick.AddListener(CreateNextGroup);
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
        StateManager.OnStateChanged -= OnStateChanged; // Unsubscribe from state changes
        if (finishedButton != null)
            finishedButton.onClick.RemoveListener(OnFinishedButtonClick);
        if (loadInstructionPDF != null)
            loadInstructionPDF.onClick.RemoveListener(OpenFileBrowser);
        if (plusGroupButton != null)
            plusGroupButton.onClick.RemoveListener(CreateNextGroup);
    }

    private void SetPanelActive(State state)
    {
        canvasInit.SetActive(state == State.Initialize);
    }

    private void OnStateChanged(State state)
    {
        if (state == State.Initialize)
        {
            PopulateGroupsFromComponents();
        }
    }

    public void Start()
    {
        // Populate the dropdown options
        PopulateComponentDropdown();
        PopulateGroupDropdown();

        // Add listeners to handle dropdown value changes
        componentDropdown.onValueChanged.AddListener(OnComponentDropdownValueChanged);
        groupDropdown.onValueChanged.AddListener(OnGroupDropdownValueChanged);

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

        // Add options to the dropdown
        groupDropdown.AddOptions(groups);
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
        string selectedGroup = groupDropdown.options[index].text;

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
    public void PopulateGroupsFromComponents()
    {
        foreach (Transform component in Manager.Instance.Components)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            if (componentObject != null)
            {
                string group = componentObject.GetGroup();
                if (!groups.Contains(group))
                {
                    groups.Add(group);
                }
            }
        }

        // Sort the groups list except the first element
        if (groups.Count > 1)
        {
            List<string> groupsToSort = groups.GetRange(1, groups.Count - 1);
            groupsToSort.Sort();
            groups = new List<string> { groups[0] }; // Keep the first element
            groups.AddRange(groupsToSort); // Add the sorted elements back
        }

        // Update the group dropdown with the new groups
        PopulateGroupDropdown();
    }

    public void CreateNextGroup()
    {
        int nextGroupNumber = groups.Count;
        string newGroupName = $"Group{nextGroupNumber:D2}";

        if (!groups.Contains(newGroupName))
        {
            groups.Add(newGroupName);
            PopulateGroupDropdown();
            groupDropdown.value = groups.IndexOf(newGroupName); // Select the newly created group
        }
    }

    public void UpdateDropdownsForSelectedComponent(GameObject selectedComponent)
    {
        if (selectedComponent != null)
        {
            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                // Ensure the group is in the list
                if (!groups.Contains(componentObject.GetGroup()))
                {
                    groups.Add(componentObject.GetGroup());
                    PopulateGroupDropdown();
                }

                // Update the dropdown values to reflect the current component type and group
                componentDropdown.value = (int)componentObject.GetComponentType();
                groupDropdown.value = groups.IndexOf(componentObject.GetGroup());
            }
        }
    }
}
