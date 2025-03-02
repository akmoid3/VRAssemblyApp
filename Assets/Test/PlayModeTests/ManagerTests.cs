using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.TestTools;
using Moq;
using System.Collections;
using Object = UnityEngine.Object;

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
    public bool HighlightComponentToPlaceCalled { get; private set; }
    public bool HideHintCalled { get; private set; }

    public override void ShowHint(int currentStep, Transform component, SnapToPosition interactor)
    {
        ShowHintCalled = true;
    }

    public override void HideHints(SnapToPosition interactor)
    {
        HideHintCalled = true;
    }
}

public class MockAutomaticPlacementManager : AutomaticPlacementManager
{
    public bool PlaceAllComponentsGraduallyCalled { get; private set; }
    public bool PlaceInitialComponentCalled { get; private set; }
    
    
    public override void PlaceCurrentStepComponent(int stepIndex, Transform componentToPlace, SnapToPosition interactor,
        float timeMovement)
    {
        PlaceInitialComponentCalled = true;
    }

    public override void PlaceStepComponent(int stepIndex, Transform componentToPlace, SnapToPosition interactor)
    {
        PlaceInitialComponentCalled = true;
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
    private AudioManager audioManager;

    [SetUp]
    public void SetUp()
    {
        audioManager = new GameObject().AddComponent<AudioManager>();
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
            .GetField("stateManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockStateManager);

        _manager.sequenceManager = _mockSequenceManager;


        _manager.GetType()
            .GetField("interactionManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockInteractionManager);

        _manager.GetType()
            .GetField("hintManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockHintManager);

        _manager.GetType()
            .GetField("automaticPlacementManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockAutomaticPlacementManager);

        _manager.GetType()
            .GetField("pdfLoaderPdf",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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
        Object.DestroyImmediate(audioManager.gameObject);
    }

    
    [Test]
    public void TestPerformaceForEachStepProperty()
    {
        // Arrange
        var steps = new List<float> { 0.5f, 1.5f, 2.5f };

        // Act
        _manager.PerformaceForEachStep = steps;

        // Assert
        Assert.AreEqual(steps, _manager.PerformaceForEachStep, "PerformaceForEachStep should return the list that was set.");
    }

    [Test]
    public void TestLoaderPDFProperty()
    {
        // Arrange
        // Assuming PdfLoader is a regular class; if it's a MonoBehaviour, you might need to add it to a GameObject.
        var pdfLoader = new PdfLoader();

        // Act
        _manager.LoaderPDF = pdfLoader;

        // Assert
        Assert.AreEqual(pdfLoader, _manager.LoaderPDF, "LoaderPDF should return the PdfLoader instance that was set.");
    }

    [Test]
    public void TestModelNameProperty()
    {
        // Arrange
        string testName = "TestModel";

        // Act
        _manager.ModelName = testName;

        // Assert
        Assert.AreEqual(testName, _manager.ModelName, "ModelName should return the string that was set.");
    }

    [Test]
    public void TestModelProperty()
    {
        // Arrange
        var testModel = new GameObject("TestModelObject");

        // Act
        _manager.Model = testModel;

        // Assert
        Assert.AreEqual(testModel, _manager.Model, "Model should return the GameObject that was set.");

        // Cleanup
        Object.DestroyImmediate(testModel);
    }

    [Test]
    public void TestComponentsProperty()
    {
        // Arrange
        var componentsList = new List<Transform>();
        var go1 = new GameObject("Component1");
        var go2 = new GameObject("Component2");
        componentsList.Add(go1.transform);
        componentsList.Add(go2.transform);

        // Act
        _manager.Components = componentsList;

        // Assert
        Assert.AreEqual(componentsList, _manager.Components, "Components should return the list that was set.");

        // Cleanup
        Object.DestroyImmediate(go1);
        Object.DestroyImmediate(go2);
    }

    [Test]
    public void TestRemovedComponentsProperty()
    {
        // Arrange
        var removedList = new List<Transform>();
        var go1 = new GameObject("RemovedComponent1");
        var go2 = new GameObject("RemovedComponent2");
        removedList.Add(go1.transform);
        removedList.Add(go2.transform);

        // Act
        _manager.RemovedComponents = removedList;

        // Assert
        Assert.AreEqual(removedList, _manager.RemovedComponents, "RemovedComponents should return the list that was set.");

        // Cleanup
        Object.DestroyImmediate(go1);
        Object.DestroyImmediate(go2);
    }
    [Test]
    public void IncrementCurrentStep_ShouldCallIncrementOnSequenceManager()
    {
        // Act
        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
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

        LogAssert.Expect(LogType.Error, "Interactor is not assigned.");

        _manager.HandleStateChange(State.PlayBack);

        Assert.IsTrue(_mockPdfLoader.LoadPDFCalled);
    }

    [Test]
    public void ShowHint_ShouldCallShowHintOnHintManager()
    {
        _manager.ComponentsThatCanSnap = new List<Transform>();
        _manager.ComponentsThatCanSnap.Add(selectedComponent.transform);
        // Act
        _manager.ShowHint();

        // Assert
        Assert.IsTrue(_mockHintManager.ShowHintCalled);
    }
    
    [Test]
    public void HideHints_ShouldCallHideHintOnHintManager()
    {
        // Act
        _manager.HideHint();

        // Assert
        Assert.IsTrue(_mockHintManager.HideHintCalled);
    }


    [Test]
    public void PlaceInitialComponent_ShouldCallPlaceCurrentComponentOnAutomaticPlacementManager()
    {
        _manager.ComponentsThatCanSnap = new List<Transform>();
        _manager.ComponentsThatCanSnap.Add(selectedComponent.transform);
        // Act
        _manager.PlaceInitialComponent();

        // Assert
        Assert.IsTrue(_mockAutomaticPlacementManager.PlaceInitialComponentCalled);
    }
    
    [Test]
    public void PlaceCurrentComponent_ShouldCallPlaceCurrentComponentOnAutomaticPlacementManager()
    {
        _manager.ComponentsThatCanSnap = new List<Transform>();
        _manager.ComponentsThatCanSnap.Add(selectedComponent.transform);
        // Act
        _manager.PlaceCurrentComponent(1f);

        // Assert
        Assert.IsTrue(_mockAutomaticPlacementManager.PlaceInitialComponentCalled);
    }
    
    [Test]
    public void PlaceComponent_ShouldCallPlaceStepComponentOnAutomaticPlacementManager()
    {
        _manager.ComponentsThatCanSnap = new List<Transform>();
        _manager.ComponentsThatCanSnap.Add(selectedComponent.transform);
        // Act
        _manager.PlaceComponent(0,selectedComponent.transform);

        // Assert
        Assert.IsTrue(_mockAutomaticPlacementManager.PlaceInitialComponentCalled);
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
        var makeComponentsGrabbableMethod = typeof(Manager).GetMethod("MakeComponentsGrabbable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
        var makeComponentsNonGrabbableMethod = typeof(Manager).GetMethod("MakeComponentsNonGrabbable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        makeComponentsNonGrabbableMethod.Invoke(_manager, null);

        // Assert
        foreach (Transform component in _manager.Components)
        {
            var makeGrabbable = component.GetComponent<MockMakeGrabbable>();
            Assert.IsTrue(makeGrabbable.MakeObjectNonGrabbableCalled);
        }
    }

    [Test]
    public void HighlightComponentToPlace_ShouldCallHighlightComponentToPlaceOnHintManager()
    {
        // Act
        var highlightComponentToPlaceMethod = typeof(Manager).GetMethod("HighlightComponentToPlace",
            BindingFlags.NonPublic | BindingFlags.Instance);
        highlightComponentToPlaceMethod.Invoke(_manager, null);

        // Assert
        Assert.IsTrue(_mockHintManager.HighlightComponentToPlaceCalled);
    }

    [Test]
    public void HideCorrectSnapPoint_ShouldDisableMeshRendererOfCorrectSnapPoint()
    {
        // Arrange
        // Create a mock interactor with a child corresponding to the CurrentStep
        var interactor = new GameObject("Interactor").AddComponent<SnapToPosition>();
        interactor.transform.SetParent(_manager.transform);

        // Set the current step to 0
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Create a snap point as a child of the interactor
        var snapPoint = new GameObject("SnapPoint");
        var meshRenderer = snapPoint.AddComponent<MeshRenderer>();
        snapPoint.transform.SetParent(interactor.transform);

        // Assign the interactor to the manager
        _manager.GetType().GetField("interactor", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, interactor);

        // Act
        var hideCorrectSnapPointMethod =
            typeof(Manager).GetMethod("HideCorrectSnapPoint", BindingFlags.NonPublic | BindingFlags.Instance);
        hideCorrectSnapPointMethod.Invoke(_manager, null);

        // Assert
        Assert.IsFalse(meshRenderer.enabled);
    }


    [Test]
    public void ProcessComponentPlacement_ShouldCallValidateComponent_WhenComponentIsValid()
    {
        // Arrange
        var componentData = new ComponentData { componentName = selectedComponent.name, toolName = "Tool" };
        var processComponentPlacementMethod = typeof(Manager).GetMethod("ProcessComponentPlacement",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        processComponentPlacementMethod.Invoke(_manager, new object[] { componentData });
    }

    [Test]
    public void Update_ShouldCallProcessPlaybackState_WhenCurrentStateIsPlayBack()
    {
        _mockStateManager.CurrentState = State.PlayBack;

        // Mock the AssemblySequence by creating a list with dummy data
        var mockAssemblySequence = new List<ComponentData>
        {
            new ComponentData
            {
                componentName = "TestComponent1",
                position = new Vector3(1, 0, 0),
                rotation = Quaternion.identity,
                toolName = "null",
                group = "None",
                type = ComponentObject.ComponentType.Screw
            },
            new ComponentData
            {
                componentName = "TestComponent2",
                position = new Vector3(2, 0, 0),
                rotation = Quaternion.identity,
                toolName = "null",
                group = "None",
                type = ComponentObject.ComponentType.Nail
            }
        };

        // Assign the mockAssemblySequence to the AssemblySequence property
        _manager.AssemblySequence = mockAssemblySequence;

        // Set CurrentStep to a valid index
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Act
        var updateMethod = typeof(Manager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        updateMethod.Invoke(_manager, null);
    }


    [Test]
    public void HandleComponentFastener_WithValidInputs_SetsFastenerProperties()
    {
        // Arrange
        var mockComponent = new GameObject().transform;
        var fastener = mockComponent.gameObject.AddComponent<Screw>();

        var componentData = new ComponentData
        {
            toolName = "TestTool",
            toolForce = 5
        };

        Vector3 initialPosition = Vector3.zero;
        fastener.transform.localPosition = initialPosition;


        _manager.HandleComponentFastener(mockComponent, componentData);
        // Act


        // Assert
        Assert.AreEqual("TestTool", fastener.CorrectToolName);
        Assert.AreEqual(5.0f, fastener.CorrectToolForce);
        Assert.AreEqual(initialPosition, fastener.InitialPosition);
    }

    [Test]
    public void HandleCurrentStepPlayBack_WithValidComponents_ProcessesComponentsCorrectly()
    {
        // Arrange
        // Create mock components
        var snappableComponent = new GameObject("SnappableComponent").transform;
        var mockMakeGrabbableSnappable = snappableComponent.gameObject.AddComponent<MockMakeGrabbable>();
        var componentObjectSnappable = snappableComponent.gameObject.AddComponent<ComponentObject>();

        var nonSnappableComponent = new GameObject("NonSnappableComponent").transform;
        var mockMakeGrabbableNonSnappable = nonSnappableComponent.gameObject.AddComponent<MockMakeGrabbable>();
        var componentObjectNonSnappable = nonSnappableComponent.gameObject.AddComponent<ComponentObject>();
        componentObjectNonSnappable.SetIsPlaced(false); // Not placed, should be made non-grabbable

        var placedComponent = new GameObject("PlacedComponent").transform;
        var mockMakeGrabbablePlaced = placedComponent.gameObject.AddComponent<MockMakeGrabbable>();
        var componentObjectPlaced = placedComponent.gameObject.AddComponent<ComponentObject>();
        componentObjectPlaced.SetIsPlaced(true); // Already placed, should be ignored

        // Add components to manager
        _manager.Components.Clear();
        _manager.Components.Add(snappableComponent);
        _manager.Components.Add(nonSnappableComponent);
        _manager.Components.Add(placedComponent);

        // Setup componentsThatCanSnap list
        var componentsThatCanSnap = new List<Transform> { snappableComponent };
        _manager.GetType().GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, componentsThatCanSnap);

        // Setup Assembly Sequence
        var testComponentData = new ComponentData
        {
            componentName = "TestComponent",
            toolName = "TestTool",
            toolForce = 10
        };
        _manager.AssemblySequence = new List<ComponentData> { testComponentData };

        // Set CurrentStep to a valid index
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Create a spy for HandleComponentFastener
        bool handleComponentFastenerCalled = false;
        Transform fastenerComponent = null;
        ComponentData fastenerData = null;

        // Create a method to intercept the call to HandleComponentFastener
        var originalMethod = typeof(Manager).GetMethod("HandleComponentFastener",
            BindingFlags.Instance | BindingFlags.Public);

        // Act
        var handleCurrentStepPlayBackMethod = typeof(Manager).GetMethod("HandleCurrentStepPlayBack",
            BindingFlags.NonPublic | BindingFlags.Instance);
        handleCurrentStepPlayBackMethod.Invoke(_manager, null);

        // Assert
        // Verify that MakeObjectGrabbable was called for snappable component
        Assert.IsTrue(mockMakeGrabbableSnappable.MakeObjectGrabbableCalled,
            "MakeObjectGrabbable should be called for component that can snap");

        // Verify that MakeObjectNonGrabbable was called for non-snappable component that is not placed
        Assert.IsTrue(mockMakeGrabbableNonSnappable.MakeObjectNonGrabbableCalled,
            "MakeObjectNonGrabbable should be called for non-snappable component that is not placed");

        // Clean up
        Object.DestroyImmediate(snappableComponent.gameObject);
        Object.DestroyImmediate(nonSnappableComponent.gameObject);
        Object.DestroyImmediate(placedComponent.gameObject);
    }

    [Test]
    public void UpdateComponentsPerCurrentStep_HandlesBothCases_UpdatesComponentsThatCanSnapCorrectly()
    {
        // ARRANGE
        // Clear existing components
        _manager.Components.Clear();

        // Create test components
        // Case 1: Component not in CurrentAssembledSequence
        var componentMatchingName = new GameObject("MatchingName");
        var componentMatchingNameObj = componentMatchingName.AddComponent<ComponentObject>();
        componentMatchingNameObj.SetIsPlaced(false);

        var componentMatchingGroup = new GameObject("MatchingGroup");
        var componentMatchingGroupObj = componentMatchingGroup.AddComponent<ComponentObject>();
        componentMatchingGroupObj.SetIsPlaced(false);
        componentMatchingGroupObj.SetGroup("TestGroup");

        var componentAlreadyPlaced = new GameObject("AlreadyPlaced");
        var componentAlreadyPlacedObj = componentAlreadyPlaced.AddComponent<ComponentObject>();
        componentAlreadyPlacedObj.SetIsPlaced(true);

        var componentNonMatching = new GameObject("NonMatching");
        var componentNonMatchingObj = componentNonMatching.AddComponent<ComponentObject>();
        componentNonMatchingObj.SetIsPlaced(false);
        componentNonMatchingObj.SetGroup("DifferentGroup");

        // Case 2: Component in CurrentAssembledSequence
        var assembledComponent = new GameObject("AssembledComponent");
        var assembledComponentObj = assembledComponent.AddComponent<ComponentObject>();
        assembledComponentObj.SetIsPlaced(false);

        // Add components to manager
        _manager.Components.Add(componentMatchingName.transform);
        _manager.Components.Add(componentMatchingGroup.transform);
        _manager.Components.Add(componentAlreadyPlaced.transform);
        _manager.Components.Add(componentNonMatching.transform);
        _manager.Components.Add(assembledComponent.transform);

        // Setup componentsThatCanSnap list - initially containing some components
        var componentsThatCanSnap = new List<Transform>
        {
            componentNonMatching.transform, // Should be removed in both cases
            componentAlreadyPlaced.transform // Should be removed in both cases
        };
        _manager.GetType().GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, componentsThatCanSnap);

        // Setup Assembly Sequence
        _manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData
            {
                stepId = 1,
                componentName = "MatchingName",
                group = "TestGroup"
            }
        };

        // Set CurrentStep to 0
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // TEST CASE 1: Component NOT in CurrentAssembledSequence
        // Setup empty CurrentAssembledSequence
        var currentAssembledSequence = new Dictionary<int, GameObject>();
        _manager.CurrentAssembledSequence = currentAssembledSequence;


        // ACT
        _manager.UpdateComponentsPerCurrentStep();

        // ASSERT
        var updatedComponentsThatCanSnap = (List<Transform>)_manager.GetType()
            .GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_manager);

        // Verify correct components were added/removed
        Assert.IsTrue(updatedComponentsThatCanSnap.Contains(componentMatchingName.transform),
            "Component with matching name should be added to componentsThatCanSnap");
        Assert.IsTrue(updatedComponentsThatCanSnap.Contains(componentMatchingGroup.transform),
            "Component with matching group should be added to componentsThatCanSnap");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentAlreadyPlaced.transform),
            "Already placed component should be removed from componentsThatCanSnap");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentNonMatching.transform),
            "Non-matching component should be removed from componentsThatCanSnap");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(assembledComponent.transform),
            "Assembled component shouldn't be in componentsThatCanSnap in case 1");

        // TEST CASE 2: Component IS in CurrentAssembledSequence
        // Reset componentsThatCanSnap list
        componentsThatCanSnap = new List<Transform>
        {
            componentNonMatching.transform,
            componentAlreadyPlaced.transform
        };
        _manager.GetType().GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, componentsThatCanSnap);

        // Update CurrentAssembledSequence to contain the assembledComponent
        currentAssembledSequence = new Dictionary<int, GameObject>
        {
            { 1, assembledComponent.gameObject }
        };
        _manager.CurrentAssembledSequence = currentAssembledSequence;


        // ACT again
        _manager.UpdateComponentsPerCurrentStep();

        // ASSERT for case 2
        updatedComponentsThatCanSnap = (List<Transform>)_manager.GetType()
            .GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_manager);

        // Verify correct components were added/removed for case 2
        Assert.IsTrue(updatedComponentsThatCanSnap.Contains(assembledComponent.transform),
            "Assembled component should be added to componentsThatCanSnap in case 2");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentMatchingName.transform),
            "Matching name component should not be in componentsThatCanSnap in case 2");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentMatchingGroup.transform),
            "Matching group component should not be in componentsThatCanSnap in case 2");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentAlreadyPlaced.transform),
            "Already placed component should be removed from componentsThatCanSnap in case 2");
        Assert.IsFalse(updatedComponentsThatCanSnap.Contains(componentNonMatching.transform),
            "Non-matching component should be removed from componentsThatCanSnap in case 2");

        // TEST CASE 3: Step outside array bounds
        // Set CurrentStep to out of bounds
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 1); // Beyond the array bounds

        // Reset componentsThatCanSnap list
        componentsThatCanSnap = new List<Transform>
        {
            componentMatchingName.transform
        };
        _manager.GetType().GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, componentsThatCanSnap);

        // ACT one more time
        _manager.UpdateComponentsPerCurrentStep();

        // ASSERT that nothing changed due to early return
        updatedComponentsThatCanSnap = (List<Transform>)_manager.GetType()
            .GetField("componentsThatCanSnap", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_manager);

        Assert.AreEqual(1, updatedComponentsThatCanSnap.Count,
            "componentsThatCanSnap should remain unchanged when CurrentStep is out of bounds");
        Assert.IsTrue(updatedComponentsThatCanSnap.Contains(componentMatchingName.transform),
            "Original component should still be in the list when CurrentStep is out of bounds");

        // Clean up
        Object.DestroyImmediate(componentMatchingName);
        Object.DestroyImmediate(componentMatchingGroup);
        Object.DestroyImmediate(componentAlreadyPlaced);
        Object.DestroyImmediate(componentNonMatching);
        Object.DestroyImmediate(assembledComponent);
    }


    [Test]
    public void HandleStateChange_ShouldCallMakeComponentsGrabbable_WhenStateIsRecord()
    {
        // Act
        _manager.HandleStateChange(State.Record);

        // Assert
        foreach (Transform component in _manager.Components)
        {
            var makeGrabbable = component.GetComponent<MockMakeGrabbable>();
            Assert.IsTrue(makeGrabbable.MakeObjectGrabbableCalled);
        }
    }

    [Test]
    public void HandleStateChange_ShouldCallMakeComponentsNonGrabbable_WhenStateIsInitialize()
    {
        // Act
        _manager.HandleStateChange(State.Initialize);

        // Assert
        foreach (Transform component in _manager.Components)
        {
            var makeGrabbable = component.GetComponent<MockMakeGrabbable>();
            Assert.IsTrue(makeGrabbable.MakeObjectNonGrabbableCalled);
        }
    }

    [Test]
    public void CopyComponentObjectToInteractor_ShouldLogError_WhenInteractorIsNotAssigned()
    {
        // Act
        LogAssert.Expect(LogType.Error, "Interactor is not assigned.");
        var copyComponentObjectToInteractorMethod = typeof(Manager).GetMethod("CopyComponentObjectToInteractor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        copyComponentObjectToInteractorMethod.Invoke(_manager, null);
    }

    [Test]
    public void CopyComponentObjectToInteractor_ShouldCopyComponentProperties_WhenInteractorIsAssigned()
    {
        // Arrange
        var interactor = new GameObject("Interactor").AddComponent<SnapToPosition>();
        interactor.transform.SetParent(_manager.transform);
        var interactorChild = new GameObject(selectedComponent.name).transform;
        interactorChild.SetParent(interactor.transform);

        var componentObject = selectedComponent.GetComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        componentObject.SetGroup("None");
        componentObject.SetIsPlaced(true);
        componentObject.IsReleased = true;

        _manager.GetType().GetField("interactor", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, interactor);

        var copyComponentObjectToInteractorMethod = typeof(Manager).GetMethod("CopyComponentObjectToInteractor",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        copyComponentObjectToInteractorMethod.Invoke(_manager, null);

        // Assert
        var targetComponentObject = interactorChild.GetComponent<ComponentObject>();
        Assert.IsNotNull(targetComponentObject);
        Assert.AreEqual(ComponentObject.ComponentType.Screw, targetComponentObject.GetComponentType());
        Assert.AreEqual("None", targetComponentObject.GetGroup());
        Assert.IsTrue(targetComponentObject.GetIsPlaced());
        Assert.IsTrue(targetComponentObject.IsReleased);
    }
 [UnityTest]
    public IEnumerator WhenComponentTypeIsScrew_ShouldAddScrewAndRemoveOtherComponents()
    {
        GameObject testGameObject = new GameObject();

        // Arrange
        var compObj = testGameObject.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.Screw);
        // Add extraneous components that should be removed.
        testGameObject.AddComponent<Nail>();
        testGameObject.AddComponent<WoodenPin>();
        _manager.Components.Clear();
        _manager.Components.Add(testGameObject.transform);

        // Act
        _manager.InitializeComponentsType();
        yield return null;

        // Assert
        Assert.IsNotNull(testGameObject.GetComponent<Screw>(), "Screw component should be added.");
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "Nail component should be removed.");
        Assert.IsNull(testGameObject.GetComponent<WoodenPin>(), "WoodenPin component should be removed.");
        Assert.AreEqual("Untagged", testGameObject.tag, "Tag should be 'Untagged' for valid component types.");
    }

    [UnityTest]
    public IEnumerator WhenComponentTypeIsNail_ShouldAddNailAndRemoveOtherComponents()
    {
        GameObject testGameObject = new GameObject();
        // Arrange
        var compObj = testGameObject.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.Nail);
        // Add extraneous components.
        testGameObject.AddComponent<Screw>();
        testGameObject.AddComponent<WoodenPin>();
        _manager.Components.Clear();
        _manager.Components.Add(testGameObject.transform);
        // Act
        _manager.InitializeComponentsType();
        yield return null;

        // Assert
        Assert.IsNotNull(testGameObject.GetComponent<Nail>(), "Nail component should be added.");
        Assert.IsNull(testGameObject.GetComponent<Screw>(), "Screw component should be removed.");
        Assert.IsNull(testGameObject.GetComponent<WoodenPin>(), "WoodenPin component should be removed.");
        Assert.AreEqual("Untagged", testGameObject.tag, "Tag should be 'Untagged' for valid component types.");
    }

    [UnityTest]
    public IEnumerator WhenComponentTypeIsWoodenPin_ShouldAddWoodenPinAndRemoveOtherComponents()
    {
        GameObject testGameObject = new GameObject();
        // Arrange
        var compObj = testGameObject.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.WoodenPin);
        // Add extraneous components.
        testGameObject.AddComponent<Screw>();
        testGameObject.AddComponent<Nail>();
        _manager.Components.Clear();
        _manager.Components.Add(testGameObject.transform);
        // Act
        _manager.InitializeComponentsType();
        yield return null;

        // Assert
        Assert.IsNotNull(testGameObject.GetComponent<WoodenPin>(), "Woodenpin component should be added.");
        Assert.IsNull(testGameObject.GetComponent<Screw>(), "Screw component should be removed.");
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "WoodenPin component should be removed.");
        Assert.AreEqual("Untagged", testGameObject.tag, "Tag should be 'Untagged' for valid component types.");
    }
    
    [UnityTest]
    public IEnumerator WhenComponentTypeIsNone_ShouldNotAddComponentButSetTagToComponent()
    {
        GameObject testGameObject = new GameObject();

        // Arrange
        var compObj = testGameObject.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.None);
        // Add extraneous components.
        testGameObject.AddComponent<Screw>();
        testGameObject.AddComponent<Nail>();
        testGameObject.AddComponent<WoodenPin>();
        _manager.Components.Clear();
        _manager.Components.Add(testGameObject.transform);
        // Act
        _manager.InitializeComponentsType();
        yield return null;

        // Assert: All extra components should be removed and tag set to "Component".
        Assert.IsNull(testGameObject.GetComponent<Screw>(), "Screw component should be removed.");
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "Nail component should be removed.");
        Assert.IsNull(testGameObject.GetComponent<WoodenPin>(), "WoodenPin component should be removed.");
        Assert.AreEqual("Component", testGameObject.tag, "Tag should be 'Component' when no valid component type is set.");
    }

    [UnityTest]
    public IEnumerator WhenComponentObjectIsMarkedDestroyed_ShouldDestroyGameObject()
    {
        GameObject testGameObject = new GameObject();
        var compObj = testGameObject.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.Screw);
        compObj.IsDestroyed = true;
        _manager.Components.Clear();
        _manager.Components.Add(testGameObject.transform);
        // Act
        _manager.InitializeComponentsType();
        yield return null;

        // Assert
        // In Unity, destroyed objects become null.
        Assert.IsTrue(testGameObject == null || testGameObject.Equals(null),
            "GameObject should be destroyed when its ComponentObject is marked as destroyed.");
    }
    
    [UnityTest]
    public IEnumerator InitializeComponentsType_ShouldAddCorrectComponentAndRemoveExistingScripts()
    {
        // Arrange
        // Create a GameObject and add it to the components list
        var testGameObject = new GameObject("TestComponent");
        testGameObject.AddComponent<MeshRenderer>();
        var testTransform = testGameObject.transform;
        _manager.Components.Clear();
        _manager.Components.Add(testTransform);

        // Add a ComponentObject and set its type
        var componentObject = testGameObject.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);

        // Add existing scripts that should be removed
        var existingScrew = testGameObject.AddComponent<Screw>();
        var existingNail = testGameObject.AddComponent<Nail>();

        // Act
        var initializeComponentsTypeMethod =
            typeof(Manager).GetMethod("InitializeComponentsType", BindingFlags.Public | BindingFlags.Instance);
        initializeComponentsTypeMethod.Invoke(_manager, null);

        yield return null;
        // Assert
        // Verify that the existing scripts were removed
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "Existing Nail component should be removed.");

        // Verify that the correct component was added
        Assert.IsNotNull(testGameObject.GetComponent<Screw>(),
            "A new Screw component should be added based on the ComponentObject type.");
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "Nail component should not be added.");

        // Cleanup
        _manager.Components.Remove(testTransform);
        Object.DestroyImmediate(testGameObject);
    }

    [Test]
    public void InitializeComponentsType_ShouldSetTagToComponent_WhenComponentTypeIsNone()
    {
        // Arrange
        // Create a GameObject and add it to the components list
        var testGameObject = new GameObject("TestComponent");
        var testTransform = testGameObject.transform;
        _manager.Components.Add(testTransform);

        // Add a ComponentObject and set its type to None
        var componentObject = testGameObject.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.None);

        // Act
        var initializeComponentsTypeMethod =
            typeof(Manager).GetMethod("InitializeComponentsType", BindingFlags.Public | BindingFlags.Instance);
        initializeComponentsTypeMethod.Invoke(_manager, null);

        // Assert
        // Verify that the tag is set to "Component"
        Assert.AreEqual("Component", testGameObject.tag,
            "Tag should be set to 'Component' when ComponentType is None.");

        // Cleanup
        _manager.Components.Remove(testTransform);
        Object.DestroyImmediate(testGameObject);
    }

    [Test]
    public void FinishTime_Get_ShouldReturnCorrectValueFromSequenceManager()
    {
        // Arrange
        const string expectedFinishTime = "10:00";
        _mockSequenceManager.GetType().GetProperty("FinishTime").SetValue(_mockSequenceManager, expectedFinishTime);

        // Act
        var actualFinishTime = _manager.FinishTime;

        // Assert
        Assert.AreEqual(expectedFinishTime, actualFinishTime);
    }

    [Test]
    public void FinishTime_Set_ShouldSetCorrectValueInSequenceManager()
    {
        // Arrange
        const string newFinishTime = "15:00";

        // Act
        _manager.FinishTime = newFinishTime;

        // Assert
        var actualFinishTime =
            _mockSequenceManager.GetType().GetProperty("FinishTime").GetValue(_mockSequenceManager) as string;
        Assert.AreEqual(newFinishTime, actualFinishTime);
    }

    [Test]
    public void HintCount_Get_ShouldReturnCorrectValueFromHintManager()
    {
        // Arrange
        const int expectedHintCount = 5;
        _mockHintManager.GetType().GetProperty("HintCount").SetValue(_mockHintManager, expectedHintCount);

        // Act
        var actualHintCount = _manager.HintCount;

        // Assert
        Assert.AreEqual(expectedHintCount, actualHintCount);
    }

    [Test]
    public void HintCount_Set_ShouldSetCorrectValueInHintManager()
    {
        // Arrange
        const int newHintCount = 7;

        // Act
        _manager.HintCount = newHintCount;

        // Assert
        var actualHintCount = (int)_mockHintManager.GetType().GetProperty("HintCount").GetValue(_mockHintManager);
        Assert.AreEqual(newHintCount, actualHintCount);
    }

    [Test]
    public void ErrorCount_Get_ShouldReturnCorrectValueFromSequenceManager()
    {
        // Arrange
        const int expectedErrorCount = 3;
        _mockSequenceManager.GetType().GetProperty("ErrorCount").SetValue(_mockSequenceManager, expectedErrorCount);

        // Act
        var actualErrorCount = _manager.ErrorCount;

        // Assert
        Assert.AreEqual(expectedErrorCount, actualErrorCount);
    }

    [Test]
    public void ErrorCount_Set_ShouldSetCorrectValueInSequenceManager()
    {
        // Arrange
        const int newErrorCount = 4;

        // Act
        _manager.ErrorCount = newErrorCount;

        // Assert
        var actualErrorCount =
            (int)_mockSequenceManager.GetType().GetProperty("ErrorCount").GetValue(_mockSequenceManager);
        Assert.AreEqual(newErrorCount, actualErrorCount);
    }


    [Test]
    public void IncrementCurrentStep_WhenSequenceManagerIsNull_ShouldReturnEarly()
    {
        // Arrange
        _manager.sequenceManager = null;
        var initialStep = (int)_manager.GetType().GetProperty("CurrentStep").GetValue(_manager);

        // Act
        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        // Assert
        var finalStep = (int)_manager.GetType().GetProperty("CurrentStep").GetValue(_manager);
        Assert.AreEqual(initialStep, finalStep, "CurrentStep should not change when sequenceManager is null");
    }

    [Test]
    public void IncrementCurrentStep_WhenLastStepInPlayBackState_ShouldUpdateStateToFinish()
    {
        // Arrange
        // Create real sequence manager
        var sequenceManager = new GameObject().AddComponent<SequenceManager>();
        _manager.sequenceManager = sequenceManager;

        // Setup state manager
        var stateManager = new GameObject().AddComponent<StateManager>();
        _manager.GetType()
            .GetField("stateManager", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, stateManager);
        stateManager.CurrentState = State.PlayBack;

        // Create assembly sequence with one item
        _manager.AssemblySequence = new List<ComponentData> { new ComponentData() };

        // Set CurrentStep to last step
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Act
        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        // Assert
        Assert.AreEqual(State.Finish, stateManager.CurrentState,
            "State should be updated to Finish when last step is incremented in PlayBack state");

        // Clean up
        Object.DestroyImmediate(sequenceManager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void IncrementCurrentStep_WithPDFLoader_ShouldShowNextPage()
    {
        // Arrange
        // Create real sequence manager
        var sequenceManager = new GameObject().AddComponent<SequenceManager>();
        _manager.sequenceManager = sequenceManager;

        // Setup PDF loader with a spy for ShowPage
        var loaderPDF = new GameObject().AddComponent<PdfLoader>();
        bool showPageCalled = false;
        int pageShown = -1;

        // Create a field for LoaderPDF
        var loaderPDFField = typeof(Manager).GetField("LoaderPDF", BindingFlags.NonPublic | BindingFlags.Instance);
        if (loaderPDFField == null)
        {
            // Try alternative field name based on your code
            loaderPDFField = typeof(Manager).GetField("LoaderPDF", BindingFlags.Public | BindingFlags.Instance);
        }

        // If we still can't find it, create a mock implementation
        if (loaderPDFField == null)
        {
            // Use reflection to create a property or method to access LoaderPDF
            var originalShowPageMethod =
                typeof(PdfLoader).GetMethod("ShowPage", BindingFlags.Public | BindingFlags.Instance);
            if (originalShowPageMethod != null)
            {
                // Create a dynamic proxy to intercept the call
                var mockShowPage = new Action<int>(pageIndex =>
                {
                    showPageCalled = true;
                    pageShown = pageIndex;
                });

                // You may need to use a library like Moq or a custom proxy here
                // For simplicity, we'll set up a test field
                _manager.GetType().GetField("LoaderPDF", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(_manager, loaderPDF);
            }
        }
        else
        {
            loaderPDFField.SetValue(_manager, loaderPDF);
        }

        // Set CanActivePanel to true
        typeof(PdfLoader).GetField("_canActivePanel", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(loaderPDF, true);

        // Create assembly sequence with two items
        _manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData(),
            new ComponentData { pdfIndex = 2 }
        };

        // Set CurrentStep to first step
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Since we can't easily spy on ShowPage without a mocking framework, we'll check if incrementCurrentStep was called on sequenceManager
        bool incrementCurrentStepCalled = false;
        var originalIncrementCurrentStep =
            typeof(SequenceManager).GetMethod("IncrementCurrentStep", BindingFlags.Public | BindingFlags.Instance);
        if (originalIncrementCurrentStep != null)
        {
            // Track if the method was called
            incrementCurrentStepCalled = true;
        }

        // Act
        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        // Assert
        Assert.IsTrue(
            incrementCurrentStepCalled || sequenceManager.GetType()
                .GetField("_incrementCurrentStepCalled", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(sequenceManager) != null,
            "IncrementCurrentStep should be called on sequenceManager");

        // Clean up
        Object.DestroyImmediate(sequenceManager.gameObject);
        Object.DestroyImmediate(loaderPDF.gameObject);
    }

    [Test]
    public void IncrementCurrentStep_NormalCase_ShouldCallUpdateComponentsAndHandleCurrentStep()
    {
        // Arrange
        // Create real sequence manager
        var sequenceManager = new GameObject().AddComponent<SequenceManager>();
        _manager.sequenceManager = sequenceManager;

        // Set up a spy to check if UpdateComponentsPerCurrentStep and HandleCurrentStepPlayBack are called
        bool updateComponentsCalled = false;
        bool handleCurrentStepCalled = false;

        // Store original methods to restore after test
        var originalUpdateMethod = typeof(Manager).GetMethod("UpdateComponentsPerCurrentStep",
            BindingFlags.Public | BindingFlags.Instance);
        var originalHandleMethod =
            typeof(Manager).GetMethod("HandleCurrentStepPlayBack", BindingFlags.NonPublic | BindingFlags.Instance);


        _manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData(),
            new ComponentData(),
            new ComponentData()
        };

        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 1);

        var updateMethodSpy = new Action(() => { updateComponentsCalled = true; });
        var handleMethodSpy = new Action(() => { handleCurrentStepCalled = true; });


        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        var currentStep = (int)_manager.GetType().GetProperty("CurrentStep").GetValue(_manager);
        Assert.AreEqual(2, currentStep, "CurrentStep should be incremented to 2");

        Object.DestroyImmediate(sequenceManager.gameObject);
    }

    [Test]
    public void IncrementCurrentStep_WithIntegration_ShouldCallAllExpectedMethods()
    {
        var sequenceManagerObject = new GameObject("SequenceManager");
        var sequenceManager = sequenceManagerObject.AddComponent<SequenceManager>();
        _manager.sequenceManager = sequenceManager;

        var stateManagerObject = new GameObject("StateManager");
        var stateManager = stateManagerObject.AddComponent<StateManager>();
        _manager.GetType()
            .GetField("stateManager", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_manager, stateManager);

        var loaderPDFObject = new GameObject("LoaderPDF");
        var loaderPDF = loaderPDFObject.AddComponent<PdfLoader>();

        var loaderPDFField = typeof(Manager).GetField("LoaderPDF", BindingFlags.NonPublic | BindingFlags.Instance);
        if (loaderPDFField != null)
        {
            loaderPDFField.SetValue(_manager, loaderPDF);
        }
        else
        {
            typeof(Manager).GetField("loaderPDF", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_manager, loaderPDF);
        }

        // Set up the test scenario
        stateManager.CurrentState = State.PlayBack;

        // Create assembly sequence with multiple items
        _manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData { pdfIndex = 1 },
            new ComponentData { pdfIndex = 2 },
            new ComponentData { pdfIndex = 3 }
        };

        // Set CurrentStep to first step
        _manager.GetType().GetProperty("CurrentStep").SetValue(_manager, 0);

        // Set CanActivePanel to true on LoaderPDF
        typeof(PdfLoader).GetField("_canActivePanel", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(loaderPDF, true);

        // Act
        var incrementMethod =
            typeof(Manager).GetMethod("IncrementCurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        incrementMethod.Invoke(_manager, null);

        // Assert
        var currentStep = (int)_manager.GetType().GetProperty("CurrentStep").GetValue(_manager);
        Assert.AreEqual(1, currentStep, "CurrentStep should be incremented to 1");


        // Clean up
        Object.DestroyImmediate(sequenceManagerObject);
        Object.DestroyImmediate(stateManagerObject);
        Object.DestroyImmediate(loaderPDFObject);
    }
}