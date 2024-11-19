using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitializeComponentManager : MonoBehaviour
{
    public TMP_Dropdown componentDropdown;
    public TMP_Dropdown groupDropdown;
    public Button plusGroupButton; // Button to automatically create new group
    public Button deleteComponent; // Button to delete components
    public GameObject canvasInit;
    public Button finishedButton;
    public Button loadInstructionPDF;
    public TextMeshProUGUI componentName;
    [SerializeField] private FileBrowserManager fileBrowserManager;

    public TMP_Dropdown axisDropdown;

    private Vector3 selectedAxis = Vector3.forward;
    private int selectedDirection = 1;




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

        PopulateAxisDropdown();

        axisDropdown.onValueChanged.AddListener(OnAxisDropdownValueChanged);
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

    public void DeleteComponent()
    {
        var component = Manager.Instance.CurrentSelectedComponent;
        if (component)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            if (componentObject != null)
                componentObject.IsDestroyed = true;
            component.gameObject.SetActive(false);
            Manager.Instance.Components.Remove(component.transform);
            Manager.Instance.RemovedComponents.Add(component.transform);
        }
    }
    public void RestoreDeletedComponents()
    {
        var removedComponents = Manager.Instance.RemovedComponents;
        var componentsToRestore = new List<Transform>(); 

        foreach (var component in removedComponents)
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();
            if (componentObject != null)
            {
                componentsToRestore.Add(component.transform); 
                componentObject.IsDestroyed = false;
                component.gameObject.SetActive(true);
                Manager.Instance.Components.Add(component);
                MakeGrabbable makeGrabbable = componentObject.gameObject.GetComponent<MakeGrabbable>();
                if (makeGrabbable != null)
                    makeGrabbable.MakeObjectNonGrabbable();
            }
        }

        foreach (var component in componentsToRestore)
        {
            removedComponents.Remove(component);
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

                // Check if the component is a Screw, Nail, or WoodenPin
                if (componentObject.GetComponentType() == ComponentObject.ComponentType.Screw ||
                    componentObject.GetComponentType() == ComponentObject.ComponentType.Nail ||
                    componentObject.GetComponentType() == ComponentObject.ComponentType.WoodenPin)
                {
                    // Enable and update the axis and direction dropdowns
                    axisDropdown.gameObject.SetActive(true);

                    // Update the axis dropdown
                    Vector3 currentAxis = componentObject.GetSelectedAxis();
                    if (currentAxis == Vector3.right)
                    {
                        axisDropdown.value = 0; // X-axis
                    }
                    else if (currentAxis == Vector3.up)
                    {
                        axisDropdown.value = 1; // Y-axis
                    }
                    else if (currentAxis == Vector3.forward)
                    {
                        axisDropdown.value = 2; // Z-axis
                    }
                    else if (currentAxis == Vector3.right * -1.0f)
                    {
                        axisDropdown.value = 3; // X-axis
                    }
                    else if (currentAxis == Vector3.up * -1.0f)
                    {
                        axisDropdown.value = 4; // Y-axis
                    }
                    else if (currentAxis == Vector3.forward * -1.0f)
                    {
                        axisDropdown.value = 5; // Z-axis
                    }

                    // Update the direction dropdown
                }
                else
                {
                    // Hide the axis and direction dropdowns if not a Screw, Nail, or WoodenPin
                    axisDropdown.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PopulateAxisDropdown()
    {
        List<string> options = new List<string> { "X", "Y", "Z", "- X", "- Y", "- Z" };
        axisDropdown.AddOptions(options);
    }

    private void OnAxisDropdownValueChanged(int index)
    {
        switch (index)
        {
            case 0: selectedAxis = Vector3.right; break;
            case 1: selectedAxis = Vector3.up; break;
            case 2: selectedAxis = Vector3.forward; break;
            case 3: selectedAxis = Vector3.right * -1.0f; break;
            case 4: selectedAxis = Vector3.up * -1.0f; break;
            case 5: selectedAxis = Vector3.forward * -1.0f; break;



        }
        UpdateSelectedFastener();
    }

    private void UpdateSelectedFastener()
    {
        GameObject selectedComponent = Manager.Instance.CurrentSelectedComponent;

        if (selectedComponent != null)
        {

            ComponentObject componentObject = selectedComponent.GetComponent<ComponentObject>();

            if (componentObject != null)
            {

                if (componentObject.GetComponentType() == ComponentObject.ComponentType.Screw ||
                    componentObject.GetComponentType() == ComponentObject.ComponentType.Nail ||
                    componentObject.GetComponentType() == ComponentObject.ComponentType.WoodenPin)
                {
                    componentObject.SetSelectedAxis(selectedAxis);
                }
            }
        }
    }
}
