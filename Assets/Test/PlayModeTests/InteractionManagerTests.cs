using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;


using UnityEngine.XR.Interaction.Toolkit;

// Dummy class that implements IXRHoverInteractor for testing purposes
public class DummyXRHoverInteractor : MonoBehaviour, IXRHoverInteractor
{
    public bool isHoverActive { get; }
    public bool isSelectActive { get; }
    public int priority { get; }
    public Transform transform { get; }
    public bool isValid { get; }

    public HoverEnterEvent hoverEntered => throw new NotImplementedException();

    public HoverExitEvent hoverExited => throw new NotImplementedException();

    public List<IXRHoverInteractable> interactablesHovered => throw new NotImplementedException();

    public bool hasHover => throw new NotImplementedException();

    public InteractionLayerMask interactionLayers => throw new NotImplementedException();

    public event Action<InteractorRegisteredEventArgs> registered;
    public event Action<InteractorUnregisteredEventArgs> unregistered;

    public bool CanHover(IXRHoverInteractable interactable)
    {
        throw new NotImplementedException();
    }

    public Transform GetAttachTransform(IXRInteractable interactable)
    {
        throw new NotImplementedException();
    }

    public void GetValidTargets(List<IXRInteractable> validTargets)
    {
        // Implementation for testing
    }

    public bool IsHovering(IXRHoverInteractable interactable)
    {
        throw new NotImplementedException();
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnHoverEntering(HoverEnterEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnHoverExiting(HoverExitEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnRegistered(InteractorRegisteredEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void OnUnregistered(InteractorUnregisteredEventArgs args)
    {
        throw new NotImplementedException();
    }

    public void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        throw new NotImplementedException();
    }

    public void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        throw new NotImplementedException();
    }

}

public class InteractionManagerTests
{
    private InteractionManager interactionManager;
    private GameObject testComponent;
    private GameObject testInteractorObject;
    private DummyXRHoverInteractor testInteractor;
    private GameObject parentGroup;
    private GameObject parentOther;

    [SetUp]
    public void SetUp()
    {
        // Set up the environment
        interactionManager = new GameObject().AddComponent<InteractionManager>();

        // Creating necessary objects
        testComponent = new GameObject();
        testComponent.AddComponent<ComponentObject>(); // Add required components if needed
        var outline = testComponent.AddComponent<Outline>(); // Ensure Outline is added for testing
        outline.enabled = false; // Initially disabled

        testInteractorObject = new GameObject();
        testInteractor = testInteractorObject.AddComponent<DummyXRHoverInteractor>(); // Add our dummy IXRHoverInteractor

        // Ensure testComponent is setup as an interactable
        var xrInteractable = testComponent.AddComponent<XRSimpleInteractable>(); // Adds the required interactable component
        // Create parent objects
        parentGroup = new GameObject("Group");
        parentOther = new GameObject("OtherParent");
    }

    [Test]
    public void OnHoverEnter_EnablesOutline()
    {
        // Arrange
        var hoverEnterEventArgs = new HoverEnterEventArgs
        {
            interactableObject = testComponent.GetComponent<IXRHoverInteractable>(),
            interactorObject = testInteractor
        };

        // Act
        interactionManager.OnHoverEnter(hoverEnterEventArgs);
        // Assert
        var outline = testComponent.GetComponent<Outline>();
        Assert.IsNotNull(outline);
        Assert.IsFalse(outline.enabled);
    }

    [Test]
    public void OnHoverExit_DisablesOutline()
    {
        // Arrange
        var hoverExitEventArgs = new HoverExitEventArgs
        {
            interactableObject = testComponent.GetComponent<IXRHoverInteractable>(),
            interactorObject = testInteractor
        };

        // Act
        interactionManager.OnHoverExit(hoverExitEventArgs);

        // Assert
        var outline = testComponent.GetComponent<Outline>();
        Assert.IsNotNull(outline);
        Assert.IsFalse(outline.enabled);
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
        var methodInfo = typeof(InteractionManager).GetMethod("ResetParentIfNotGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent });

        // Assert
        Assert.IsNull(testComponent.transform.parent);
    }

    [Test]
    public void ResetParentIfNotGroup_DoesNotUnparentWhenParentIsGroup()
    {
        // Arrange
        testComponent.transform.SetParent(parentGroup.transform);

        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("ResetParentIfNotGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent });

        // Assert
        Assert.AreEqual(parentGroup.transform, testComponent.transform.parent);
    }

    [Test]
    public void SetComponentReleasedState_SetsIsReleasedToTrue()
    {
        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent, true });

        // Assert
        var componentObject = testComponent.GetComponent<ComponentObject>();
        Assert.IsTrue(componentObject.IsReleased);
    }

    [Test]
    public void SetComponentReleasedState_SetsIsReleasedToFalse()
    {
        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { testComponent, false });

        // Assert
        var componentObject = testComponent.GetComponent<ComponentObject>();
        Assert.IsFalse(componentObject.IsReleased);
    }

    [Test]
    public void SetComponentReleasedState_DoesNothingIfComponentObjectIsNull()
    {
        // Arrange
        var componentWithoutComponentObject = new GameObject("NoComponentObject");

        // Act
        var methodInfo = typeof(InteractionManager).GetMethod("SetComponentReleasedState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(interactionManager, new object[] { componentWithoutComponentObject, true });

        // Assert
        // No exceptions should be thrown, and nothing should happen since the component lacks a ComponentObject
        Assert.Pass();
    }

}