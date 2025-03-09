using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class InitializeComponentManagerTests
{
    private InitializeComponentManager componentManager;
    private TMP_Dropdown componentDropdown;
    private TMP_Dropdown groupDropdown;
    private TMP_Dropdown axisDropdown;
    private Button plusGroupButton;
    private Button deleteComponent;
    private Button finishedButton;
    private Button loadInstructionPDF;
    private GameObject selectedComponent;
    private ComponentObject mockComponentObject;
    private Manager manager;
    private StateManager stateManager;
    private InteractionManager interactionManager;
    private FileBrowserManager testFileBrowserManager;

    [SetUp]
    public void SetUp()
    {
        // Set up FileBrowserManager
        testFileBrowserManager = new GameObject().AddComponent<FileBrowserManager>();
        SetPrivateField(testFileBrowserManager, "fileBrowserCanvas", new GameObject());

        // Set up Manager and StateManager
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();
        
        // Create a new GameObject and attach the InitializeComponentManager component
        var gameObject = new GameObject();
        componentManager = gameObject.AddComponent<InitializeComponentManager>();

        // Create and assign dropdowns
        componentDropdown = new GameObject().AddComponent<TMP_Dropdown>();
        groupDropdown = new GameObject().AddComponent<TMP_Dropdown>();
        axisDropdown = new GameObject().AddComponent<TMP_Dropdown>();
        componentManager.componentDropdown = componentDropdown;
        componentManager.groupDropdown = groupDropdown;
        componentManager.axisDropdown = axisDropdown;

        // Create and assign buttons
        plusGroupButton = new GameObject().AddComponent<Button>();
        deleteComponent = new GameObject().AddComponent<Button>();
        finishedButton = new GameObject().AddComponent<Button>();
        loadInstructionPDF = new GameObject().AddComponent<Button>();
        componentManager.plusGroupButton = plusGroupButton;
        componentManager.deleteComponent = deleteComponent;
        componentManager.finishedButton = finishedButton;
        componentManager.loadInstructionPDF = loadInstructionPDF;

        // Set up InteractionManager
        interactionManager = new GameObject().AddComponent<InteractionManager>();
        
        // Setup a mock selected component
        selectedComponent = new GameObject("MockComponent");
        mockComponentObject = selectedComponent.AddComponent<ComponentObject>();

        // Set up Manager's dependencies
        SetPrivateField(manager, "interactionManager", interactionManager);
        manager.CurrentSelectedComponent = selectedComponent;
        
        // Initialize Components and RemovedComponents lists
        manager.Components = new List<Transform>();
        manager.RemovedComponents = new List<Transform>();

        // Set up remaining InitializeComponentManager properties
        componentManager.canvasInit = new GameObject();
        componentManager.componentName = new GameObject().AddComponent<TextMeshProUGUI>();
        componentManager.FileBrowserManager = testFileBrowserManager;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(componentManager.gameObject);
        Object.DestroyImmediate(selectedComponent);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(interactionManager.gameObject);
        
        // Reset static event handlers
        var field = typeof(StateManager).GetField("OnStateChanged", BindingFlags.Public | BindingFlags.Static);
        if (field != null)
        {
            field.SetValue(null, null);
        }
    }

    [Test]
    public void TestAwake_RegistersEventHandlers()
    {
        // Act
        componentManager.Awake();
        
        // Manually invoke the event to test if the handler is registered
        
        
        // Reset canvas state for testing
        componentManager.canvasInit.SetActive(false);
        
        // Invoke the event with Initialize state
        stateManager.UpdateState(State.Initialize);
        
        // Assert
        Assert.IsTrue(componentManager.canvasInit.activeSelf);
    }

    [Test]
    public void TestOnDestroy_UnregistersEventHandlers()
    {
        // Arrange - Register event handlers
        componentManager.Awake();
        
        // Act - Manually invoke OnDestroy
        var onDestroyMethod = typeof(InitializeComponentManager).GetMethod("OnDestroy", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        onDestroyMethod.Invoke(componentManager, null);
        
        // Reset canvas state for testing
        componentManager.canvasInit.SetActive(false);
        
        // Manually invoke the event
        stateManager.UpdateState(State.Initialize);
        
        // Assert - The canvas should not be activated because the handler is unregistered
        Assert.IsFalse(componentManager.canvasInit.activeSelf);
    }

    [UnityTest]
    public IEnumerator Update_UpdatesComponentNameAndDropdowns_WhenSelectedComponentIsNotNull()
    {
        // Arrange
        componentManager.componentName.text = ""; // Ensure it's initially empty

        // Act
        componentManager.Update();

        // Assert
        Assert.AreEqual("MockComponent", componentManager.componentName.text);
        yield return null;
    }

    [Test]
    public void TestPopulateComponentDropdown()
    {
        // Act
        componentManager.PopulateComponentDropdown();

        // Assert: verify that dropdown has options and each option has non-empty text
        Assert.IsTrue(componentDropdown.options.Count > 0);
        foreach (var option in componentDropdown.options)
        {
            Assert.IsFalse(string.IsNullOrEmpty(option.text));
        }
    }

    [Test]
    public void TestPopulateGroupDropdown()
    {
        // Act
        componentManager.PopulateGroupDropdown();

        // Assert: verify that dropdown has options and each option has non-empty text
        Assert.IsTrue(groupDropdown.options.Count > 0);
        foreach (var option in groupDropdown.options)
        {
            Assert.IsFalse(string.IsNullOrEmpty(option.text));
        }
    }

    [Test]
    public void TestOnComponentDropdownValueChanged()
    {
        // Arrange
        int index = 0;

        // Act
        componentManager.OnComponentDropdownValueChanged(index);

        // Assert
        Assert.AreEqual((ComponentObject.ComponentType)index, mockComponentObject.GetComponentType());
    }

    [Test]
    public void TestOnGroupDropdownValueChanged()
    {
        // Arrange
        int index = 0;
        componentManager.PopulateGroupDropdown();
        string expectedValue = groupDropdown.options[index].text;

        // Act
        componentManager.OnGroupDropdownValueChanged(index);

        // Assert: verify that the value set in ComponentObject matches the option text
        Assert.AreEqual(expectedValue, mockComponentObject.GetGroup());
    }

    [Test]
    public void TestUpdateDropdownsForSelectedComponent()
    {
        // Arrange
        // Set ComponentObject with "None" values (or default value used)
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.None);
        mockComponentObject.SetGroup("None");

        // Act: update dropdowns based on the selected component properties
        componentManager.UpdateDropdownsForSelectedComponent(selectedComponent);

        // Assert
        Assert.AreEqual((int)ComponentObject.ComponentType.None, componentDropdown.value);

        int groupIndex = groupDropdown.options.FindIndex(option => option.text == "None");
        Assert.AreEqual(groupIndex, groupDropdown.value);
    }

    [Test]
    public void TestUpdateDropdownsForSelectedComponent_WithScrew_EnablesAxisDropdown()
    {
        // Arrange
        axisDropdown.gameObject.SetActive(false);
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        
        // Act
        componentManager.UpdateDropdownsForSelectedComponent(selectedComponent);
        
        // Assert
        Assert.IsTrue(axisDropdown.gameObject.activeSelf);
    }

    [Test]
    public void TestUpdateDropdownsForSelectedComponent_WithBoard_DisablesAxisDropdown()
    {
        // Arrange
        axisDropdown.gameObject.SetActive(true);
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.None);
        
        // Act
        componentManager.UpdateDropdownsForSelectedComponent(selectedComponent);
        
        // Assert
        Assert.IsFalse(axisDropdown.gameObject.activeSelf);
    }

    [Test]
    public void TestOpenFileBrowser_CallsShowDialog()
    {
        // Act
        componentManager.OpenFileBrowser();

        // Get the private field value using the GetPrivateField method
        var fileBrowserCanvas = GetPrivateField(testFileBrowserManager, "fileBrowserCanvas") as GameObject;

        // Assert that the canvas is not null and is active in the hierarchy
        Assert.IsNotNull(fileBrowserCanvas, "fileBrowserCanvas is null");
        Assert.IsTrue(fileBrowserCanvas.activeInHierarchy, "fileBrowserCanvas is not active in the hierarchy");
    }

    [Test]
    public void TestOnFinishedButtonClick_UpdatesState()
    {
        // Act
        componentManager.OnFinishedButtonClick();

        // Assert
        Assert.AreEqual(State.SelectingMode, stateManager.CurrentState);
    }

    [Test]
    public void TestSetPanelActive_SetsCanvasActive_WhenStateIsInitialize()
    {
        // Arrange
        componentManager.canvasInit.SetActive(false);
        
        // Act - Use reflection to call private method
        var setPanelActiveMethod = typeof(InitializeComponentManager).GetMethod("SetPanelActive", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        setPanelActiveMethod.Invoke(componentManager, new object[] { State.Initialize });
        
        // Assert
        Assert.IsTrue(componentManager.canvasInit.activeSelf);
    }

    [Test]
    public void TestSetPanelActive_SetsCanvasInactive_WhenStateIsNotInitialize()
    {
        // Arrange
        componentManager.canvasInit.SetActive(true);
        
        // Act - Use reflection to call private method
        var setPanelActiveMethod = typeof(InitializeComponentManager).GetMethod("SetPanelActive", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        setPanelActiveMethod.Invoke(componentManager, new object[] { State.SelectingMode });
        
        // Assert
        Assert.IsFalse(componentManager.canvasInit.activeSelf);
    }

    [Test]
    public void TestPopulateGroupsFromComponents()
    {
        // Arrange
        // Create test components with different groups
        var comp1 = new GameObject("Comp1");
        var comp1Object = comp1.AddComponent<ComponentObject>();
        comp1Object.SetGroup("Group01");
        
        var comp2 = new GameObject("Comp2");
        var comp2Object = comp2.AddComponent<ComponentObject>();
        comp2Object.SetGroup("Group02");
        
        // Add components to Manager.Components
        manager.Components.Add(comp1.transform);
        manager.Components.Add(comp2.transform);
        
        // Clear groups and add initial "None" group
        var groupsField = typeof(InitializeComponentManager).GetField("groups", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var groups = new List<string> { "None" };
        groupsField.SetValue(componentManager, groups);
        
        // Act
        componentManager.PopulateGroupsFromComponents();
        
        // Assert
        var updatedGroups = groupsField.GetValue(componentManager) as List<string>;
        Assert.AreEqual(3, updatedGroups.Count);
        Assert.IsTrue(updatedGroups.Contains("None"));
        Assert.IsTrue(updatedGroups.Contains("Group01"));
        Assert.IsTrue(updatedGroups.Contains("Group02"));
        
        // Clean up
        Object.DestroyImmediate(comp1);
        Object.DestroyImmediate(comp2);
    }

    [Test]
    public void TestCreateNextGroup()
    {
        // Arrange
        // Set initial groups
        var groupsField = typeof(InitializeComponentManager).GetField("groups", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var groups = new List<string> { "None" };
        groupsField.SetValue(componentManager, groups);
        
        // Act
        componentManager.CreateNextGroup();
        
        // Assert
        var updatedGroups = groupsField.GetValue(componentManager) as List<string>;
        Assert.AreEqual(2, updatedGroups.Count);
        Assert.AreEqual("Group01", updatedGroups[1]);
    }

    [Test]
    public void TestDeleteComponent()
    {
        // Arrange
        manager.Components.Add(selectedComponent.transform);
        
        // Act
        componentManager.DeleteComponent();
        
        // Assert
        Assert.AreEqual(0, manager.Components.Count);
        Assert.AreEqual(1, manager.RemovedComponents.Count);
        Assert.IsFalse(selectedComponent.activeSelf);
        Assert.IsTrue(mockComponentObject.IsDestroyed);
    }

    [Test]
    public void TestRestoreDeletedComponents()
    {
        // Arrange
        selectedComponent.SetActive(false);
        mockComponentObject.IsDestroyed = true;
        manager.RemovedComponents.Add(selectedComponent.transform);
        
        // Add MakeGrabbable component for testing
        var makeGrabbable = selectedComponent.AddComponent<MakeGrabbable>();
        
        // Act
        componentManager.RestoreDeletedComponents();
        
        // Assert
        Assert.AreEqual(1, manager.Components.Count);
        Assert.AreEqual(0, manager.RemovedComponents.Count);
        Assert.IsTrue(selectedComponent.activeSelf);
        Assert.IsFalse(mockComponentObject.IsDestroyed);
    }

    [Test]
    public void TestPopulateAxisDropdown()
    {
        // Arrange
        axisDropdown.ClearOptions();
        
        // Act - Use reflection to call private method
        var populateAxisMethod = typeof(InitializeComponentManager).GetMethod("PopulateAxisDropdown", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        populateAxisMethod.Invoke(componentManager, null);
        
        // Assert
        Assert.AreEqual(6, axisDropdown.options.Count); // X, Y, Z, -X, -Y, -Z
        Assert.AreEqual("X", axisDropdown.options[0].text);
        Assert.AreEqual("Y", axisDropdown.options[1].text);
        Assert.AreEqual("Z", axisDropdown.options[2].text);
        Assert.AreEqual("- X", axisDropdown.options[3].text);
        Assert.AreEqual("- Y", axisDropdown.options[4].text);
        Assert.AreEqual("- Z", axisDropdown.options[5].text);
    }

    [Test]
    public void TestOnAxisDropdownValueChanged()
    {
        // Arrange
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        
        // Get private fields
        var selectedAxisField = typeof(InitializeComponentManager).GetField("selectedAxis", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Act - Test each axis option
        TestAxisChange(0, Vector3.right);
        TestAxisChange(1, Vector3.up);
        TestAxisChange(2, Vector3.forward);
        TestAxisChange(3, Vector3.right * -1.0f);
        TestAxisChange(4, Vector3.up * -1.0f);
        TestAxisChange(5, Vector3.forward * -1.0f);
        
        // Local helper method to test axis changes
        void TestAxisChange(int dropdownIndex, Vector3 expectedAxis)
        {
            // Call private method
            var onAxisChangedMethod = typeof(InitializeComponentManager).GetMethod("OnAxisDropdownValueChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            onAxisChangedMethod.Invoke(componentManager, new object[] { dropdownIndex });
            
            // Get updated axis value
            var currentAxis = (Vector3)selectedAxisField.GetValue(componentManager);
            
            // Verify axis was updated
            Assert.AreEqual(expectedAxis.x, currentAxis.x, 0.001f);
            Assert.AreEqual(expectedAxis.y, currentAxis.y, 0.001f);
            Assert.AreEqual(expectedAxis.z, currentAxis.z, 0.001f);
        }
    }

    [Test]
    public void TestUpdateSelectedFastener()
    {
        // Arrange
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        
        // Get private fields
        var selectedAxisField = typeof(InitializeComponentManager).GetField("selectedAxis", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Set selected axis to right
        selectedAxisField.SetValue(componentManager, Vector3.right);
        
        // Act - Call private method
        var updateSelectedFastenerMethod = typeof(InitializeComponentManager).GetMethod("UpdateSelectedFastener", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        updateSelectedFastenerMethod.Invoke(componentManager, null);
        
        // Assert
        Assert.AreEqual(Vector3.right, mockComponentObject.GetSelectedAxis());
    }

    [Test]
    public void TestStart_InitializesDropdownsAndAddListeners()
    {
        // Act
        componentManager.Start();
        
        // Assert
        // Check that dropdowns are populated
        Assert.IsTrue(componentDropdown.options.Count > 0);
        Assert.IsTrue(groupDropdown.options.Count > 0);
        
        // Check that listeners are added (indirectly by verifying the delegates are not null)
        var hasComponentListener = componentDropdown.onValueChanged.GetPersistentEventCount() > 0;
        var hasGroupListener = groupDropdown.onValueChanged.GetPersistentEventCount() > 0;
        
        Assert.IsTrue(hasComponentListener);
        Assert.IsTrue(hasGroupListener);
    }

    [Test]
    public void TestOnStateChanged_InitializeState_PopulatesGroups()
    {
        // Arrange
        var onStateChangedMethod = typeof(InitializeComponentManager).GetMethod("OnStateChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Create a component to ensure groups are populated
        var comp = new GameObject("Comp");
        var compObject = comp.AddComponent<ComponentObject>();
        compObject.SetGroup("TestGroup");
        manager.Components.Add(comp.transform);
        
        // Act
        onStateChangedMethod.Invoke(componentManager, new object[] { State.Initialize });
        
        // Assert
        var groupsField = typeof(InitializeComponentManager).GetField("groups", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var groups = groupsField.GetValue(componentManager) as List<string>;
        
        Assert.IsTrue(groups.Contains("TestGroup"));
        
        // Clean up
        Object.DestroyImmediate(comp);
    }

    [Test]
    public void TestOnStateChanged_NonInitializeState_DoesNotPopulateGroups()
    {
        // Arrange
        var onStateChangedMethod = typeof(InitializeComponentManager).GetMethod("OnStateChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Set initial groups
        var groupsField = typeof(InitializeComponentManager).GetField("groups", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var initialGroups = new List<string> { "None" };
        groupsField.SetValue(componentManager, initialGroups);
        
        // Act
        onStateChangedMethod.Invoke(componentManager, new object[] { State.SelectingMode });
        
        // Assert
        var groups = groupsField.GetValue(componentManager) as List<string>;
        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual("None", groups[0]);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {obj.GetType()}");
        }
    }

    private object GetPrivateField(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(obj);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {obj.GetType()}");
            return null;
        }
    }
}