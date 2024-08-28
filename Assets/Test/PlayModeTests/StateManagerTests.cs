using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.TestTools;

[TestFixture]
public class StateManagerTests
{
    private GameObject gameObject;
    private StateManager stateManager;

    [SetUp]
    public void SetUp()
    {
        // Create a new GameObject and attach the StateManager script to it
        gameObject = new GameObject("StateManagerTestObject");
        stateManager = gameObject.AddComponent<StateManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        GameObject.DestroyImmediate(gameObject);
    }

    [Test]
    public void TestSingletonInstance()
    {
        // Test if the StateManager creates a singleton instance
        Assert.IsNotNull(StateManager.Instance);
        Assert.AreEqual(stateManager, StateManager.Instance);

        // Create another GameObject with StateManager attached
        var anotherGameObject = new GameObject("AnotherStateManagerTestObject");
        var anotherStateManager = anotherGameObject.AddComponent<StateManager>();

        // Ensure that the new instance does not overwrite the existing singleton
        Assert.AreEqual(stateManager, StateManager.Instance);

        // Cleanup
        GameObject.DestroyImmediate(anotherGameObject);
    }

    [Test]
    public void TestUpdateState()
    {
        // Set up a flag to ensure the event is triggered
        bool eventTriggered = false;
        State triggeredState = State.ChoosingModel;

        // Subscribe to the OnStateChanged event
        StateManager.OnStateChanged += (state) =>
        {
            eventTriggered = true;
            triggeredState = state;
        };

        // Call UpdateState and verify the state is updated
        stateManager.UpdateState(State.Record);
        Assert.AreEqual(State.Record, stateManager.CurrentState);

        // Verify the event was triggered and with the correct state
        Assert.IsTrue(eventTriggered);
        Assert.AreEqual(State.Record, triggeredState);

        // Unsubscribe from the event to avoid interference in other tests
        StateManager.OnStateChanged -= (state) =>
        {
            eventTriggered = true;
            triggeredState = state;
        };
    }

    [UnityTest]
    public IEnumerator TestAwakeDestroysDuplicateInstances()
    {
        // Simulate a duplicate instance creation
        var duplicateGameObject = new GameObject("DuplicateStateManagerTestObject");
        var duplicateStateManager = duplicateGameObject.AddComponent<StateManager>();

        // Check that the duplicate instance is not the active singleton instance
        Assert.AreNotEqual(duplicateStateManager, StateManager.Instance);
       
        yield return null;

        // Wait until the end of the frame to ensure the destruction occurs
        Assert.IsTrue(duplicateStateManager == null);


        // Assert that the duplicate instance was destroyed
        Assert.AreEqual(stateManager, StateManager.Instance);

        // Cleanup
        GameObject.DestroyImmediate(duplicateGameObject);
    }

}
