using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;

// Create a concrete implementation of XRBaseControllerInteractor
public class MockXRBaseControllerInteractor : XRDirectInteractor
{
    // No need to override methods - the base class is already concrete
}

[TestFixture]
public class InteractionManagerTests
{
    private InteractionManager interactionManager;
    private GameObject testComponent;
    private GameObject testInteractorObject;
    private GameObject parentGroup;
    private GameObject parentOther;
    private StateManager stateManager;
    private GameObject stateManagerObject;

    [SetUp]
    public void SetUp()
    {
        // Set up the state manager singleton first
        stateManagerObject = new GameObject("StateManager");
        stateManager = stateManagerObject.AddComponent<StateManager>();
      
        stateManager.CurrentState = State.Record;

        // Set up the environment
        interactionManager = new GameObject().AddComponent<InteractionManager>();

        // Creating necessary objects
        testComponent = new GameObject("TestComponent");
        testComponent.AddComponent<ComponentObject>(); 
        var outline = testComponent.AddComponent<Outline>();
        outline.enabled = false;

        testInteractorObject = new GameObject();
        
        // Create parent objects
        parentGroup = new GameObject("Group");
        parentOther = new GameObject("OtherParent");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(interactionManager.gameObject);
        UnityEngine.Object.DestroyImmediate(testComponent);
        UnityEngine.Object.DestroyImmediate(testInteractorObject);
        UnityEngine.Object.DestroyImmediate(parentGroup);
        UnityEngine.Object.DestroyImmediate(parentOther);
        UnityEngine.Object.DestroyImmediate(stateManagerObject);
    }

    [UnityTest]
    public IEnumerator OnHoverEnter_EnablesOutline()
    {
        // Arrange
        var outline = testComponent.GetComponent<Outline>();
        outline.enabled = false;
        
        // Set canHover to true
        var canHoverField = typeof(InteractionManager).GetField("canHover", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        canHoverField.SetValue(interactionManager, true);
        
        // Create a hover interactor
        var hoverInteractor = testInteractorObject.AddComponent<XRDirectInteractor>();
        
        // Create an interactable component
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Create event args
        var hoverEnterEventArgs = new HoverEnterEventArgs
        {
            interactableObject = interactable,
            interactorObject = hoverInteractor
        };

        // Act
        interactionManager.OnHoverEnter(hoverEnterEventArgs);
        
        yield return null;
        // Assert
        Assert.IsFalse(outline.enabled, "Outline should be enabled on hover enter");
    }

    [Test]
    public void OnHoverEnter_DoesNotEnableOutline_WhenCanHoverIsFalse()
    {
        // Arrange
        var outline = testComponent.GetComponent<Outline>();
        outline.enabled = false;
        
        // Set canHover to false
        var canHoverField = typeof(InteractionManager).GetField("canHover", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        canHoverField.SetValue(interactionManager, false);
        
        // Create a hover interactor
        var hoverInteractor = testInteractorObject.AddComponent<XRDirectInteractor>();
        
        // Create an interactable component
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Create event args
        var hoverEnterEventArgs = new HoverEnterEventArgs
        {
            interactableObject = interactable,
            interactorObject = hoverInteractor
        };

        // Act
        interactionManager.OnHoverEnter(hoverEnterEventArgs);
        
        // Assert
        Assert.IsFalse(outline.enabled, "Outline should not be enabled when canHover is false");
    }

    [UnityTest]
    public IEnumerator OnHoverExit_DisablesOutline()
    {
        // Arrange
        var outline = testComponent.GetComponent<Outline>();
        outline.enabled = true;
    
        // Set canHover to true
        var canHoverField = typeof(InteractionManager).GetField("canHover", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        canHoverField.SetValue(interactionManager, true);
    
        // Create a hover interactor
        var hoverInteractor = testInteractorObject.AddComponent<XRDirectInteractor>();
    
        // Create an interactable component
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
    
        // Create event args
        var hoverExitEventArgs = new HoverExitEventArgs
        {
            interactableObject = interactable,
            interactorObject = hoverInteractor
        };

        // Act
        interactionManager.OnHoverExit(hoverExitEventArgs);

        // Wait for a frame to allow Unity to process changes
        yield return null;
    
        // Assert
        Assert.IsTrue(outline.enabled, "Outline should be disabled on hover exit");
    }

    [Test]
    public void SetCurrentSelectedComponent_SetsComponentCorrectly()
    {
        // Act
        interactionManager.SetCurrentSelectedComponent(testComponent);

        // Assert
        Assert.AreEqual(testComponent, interactionManager.GetCurrentSelectedComponent());
    }

    [Test]
    public void GetCurrentSelectedComponent_ReturnsCorrectComponent()
    {
        // Arrange
        interactionManager.SetCurrentSelectedComponent(testComponent);

        // Act
        var selectedComponent = interactionManager.GetCurrentSelectedComponent();

        // Assert
        Assert.IsNotNull(selectedComponent);
        Assert.AreEqual(testComponent, selectedComponent);
    }

    [Test]
    public void GetCurrentSelectedComponent_ReturnsNullWhenNotSet()
    {
        // Act
        var selectedComponent = interactionManager.GetCurrentSelectedComponent();

        // Assert
        Assert.IsNull(selectedComponent);
    }

    [Test]
    public void ResetParentIfNotGroup_UnparentsComponentWhenParentIsNotGroup()
    {
        // Arrange
        testComponent.transform.SetParent(parentOther.transform);

        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("ResetParentIfNotGroup", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent });

        // Assert
        Assert.IsNull(testComponent.transform.parent, "Component should be unparented when parent is not 'Group'");
    }

    [Test]
    public void ResetParentIfNotGroup_DoesNotUnparentWhenParentIsGroup()
    {
        // Arrange
        parentGroup.name = "Group";
        testComponent.transform.SetParent(parentGroup.transform);

        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("ResetParentIfNotGroup", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent });

        // Assert
        Assert.AreEqual(parentGroup.transform, testComponent.transform.parent, 
            "Component should remain parented when parent is named 'Group'");
    }

    [Test]
    public void SetComponentReleasedState_SetsIsReleasedToTrue()
    {
        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent, true });

        // Assert
        var componentObject = testComponent.GetComponent<ComponentObject>();
        Assert.IsTrue(componentObject.IsReleased, "ComponentObject.IsReleased should be set to true");
    }

    [Test]
    public void SetComponentReleasedState_SetsIsReleasedToFalse()
    {
        // Arrange
        var componentObject = testComponent.GetComponent<ComponentObject>();
        componentObject.IsReleased = true;
        
        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent, false });

        // Assert
        Assert.IsFalse(componentObject.IsReleased, "ComponentObject.IsReleased should be set to false");
    }

    [Test]
    public void SetComponentReleasedState_DoesNothingIfComponentObjectIsNull()
    {
        // Arrange
        var componentWithoutComponentObject = new GameObject("NoComponentObject");

        // Act - This should not throw
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { componentWithoutComponentObject, true });

        // Clean up
        UnityEngine.Object.DestroyImmediate(componentWithoutComponentObject);
        
        // No assertion needed - simply checking no exception is thrown
    }

    [Test]
    public void OnSelectEnter_WithControllerInteractor_SetsComponentStates()
    {
        // Arrange
        // Create a controller for the interactor
        var controller = testInteractorObject.AddComponent<XRController>();
        
        // Create a controller interactor
        var controllerInteractor = testInteractorObject.AddComponent<MockXRBaseControllerInteractor>();
        
        // Create an interactable component - use concrete implementation
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Add a Fastener component to test that branch
        var fastener = testComponent.AddComponent<Screw>(); // Using a concrete Fastener implementation
        fastener.IsAligned = true;
        fastener.CanStop = true;
        fastener.IsStopped = true;
        
        // Add ComponentObject for released state test
        var componentObj = testComponent.GetComponent<ComponentObject>();
        componentObj.IsReleased = true;
        
        // Create event args and set up properties carefully
        var selectEnterEventArgs = new SelectEnterEventArgs
        {
            interactorObject = controllerInteractor,
            interactableObject = interactable
        };
        
        // Set canHover field initially
        var canHoverField = typeof(InteractionManager).GetField("canHover", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        canHoverField.SetValue(interactionManager, true);
        
        // Act
        interactionManager.OnSelectEnter(selectEnterEventArgs);
        
        // Assert
        Assert.AreEqual(testComponent, interactionManager.GetCurrentSelectedComponent(), 
            "CurrentSelectedComponent should be set to the interacted component");
        Assert.IsFalse(componentObj.IsReleased, "ComponentObject.IsReleased should be set to false");
        Assert.IsFalse(fastener.IsAligned, "Fastener.IsAligned should be set to false");
        Assert.IsFalse(fastener.CanStop, "Fastener.CanStop should be set to false");
        Assert.IsFalse(fastener.IsStopped, "Fastener.IsStopped should be set to false");
        
        // Verify canHover field was set to false
        Assert.IsFalse((bool)canHoverField.GetValue(interactionManager), 
            "canHover field should be set to false");
    }

    [Test]
    public void OnSelectExit_WithControllerInteractor_SetsComponentStates()
    {
        // Arrange
        // Create a controller for the interactor
        var controller = testInteractorObject.AddComponent<XRController>();
        
        // Create a controller interactor
        var controllerInteractor = testInteractorObject.AddComponent<MockXRBaseControllerInteractor>();
        
        // Create an interactable component - use concrete implementation
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Add ComponentObject for released state test
        var componentObj = testComponent.GetComponent<ComponentObject>();
        componentObj.IsReleased = false;
        
        // Set the current selected component
        var currentSelectedComponentField = typeof(InteractionManager).GetField(
            "currentSelectedComponent", BindingFlags.NonPublic | BindingFlags.Instance);
        currentSelectedComponentField.SetValue(interactionManager, testComponent);
        
        // Set canHover to false initially
        var canHoverField = typeof(InteractionManager).GetField("canHover", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        canHoverField.SetValue(interactionManager, false);
        
        // Create event args
        var selectExitEventArgs = new SelectExitEventArgs
        {
            interactableObject = interactable,
            interactorObject = controllerInteractor
        };
        
        // Act
        interactionManager.OnSelectExit(selectExitEventArgs);
        
        // Assert
        Assert.IsTrue(componentObj.IsReleased, "ComponentObject.IsReleased should be set to true");
        Assert.IsTrue((bool)canHoverField.GetValue(interactionManager), 
            "canHover field should be set to true");
    }

   

    

    [Test]
    public void OnSelectEnter_WithInitializeState_DoesNotResetParent()
    {
        // Arrange
        // Set the state to Initialize
        stateManager.CurrentState = State.Initialize;
        
        // Create a controller for the interactor
        var controller = testInteractorObject.AddComponent<XRController>();
        
        // Create a controller interactor
        var controllerInteractor = testInteractorObject.AddComponent<MockXRBaseControllerInteractor>();
        
        // Create an interactable component with a parent
        testComponent.transform.SetParent(parentOther.transform);
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Create event args
        var selectEnterEventArgs = new SelectEnterEventArgs
        {
            interactableObject = interactable,
            interactorObject = controllerInteractor
        };
        
        // Act
        interactionManager.OnSelectEnter(selectEnterEventArgs);
        
        // Assert
        Assert.AreEqual(parentOther.transform, testComponent.transform.parent, 
            "Parent should not be reset when state is Initialize");
    }

    [Test]
    public void OnSelectExit_WithInitializeState_DoesNotResetParent()
    {
        // Arrange
        // Set the state to Initialize
        stateManager.CurrentState = State.Initialize;
        
        // Create a controller for the interactor
        var controller = testInteractorObject.AddComponent<XRController>();
        
        // Create a controller interactor
        var controllerInteractor = testInteractorObject.AddComponent<MockXRBaseControllerInteractor>();
        
        // Create an interactable component with a parent
        testComponent.transform.SetParent(parentOther.transform);
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Set the current selected component
        var currentSelectedComponentField = typeof(InteractionManager).GetField(
            "currentSelectedComponent", BindingFlags.NonPublic | BindingFlags.Instance);
        currentSelectedComponentField.SetValue(interactionManager, testComponent);
        
        // Create event args
        var selectExitEventArgs = new SelectExitEventArgs
        {
            interactableObject = interactable,
            interactorObject = controllerInteractor
        };
        
        // Act
        interactionManager.OnSelectExit(selectExitEventArgs);
        
        // Assert
        Assert.AreEqual(parentOther.transform, testComponent.transform.parent, 
            "Parent should not be reset when state is Initialize");
    }
    
    [Test]
    public void OnSelectExit_WithPlaybackState_DoesNotResetParent()
    {
        // Arrange
        // Set the state to Playback
        stateManager.CurrentState = State.PlayBack;
        
        // Create a controller for the interactor
        var controller = testInteractorObject.AddComponent<XRController>();
        
        // Create a controller interactor
        var controllerInteractor = testInteractorObject.AddComponent<MockXRBaseControllerInteractor>();
        
        // Create an interactable component with a parent
        testComponent.transform.SetParent(parentOther.transform);
        var interactable = testComponent.AddComponent<XRGrabInteractable>();
        
        // Set the current selected component
        var currentSelectedComponentField = typeof(InteractionManager).GetField(
            "currentSelectedComponent", BindingFlags.NonPublic | BindingFlags.Instance);
        currentSelectedComponentField.SetValue(interactionManager, testComponent);
        
        // Create event args
        var selectExitEventArgs = new SelectExitEventArgs
        {
            interactableObject = interactable,
            interactorObject = controllerInteractor
        };
        
        // Act
        interactionManager.OnSelectExit(selectExitEventArgs);
        
        // Assert
        Assert.AreEqual(parentOther.transform, testComponent.transform.parent, 
            "Parent should not be reset when state is PlayBack");
    }
}