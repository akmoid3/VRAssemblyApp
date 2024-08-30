using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMenuManagerTests
{
    private HandMenuManager _handMenuManager;
    private Manager manager;
    private StateManager stateManager;



    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        manager = new GameObject().AddComponent<Manager>();
        _handMenuManager = new GameObject().AddComponent<HandMenuManager>();
        _handMenuManager.Manager = manager;

        _handMenuManager.groupPanel = new GameObject("GroupPanel");
        _handMenuManager.handMenuPanel = new GameObject("HandMenuPanel");

        // Set up componentNameText
        _handMenuManager.componentNameText = new GameObject("ComponentNameText").AddComponent<TextMeshProUGUI>();

        // Set up position and rotation text fields using TextMeshProUGUI
        _handMenuManager.positionXText = new GameObject("PositionXText").AddComponent<TextMeshProUGUI>();
        _handMenuManager.positionYText = new GameObject("PositionYText").AddComponent<TextMeshProUGUI>();
        _handMenuManager.positionZText = new GameObject("PositionZText").AddComponent<TextMeshProUGUI>();
        _handMenuManager.rotationXText = new GameObject("RotationXText").AddComponent<TextMeshProUGUI>();
        _handMenuManager.rotationYText = new GameObject("RotationYText").AddComponent<TextMeshProUGUI>();
        _handMenuManager.rotationZText = new GameObject("RotationZText").AddComponent<TextMeshProUGUI>();


        // Create dummy buttons and add them to the HandMenuManager
        _handMenuManager.addPosXButton = new GameObject("AddPosXButton").AddComponent<Button>();
        _handMenuManager.addPosYButton = new GameObject("AddPosYButton").AddComponent<Button>();
        _handMenuManager.addPosZButton = new GameObject("AddPosZButton").AddComponent<Button>();
        _handMenuManager.addRotXButton = new GameObject("AddRotXButton").AddComponent<Button>();
        _handMenuManager.addRotYButton = new GameObject("AddRotYButton").AddComponent<Button>();
        _handMenuManager.addRotZButton = new GameObject("AddRotZButton").AddComponent<Button>();
        _handMenuManager.reducePosXButton = new GameObject("ReducePosXButton").AddComponent<Button>();
        _handMenuManager.reducePosYButton = new GameObject("ReducePosYButton").AddComponent<Button>();
        _handMenuManager.reducePosZButton = new GameObject("ReducePosZButton").AddComponent<Button>();
        _handMenuManager.reduceRotXButton = new GameObject("ReduceRotXButton").AddComponent<Button>();
        _handMenuManager.reduceRotYButton = new GameObject("ReduceRotYButton").AddComponent<Button>();
        _handMenuManager.reduceRotZButton = new GameObject("ReduceRotZButton").AddComponent<Button>();
        _handMenuManager.modifyButton = new GameObject("ModifyButton").AddComponent<Button>();
        _handMenuManager.addStepButton = new GameObject("AddStepButton").AddComponent<Button>();
        _handMenuManager.removeButton = new GameObject("RemoveButton").AddComponent<Button>();
        _handMenuManager.groupSelectionButton = new GameObject("GroupSelectionButton").AddComponent<Button>();
        _handMenuManager.groupSelectionButton2 = new GameObject("GroupSelectionButton2").AddComponent<Button>();
        _handMenuManager.saveComponentButton = new GameObject("SaveComponentButton").AddComponent<Button>();

        // Create a dummy dropdown
        _handMenuManager.incrementDropdown = new GameObject("IncrementDropdown").AddComponent<TMP_Dropdown>();

        // Manually call Start method
        _handMenuManager.Start();

        // Set up the list of buttons to deactivate
        _handMenuManager.AllButtonsToDeactivate = new List<Button>
        {
            new GameObject("AddPosXButton").AddComponent<Button>(),
            new GameObject("AddPosYButton").AddComponent<Button>(),
            new GameObject("AddPosZButton").AddComponent<Button>(),
            new GameObject("AddRotXButton").AddComponent<Button>(),
            new GameObject("AddRotYButton").AddComponent<Button>(),
            new GameObject("AddRotZButton").AddComponent<Button>(),
            new GameObject("ReducePosXButton").AddComponent<Button>(),
            new GameObject("ReducePosYButton").AddComponent<Button>(),
            new GameObject("ReducePosZButton").AddComponent<Button>(),
            new GameObject("ReduceRotXButton").AddComponent<Button>(),
            new GameObject("ReduceRotYButton").AddComponent<Button>(),
            new GameObject("ReduceRotZButton").AddComponent<Button>(),
            new GameObject("SaveComponentButton").AddComponent<Button>()
        };

    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(_handMenuManager.gameObject);
    }


    [Test]
    public void Update_WhenGroupHasChildren_ActivatesGroupSelectionButton()
    {
        // Arrange
        _handMenuManager.Group = new GameObject("Group");
        new GameObject("Child").transform.SetParent(_handMenuManager.Group.transform);

        // Act
        _handMenuManager.Update();

        // Assert
        Assert.IsTrue(_handMenuManager.groupSelectionButton.gameObject.activeSelf);
    }

    [Test]
    public void Update_WhenGroupHasNoChildren_DeactivatesGroupSelectionButton()
    {
        // Arrange
        _handMenuManager.Group = new GameObject("Group");

        // Act
        _handMenuManager.Update();

        // Assert
        Assert.IsFalse(_handMenuManager.groupSelectionButton.gameObject.activeSelf);
    }

    [Test]
    public void Start_InitializesUIElementsCorrectly()
    {
        // Assert that the panels are set to the correct active states
        Assert.IsFalse(_handMenuManager.groupPanel.activeSelf);
        Assert.IsTrue(_handMenuManager.handMenuPanel.activeSelf);
    }

    [Test]
    public void Start_AssignsManagerInstance()
    {
        // Assert that the manager instance is assigned correctly
        Assert.AreEqual(manager, _handMenuManager.Manager);
    }

    [Test]
    public void Start_InitializesButtonList()
    {
        // Arrange: Set up the expected list of buttons directly
        var expectedButtons = new List<Button>
    {
        _handMenuManager.addPosXButton,
        _handMenuManager.addPosYButton,
        _handMenuManager.addPosZButton,
        _handMenuManager.addRotXButton,
        _handMenuManager.addRotYButton,
        _handMenuManager.addRotZButton,
        _handMenuManager.reducePosXButton,
        _handMenuManager.reducePosYButton,
        _handMenuManager.reducePosZButton,
        _handMenuManager.reduceRotXButton,
        _handMenuManager.reduceRotYButton,
        _handMenuManager.reduceRotZButton,
        _handMenuManager.saveComponentButton
    };

        // Act: Clear the AllButtonsToDeactivate list and manually call Start to reinitialize it
        _handMenuManager.AllButtonsToDeactivate.Clear();
        _handMenuManager.Start();

        // Assert: Check that the initialized list matches the expected list
        CollectionAssert.AreEquivalent(expectedButtons, _handMenuManager.AllButtonsToDeactivate);
    }
    [Test]
    public void UpdateComponentUI_UpdatesTextFieldsCorrectly()
    {
        // Arrange
        var component = new GameObject("TestComponent");
        component.transform.position = new Vector3(1.2345f, 2.3456f, 3.4567f);
        component.transform.eulerAngles = new Vector3(45.1234f, 90.5678f, 180.8765f);

        // Act
        _handMenuManager.UpdateComponentUI(component);

        // Assert
        Assert.AreEqual("TestComponent", _handMenuManager.componentNameText.text);
        Assert.AreEqual("1,23", _handMenuManager.positionXText.text);
        Assert.AreEqual("2,35", _handMenuManager.positionYText.text);
        Assert.AreEqual("3,46", _handMenuManager.positionZText.text);
        Assert.AreEqual("45,12", _handMenuManager.rotationXText.text);
        Assert.AreEqual("90,57", _handMenuManager.rotationYText.text);
        Assert.AreEqual("180,88", _handMenuManager.rotationZText.text);
    }

    [Test]
    public void HandleComponentObject_WhenComponentIsNotGroupAndIsPlaced_DisablesTransformButtons()
    {
        // Arrange
        var component = new GameObject("TestComponent");
        var componentObject = component.AddComponent<ComponentObject>();
        componentObject.SetIsPlaced(true);

        // Act
        _handMenuManager.HandleComponentObject(component);

        // Assert
        Assert.IsTrue(_handMenuManager.modifyButton.gameObject.activeSelf);
        Assert.IsTrue(_handMenuManager.addStepButton.gameObject.activeSelf);
        Assert.IsTrue(_handMenuManager.removeButton.gameObject.activeSelf);

        foreach (var button in _handMenuManager.AllButtonsToDeactivate)
        {
            Assert.IsFalse(button.gameObject.activeSelf);
        }
    }

    [Test]
    public void HandleComponentObject_WhenComponentIsNotGroupAndIsNotPlaced_EnablesTransformButtons()
    {
        // Arrange
        var component = new GameObject("TestComponent");
        var componentObject = component.AddComponent<ComponentObject>();
        componentObject.SetIsPlaced(false);

        // Act
        _handMenuManager.HandleComponentObject(component);

        // Assert
        Assert.IsFalse(_handMenuManager.modifyButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.addStepButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.removeButton.gameObject.activeSelf);

        foreach (var button in _handMenuManager.AllButtonsToDeactivate)
        {
            Assert.IsTrue(button.gameObject.activeSelf);
        }
    }

    [Test]
    public void SetComponentModificationUI_SetsButtonActiveStatesCorrectly()
    {
        // Act - true
        _handMenuManager.SetComponentModificationUI(true);

        // Assert
        Assert.IsTrue(_handMenuManager.modifyButton.gameObject.activeSelf);
        Assert.IsTrue(_handMenuManager.addStepButton.gameObject.activeSelf);
        Assert.IsTrue(_handMenuManager.removeButton.gameObject.activeSelf);

        // Act - false
        _handMenuManager.SetComponentModificationUI(false);

        // Assert
        Assert.IsFalse(_handMenuManager.modifyButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.addStepButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.removeButton.gameObject.activeSelf);
    }

    [Test]
    public void ResetUIForNoSelection_ResetsTextFieldsAndDisablesButtons()
    {
        // Act
        _handMenuManager.ResetUIForNoSelection();

        // Assert
        Assert.AreEqual("N/A", _handMenuManager.positionXText.text);
        Assert.AreEqual("N/A", _handMenuManager.positionYText.text);
        Assert.AreEqual("N/A", _handMenuManager.positionZText.text);
        Assert.AreEqual("N/A", _handMenuManager.rotationXText.text);
        Assert.AreEqual("N/A", _handMenuManager.rotationYText.text);
        Assert.AreEqual("N/A", _handMenuManager.rotationZText.text);

        Assert.IsFalse(_handMenuManager.modifyButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.addStepButton.gameObject.activeSelf);
        Assert.IsFalse(_handMenuManager.removeButton.gameObject.activeSelf);

        foreach (var button in _handMenuManager.AllButtonsToDeactivate)
        {
            Assert.IsFalse(button.gameObject.activeSelf);
        }
    }

    [Test]
    public void UpdateGroupSelectionButton_WhenGroupHasChildren_ActivatesButton()
    {
        // Arrange
        _handMenuManager.Group = new GameObject("Group");
        new GameObject("Child").transform.SetParent(_handMenuManager.Group.transform);

        // Act
        _handMenuManager.UpdateGroupSelectionButton();

        // Assert
        Assert.IsTrue(_handMenuManager.groupSelectionButton.gameObject.activeSelf);
    }

    [Test]
    public void UpdateGroupSelectionButton_WhenGroupHasNoChildren_DeactivatesButton()
    {
        // Arrange
        _handMenuManager.Group = new GameObject("Group");

        // Act
        _handMenuManager.UpdateGroupSelectionButton();

        // Assert
        Assert.IsFalse(_handMenuManager.groupSelectionButton.gameObject.activeSelf);
    }


    [Test]
    [TestCase(0, 0.1f)]
    [TestCase(1, 0.05f)]
    [TestCase(2, 0.01f)]
    [TestCase(3, 1f)]
    [TestCase(4, 0.1f)] // Default case
    public void UpdateIncrement_ChangesIncrementCorrectly(int index, float expectedIncrement)
    {
        _handMenuManager.UpdateIncrement(index);
        Assert.AreEqual(expectedIncrement, _handMenuManager.Increment);
    }



    [Test]
    public void NewStep_SetsNewStepFlag()
    {
        // Act
        _handMenuManager.SetNewStepTrue();

        // Assert
        Assert.IsTrue(_handMenuManager.NewStep);
    }

    [Test]
    public void Modifying_SetsModifyingFlag()
    {
        // Act
        _handMenuManager.SetModifyingTrue();

        // Assert
        Assert.IsTrue(_handMenuManager.Modifying);
    }
    [Test]
    public void AddToPosition_AddsPositionCorrectly()
    {
        // Arrange
        var component = new GameObject("Component");

        Vector3 initialPosition = component.transform.position;

        // Act
        _handMenuManager.AddToPosition(Vector3.right * 1f, component);

        // Assert
        Assert.AreEqual(initialPosition + Vector3.right * 1f, component.transform.position);
    }

    [Test]
    public void AddToRotation_AddsRotationCorrectly()
    {
        // Arrange
        var component = new GameObject("Component");

        Vector3 initialRotation = component.transform.eulerAngles;

        // Act
        _handMenuManager.AddToRotation(Vector3.right * 10f, component);

        // Assert with tolerance
        Vector3 expectedRotation = initialRotation + Vector3.right * 10f;
        Assert.That(component.transform.eulerAngles, Is.EqualTo(expectedRotation).Using(Vector3EqualityComparer.Instance));
    }


    [Test]
    public void ModifyComponent_ModifiesComponentCorrectly()
    {
        // Arrange
        var component = new GameObject("Component");
        var componentObject = component.AddComponent<ComponentObject>();
        componentObject.GetComponent<MakeGrabbable>();
        componentObject.SetIsPlaced(true);

        var makeGrabbable = component.AddComponent<MakeGrabbable>();

        // Act
        _handMenuManager.ModifyComponent(component);

        // Assert
        Assert.IsFalse(componentObject.GetIsPlaced());
        Assert.IsNull(component.transform.parent);
        Assert.IsTrue(component.GetComponent<XRGrabInteractable>() != null);
    }

    [Test]
    public void SaveComponent_SavesComponentCorrectly()
    {
        // Arrange
        var component = new GameObject("Component");
        component.AddComponent<ComponentObject>();
        component.AddComponent<MakeGrabbable>();



        _handMenuManager.Group = null;

        // Act
        _handMenuManager.SaveComponent(component);

        // Assert
        Assert.IsNotNull(_handMenuManager.Group);
        Assert.AreEqual(component.transform.parent, _handMenuManager.Group.transform);
    }

    [Test]
    public void SaveComponent_SavesComponentCorrectly_Modifying()
    {
        // Arrange
        var component = new GameObject("Component");
        component.AddComponent<ComponentObject>();

        _handMenuManager.Modifying = true;
        _handMenuManager.Group = null;

        // Act
        _handMenuManager.SaveComponent(component);

        // Assert
        Assert.IsNotNull(_handMenuManager.Group);
        Assert.AreEqual(component.transform.parent, _handMenuManager.Group.transform);
    }

    [Test]
    public void SaveComponent_SavesComponentCorrectly_NewStep()
    {
        // Arrange
        var component = new GameObject("Component");
        component.AddComponent<ComponentObject>();


        _handMenuManager.Group = null;
        _handMenuManager.NewStep = true;


        // Act
        _handMenuManager.SaveComponent(component);

        // Assert
        Assert.IsNotNull(_handMenuManager.Group);
        Assert.AreEqual(component.transform.parent, _handMenuManager.Group.transform);
    }

    [Test]
    public void RemoveComponent_ModifiesAndRemovesComponent()
    {
        // Arrange
        var component = new GameObject("Component");
        component.AddComponent<ComponentObject>();

        // Act
        _handMenuManager.RemoveComponent(component);

    }

    [Test]
    public void SetTransformsButtonsActive_ActivatesOrDeactivatesButtonsCorrectly()
    {
        // Arrange
        List<Button> buttonsToTest = new List<Button>();

        // Create and add mock buttons to the list
        for (int i = 0; i < 5; i++)
        {
            var button = new GameObject($"Button{i}").AddComponent<Button>();
            buttonsToTest.Add(button);
        }

        // Act: Set buttons to active
        _handMenuManager.SetTransformsButtonsActive(true, buttonsToTest);

        // Assert: All buttons should be active
        foreach (var button in buttonsToTest)
        {
            Assert.IsTrue(button.gameObject.activeSelf);
        }

        // Act: Set buttons to inactive
        _handMenuManager.SetTransformsButtonsActive(false, buttonsToTest);

        // Assert: All buttons should be inactive
        foreach (var button in buttonsToTest)
        {
            Assert.IsFalse(button.gameObject.activeSelf);
        }
    }



    [Test]
    public void GroupSelection_TogglesGroupInteractability()
    {
        // Arrange
        _handMenuManager.Group = new GameObject("Group");
        GameObject child = new GameObject("Child");
        child.AddComponent<XRSimpleInteractable>();
        child.AddComponent<BoxCollider>();
        child.AddComponent<ComponentObject>();


        child.transform.SetParent(_handMenuManager.Group.transform);


        var mockInteractionManager = new GameObject().AddComponent<XRInteractionManager>();
        _handMenuManager.InteractionManager = mockInteractionManager;

        _handMenuManager.groupSelectionButton = new GameObject().AddComponent<Button>();
        _handMenuManager.groupPanel = new GameObject();
        _handMenuManager.handMenuPanel = new GameObject();

        // Act
        _handMenuManager.GroupSelection();

        // Assert
        Assert.IsTrue(_handMenuManager.IsGrabInteractableEnabled);
        Assert.IsTrue(_handMenuManager.groupPanel.activeSelf);
        Assert.IsFalse(_handMenuManager.handMenuPanel.activeSelf);

        // Act again to toggle back
        _handMenuManager.GroupSelection();

        // Assert again
        Assert.IsFalse(_handMenuManager.IsGrabInteractableEnabled);
        Assert.IsFalse(_handMenuManager.groupPanel.activeSelf);
        Assert.IsTrue(_handMenuManager.handMenuPanel.activeSelf);
    }

}
