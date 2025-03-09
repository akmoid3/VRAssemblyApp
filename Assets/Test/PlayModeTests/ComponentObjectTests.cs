using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ComponentObjectTests
{
    private GameObject testObject;
    private ComponentObject componentObject;
    private StateManager stateManager;
    private AudioManager audioManager;
    [SetUp]
    public void Setup()
    {
        // Create test GameObject with required components
        testObject = new GameObject("TestComponentObject");
        // Line renderer will be added by ComponentObject's Start method
        componentObject = testObject.AddComponent<ComponentObject>();
        
        // Add AudioListener to fix audio warning
        Camera.main?.gameObject.AddComponent<AudioListener>();
        if (Camera.main == null)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }
        
        // Create real StateManager if it doesn't exist
        if (StateManager.Instance == null)
        {
            var stateManagerObject = new GameObject("StateManager");
            stateManager = stateManagerObject.AddComponent<StateManager>();
        }
        
        // Create real AudioManager if it doesn't exist
        if (AudioManager.Instance == null)
        {
            var audioManagerObject = new GameObject("AudioManager");
            audioManager = audioManagerObject.AddComponent<AudioManager>();
        }
        
        // Allow Start() to execute naturally
        componentObject.Start();
        
    }

    [TearDown]
    public void TearDown()
    {
        if (testObject != null)
        {
            Object.Destroy(testObject);
        }
        if (stateManager != null)
        {
            Object.Destroy(stateManager.gameObject);
        }
        if (audioManager != null)
        {
            Object.Destroy(audioManager.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator Start_WithMissingLineRenderer_AddsLineRenderer()
    {
        // Create a new test object without LineRenderer
        var newTestObject = new GameObject("NewTestObject");
        var newComponent = newTestObject.AddComponent<ComponentObject>();
        
        // Allow Start() to run
        // Note: This is automatically called by Unity once the component is added and enabled
        componentObject.Start();
        // Verify LineRenderer was added
        
        yield return null;
        Assert.IsNotNull(newTestObject.GetComponent<LineRenderer>(), 
            "LineRenderer should be added when missing");
        
        // Clean up
        Object.Destroy(newTestObject);
    }
    
    [Test]
    public void CopyFrom_ValidObject_CopiesAllProperties()
    {
        // Setup
        var otherObject = new GameObject("OtherObject");
        var other = otherObject.AddComponent<ComponentObject>();
        
        // Configure the other object with specific values
        other.SetComponentType(ComponentObject.ComponentType.Screw);
        other.SetGroup("TestGroup");
        other.SetSelectedAxis(new Vector3(1, 2, 3));
        other.SetIsPlaced(true);
        other.IsReleased = true;
        other.IsDestroyed = true;
        
        // Execute
        componentObject.CopyFrom(other);
        
        // Assert
        Assert.AreEqual(ComponentObject.ComponentType.Screw, componentObject.GetComponentType());
        Assert.AreEqual("TestGroup", componentObject.GetGroup());
        Assert.AreEqual(new Vector3(1, 2, 3), componentObject.GetSelectedAxis());
        Assert.IsTrue(componentObject.GetIsPlaced());
        Assert.IsTrue(componentObject.IsReleased);
        Assert.IsTrue(componentObject.IsDestroyed);
        
        // Cleanup
        Object.Destroy(otherObject);
    }
    
    [Test]
    public void CopyFrom_NullObject_DoesNothing()
    {
        // Setup - Save initial values
        var initialType = componentObject.GetComponentType();
        var initialGroup = componentObject.GetGroup();
        
        // Execute
        componentObject.CopyFrom(null);
        
        // Assert - Values should remain unchanged
        Assert.AreEqual(initialType, componentObject.GetComponentType());
        Assert.AreEqual(initialGroup, componentObject.GetGroup());
    }
    
    [Test]
    public void PlayBuildPopSound_CallsAudioManager()
    {
        // This assumes AudioManager.Instance and "BuildPop" sound are properly set up
        componentObject.PlayBuildPopSound();
        
        // Since we're using real AudioManager, we can only verify the method doesn't throw exceptions
        Assert.Pass("PlayBuildPopSound executed without exceptions");
    }
    
    [UnityTest]
    public IEnumerator Update_InInitializeState_EnablesAndConfiguresLineRenderer()
    {
        // Setup - Set to Initialize state
        componentObject.SetSelectedAxis(Vector3.forward);
        
        // Wait a frame to ensure Start has completed
        yield return null;
        
        // Get the LineRenderer component that should have been added in Start
        var lineRenderer = testObject.GetComponent<LineRenderer>();
        Assert.IsNotNull(lineRenderer, "LineRenderer should exist after Start");
        
        // Set the state to Initialize
        if (StateManager.Instance != null)
        {
            StateManager.Instance.UpdateState(State.Initialize);
        }
        else
        {
            Assert.Fail("StateManager.Instance is null");
        }
        
        // Wait for Update to execute
        yield return null;
        
        // Assert
        Assert.IsTrue(lineRenderer.enabled, "LineRenderer should be enabled in Initialize state");
        
        // Verify line positions
        Vector3 expectedEndPosition = testObject.transform.position + 
            testObject.transform.rotation * Vector3.forward * 0.2f;
        
        Assert.AreEqual(testObject.transform.position, lineRenderer.GetPosition(0), 
            "Start position should match object position");
        Assert.AreEqual(expectedEndPosition, lineRenderer.GetPosition(1), 
            "End position should be calculated correctly");
    }
    
    [UnityTest]
    public IEnumerator Update_NotInInitializeState_DisablesLineRenderer()
    {
        // Wait a frame to ensure Start has completed
        yield return null;
        
        // Get the LineRenderer component that should have been added in Start
        var lineRenderer = testObject.GetComponent<LineRenderer>();
        Assert.IsNotNull(lineRenderer, "LineRenderer should exist after Start");
        
        // Ensure LineRenderer is enabled
        lineRenderer.enabled = true;
        
        // Set to a non-Initialize state
            stateManager.UpdateState(State.PlayBack);
        
        // Wait for Update to execute
        yield return null;
        
        // Assert
        Assert.IsFalse(lineRenderer.enabled, "LineRenderer should be disabled in non-Initialize states");
    }
    
    [Test]
    public void IsAutomaticSnap_GetSet_WorksCorrectly()
    {
        // Test getter and setter
        componentObject.IsAutomaticSnap = true;
        Assert.IsTrue(componentObject.IsAutomaticSnap);
        
        componentObject.IsAutomaticSnap = false;
        Assert.IsFalse(componentObject.IsAutomaticSnap);
    }
    
    [Test]
    public void HasMoved_GetSet_WorksCorrectly()
    {
        // Test getter and setter
        componentObject.HasMoved = true;
        Assert.IsTrue(componentObject.HasMoved);
        
        componentObject.HasMoved = false;
        Assert.IsFalse(componentObject.HasMoved);
    }
    
    [Test]
    public void IsReleased_GetSet_WorksCorrectly()
    {
        componentObject.IsReleased = true;
        Assert.IsTrue(componentObject.IsReleased);
        
        componentObject.IsReleased = false;
        Assert.IsFalse(componentObject.IsReleased);
    }
    
    [Test]
    public void IsDestroyed_GetSet_WorksCorrectly()
    {
        componentObject.IsDestroyed = true;
        Assert.IsTrue(componentObject.IsDestroyed);
        
        componentObject.IsDestroyed = false;
        Assert.IsFalse(componentObject.IsDestroyed);
    }
}