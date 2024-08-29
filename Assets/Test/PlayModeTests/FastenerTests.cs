using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Reflection;
using UnityEngine;
public class TestFastener : Fastener
{
    public bool HandleInteractionCalled { get; private set; }
    public bool OnToolCollisionEnterCalled { get; private set; }
    public bool OnToolCollisionExitCalled { get; private set; }
    public bool AlignWithComponentCalled { get; private set; }

    protected override void HandleInteraction()
    {
        HandleInteractionCalled = true;
    }

    protected override void OnToolCollisionEnter(Collider other)
    {
        OnToolCollisionEnterCalled = true;
    }

    protected override void OnToolCollisionExit(Collider other)
    {
        OnToolCollisionExitCalled = true;
    }

}

public class TestableFastener : TestFastener
{
    // Public method to expose the protected AlignWithComponent method for testing
    public void TestAlignWithComponent(Vector3 contactPoint, Vector3 contactNormal)
    {
        base.AlignWithComponent(contactPoint, contactNormal);
    }

    public void TestHandleInteraction()
    {
        base.HandleInteraction();
    }

    public void TestOnToolCollisionEnter(Collider other)
    {
        OnToolCollisionEnter(other);
    }
    public void TestOnToolCollisionExit(Collider other)
    {
        OnToolCollisionExit(other);
    }
}

public class FastenerAlignmentTests
{
    private GameObject fastenerObject;
    private TestableFastener fastener;
    private GameObject toolObject;
    private StateManager stateManager;
    private Manager manager;



    [SetUp]
    public void SetUp()
    {
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a cube GameObject for the fastener
        fastenerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fastenerObject.name = "FastenerCube";

        // Add the TestableFastener component to the GameObject
        fastener = fastenerObject.AddComponent<TestableFastener>();

        // Set up the Renderer
        fastener.FastenerRenderer = fastenerObject.GetComponent<Renderer>();
        fastenerObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

        // Set up the initial position and other necessary properties
        fastenerObject.transform.position = Vector3.zero;

        // Mock the fastener length (assume a unit length for simplicity)
        FieldInfo fastenerLengthField = typeof(Fastener).GetField("fastenerLength", BindingFlags.NonPublic | BindingFlags.Instance);
        fastenerLengthField.SetValue(fastener, 1.0f);

        toolObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toolObject.name = "ToolCube";
        toolObject.tag = "Tool";
        // Position tool near the fastener to simulate an interaction
        toolObject.transform.position = Vector3.forward;
    }

    [Test]
    public void TestFastenerInitialization()
    {
        // Test initial conditions
        Assert.IsFalse(fastener.IsAligned, "Fastener should not be aligned initially");
        Assert.IsFalse(fastener.IsStopped, "Fastener should not be stopped initially");
        Assert.IsFalse(fastener.CanStop, "Fastener should not be able to stop initially");
    }


    [Test]
    public void TestOnTriggerExit()
    {
        // Create a tool collider
        Collider toolCollider = toolObject.GetComponent<Collider>();

        // Set the fastener's initial state to simulate that the tool was colliding
        typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fastener, true);

        // Simulate the OnTriggerExit event
        typeof(Fastener).GetMethod("OnTriggerExit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fastener, new object[] { toolCollider });

        // Use reflection to get the updated value of isCollidingWithTool
        bool isCollidingWithTool = (bool)typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);

        // Assert that the isCollidingWithTool flag is now false
        Assert.IsFalse(isCollidingWithTool, "isCollidingWithTool should be false after OnTriggerExit.");
    }

    [Test]
    public void TestOnTriggerExit_StateManagerCheck()
    {
        // Mock the StateManager to control its state
        stateManager.CurrentState = State.PlayBack;

        // Create a tool collider
        Collider toolCollider = toolObject.GetComponent<Collider>();

        // Set the fastener's initial state to simulate that the tool was colliding
        typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fastener, true);
        fastener.SetField("isFirstError", false); // Using reflection for private field access if necessary

        // Simulate the OnTriggerExit event
        typeof(Fastener).GetMethod("OnTriggerExit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fastener, new object[] { toolCollider });

        // Use reflection to get the updated value of isFirstError
        bool isFirstError = (bool)fastener.GetField("isFirstError");

        // Assert that the isFirstError flag is reset to true if in PlayBack state
        Assert.IsTrue(isFirstError, "isFirstError should be true after OnTriggerExit if in PlayBack state.");
    }


    [Test]
    public void TestOnTriggerEnter()
    {
        // Create a tool collider
        Collider toolCollider = toolObject.GetComponent<Collider>();


        // Simulate the OnTriggerEnter event
        typeof(Fastener).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fastener, new object[] { toolCollider });

        // Use reflection to get the updated values
        bool isCollidingWithTool = (bool)typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);
        GameObject tool = fastener.getTool();
        bool canStop = (bool)typeof(Fastener).GetField("canStop", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);

        // Assert that the isCollidingWithTool flag is set to true
        Assert.IsTrue(isCollidingWithTool, "isCollidingWithTool should be true after OnTriggerEnter.");

        // Assert that the tool object is correctly assigned
        Assert.AreEqual(toolObject, tool, "Tool should be assigned correctly after OnTriggerEnter.");

        // Assert that canStop is set to true
        Assert.IsTrue(canStop, "canStop should be true after OnTriggerEnter.");
    }

    [Test]
    public void TestOnTriggerEnter_StateManagerCheck()
    {
        // Mock the StateManager to control its state
        stateManager.CurrentState = State.PlayBack;

        // Create a tool collider
        Collider toolCollider = toolObject.GetComponent<Collider>();

        typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fastener, true);


        // Simulate the OnTriggerEnter event
        typeof(Fastener).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fastener, new object[] { toolCollider });

        // Use reflection to get the updated value of isFirstError
        bool isFirstError = (bool)typeof(Fastener).GetField("isFirstError", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);

        // Assert that isFirstError flag is set to false after the tool enters and an error is registered
        Assert.IsFalse(isFirstError, "isFirstError should be false after OnTriggerEnter if an error was registered.");
    }


    [Test]
    public void TestAlignWithComponent()
    {
        // Set up a contact point and normal as they would be in a collision
        Vector3 contactPoint = new Vector3(0, 0, 0.15f);
        Vector3 contactNormal = new Vector3(0, 0, -1);

        // Call the TestAlignWithComponent method
        fastener.TestAlignWithComponent(contactPoint, contactNormal);

        // Expected position and rotation after alignment
        Vector3 expectedPosition = new Vector3(0, 0, 0.15f - 0.5f);  // Adjust based on your fastenerLength
        Quaternion expectedRotation = Quaternion.LookRotation(-contactNormal);

        // Check if the fastener is aligned (position and rotation)
        Assert.AreEqual(expectedPosition, fastener.transform.position, "Fastener position is not aligned correctly.");
        Assert.AreEqual(expectedRotation, fastener.transform.rotation, "Fastener rotation is not aligned correctly.");
        Assert.IsTrue(fastener.IsAligned, "Fastener should be aligned after calling AlignWithComponent.");
    }

    [Test]
    public void TestHandleInteractionMethodCalled()
    {
        // Simulate conditions for HandleInteraction to be called
        fastener.TestHandleInteraction();

        // Assert that the method was called
        Assert.IsTrue(fastener.HandleInteractionCalled, "HandleInteraction should have been called.");
    }


    [Test]
    public void TestSetSocketTransformMethodCalled()
    {
        // Create a mock socket transform
        Transform mockSocket = new GameObject("MockSocket").transform;

        // Call SetSocketTransform
        fastener.SetSocketTransform(mockSocket);

        // Assert that the SetSocketTransform method was called
        Assert.IsTrue(fastener.getSocketTransform(), "SetSocketTransform should have been called.");
        Assert.IsTrue(mockSocket == fastener.getSocketTransform());

    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(fastenerObject);
        Object.DestroyImmediate(toolObject);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(manager.gameObject);

        
    }

    [Test]
    public void TestPerformComponentRaycast_NoHit()
    {
        // Ensure the Fastener is not facing towards the component
        fastenerObject.transform.position = new Vector3(0, 0, 0);
        fastenerObject.transform.forward = Vector3.up;

        // Call PerformComponentRaycast
        typeof(Fastener).GetMethod("PerformComponentRaycast", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(fastener, null);

        // Use reflection to get the updated values
        bool isCollidingWithComponent = (bool)ReflectionExtensions.GetField(fastener, "isCollidingWithComponent");
        bool isAligned = (bool)ReflectionExtensions.GetField(fastener, "isAligned");
        Color currentColor = fastener.FastenerRenderer.material.color;

        // Assert that no component collision is detected
        Assert.IsFalse(isCollidingWithComponent, "isCollidingWithComponent should be false if no raycast hit.");

        // Assert that the color is reset to defaultColor
        // Assert.AreEqual(fastener.defaultColor, currentColor, "Renderer color should be defaultColor if no alignment is found.");

        // Assert that the fastener is not aligned
        Assert.IsFalse(isAligned, "Fastener should not be aligned if no component was hit.");
    }

}

// Extension methods for reflection
public static class ReflectionExtensions
{
    public static void SetField(this object obj, string fieldName, object value)
    {
        FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    public static object GetField(this object obj, string fieldName)
    {
        FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(obj);
        }
        return null;
    }
}

