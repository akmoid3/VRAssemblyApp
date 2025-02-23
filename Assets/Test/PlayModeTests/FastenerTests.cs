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
    private ToolManager toolManager;
    private AudioManager audioManager;


    [SetUp]
    public void SetUp()
    {
        audioManager = new GameObject("AudioManager").AddComponent<AudioManager>();
        toolManager = new GameObject().AddComponent<ToolManager>();
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();
        fastenerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fastenerObject.name = "FastenerCube";

        fastener = fastenerObject.AddComponent<TestableFastener>();
        fastenerObject.AddComponent<ComponentObject>();

        fastener.FastenerRenderer = fastenerObject.GetComponent<Renderer>();
        fastenerObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

        fastenerObject.transform.position = Vector3.zero;

        FieldInfo fastenerLengthField =
            typeof(Fastener).GetField("fastenerLength", BindingFlags.NonPublic | BindingFlags.Instance);
        fastenerLengthField.SetValue(fastener, 1.0f);

        toolObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Tool tool = toolObject.AddComponent<DynamometerScrewDriver>();
        tool.ToolName = "ToolCube";
        toolObject.name = "ToolCube";
        toolObject.tag = "Tool";

        toolObject.transform.position = Vector3.forward;
        fastener.Start();
    }

    [Test]
    public void TestFastenerInitialization()
    {
        Assert.IsFalse(fastener.IsAligned, "Fastener should not be aligned initially");
        Assert.IsFalse(fastener.IsStopped, "Fastener should not be stopped initially");
        Assert.IsFalse(fastener.CanStop, "Fastener should not be able to stop initially");
    }


    [Test]
    public void TestOnTriggerExit()
    {
        // Create a tool collider
        Collider toolCollider = toolObject.GetComponent<Collider>();

        typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(fastener, true);

        typeof(Fastener).GetMethod("OnTriggerExit", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(fastener, new object[] { toolCollider });

        bool isCollidingWithTool = (bool)typeof(Fastener)
            .GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);

        Assert.IsFalse(isCollidingWithTool, "isCollidingWithTool should be false after OnTriggerExit.");
    }

    [Test]
    public void TestOnTriggerExit_StateManagerCheck()
    {
        stateManager.CurrentState = State.PlayBack;

        Collider toolCollider = toolObject.GetComponent<Collider>();

        typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(fastener, true);
        fastener.SetField("isFirstError", false);

        typeof(Fastener).GetMethod("OnTriggerExit", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(fastener, new object[] { toolCollider });

        bool isFirstError = (bool)fastener.GetField("isFirstError");

        Assert.IsTrue(isFirstError, "isFirstError should be true after OnTriggerExit if in PlayBack state.");
    }
    

    [Test]
    public void TestOnTriggerEnter()
    {
        stateManager.CurrentState = State.PlayBack;
        Collider toolCollider = toolObject.GetComponent<Collider>();


        typeof(Fastener).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(fastener, new object[] { toolCollider });

        bool isCollidingWithTool = (bool)typeof(Fastener)
            .GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fastener);
        bool canStop = (bool)typeof(Fastener).GetField("canStop", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(fastener);

        Assert.IsTrue(isCollidingWithTool, "isCollidingWithTool should be true after OnTriggerEnter.");

        //Assert.AreEqual(toolObject, tool, "Tool should be assigned correctly after OnTriggerEnter.");

        Assert.IsTrue(canStop, "canStop should be true after OnTriggerEnter.");
    }

    [Test]
    public void TestOnTriggerEnter_StateManagerCheck_Branch1()
    {
        Collider toolCollider = toolObject.GetComponent<Collider>();
        Assert.IsNotNull(toolCollider, "Tool collider should not be null.");

        MethodInfo onTriggerEnterMethod =
            typeof(Fastener).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(onTriggerEnterMethod, "OnTriggerEnter method not found.");

        onTriggerEnterMethod.Invoke(fastener, new object[] { toolCollider });

        FieldInfo isFirstErrorField =
            typeof(Fastener).GetField("isFirstError", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isFirstError = (bool)isFirstErrorField.GetValue(fastener);

        //Assert.IsFalse(isFirstError, "isFirstError should be false after OnTriggerEnter if an error was registered.");
    }


    [Test]
    public void TestAlignWithComponent1()
    {
        Vector3 contactPoint = new Vector3(0, 0, 0.15f);
        Vector3 contactNormal = new Vector3(0, 0, -1);

        fastener.TestAlignWithComponent(contactPoint, contactNormal);

        Vector3 expectedPosition = new Vector3(0, 0, 0);
        Quaternion expectedRotation = Quaternion.LookRotation(-contactNormal);

        Assert.AreEqual(expectedPosition, fastener.transform.position, "Fastener position is not aligned correctly.");
        Assert.AreEqual(expectedRotation, fastener.transform.rotation, "Fastener rotation is not aligned correctly.");
        //Assert.IsTrue(fastener.IsAligned, "Fastener should be aligned after calling AlignWithComponent.");
    }

    [Test]
    public void TestHandleInteractionMethodCalled()
    {
        fastener.TestHandleInteraction();

        Assert.IsTrue(fastener.HandleInteractionCalled, "HandleInteraction should have been called.");
    }


    [Test]
    public void TestSetSocketTransformMethodCalled()
    {
        Transform mockSocket = new GameObject("MockSocket").transform;

        fastener.SetSocketTransform(mockSocket);

        Assert.IsTrue(fastener.GetSocketTransform(), "SetSocketTransform should have been called.");
        Assert.IsTrue(mockSocket == fastener.GetSocketTransform());
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(fastenerObject);
        Object.DestroyImmediate(toolObject);
        if (stateManager != null)
            Object.DestroyImmediate(stateManager.gameObject);
        if (manager != null)
            Object.DestroyImmediate(manager.gameObject);
        if (toolManager != null)
            Object.DestroyImmediate(toolManager.gameObject);
        if (audioManager != null)
            Object.DestroyImmediate(audioManager.gameObject);
    }

    [Test]
    public void TestPerformComponentRaycast_NoHit1()
    {
        fastenerObject.transform.position = new Vector3(0, 0, 0);
        fastenerObject.transform.forward = Vector3.up;

        typeof(Fastener).GetMethod("PerformComponentRaycast", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(fastener, null);

        bool isCollidingWithComponent = (bool)ReflectionExtensions.GetField(fastener, "isCollidingWithComponent");
        bool isAligned = (bool)ReflectionExtensions.GetField(fastener, "isAligned");
        Color currentColor = fastener.FastenerRenderer.material.color;

        Assert.IsFalse(isCollidingWithComponent, "isCollidingWithComponent should be false if no raycast hit.");


        Assert.IsFalse(isAligned, "Fastener should not be aligned if no component was hit.");
    }

    [Test]
    public void TestGetterSetters()
    {
        fastener.IsAligned = true;
        Assert.IsTrue(fastener.IsAligned, "IsAligned non viene impostato correttamente.");

        fastener.IsStopped = true;
        Assert.IsTrue(fastener.IsStopped, "IsStopped non viene impostato correttamente.");

        fastener.CanStop = true;
        Assert.IsTrue(fastener.CanStop, "CanStop non viene impostato correttamente.");

        fastener.CorrectToolName = "RealTool";
        Assert.AreEqual("RealTool", fastener.CorrectToolName, "CorrectToolName non viene impostato correttamente.");

        fastener.CorrectToolForce = 50;
        Assert.AreEqual(50, fastener.CorrectToolForce, "CorrectToolForce non viene impostato correttamente.");

        Vector3 testPos = new Vector3(1, 2, 3);
        fastener.InitialPosition = testPos;
        Assert.AreEqual(testPos, fastener.InitialPosition, "InitialPosition non viene impostato correttamente.");
    }

    [Test]
    public void TestGetToolAndSetTool()
    {
        Tool realTool = toolObject.GetComponent<Tool>();
        fastener.Tool = realTool;
        Assert.AreEqual(realTool, fastener.Tool, "La proprietà Tool non restituisce il valore impostato.");
        Assert.AreEqual(realTool, fastener.getTool(), "Il metodo getTool() non restituisce il valore corretto.");
    }

    [Test]
    public void TestSetAndGetSocketTransform()
    {
        GameObject socketObj = new GameObject("Socket");
        fastener.SetSocketTransform(socketObj.transform);
        Assert.AreEqual(socketObj.transform, fastener.GetSocketTransform(),
            "GetSocketTransform non restituisce il socket impostato.");
        Assert.AreEqual(socketObj.transform.localPosition, fastener.initialSocketPosition,
            "initialSocketPosition non viene impostato correttamente.");
        Object.DestroyImmediate(socketObj);
    }

    [Test]
    public void TestMapSelectedAxisToTransformDirection()
    {
        Vector3 resultForward = fastener.MapSelectedAxisToTransformDirection(Vector3.forward);
        Assert.AreEqual(fastener.transform.forward, resultForward, "Mapping per Vector3.forward non corretto.");

        Vector3 resultRight = fastener.MapSelectedAxisToTransformDirection(Vector3.right);
        Assert.AreEqual(fastener.transform.right, resultRight, "Mapping per Vector3.right non corretto.");

        Vector3 resultUp = fastener.MapSelectedAxisToTransformDirection(Vector3.up);
        Assert.AreEqual(fastener.transform.up, resultUp, "Mapping per Vector3.up non corretto.");

        Vector3 resultNegForward = fastener.MapSelectedAxisToTransformDirection(Vector3.forward * -1);
        Assert.AreEqual(fastener.transform.forward * -1, resultNegForward,
            "Mapping per -Vector3.forward non corretto.");

        Vector3 resultNegRight = fastener.MapSelectedAxisToTransformDirection(Vector3.right * -1);
        Assert.AreEqual(fastener.transform.right * -1, resultNegRight, "Mapping per -Vector3.right non corretto.");

        Vector3 resultNegUp = fastener.MapSelectedAxisToTransformDirection(Vector3.up * -1);
        Assert.AreEqual(fastener.transform.up * -1, resultNegUp, "Mapping per -Vector3.up non corretto.");
    }

    [Test]
    public void TestOmniLookRotation()
    {
        Quaternion rot = Fastener.OmniLookRotation(Vector3.up, Vector3.forward, Vector3.right, Vector3.up);
        Vector3 rotatedAxis = rot * Vector3.up;
        float dot = Vector3.Dot(rotatedAxis.normalized, Vector3.forward.normalized);
        Assert.IsTrue(dot > 0.99f, "OmniLookRotation non mappa correttamente l'asse.");
    }

    
    [Test]
    public void TestOnTriggerEnterAndExit()
    {
        Collider toolCollider = toolObject.GetComponent<Collider>();

        MethodInfo onTriggerEnter =
            typeof(Fastener).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        onTriggerEnter.Invoke(fastener, new object[] { toolCollider });

        FieldInfo fieldColliding =
            typeof(Fastener).GetField("isCollidingWithTool", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isColliding = (bool)fieldColliding.GetValue(fastener);
        Assert.IsTrue(isColliding, "isCollidingWithTool dovrebbe essere true dopo OnTriggerEnter.");

        MethodInfo onTriggerExit =
            typeof(Fastener).GetMethod("OnTriggerExit", BindingFlags.NonPublic | BindingFlags.Instance);
        onTriggerExit.Invoke(fastener, new object[] { toolCollider });

        isColliding = (bool)fieldColliding.GetValue(fastener);
        Assert.IsFalse(isColliding, "isCollidingWithTool dovrebbe essere false dopo OnTriggerExit.");
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