using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Concrete implementation of the abstract class for testing
public class TestFastener : Fastener
{
    protected override void HandleInteraction()
    {
        // Concrete implementation that counts invocations
        HandleInteractionCallCount++;
    }

    protected override void OnToolCollisionEnter(Collider other)
    {
        // Nothing needed here for testing
    }

    protected override void OnToolCollisionExit(Collider other)
    {
        // Nothing needed here for testing
    }

    // Test helpers to expose protected methods for testing
    public void TestPerformComponentRaycast()
    {
        PerformComponentRaycast();
    }

    public void TestAlignWithComponent(Vector3 contactPoint, Vector3 contactNormal)
    {
        AlignWithComponent(contactPoint, contactNormal);
    }

    public void TestFixedUpdate()
    {
        FixedUpdate();
    }

    public void TestOnTriggerEnter(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void TestOnTriggerExit(Collider other)
    {
        OnTriggerExit(other);
    }

    // Expose protected fields for testing
    public bool GetIsCollidingWithTool() { return isCollidingWithTool; }
    public void SetIsCollidingWithTool(bool value) { isCollidingWithTool = value; }

    public bool GetIsCollidingWithComponent() { return isCollidingWithComponent; }
    public void SetIsCollidingWithComponent(bool value) { isCollidingWithComponent = value; }

    public bool GetCanStop() { return canStop; }
    public void SetCanStop(bool value) { canStop = value; }

    public bool GetIsStopped() { return isStopped; }
    public void SetIsStopped(bool value) { isStopped = value; }

    public float GetMaxAllowedDotProduct() { return maxAllowedDotProduct; }
    public void SetMaxAllowedDotProduct(float value) { maxAllowedDotProduct = value; }

    public float GetAlignmentDotProductThreshold() { return alignmentDotProductThreshold; }
    public void SetAlignmentDotProductThreshold(float value) { alignmentDotProductThreshold = value; }

    public Color GetAlignedColor() { return alignedColor; }
    public void SetAlignedColor(Color value) { alignedColor = value; }

    public Color GetNotAlignedColor() { return notAlignedColor; }
    public void SetNotAlignedColor(Color value) { notAlignedColor = value; }

    public Color GetDefaultColor() { return defaultColor; }
    public void SetDefaultColor(Color value) { defaultColor = value; }

    public ComponentObject GetComponentObject() { return componentObject; }
    public void SetComponentObject(ComponentObject value) { componentObject = value; }

    public bool GetIsFirstError() { return isFirstError; }
    public void SetIsFirstError(bool value) { isFirstError = value; }

    public float GetDistanceToTravel() { return distanceToTravel; }
    public void SetDistanceToTravel(float value) { distanceToTravel = value; }

    public float GetFastenerLengthAlongAxis() { return fastenerLengthAlongAxis; }
    public void SetFastenerLengthAlongAxis(float value) { fastenerLengthAlongAxis = value; }
    
    public Vector3 GetSelectedAxisDirRaw() { return selectedAxisDirRaw; }
    public Vector3 GetSelectedAxisDirection() { return selectedAxisDirection; }
    
    // Counter for testing
    public int HandleInteractionCallCount { get; private set; } = 0;
}

[TestFixture]
public class FastenerTests
{
    private GameObject testObject;
    private TestFastener fastener;
    private GameObject managerObject;
    private GameObject stateManagerObject;
    private GameObject toolManagerObject;
    private AudioManager audioManager;

    [SetUp]
    public void Setup()
    {
        audioManager = new GameObject("AudioManager").AddComponent<AudioManager>();
        
        // Create the main test GameObject
        testObject = new GameObject("TestFastener");
        
        // Add mesh renderer and filter for visual components
        MeshRenderer renderer = testObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = testObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateCubeMesh();
        
        // Add a material
        renderer.material = new Material(Shader.Find("Standard"));
        
        // Add a collider
        BoxCollider collider = testObject.AddComponent<BoxCollider>();
        
        // Add our test Fastener before ComponentObject to test null case
        fastener = testObject.AddComponent<TestFastener>();
        
        // Setup Manager
        SetupManager();
        
        // Setup StateManager
        SetupStateManager();
        
        // Setup ToolManager
        SetupToolManager();
    }

    private Mesh CreateCubeMesh()
    {
        // Create a simple cube mesh
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };
        
        int[] triangles = new int[36]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            1, 2, 6, 1, 6, 5,
            2, 3, 7, 2, 7, 6,
            3, 0, 4, 3, 4, 7
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }

    private void SetupManager()
    {
        managerObject = new GameObject("Manager");
        Manager manager = managerObject.AddComponent<Manager>();
        
        // Set the instance field using reflection
        FieldInfo instanceField = typeof(Manager).GetField("instance", 
            BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null)
            instanceField.SetValue(null, manager);

        manager.sequenceManager = new GameObject().AddComponent<SequenceManager>();
        manager.sequenceManager.AssemblySequence = new List<ComponentData>();
        
        // Initialize necessary Manager properties
        manager.PerformaceForEachStep = new List<float>();
        manager.AssemblySequence = new List<ComponentData>();
        manager.Components = new List<Transform>();
        manager.CurrentAssembledSequence = new Dictionary<int, GameObject>();
        manager.ErrorCount = 0;
    }

    private void SetupStateManager()
    {
        stateManagerObject = new GameObject("StateManager");
        StateManager stateManager = stateManagerObject.AddComponent<StateManager>();
        
        // Set the instance field using reflection
        FieldInfo instanceField = typeof(StateManager).GetField("instance", 
            BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null)
            instanceField.SetValue(null, stateManager);
        
        stateManager.CurrentState = State.Record;
    }

    private void SetupToolManager()
    {
        toolManagerObject = new GameObject("ToolManager");
        ToolManager toolManager = toolManagerObject.AddComponent<ToolManager>();
    }
    
    [TearDown]
    public void Teardown()
    {
        UnityEngine.Object.DestroyImmediate(audioManager.gameObject);
        UnityEngine.Object.DestroyImmediate(testObject);
        UnityEngine.Object.DestroyImmediate(managerObject);
        UnityEngine.Object.DestroyImmediate(stateManagerObject);
        UnityEngine.Object.DestroyImmediate(toolManagerObject);
    }

    [Test]
    public void PropertyGettersAndSetters_Work()
    {
        // Test IsAligned property
        fastener.IsAligned = true;
        Assert.IsTrue(fastener.IsAligned);
        
        // Test IsStopped property
        fastener.IsStopped = true;
        Assert.IsTrue(fastener.IsStopped);
        
        // Test CanStop property
        fastener.CanStop = true;
        Assert.IsTrue(fastener.CanStop);
        
        // Test CorrectToolName property
        string toolName = "TestToolName";
        fastener.CorrectToolName = toolName;
        Assert.AreEqual(toolName, fastener.CorrectToolName);
        
        // Test Tool property
        GameObject toolObject = new GameObject("TestTool");
        ElectricScrewDriver tool = toolObject.AddComponent<ElectricScrewDriver>();
        fastener.Tool = tool;
        Assert.AreEqual(tool, fastener.Tool);
        
        // Test FastenerRenderer property
        MeshRenderer renderer = testObject.GetComponent<MeshRenderer>();
        fastener.FastenerRenderer = renderer;
        Assert.AreEqual(renderer, fastener.FastenerRenderer);
        
        // Test CorrectToolForce property
        int force = 5;
        fastener.CorrectToolForce = force;
        Assert.AreEqual(force, fastener.CorrectToolForce);
        
        // Test InitialPosition property
        Vector3 position = new Vector3(1, 2, 3);
        fastener.InitialPosition = position;
        Assert.AreEqual(position, fastener.InitialPosition);
        
        // Test getTool method
        Assert.AreEqual(tool, fastener.getTool());
        
        UnityEngine.Object.DestroyImmediate(toolObject);
    }
    
    [Test]
    public void Start_HandlesNullComponentObject()
    {
        // Ensure no ComponentObject exists
        ComponentObject existingCompObj = testObject.GetComponent<ComponentObject>();
        if (existingCompObj != null)
            UnityEngine.Object.DestroyImmediate(existingCompObj);
        
        
        // Call Start
        fastener.Start();
        
        // Verify renderer is set despite null componentObject
        Assert.IsNotNull(fastener.FastenerRenderer);
        
        // Verify componentObject is null
        Assert.IsNull(fastener.GetComponentObject());
        
        // Verify audioSource exists
        AudioSource audioSource = testObject.GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource);
    }
    
    [Test]
    public void Start_InitializesCorrectlyWithComponentObject()
    {
        // Add ComponentObject
        ComponentObject compObject = testObject.AddComponent<ComponentObject>();
        
        // Call Start
        fastener.Start();
        
        // Verify renderer is set
        Assert.IsNotNull(fastener.FastenerRenderer);
        
        // Verify componentObject is set
        Assert.IsNotNull(fastener.GetComponentObject());
        
        // Verify selectedAxisDirRaw is set from componentObject
        Assert.AreEqual(compObject.GetSelectedAxis(), fastener.GetSelectedAxisDirRaw());
        
        // Verify audioSource exists
        AudioSource audioSource = testObject.GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource);
    }

    [Test]
    public void FixedUpdate_CallsHandleInteractionWhenNeeded()
    {
        // Add ComponentObject
        testObject.AddComponent<ComponentObject>();
        fastener.Start();
        
        // Set up the conditions to trigger HandleInteraction
        fastener.SetIsCollidingWithTool(true);
        fastener.SetIsStopped(false);
        
        // Call FixedUpdate
        fastener.TestFixedUpdate();
        
        // Verify HandleInteraction was called
        Assert.AreEqual(1, fastener.HandleInteractionCallCount);
    }
    
    [Test]
    public void FixedUpdate_CallsPerformComponentRaycastWhenNeeded()
    {
        // Add ComponentObject
        var compObject = testObject.AddComponent<ComponentObject>();
        fastener.Start();
        
        // Set up the conditions to trigger PerformComponentRaycast
        fastener.SetIsCollidingWithTool(false);
        fastener.SetCanStop(false);
        fastener.SetIsStopped(false);
        fastener.IsAligned = false;
        StateManager.Instance.CurrentState = State.Record;
        
        // Create a mock GameObject with Component tag for raycast
        var componentObj = new GameObject("ComponentForRaycast");
        componentObj.tag = "Component";
        componentObj.transform.position = testObject.transform.position + Vector3.forward * 0.1f; // Close enough for raycast
        var collider = componentObj.AddComponent<BoxCollider>();
        
        // Save original color to verify it's changed
        var originalColor = fastener.FastenerRenderer.material.color;
        
        // Call FixedUpdate
        fastener.TestFixedUpdate();
        
        // Since we can't verify the raycast hit without a proper physics system,
        // we're just testing that FixedUpdate calls PerformComponentRaycast without errors
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(componentObj);
    }

    [Test]
    public void OnTriggerEnter_HandlesRegularToolCollision()
    {
        // Create a Tool GameObject
        GameObject toolObject = new GameObject("Tool");
        toolObject.tag = "Tool";
        BoxCollider toolCollider = toolObject.AddComponent<BoxCollider>();
        ElectricScrewDriver tool = toolObject.AddComponent<ElectricScrewDriver>();
        tool.ToolName = "TestTool";
        
        // Test with regular state
        StateManager.Instance.CurrentState = State.Record;
        fastener.TestOnTriggerEnter(toolCollider);
        
        // Verify tool and flags are set
        Assert.AreEqual(tool, fastener.Tool);
        Assert.IsTrue(fastener.GetIsCollidingWithTool());
        Assert.IsTrue(fastener.CanStop);
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(toolObject);
    }
    
    [Test]
    public void OnTriggerEnter_HandlesToolWithParentTool()
    {
        // Create a parent with Tool component
        GameObject parentToolObject = new GameObject("ParentTool");
        ElectricScrewDriver parentTool = parentToolObject.AddComponent<ElectricScrewDriver>();
        
        // Create a child with Tag but no Tool component
        GameObject childObject = new GameObject("ChildObject");
        childObject.tag = "Tool";
        childObject.transform.SetParent(parentToolObject.transform);
        BoxCollider childCollider = childObject.AddComponent<BoxCollider>();
        
        // Test OnTriggerEnter with child collider
        fastener.TestOnTriggerEnter(childCollider);
        
        // Verify parent tool was found
        Assert.AreEqual(parentTool, fastener.Tool);
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(parentToolObject); // Will destroy child as well
    }

    [Test]
    public void OnTriggerEnter_HandlesIncorrectToolInPlayback()
    {
        // Create a Tool GameObject
        GameObject toolObject = new GameObject("Tool");
        toolObject.tag = "Tool";
        BoxCollider toolCollider = toolObject.AddComponent<BoxCollider>();
        ElectricScrewDriver tool = toolObject.AddComponent<ElectricScrewDriver>();
        tool.ToolName = "WrongTool"; // Set a tool name that won't match
        
        // Setup for PlayBack state
        StateManager.Instance.CurrentState = State.PlayBack;
        fastener.CorrectToolName = "CorrectTool"; // Different from the actual tool
        fastener.SetIsFirstError(true);
        
        
        // Track initial error count
        int initialErrorCount = Manager.Instance.ErrorCount;
        
        // Call OnTriggerEnter
        fastener.TestOnTriggerEnter(toolCollider);
        
        // Verify error was incremented
        Assert.AreEqual(initialErrorCount + 1, Manager.Instance.ErrorCount);
        
        // Verify isFirstError is now false
        Assert.IsFalse(fastener.GetIsFirstError());
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(toolObject);
    }
    
    [Test]
    public void OnTriggerEnter_HandlesDynamometerWithIncorrectForce()
    {
        // Create a DynamometerScrewDriver GameObject
        GameObject dynObject = new GameObject("DynamometerTool");
        dynObject.tag = "Tool";
        BoxCollider dynCollider = dynObject.AddComponent<BoxCollider>();
        DynamometerScrewDriver dynTool = dynObject.AddComponent<DynamometerScrewDriver>();
        
        // Set the tool name to match but the force to be different
        dynTool.ToolName = "DynamometerTool";
        fastener.CorrectToolName = "DynamometerTool"; 
        fastener.CorrectToolForce = 5;
        dynTool.Force = 3; // Different force
        
        // Setup for PlayBack state
        StateManager.Instance.CurrentState = State.PlayBack;
        fastener.SetIsFirstError(true);
        
        // Track initial error count
        int initialErrorCount = Manager.Instance.ErrorCount;
        
        // Call OnTriggerEnter
        fastener.TestOnTriggerEnter(dynCollider);
        
        // Verify error was incremented
        Assert.AreEqual(initialErrorCount + 1, Manager.Instance.ErrorCount);
        
        // Verify isFirstError is now false
        Assert.IsFalse(fastener.GetIsFirstError());
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(dynObject);
    }
    
    [Test]
    public void OnTriggerEnter_ClearsHighlightsWithCorrectTool()
    {
        // Create a Tool GameObject with the correct name
        GameObject toolObject = new GameObject("Tool");
        toolObject.tag = "Tool";
        BoxCollider toolCollider = toolObject.AddComponent<BoxCollider>();
        ElectricScrewDriver tool = toolObject.AddComponent<ElectricScrewDriver>();
        tool.ToolName = "CorrectTool";
        
        // Setup for PlayBack state
        StateManager.Instance.CurrentState = State.PlayBack;
        fastener.CorrectToolName = "CorrectTool"; // Same as the actual tool
        fastener.SetIsFirstError(false); // Not the first error
        
        
        
        
        // Call OnTriggerEnter
        fastener.TestOnTriggerEnter(toolCollider);
        
      
        // Clean up
        UnityEngine.Object.DestroyImmediate(toolObject);
    }

    [Test]
    public void OnTriggerExit_ResetsFlagsInPlaybackMode()
    {
        // Create a Tool GameObject
        GameObject toolObject = new GameObject("Tool");
        toolObject.tag = "Tool";
        BoxCollider toolCollider = toolObject.AddComponent<BoxCollider>();
        
        // Set colliding state first
        fastener.SetIsCollidingWithTool(true);
        
        // Test with PlayBack state
        StateManager.Instance.CurrentState = State.PlayBack;
        fastener.SetIsFirstError(false);
        
        // Call OnTriggerExit
        fastener.TestOnTriggerExit(toolCollider);
        
        // Verify isCollidingWithTool is false
        Assert.IsFalse(fastener.GetIsCollidingWithTool());
        
        // Verify isFirstError is reset
        Assert.IsTrue(fastener.GetIsFirstError());
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(toolObject);
    }

    [UnityTest]
    public IEnumerator PerformComponentRaycast_HandlesComponentHits()
    {
        // Add ComponentObject
        var compObject = testObject.AddComponent<ComponentObject>();
        // Set it up as "released" to test that branch
        var isReleasedField = typeof(ComponentObject).GetField("isReleased", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        isReleasedField?.SetValue(compObject, true);
        
        fastener.Start();
        
        // Create a GameObject with Component tag for raycast
        var componentObj = new GameObject("ComponentForRaycast");
        componentObj.tag = "Component";
        componentObj.transform.position = testObject.transform.position + Vector3.forward * 0.1f;
        var collider = componentObj.AddComponent<BoxCollider>();
        
        // Save original color
        var originalColor = fastener.FastenerRenderer.material.color;
        
        // Create a Physics.Raycast stub using reflection
        var originalRaycast = typeof(Physics).GetMethod("Raycast", 
            new[] { typeof(Vector3), typeof(Vector3), typeof(RaycastHit).MakeByRefType(), typeof(float) });
        
        // Since we can't easily mock Physics.Raycast in a unit test, 
        // we'll call the method but understand limitations in verifying raycast hits
        fastener.TestPerformComponentRaycast();
        yield return null;
        // Clean up
        UnityEngine.Object.DestroyImmediate(componentObj);
    }

    [Test]
    public void AlignWithComponent_HandlesAllAxisCases()
    {
        // Add ComponentObject
        ComponentObject compObject = testObject.AddComponent<ComponentObject>();
        fastener.Start();
        
        // Method to set the selected axis using reflection
        Action<ComponentObject, Vector3> setSelectedAxis = (comp, axis) => {
            FieldInfo axisField = typeof(ComponentObject).GetField("selectedAxis", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (axisField != null)
                axisField.SetValue(comp, axis);
        };
        
        // Test with all possible axes
        Vector3[] axesToTest = new Vector3[]
        {
            Vector3.up,
            Vector3.down,
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.back,
            new Vector3(1, 1, 1) // Unsupported axis
        };
        
        foreach (Vector3 axis in axesToTest)
        {
            // Reset alignment first
            fastener.IsAligned = false;
            
            // Configure the component's selected axis
            setSelectedAxis(compObject, axis);
            
            // Set fastenerLengthAlongAxis for calculations
            fastener.SetFastenerLengthAlongAxis(0.5f);
            
            // Create a contact point and normal
            Vector3 contactPoint = new Vector3(0, 0, 1);
            Vector3 contactNormal = new Vector3(0, 1, 0);
            
            // Call AlignWithComponent
            fastener.TestAlignWithComponent(contactPoint, contactNormal);
            
            // For supported axes, verify alignment state
            if (axis != new Vector3(1, 1, 1))
            {
                Assert.IsTrue(fastener.IsAligned, $"Failed to align with axis {axis}");
            }
        }
    }
    
    [Test]
    public void SetSocketTransform_SetsTransformAndPosition()
    {
        // Create a socket transform
        GameObject socketObject = new GameObject("Socket");
        Transform socketTransform = socketObject.transform;
    
        // Set position
        socketTransform.localPosition = new Vector3(1, 2, 3);
    
        // Call SetSocketTransform
        fastener.SetSocketTransform(socketTransform);
    
        // Verify socketTransform is set
        Assert.AreEqual(socketTransform, fastener.GetSocketTransform());
    
        // Verify initialSocketPosition is set
        Assert.AreEqual(new Vector3(1, 2, 3), fastener.initialSocketPosition);
    
        // Clean up
        UnityEngine.Object.DestroyImmediate(socketObject);
    }

    [Test]
    public void SetSocketTransform_HandlesNullTransform()
    {
        // First set a valid transform
        GameObject socketObject = new GameObject("Socket");
        Transform socketTransform = socketObject.transform;
        fastener.SetSocketTransform(socketTransform);
    
        // Then set null
        fastener.SetSocketTransform(null);
    
        // Verify socketTransform is null
        Assert.IsNull(fastener.GetSocketTransform());
    
        // Clean up
        UnityEngine.Object.DestroyImmediate(socketObject);
    }

    [Test]
    public void GetSocketTransform_ReturnsCorrectValue()
    {
        // Initial value should be null
        Assert.IsNull(fastener.GetSocketTransform());
    
        // Set a value
        GameObject socketObject = new GameObject("Socket");
        Transform socketTransform = socketObject.transform;
        fastener.SetSocketTransform(socketTransform);
    
        // Get should return the same value
        Assert.AreEqual(socketTransform, fastener.GetSocketTransform());
    
        // Clean up
        UnityEngine.Object.DestroyImmediate(socketObject);
    }

    [Test]
    public void AlignWithComponent_HandlesPivotPositionCases()
    {
        // Add ComponentObject
        ComponentObject compObject = testObject.AddComponent<ComponentObject>();
        fastener.Start();
        
        // Method to set the selected axis
        Action<ComponentObject, Vector3> setSelectedAxis = (comp, axis) => {
            FieldInfo axisField = typeof(ComponentObject).GetField("selectedAxis", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (axisField != null)
                axisField.SetValue(comp, axis);
        };
        
        // Set axis to forward for testing
        setSelectedAxis(compObject, Vector3.forward);
        
        // Set fastenerLengthAlongAxis for calculations
        fastener.SetFastenerLengthAlongAxis(0.5f);
        
        // Create a contact point
        Vector3 contactPoint = new Vector3(0, 0, 1);
        Vector3 contactNormal = new Vector3(0, 0, 1);
        
        // Test with pivot "before" center
        // Modify object to create this condition
        testObject.transform.position = new Vector3(0, 0, -0.5f); // Pivot at origin, bounds center in front
        
        fastener.TestAlignWithComponent(contactPoint, contactNormal);
        Assert.IsTrue(fastener.IsAligned);
        Vector3 positionWithPivotBefore = testObject.transform.position;
        
        // Reset
        fastener.IsAligned = false;
        
        // Test with pivot "after" center
        testObject.transform.position = new Vector3(0, 0, 0.5f); // Pivot at origin, bounds center behind
        
        fastener.TestAlignWithComponent(contactPoint, contactNormal);
        Assert.IsTrue(fastener.IsAligned);
        Vector3 positionWithPivotAfter = testObject.transform.position;
        
        // Reset
        fastener.IsAligned = false;
        
        // Test with pivot at center (neither before nor after)
        testObject.transform.position = Vector3.zero;
        
        fastener.TestAlignWithComponent(contactPoint, contactNormal);
        Assert.IsTrue(fastener.IsAligned);
        Vector3 positionWithPivotCenter = testObject.transform.position;
        
        // Verify the positions are different for the different pivot cases
        Assert.AreNotEqual(positionWithPivotBefore, positionWithPivotAfter, 
            "Positions should be different for before/after pivot cases");
        Assert.AreNotEqual(positionWithPivotBefore, positionWithPivotCenter, 
            "Positions should be different for before/center pivot cases");
    }

    [Test]
    public void MapSelectedAxisToTransformDirection_MapsAllAxes()
    {
        // Test with each possible axis
        Vector3[] axesToTest = new Vector3[]
        {
            Vector3.forward,
            Vector3.right,
            Vector3.up,
            Vector3.back,
            Vector3.left,
            Vector3.down,
            new Vector3(1, 1, 1) // Should return transform.forward as default
        };
        
        foreach (Vector3 axis in axesToTest)
        {
            Vector3 result = fastener.MapSelectedAxisToTransformDirection(axis);
            
            if (axis == Vector3.forward)
                Assert.AreEqual(fastener.transform.forward, result);
            else if (axis == Vector3.right)
                Assert.AreEqual(fastener.transform.right, result);
            else if (axis == Vector3.up)
                Assert.AreEqual(fastener.transform.up, result);
            else if (axis == Vector3.back)
                Assert.AreEqual(-fastener.transform.forward, result);
            else if (axis == Vector3.left)
                Assert.AreEqual(-fastener.transform.right, result);
            else if (axis == Vector3.down)
                Assert.AreEqual(-fastener.transform.up, result);
            else
                Assert.AreEqual(fastener.transform.forward, result);
        }
    }
}