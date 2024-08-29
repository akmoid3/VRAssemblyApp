using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

public class MakeGrabbableTests
{
    private GameObject testObject;
    private MakeGrabbable makeGrabbable;
    private Manager manager;
    private StateManager stateManager;



    [SetUp]
    public void Setup()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a test object and add the MakeGrabbable script to it
        testObject = new GameObject("TestObject");
        makeGrabbable = testObject.AddComponent<MakeGrabbable>();
        manager = new GameObject("Manager").AddComponent<Manager>();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up after each test
        Object.DestroyImmediate(testObject);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void TestMakeObjectGrabbable()
    {
        // Act
        makeGrabbable.MakeObjectGrabbable();

        // Assert
        var grabInteractable = testObject.GetComponent<XRGrabInteractable>();
        Assert.IsNotNull(grabInteractable, "XRGrabInteractable should be added to the object.");
        Assert.IsTrue(grabInteractable.enabled, "XRGrabInteractable should be enabled.");
        Assert.AreEqual(makeGrabbable.ThrowOnDetach, grabInteractable.throwOnDetach, "throwOnDetach should be set correctly.");
        Assert.AreEqual(makeGrabbable.MovementType, grabInteractable.movementType, "movementType should be set correctly.");
        Assert.AreEqual(makeGrabbable.UseDynamicAttach, grabInteractable.useDynamicAttach, "useDynamicAttach should be set correctly.");
        Assert.AreEqual(makeGrabbable.SelectMode, grabInteractable.selectMode, "selectMode should be set correctly.");
    }

    [UnityTest]
    public IEnumerator TestManagerInitialization()
    {

        // Call Start manually to simulate Unity's Start event
        makeGrabbable.Invoke("Start", 0f);

        yield return null;
        // Use reflection to access the private 'manager' field
        var managerField = typeof(MakeGrabbable).GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance);
        var managerValue = managerField.GetValue(makeGrabbable);

        // Assert that the manager field is not null and equals the Manager.Instance
        Assert.IsNotNull(managerValue, "Manager should be initialized.");
        Assert.AreEqual(manager, managerValue, "Manager field should be assigned the Manager.Instance.");
    }

    [Test]
    public void TestMakeObjectNonGrabbable()
    {
        // Act
        makeGrabbable.MakeObjectNonGrabbable();

        // Assert
        var simpleInteractable = testObject.GetComponent<XRSimpleInteractable>();
        Assert.IsNotNull(simpleInteractable, "XRSimpleInteractable should be added to the object.");
        Assert.IsTrue(simpleInteractable.enabled, "XRSimpleInteractable should be enabled.");
        Assert.AreEqual(makeGrabbable.SelectMode, simpleInteractable.selectMode, "selectMode should be set correctly.");
    }
}
