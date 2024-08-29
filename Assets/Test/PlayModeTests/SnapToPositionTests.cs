using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Reflection;
using UnityEngine.TestTools;
using System.Collections.Generic;

[TestFixture]
public class SnapToPositionTests
{
    private GameObject snapObject;
    private SnapToPosition snapToPosition;
    private GameObject otherObject;
    private ComponentObject componentObject;
    private Fastener fastener;
    private MeshRenderer meshRenderer;
    private Rigidbody rigidbody;
    private XRInteractionManager interactionManager;
    private StateManager stateManager;
    private AudioManager audioManager;

    [SetUp]
    public void Setup()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        audioManager = new GameObject().AddComponent<AudioManager>();
        
        // Create interaction manager
        interactionManager = new GameObject("XRInteractionManager").AddComponent<XRInteractionManager>();

        // Create and setup snap object
        snapObject = new GameObject("SnapObject");
        snapToPosition = snapObject.AddComponent<SnapToPosition>();

        // Create and setup snap point
        GameObject snapPointObject = new GameObject("SnapPoint");
        snapPointObject.transform.parent = snapObject.transform;
        meshRenderer = snapPointObject.AddComponent<MeshRenderer>();
        componentObject = snapPointObject.AddComponent<ComponentObject>();

        // Simulate the Awake and Start methods
        InvokePrivateMethod(snapToPosition, "Awake", null);
        InvokePrivateMethod(snapToPosition, "Start", null);

        // Create and setup other object
        otherObject = new GameObject("OtherObject");
        otherObject.transform.position = snapPointObject.transform.position + Vector3.up * 0.05f; // Close to snap point
        componentObject = otherObject.AddComponent<ComponentObject>();
        fastener = otherObject.AddComponent<Nail>();
        otherObject.AddComponent<MeshRenderer>();

        rigidbody = otherObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = false;

        // Assign group or type if necessary
        componentObject.SetGroup(ComponentObject.Group.None);
        componentObject.SetComponentType(ComponentObject.ComponentType.None);

        // Set the other object to be dynamic
        otherObject.AddComponent<BoxCollider>().isTrigger = false;
    }

 


    [Test]
    public void TestCheckSnap()
    {
        GameObject snapPointObject = new GameObject("SnapPoint");
        snapPointObject.transform.parent = snapObject.transform;
        snapPointObject.transform.position = snapObject.transform.position;
        snapPointObject.transform.rotation = snapObject.transform.rotation;
        MeshRenderer snapPointMeshRenderer = snapPointObject.AddComponent<MeshRenderer>();
        ComponentObject snapPointComponentObject = snapPointObject.AddComponent<ComponentObject>();
        // Initialize snapPoints list and add the created snap point
        List<SnapPoint> snapPoints = new List<SnapPoint>
    {
        new SnapPoint
        {
            snapTransform = snapPointObject.transform,
            componentName = "OtherObject",
            meshRenderer = snapPointMeshRenderer,
            componentObject = snapPointComponentObject
        }
    };

        // Set the private field 'snapPoints' using reflection
        SetPrivateField(snapToPosition, "snapPoints", snapPoints);
        // Call the private CheckSnap method using reflection
        InvokePrivateMethod(snapToPosition, "CheckSnap", new object[] { otherObject.GetComponent<Collider>() });

        // Verify the object has snapped correctly
        Assert.AreEqual(snapObject.transform.position, otherObject.transform.position, "The object should have snapped to the correct position.");
        Assert.AreEqual(snapObject.transform.rotation, otherObject.transform.rotation, "The object should have snapped to the correct rotation.");
    }

    [Test]
    public void TestAddGrabbable()
    {
        // Call the private AddGrabbable method using reflection
        InvokePrivateMethod(snapToPosition, "AddGrabbable", new object[] { otherObject.GetComponent<Collider>() });

        // Verify that the XRGrabInteractable component is correctly configured and enabled
        XRGrabInteractable grabInteractable = snapObject.GetComponent<XRGrabInteractable>();
        Assert.IsNotNull(grabInteractable, "The XRGrabInteractable component should be added.");
        Assert.IsTrue(grabInteractable.enabled, "The XRGrabInteractable component should be enabled.");

        // Call the private AddGrabbable method using reflection
        InvokePrivateMethod(snapToPosition, "AddGrabbable", new object[] { otherObject.GetComponent<Collider>() });


        // Verify that the XRGrabInteractable component is correctly configured and enabled
        XRGrabInteractable grabInteractable2 = snapObject.GetComponent<XRGrabInteractable>();
        Assert.IsNotNull(grabInteractable2, "The XRGrabInteractable component should be added.");
        Assert.IsTrue(grabInteractable2.enabled, "The XRGrabInteractable component should be enabled.");
    }



    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(snapObject);
        GameObject.DestroyImmediate(otherObject);
        GameObject.DestroyImmediate(interactionManager.gameObject);
        GameObject.DestroyImmediate(stateManager.gameObject);
        GameObject.DestroyImmediate(audioManager.gameObject);
    }

    // Method to invoke private/protected methods using reflection
    private void InvokePrivateMethod(object target, string methodName, object[] parameters)
    {
        MethodInfo methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(target, parameters);
    }
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private object GetPrivateField(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(target);
    }
}
