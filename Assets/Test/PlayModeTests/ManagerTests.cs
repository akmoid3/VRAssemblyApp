using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.TestTools;
using Moq;
using System.Collections;

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


    public override void ShowHint(List<ComponentData> assemblySequence, int currentStep, List<Transform> components, SnapToPosition interactor)
    {
        ShowHintCalled = true;
    }

    public override void HighlightComponentToPlace(List<ComponentData> assemblySequence, int currentStep, List<Transform> components)
    {
        HighlightComponentToPlaceCalled = true;
    }
}

public class MockAutomaticPlacementManager : AutomaticPlacementManager
{
    public bool PlaceAllComponentsGraduallyCalled { get; private set; }
    public bool PlaceInitialComponentCalled { get; private set; }


    public override void PlaceAllComponentsGradually(float delay, SnapToPosition interactor, List<ComponentData> assemblySequence, List<Transform> components, ToolManager toolManager)
    {
        PlaceAllComponentsGraduallyCalled = true;
    }

    public override void PlaceInitialComponent(List<ComponentData> assemblySequence, List<Transform> components, SnapToPosition interactor)
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
        Object.DestroyImmediate(audioManager.gameObject);

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

        LogAssert.Expect(LogType.Error, "Interactor is not assigned.");

        _manager.HandleStateChange(State.PlayBack);

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
    public void PlaceInitialComponent_ShouldCallPlaceInitialComponentGraduallyOnAutomaticPlacementManager()
    {

        // Act
        _manager.PlaceInitialComponent();

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

    [Test]
    public void HighlightComponentToPlace_ShouldCallHighlightComponentToPlaceOnHintManager()
    {
        // Act
        var highlightComponentToPlaceMethod = typeof(Manager).GetMethod("HighlightComponentToPlace", BindingFlags.NonPublic | BindingFlags.Instance);
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
        _manager.GetType().GetField("interactor", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_manager, interactor);

        // Act
        var hideCorrectSnapPointMethod = typeof(Manager).GetMethod("HideCorrectSnapPoint", BindingFlags.NonPublic | BindingFlags.Instance);
        hideCorrectSnapPointMethod.Invoke(_manager, null);

        // Assert
        Assert.IsFalse(meshRenderer.enabled);
    }


    [Test]
    public void ProcessComponentPlacement_ShouldCallValidateComponent_WhenComponentIsValid()
    {
        // Arrange
        var componentData = new ComponentData { componentName = selectedComponent.name, toolName = "Tool" };
        var processComponentPlacementMethod = typeof(Manager).GetMethod("ProcessComponentPlacement", BindingFlags.NonPublic | BindingFlags.Instance);

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
            group = ComponentObject.Group.None,
            type = ComponentObject.ComponentType.Screw
        },
        new ComponentData
        {
            componentName = "TestComponent2",
            position = new Vector3(2, 0, 0),
            rotation = Quaternion.identity,
            toolName = "null",
            group = ComponentObject.Group.None,
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
    public void HandleComponentFastening_ShouldCallOnFastenerStopped_WhenFastenerIsStopped()
    {
        // Arrange
        var componentData = new ComponentData { componentName = selectedComponent.name, toolName = "Tool" };
        selectedComponent.AddComponent<Nail>().IsStopped = true;
        selectedComponent.AddComponent<MeshRenderer>();


        var handleComponentFasteningMethod = typeof(Manager).GetMethod("HandleComponentFastening", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        handleComponentFasteningMethod.Invoke(_manager, new object[] { selectedComponent.transform, componentData });

        // Assert
        Assert.IsTrue(_mockSequenceManager.IncrementCurrentStepCalled);
        Assert.IsTrue(_mockSequenceManager.ValidateComponentCalled);
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
        var copyComponentObjectToInteractorMethod = typeof(Manager).GetMethod("CopyComponentObjectToInteractor", BindingFlags.NonPublic | BindingFlags.Instance);
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
        componentObject.SetGroup(ComponentObject.Group.None);
        componentObject.SetIsPlaced(true);
        componentObject.IsReleased = true;

        _manager.GetType().GetField("interactor", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_manager, interactor);

        var copyComponentObjectToInteractorMethod = typeof(Manager).GetMethod("CopyComponentObjectToInteractor", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        copyComponentObjectToInteractorMethod.Invoke(_manager, null);

        // Assert
        var targetComponentObject = interactorChild.GetComponent<ComponentObject>();
        Assert.IsNotNull(targetComponentObject);
        Assert.AreEqual(ComponentObject.ComponentType.Screw, targetComponentObject.GetComponentType());
        Assert.AreEqual(ComponentObject.Group.None, targetComponentObject.GetGroup());
        Assert.IsTrue(targetComponentObject.GetIsPlaced());
        Assert.IsTrue(targetComponentObject.IsReleased);
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
        var initializeComponentsTypeMethod = typeof(Manager).GetMethod("InitializeComponentsType", BindingFlags.Public | BindingFlags.Instance);
        initializeComponentsTypeMethod.Invoke(_manager, null);

        yield return null;
        // Assert
        // Verify that the existing scripts were removed
        Assert.IsNull(testGameObject.GetComponent<Nail>(), "Existing Nail component should be removed.");

        // Verify that the correct component was added
        Assert.IsNotNull(testGameObject.GetComponent<Screw>(), "A new Screw component should be added based on the ComponentObject type.");
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
        var initializeComponentsTypeMethod = typeof(Manager).GetMethod("InitializeComponentsType", BindingFlags.Public | BindingFlags.Instance);
        initializeComponentsTypeMethod.Invoke(_manager, null);

        // Assert
        // Verify that the tag is set to "Component"
        Assert.AreEqual("Component", testGameObject.tag, "Tag should be set to 'Component' when ComponentType is None.");

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
        var actualFinishTime = _mockSequenceManager.GetType().GetProperty("FinishTime").GetValue(_mockSequenceManager) as string;
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
        var actualErrorCount = (int)_mockSequenceManager.GetType().GetProperty("ErrorCount").GetValue(_mockSequenceManager);
        Assert.AreEqual(newErrorCount, actualErrorCount);
    }


}
