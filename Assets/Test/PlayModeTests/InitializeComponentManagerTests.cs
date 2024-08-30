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
    private GameObject selectedComponent;
    private ComponentObject mockComponentObject;
    private Manager manager;
    private StateManager stateManager;
    private InteractionManager interactionManager;
    private FileBrowserManager testFileBrowserManager;



    [SetUp]
    public void SetUp()
    {
        testFileBrowserManager = new GameObject().AddComponent<FileBrowserManager>();
        SetPrivateField(testFileBrowserManager, "fileBrowserCanvas", new GameObject());

        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a new GameObject and attach the InitializeComponentManager component
        var gameObject = new GameObject();
        componentManager = gameObject.AddComponent<InitializeComponentManager>();

        // Create and assign dropdowns
        componentDropdown = new GameObject().AddComponent<TMP_Dropdown>();
        groupDropdown = new GameObject().AddComponent<TMP_Dropdown>();
        componentManager.componentDropdown = componentDropdown;
        componentManager.groupDropdown = groupDropdown;

        interactionManager = new GameObject().AddComponent<InteractionManager>();
        // Setup a mock selected component
        selectedComponent = new GameObject("MockComponent");
        mockComponentObject = selectedComponent.AddComponent<ComponentObject>();


        SetPrivateField(manager, "interactionManager", interactionManager);
        manager.CurrentSelectedComponent = selectedComponent;


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

        // Assert
        Assert.IsTrue(componentDropdown.options.Count > 0);
        foreach (var option in componentDropdown.options)
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ComponentObject.ComponentType), option.text));
        }
    }

    [Test]
    public void TestPopulateGroupDropdown()
    {
        // Act
        componentManager.PopulateGroupDropdown();

        // Assert
        Assert.IsTrue(groupDropdown.options.Count > 0);
        foreach (var option in groupDropdown.options)
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ComponentObject.Group), option.text));
        }
    }

    [Test]
    public void TestOnComponentDropdownValueChanged()
    {
        // Arrange
        int index = 0;
        componentManager.OnComponentDropdownValueChanged(index);

        // Assert
        Assert.AreEqual((ComponentObject.ComponentType)index, mockComponentObject.GetComponentType());
    }

    [Test]
    public void TestOnGroupDropdownValueChanged()
    {
        // Arrange
        int index = 0;
        componentManager.OnGroupDropdownValueChanged(index);

        // Assert
        Assert.AreEqual((ComponentObject.Group)index, mockComponentObject.GetGroup());
    }

    [Test]
    public void TestUpdateDropdownsForSelectedComponent()
    {
        // Arrange
        mockComponentObject.SetComponentType(ComponentObject.ComponentType.None);
        mockComponentObject.SetGroup(ComponentObject.Group.None);

        // Act
        componentManager.UpdateDropdownsForSelectedComponent(selectedComponent);

        // Assert
        Assert.AreEqual((int)ComponentObject.ComponentType.None, componentDropdown.value);
        Assert.AreEqual((int)ComponentObject.Group.None, groupDropdown.value);
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
