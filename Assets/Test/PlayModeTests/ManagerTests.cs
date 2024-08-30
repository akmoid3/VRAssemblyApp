using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;

public class MockSequenceManager : SequenceManager
{
    public bool IncrementCurrentStepCalled { get; private set; }
    public bool IncrementCurrentErrorCalled { get; private set; }
    public bool InitializeSequenceCalled { get; private set; }
    public bool SaveBuildingSequenceCalled { get; private set; }
    public bool ModifyBuildingSequenceCalled { get; private set; }
    public bool RemoveComponentFromSequenceCalled { get; private set; }
    public bool ValidateComponentCalled { get; private set; }

    public override void IncrementCurrentStep()
    {
        IncrementCurrentStepCalled = true;
    }

    public override void IncrementCurrentError()
    {
        IncrementCurrentErrorCalled = true;
    }

    public override void InitializeSequence(List<ComponentData> sequence)
    {
        InitializeSequenceCalled = true;
    }

    public override void SaveBuildingSequence(GameObject selectedComponent, string modelName)
    {
        SaveBuildingSequenceCalled = true;
    }

    public override void ModifyBuildingSequence(GameObject selectedComponent, string modelName)
    {
        ModifyBuildingSequenceCalled = true;
    }

    public override void RemoveComponentFromSequence(GameObject selectedComponent, string modelName)
    {
        RemoveComponentFromSequenceCalled = true;
    }

    public override void ValidateComponent(GameObject component)
    {
        ValidateComponentCalled = true;
    }
}

public class MockInteractionManager : InteractionManager
{
    public bool OnSelectEnterCalled { get; private set; }
    public bool OnSelectExitCalled { get; private set; }
    public bool OnHoverEnterCalled { get; private set; }
    public bool OnHoverExitCalled { get; private set; }

    public override void OnSelectEnter(SelectEnterEventArgs args)
    {
        OnSelectEnterCalled = true;
    }

    public override void OnSelectExit(SelectExitEventArgs args)
    {
        OnSelectExitCalled = true;
    }

    public override void OnHoverEnter(HoverEnterEventArgs args)
    {
        OnHoverEnterCalled = true;
    }

    public override void OnHoverExit(HoverExitEventArgs args)
    {
        OnHoverExitCalled = true;
    }
}

public class MockHintManager : HintManager
{
    public bool ShowHintCalled { get; private set; }

    public override void ShowHint(List<ComponentData> assemblySequence, int currentStep, List<Transform> components, SnapToPosition interactor)
    {
        ShowHintCalled = true;
    }
}

public class MockAutomaticPlacementManager : AutomaticPlacementManager
{
    public bool PlaceAllComponentsGraduallyCalled { get; private set; }

    public override void PlaceAllComponentsGradually(float delay, SnapToPosition interactor, List<ComponentData> assemblySequence, List<Transform> components, ToolManager toolManager)
    {
        PlaceAllComponentsGraduallyCalled = true;
    }
}

public class MockPdfLoader : PdfLoader
{
    public bool LoadPDFCalled { get; private set; }

    public override void LoadPDF(string modelName)
    {
        LoadPDFCalled = true;
    }
}
public class MockMakeGrabbable : MakeGrabbable
{
    public bool MakeObjectGrabbableCalled { get; private set; }
    public bool MakeObjectNonGrabbableCalled { get; private set; }

    public override void MakeObjectGrabbable()
    {
        MakeObjectGrabbableCalled = true;
    }

    public override void MakeObjectNonGrabbable()
    {
        MakeObjectNonGrabbableCalled = true;
    }
}


[TestFixture]
public class ManagerTests
{
    private Manager _manager;
    private StateManager _mockStateManager;
    private MockSequenceManager _mockSequenceManager;
    private MockInteractionManager _mockInteractionManager;
    private MockHintManager _mockHintManager;
    private MockAutomaticPlacementManager _mockAutomaticPlacementManager;
    private MockPdfLoader _mockPdfLoader;
    private GameObject selectedComponent;
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockStateManager = new GameObject().AddComponent<StateManager>();
        _mockSequenceManager = new GameObject().AddComponent<MockSequenceManager>();
        _mockInteractionManager = new GameObject().AddComponent<MockInteractionManager>();
        _mockHintManager = new GameObject().AddComponent<MockHintManager>();
        _mockAutomaticPlacementManager = new GameObject().AddComponent<MockAutomaticPlacementManager>();
        _mockPdfLoader = new GameObject().AddComponent<MockPdfLoader>();

        // Initialize the manager with mock dependencies
        _manager = new GameObject().AddComponent<Manager>();

        _manager.GetType()
            .GetField("stateManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockStateManager);

        _manager.GetType()
            .GetField("sequenceManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockSequenceManager);

        _manager.GetType()
            .GetField("interactionManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockInteractionManager);

        _manager.GetType()
            .GetField("hintManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockHintManager);

        _manager.GetType()
            .GetField("automaticPlacementManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockAutomaticPlacementManager);

        _manager.GetType()
            .GetField("pdfLoader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockPdfLoader);

        selectedComponent = new GameObject("SelectedComponent");
        selectedComponent.AddComponent<MockMakeGrabbable>();
        selectedComponent.AddComponent<ComponentObject>();
        _manager.CurrentSelectedComponent = selectedComponent;
        _manager.Model = new GameObject("Model");
        _manager.Components.Add(selectedComponent.transform);

    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(_manager.gameObject);
        Object.DestroyImmediate(_mockStateManager.gameObject);
        Object.DestroyImmediate(_mockSequenceManager.gameObject);
        Object.DestroyImmediate(_mockInteractionManager.gameObject);
        Object.DestroyImmediate(_mockHintManager.gameObject);
        Object.DestroyImmediate(_mockAutomaticPlacementManager.gameObject);
        Object.DestroyImmediate(_mockPdfLoader.gameObject); 
    }
    [Test]
    public void IncrementCurrentStep_ShouldCallIncrementOnSequenceManager()
    {
        // Act
        var incrementMethod = typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        // Assert
        Assert.IsTrue(_mockSequenceManager.IncrementCurrentStepCalled);
    }

    [Test]
    public void IncrementCurrentError_ShouldCallIncrementErrorOnSequenceManager()
    {
        // Act
        _manager.IncrementCurrentError();

        // Assert
        Assert.IsTrue(_mockSequenceManager.IncrementCurrentErrorCalled);
    }

    [Test]
    public void OnSelectEnter_ShouldCallOnSelectEnterOnInteractionManager()
    {
        _mockStateManager.CurrentState = State.PlayBack;
        // Act
        _manager.OnSelectEnter(new SelectEnterEventArgs());

        // Assert
        Assert.IsTrue(_mockInteractionManager.OnSelectEnterCalled);
    }

    [Test]
    public void OnSelectExit_ShouldCallOnSelectExitOnInteractionManager()
    {
        // Act
        _manager.OnSelectExit(new SelectExitEventArgs());

        // Assert
        Assert.IsTrue(_mockInteractionManager.OnSelectExitCalled);
    }

    [Test]
    public void OnHoverEnter_ShouldCallOnHoverEnterOnInteractionManager()
    {
        // Act
        _manager.OnHoverEnter(new HoverEnterEventArgs());

        // Assert
        Assert.IsTrue(_mockInteractionManager.OnHoverEnterCalled);
    }

    [Test]
    public void OnHoverExit_ShouldCallOnHoverExitOnInteractionManager()
    {
        // Act
        _manager.OnHoverExit(new HoverExitEventArgs());

        // Assert
        Assert.IsTrue(_mockInteractionManager.OnHoverExitCalled);
    }

    [Test]
    public void HandleStateChange_ShouldCallLoadPDF_WhenStateIsRecord()
    {
        // Act
        _manager.HandleStateChange(State.Record);

        // Assert
        Assert.IsTrue(_mockPdfLoader.LoadPDFCalled);
    }

    [Test]
    public void ShowHint_ShouldCallShowHintOnHintManager()
    {
        // Act
        _manager.ShowHint();

        // Assert
        Assert.IsTrue(_mockHintManager.ShowHintCalled);
    }

    [Test]
    public void PlaceAllComponentsGradually_ShouldCallPlaceAllComponentsGraduallyOnAutomaticPlacementManager()
    {
        // Arrange
        float delay = 1.0f;

        // Act
        _manager.PlaceAllComponentsGradually(delay);

        // Assert
        Assert.IsTrue(_mockAutomaticPlacementManager.PlaceAllComponentsGraduallyCalled);
    }

    [Test]
    public void InitializeSequence_ShouldCallInitializeSequenceOnSequenceManager()
    {
        // Arrange
        var sequence = new List<ComponentData>();

        // Act
        _manager.InitializeSequence(sequence);

        // Assert
        Assert.IsTrue(_mockSequenceManager.InitializeSequenceCalled);
    }

    [Test]
    public void SaveBuildingSequence_ShouldCallSaveBuildingSequenceOnSequenceManager()
    {
        

        // Act
        _manager.SaveBuildingSequence();

        // Assert
        Assert.IsTrue(_mockSequenceManager.SaveBuildingSequenceCalled);
    }

    [Test]
    public void ModifyBuildingSequence_ShouldCallModifyBuildingSequenceOnSequenceManager()
    {
        // Act
        _manager.ModifyBuildingSequence();

        // Assert
        Assert.IsTrue(_mockSequenceManager.ModifyBuildingSequenceCalled);
    }

    [Test]
    public void RemoveComponentFromSequence_ShouldCallRemoveComponentFromSequenceOnSequenceManager()
    {
    
        // Act
        _manager.RemoveComponentFromSequence();

        // Assert
        Assert.IsTrue(_mockSequenceManager.RemoveComponentFromSequenceCalled);
    }

    [Test]
    public void MakeComponentsGrabbable_ShouldCallMakeObjectGrabbableOnAllComponents()
    {
        // Act
        var makeComponentsGrabbableMethod = typeof(Manager).GetMethod("MakeComponentsGrabbable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        makeComponentsGrabbableMethod.Invoke(_manager, null);
        // Assert
        foreach (Transform component in _manager.Components)
        {
            var makeGrabbable = component.GetComponent<MockMakeGrabbable>();
            Assert.IsTrue(makeGrabbable.MakeObjectGrabbableCalled);
        }
    }

    [Test]
    public void MakeComponentsNonGrabbable_ShouldCallMakeObjectNonGrabbableOnAllComponents()
    {
        // Act
        var makeComponentsNonGrabbableMethod = typeof(Manager).GetMethod("MakeComponentsNonGrabbable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        makeComponentsNonGrabbableMethod.Invoke(_manager, null);

        // Assert
        foreach (Transform component in _manager.Components)
        {
            var makeGrabbable = component.GetComponent<MockMakeGrabbable>();
            Assert.IsTrue(makeGrabbable.MakeObjectNonGrabbableCalled);
        }
    }
}
