using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StateManagerTests
{
    private GameObject gameObject1;
    private GameObject gameObject2;
    private StateManager stateManager1;
    private StateManager stateManager2;
    private State receivedState;
    private int eventCallCount;

    [SetUp]
    public void SetUp()
    {
        // Reset static instance before each test
        var instanceField = typeof(StateManager).GetProperty("Instance", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        instanceField.SetValue(null, null);

        // Create a GameObject and add the StateManager component
        gameObject1 = new GameObject("StateManager1");
        stateManager1 = gameObject1.AddComponent<StateManager>();

        // Reset event tracking variables
        receivedState = default;
        eventCallCount = 0;
    }

    [TearDown]
    public void TearDown()
    {
        // Remove event handler if registered
        StateManager.OnStateChanged -= OnStateChangedHandler;
        
        // Destroy test GameObjects
        if (gameObject1 != null)
            UnityEngine.Object.DestroyImmediate(gameObject1);
        if (gameObject2 != null)
            UnityEngine.Object.DestroyImmediate(gameObject2);
    }

    private void OnStateChangedHandler(State state)
    {
        receivedState = state;
        eventCallCount++;
    }

    [Test]
    public void Awake_SetsSingletonInstance()
    {
        // Assert
        Assert.IsNotNull(StateManager.Instance);
        Assert.AreEqual(stateManager1, StateManager.Instance);
    }

    [Test]
    public void Awake_DestroysDuplicateInstances()
    {
        // Arrange
        gameObject2 = new GameObject("StateManager2");
        stateManager2 = gameObject2.AddComponent<StateManager>();

        // Act - Give a frame for the Awake method to execute
        // The MonoBehaviour's Awake will be called automatically

        // Assert
        Assert.IsNotNull(StateManager.Instance);
        Assert.AreEqual(stateManager1, StateManager.Instance);
        Assert.AreNotEqual(stateManager2, StateManager.Instance);
    }

    [Test]
    public void UpdateState_ChangesCurrentState()
    {
        // Arrange
        var newState = State.Record;

        // Act
        stateManager1.UpdateState(newState);

        // Assert
        Assert.AreEqual(newState, stateManager1.CurrentState);
    }

    [Test]
    public void UpdateState_TriggersOnStateChangedEvent()
    {
        // Arrange
        var newState = State.PlayBack;
        StateManager.OnStateChanged += OnStateChangedHandler;

        // Act
        stateManager1.UpdateState(newState);

        // Assert
        Assert.AreEqual(newState, receivedState);
        Assert.AreEqual(1, eventCallCount);
    }

    [Test]
    public void UpdateState_MultipleCalls_TriggersEventCorrectly()
    {
        // Arrange
        StateManager.OnStateChanged += OnStateChangedHandler;

        // Act
        stateManager1.UpdateState(State.Initialize);
        stateManager1.UpdateState(State.Record);
        stateManager1.UpdateState(State.PlayBack);

        // Assert
        Assert.AreEqual(State.PlayBack, receivedState);
        Assert.AreEqual(3, eventCallCount);
    }

    [Test]
    public void Instance_AccessibleFromAnywhere()
    {
        // Act
        var instance = StateManager.Instance;

        // Assert
        Assert.IsNotNull(instance);
        Assert.AreEqual(stateManager1, instance);
    }

    [Test]
    public void InitialState_ShouldBeDefault()
    {
        // Assert - by default, enum values are initialized to 0, which is the first value
        Assert.AreEqual(default(State), stateManager1.CurrentState);
    }

    [Test]
    public void OnStateChanged_MultipleSubscribers_AllReceiveUpdates()
    {
        // Arrange
        int secondSubscriberCount = 0;
        State secondSubscriberState = default;

        StateManager.OnStateChanged += OnStateChangedHandler;
        StateManager.OnStateChanged += (state) => {
            secondSubscriberCount++;
            secondSubscriberState = state;
        };

        // Act
        stateManager1.UpdateState(State.Finish);

        // Assert
        Assert.AreEqual(1, eventCallCount);
        Assert.AreEqual(1, secondSubscriberCount);
        Assert.AreEqual(State.Finish, receivedState);
        Assert.AreEqual(State.Finish, secondSubscriberState);
    }

    [Test]
    public void UnsubscribedHandlers_DoNotReceiveEvents()
    {
        // Arrange
        StateManager.OnStateChanged += OnStateChangedHandler;
        stateManager1.UpdateState(State.SelectingMode); // Should trigger event
        
        // Reset counter and unsubscribe
        eventCallCount = 0;
        StateManager.OnStateChanged -= OnStateChangedHandler;

        // Act
        stateManager1.UpdateState(State.Initialize);

        // Assert
        Assert.AreEqual(0, eventCallCount, "Unsubscribed handler should not receive events");
    }
}