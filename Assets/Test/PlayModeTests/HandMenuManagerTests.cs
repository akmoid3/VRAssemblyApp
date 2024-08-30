using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMenuManagerTests
{
    private HandMenuManager _handMenuManager;
    private Manager manager;


    [SetUp]
    public void SetUp()
    {
        manager = new GameObject().AddComponent<Manager>();
        _handMenuManager = new GameObject().AddComponent<HandMenuManager>();
        _handMenuManager.Manager = manager;

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

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(_handMenuManager.gameObject);
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
        _handMenuManager.AddToPosition(Vector3.right * 1f,component);

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


        _handMenuManager.Group = null;

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
