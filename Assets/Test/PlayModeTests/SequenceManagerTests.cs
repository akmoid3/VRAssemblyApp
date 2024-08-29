using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;

public class SequenceManagerTests
{
    private SequenceManager sequenceManager;
    private StateManager stateManager;

    private GameObject testComponent;
    private List<ComponentData> testSequence;

    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a new GameObject and attach SequenceManager component
        GameObject gameObject = new GameObject();
        sequenceManager = gameObject.AddComponent<SequenceManager>();

        // Initialize testComponent and testSequence for testing
        testComponent = new GameObject("TestComponent");
        testComponent.AddComponent<ComponentObject>().SetGroup(ComponentObject.Group.None);

        testSequence = new List<ComponentData>
        {
            new ComponentData { componentName = "TestComponent", group = ComponentObject.Group.None }
        };

    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(sequenceManager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(testComponent);
    }

    [Test]
    public void InitializeSequence_ShouldInitializeProperly()
    {
        sequenceManager.InitializeSequence(testSequence);

        Assert.AreEqual(testSequence, sequenceManager.AssemblySequence);
        Assert.AreEqual(0, sequenceManager.CurrentStep);
        Assert.AreEqual(0, sequenceManager.ErrorCount);
    }

    [Test]
    public void IncrementCurrentStep_ShouldIncrementStepAndInvokeEvent()
    {
        bool eventInvoked = false;
        SequenceManager.OnStepChanged += (step) => eventInvoked = true;

        sequenceManager.InitializeSequence(testSequence);
        sequenceManager.IncrementCurrentStep();

        Assert.AreEqual(1, sequenceManager.CurrentStep);
        Assert.IsTrue(eventInvoked);

        SequenceManager.OnStepChanged -= (step) => eventInvoked = true;
    }

    [Test]
    public void IncrementCurrentError_ShouldIncrementErrorAndInvokeEvent()
    {
        bool eventInvoked = false;
        SequenceManager.OnErrorCountChanged += (errorCount) => eventInvoked = true;

        sequenceManager.InitializeSequence(testSequence);
        sequenceManager.IncrementCurrentError();

        Assert.AreEqual(1, sequenceManager.ErrorCount);
        Assert.IsTrue(eventInvoked);

        SequenceManager.OnErrorCountChanged -= (errorCount) => eventInvoked = true;
    }

    [Test]
    public void SaveBuildingSequence_ShouldInvokeSaveMethods()
    {
        // Create a new GameObject and add the SaveSequenceMock component
        var saveSequenceMockObject = new GameObject();
        var saveSequenceMock = saveSequenceMockObject.AddComponent<SaveSequenceMock>();

        // Set the saveSequence field of SequenceManager to the mock
        sequenceManager.GetType().GetField("saveSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(sequenceManager, saveSequenceMock);

        // Call the method under test
        sequenceManager.SaveBuildingSequence(testComponent, "TestModel");

        // Assert that the correct methods were called
        Assert.IsTrue(saveSequenceMock.SaveComponentCalled);
        Assert.IsTrue(saveSequenceMock.SaveSequenceToJSONCalled);

        // Clean up the GameObject after test
        Object.DestroyImmediate(saveSequenceMockObject);
    }


    [Test]
    public void ValidateComponent_ShouldIncrementErrorOnInvalidComponent()
    {
        // Initialize the sequence with a valid component
        sequenceManager.InitializeSequence(testSequence);

        // Create an invalid GameObject and attach the ComponentObject component
        GameObject invalidComponent = new GameObject("InvalidComponent");
        var componentObject = invalidComponent.AddComponent<ComponentObject>();
        componentObject.SetGroup(ComponentObject.Group.Group02); // Setting a group that is different from the expected one

        // Validate the invalid component
        sequenceManager.ValidateComponent(invalidComponent);

        // Assert that the error count was incremented
        Assert.AreEqual(1, sequenceManager.ErrorCount);

        // Clean up the GameObject after test
        Object.DestroyImmediate(invalidComponent);
    }

    [Test]
    public void ModifyBuildingSequence_ShouldInvokeModifyComponentAndSaveSequenceToJSON()
    {
        // Create a new GameObject and add the SaveSequenceMock component
        var saveSequenceMockObject = new GameObject();
        var saveSequenceMock = saveSequenceMockObject.AddComponent<SaveSequenceMock>();

        // Set the saveSequence field of SequenceManager to the mock
        sequenceManager.GetType().GetField("saveSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(sequenceManager, saveSequenceMock);

        // Call the method under test
        sequenceManager.ModifyBuildingSequence(testComponent, "TestModel");

        // Assert that the correct methods were called
        Assert.IsTrue(saveSequenceMock.ModifyComponentCalled);
        Assert.IsTrue(saveSequenceMock.SaveSequenceToJSONCalled);

        // Clean up the GameObject after test
        Object.DestroyImmediate(saveSequenceMockObject);
    }

    [Test]
    public void RemoveComponentFromSequence_ShouldInvokeRemoveComponentAndSaveSequenceToJSON()
    {
        // Create a new GameObject and add the SaveSequenceMock component
        var saveSequenceMockObject = new GameObject();
        var saveSequenceMock = saveSequenceMockObject.AddComponent<SaveSequenceMock>();

        // Set the saveSequence field of SequenceManager to the mock
        sequenceManager.GetType().GetField("saveSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(sequenceManager, saveSequenceMock);

        // Call the method under test
        sequenceManager.RemoveComponentFromSequence(testComponent, "TestModel");

        // Assert that the correct methods were called
        Assert.IsTrue(saveSequenceMock.RemoveComponentCalled);
        Assert.IsTrue(saveSequenceMock.SaveSequenceToJSONCalled);

        // Clean up the GameObject after test
        Object.DestroyImmediate(saveSequenceMockObject);
    }


    [Test]
    public void ValidateComponent_ShouldNotIncrementErrorOnValidComponent()
    {
        sequenceManager.InitializeSequence(testSequence);

        sequenceManager.ValidateComponent(testComponent);

        Assert.AreEqual(0, sequenceManager.ErrorCount);
    }

    [Test]
    public void FinishTime_ShouldSetAndGetCorrectly()
    {
        string expectedFinishTime = "2024-08-28 15:30:00";

        sequenceManager.FinishTime = expectedFinishTime;
        string actualFinishTime = sequenceManager.FinishTime;

        Assert.AreEqual(expectedFinishTime, actualFinishTime);
    }

}

public class SaveSequenceMock : SaveSequence
{
    public bool ModifyComponentCalled { get; private set; } = false;
    public bool RemoveComponentCalled { get; private set; } = false;
    public bool SaveComponentCalled { get; private set; } = false;
    public bool SaveSequenceToJSONCalled { get; private set; } = false;

    public override void ModifyComponent(GameObject component)
    {
        ModifyComponentCalled = true;
    }

    public override void RemoveComponent(GameObject component)
    {
        RemoveComponentCalled = true;
    }

    public override void SaveComponent(GameObject component)
    {
        SaveComponentCalled = true;
    }

    public override void SaveSequenceToJSON(string modelName)
    {
        SaveSequenceToJSONCalled = true;
    }
}

