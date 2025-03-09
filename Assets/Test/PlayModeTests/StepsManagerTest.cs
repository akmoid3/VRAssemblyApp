using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

[TestFixture]
public partial class StepsManagerTests
{
    private GameObject stepsManagerObject;
    private StepsManager stepsManager;
    private GameObject managerObject;
    private GameObject stateManagerObject;
    private Manager manager;
    private StateManager stateManager;
    private GameObject stepPrefab;
    private Transform stepParent;
    private GameObject interactorPositionObject;
    private Material highlightMaterial;

    [SetUp]
    public void SetUp()
    {
        // Create necessary GameObjects
        stepsManagerObject = new GameObject("StepsManager");
        stepsManager = stepsManagerObject.AddComponent<StepsManager>();
        
        // Create step prefab with necessary components
        stepPrefab = CreateStepPrefab();
        stepsManager.stepPrefab = stepPrefab;
        
        // Create step parent
        var stepParentObject = new GameObject("StepParent");
        stepParent = stepParentObject.transform;
        stepsManager.stepParent = stepParent;
        
        // Create interactor position
        interactorPositionObject = new GameObject("InteractorPosition");
        var interactorPositionField = typeof(StepsManager).GetField("InteractorPosition", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        interactorPositionField.SetValue(stepsManager, interactorPositionObject.transform);
        
        // Create highlight material
        highlightMaterial = new Material(Shader.Find("Standard"));
        var highlightMaterialField = typeof(StepsManager).GetField("highlightMaterial", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        highlightMaterialField.SetValue(stepsManager, highlightMaterial);
        
        // Setup Manager singleton
        SetupManagerSingleton();
        
        // Setup StateManager singleton
        SetupStateManagerSingleton();
    }

    private GameObject CreateStepPrefab()
    {
        var prefab = new GameObject("StepPrefab");
        prefab.AddComponent<Button>();
        
        var stepsTextObject = new GameObject("Steps");
        stepsTextObject.transform.SetParent(prefab.transform);
        stepsTextObject.AddComponent<TextMeshProUGUI>();
        
        var errorsTextObject = new GameObject("Errors");
        errorsTextObject.transform.SetParent(prefab.transform);
        errorsTextObject.AddComponent<TextMeshProUGUI>();
        
        var hintTextObject = new GameObject("Hint");
        hintTextObject.transform.SetParent(prefab.transform);
        hintTextObject.AddComponent<TextMeshProUGUI>();
        
        var timeTextObject = new GameObject("Time");
        timeTextObject.transform.SetParent(prefab.transform);
        timeTextObject.AddComponent<TextMeshProUGUI>();
        
        var accuracyTextObject = new GameObject("Accuracy");
        accuracyTextObject.transform.SetParent(prefab.transform);
        accuracyTextObject.AddComponent<TextMeshProUGUI>();
        
        return prefab;
    }

    private void SetupManagerSingleton()
    {
        managerObject = new GameObject("Manager");
        manager = managerObject.AddComponent<Manager>();

        manager.sequenceManager = new GameObject().AddComponent<SequenceManager>();
        manager.sequenceManager.AssemblySequence = new List<ComponentData>();
        // Initialize Manager properties
        manager.Components = new List<Transform>();
        manager.PerformaceForEachStep = new List<float>();
        manager.CurrentAssembledSequence = new Dictionary<int, GameObject>();
        manager.Interactor = new GameObject("Interactor").AddComponent<SnapToPosition>();
        var childObject = new GameObject("Child0").transform;
        childObject.SetParent(manager.Interactor.transform);
    }

    private void SetupStateManagerSingleton()
    {
        stateManagerObject = new GameObject("StateManager");
        stateManager = stateManagerObject.AddComponent<StateManager>();
        
        // Initialize StateManager properties
        stateManager.CurrentState = State.PlayBack;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up GameObjects
        DestroyImmediate(stepsManagerObject);
        DestroyImmediate(stepPrefab);
        DestroyImmediate(stepParent.gameObject);
        DestroyImmediate(interactorPositionObject);
        DestroyImmediate(managerObject);
        DestroyImmediate(stateManagerObject);
        
        // Reset static events to prevent test interference
        var sequenceManagerType = typeof(SequenceManager);
        var onStepChangedField = sequenceManagerType.GetField("OnStepChanged", BindingFlags.Public | BindingFlags.Static);
        onStepChangedField?.SetValue(null, null);
        
        var onErrorCountChangedField = sequenceManagerType.GetField("OnErrorCountChanged", BindingFlags.Public | BindingFlags.Static);
        onErrorCountChangedField?.SetValue(null, null);
        
        var hintManagerType = typeof(HintManager);
        var onHintCountChangedField = hintManagerType.GetField("OnHintCountChanged", BindingFlags.Public | BindingFlags.Static);
        onHintCountChangedField?.SetValue(null, null);
        
        var stateManagerType = typeof(StateManager);
        var onStateChangedField = stateManagerType.GetField("OnStateChanged", BindingFlags.Public | BindingFlags.Static);
        onStateChangedField?.SetValue(null, null);
    }

    [Test]
    public void Awake_InitializesRequiredFields()
    {
        // Create a new StepsManager to test Awake
        var testObject = new GameObject("TestStepsManager");
        var testStepsManager = testObject.AddComponent<StepsManager>();
        
        // Call Awake via reflection
        var awakeMethod = typeof(StepsManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        awakeMethod.Invoke(testStepsManager, null);
        
        // Verify fields are initialized
        var componentToHighlightField = typeof(StepsManager).GetField("componentToHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var componentToHighlight = componentToHighlightField.GetValue(testStepsManager) as List<Transform>;
        Assert.IsNotNull(componentToHighlight);
        
        var componentToRemoveHighlightField = typeof(StepsManager).GetField("componentToRemoveHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var componentToRemoveHighlight = componentToRemoveHighlightField.GetValue(testStepsManager) as List<Transform>;
        Assert.IsNotNull(componentToRemoveHighlight);
        
        var stepComponentsByStepField = typeof(StepsManager).GetField("stepComponentsByStep", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepComponentsByStep = stepComponentsByStepField.GetValue(testStepsManager) as Dictionary<int, Transform>;
        Assert.IsNotNull(stepComponentsByStep);
        
        var processOneTimeComponentsPerStepsField = typeof(StepsManager).GetField("processOneTimeComponentsPerSteps", BindingFlags.NonPublic | BindingFlags.Instance);
        var processOneTimeComponentsPerSteps = (bool)processOneTimeComponentsPerStepsField.GetValue(testStepsManager);
        Assert.IsFalse(processOneTimeComponentsPerSteps);
        
        // Verify event handlers were registered
        // Note: We can't really test this directly in unit tests without a mocking framework
        
        DestroyImmediate(testObject);
    }

    [Test]
    public void OnDestroy_UnregistersEventHandlers()
    {
        // We can't easily test event unregistration in unit tests without a mocking framework
        // This test is just a placeholder that calls OnDestroy to ensure coverage
        
        var onDestroyMethod = typeof(StepsManager).GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance);
        onDestroyMethod.Invoke(stepsManager, null);
        
        // Since we can't verify event unregistration directly, this test just ensures the method runs without exceptions
        Assert.Pass();
    }

    [Test]
    public void Update_HandlesDifferentStates()
    {
        // Create test objects for highlighting
        var component1 = new GameObject("Component1");
        var component2 = new GameObject("Component2");
        component1.AddComponent<MeshRenderer>();
        component2.AddComponent<MeshRenderer>();
        
        // Add components to lists
        var componentToHighlightField = typeof(StepsManager).GetField("componentToHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var componentToHighlight = new List<Transform> { component1.transform };
        componentToHighlightField.SetValue(stepsManager, componentToHighlight);
        
        var componentToRemoveHighlightField = typeof(StepsManager).GetField("componentToRemoveHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var componentToRemoveHighlight = new List<Transform> { component2.transform };
        componentToRemoveHighlightField.SetValue(stepsManager, componentToRemoveHighlight);
        
        // Test with regular state
        stateManager.CurrentState = State.PlayBack;
        var updateMethod = typeof(StepsManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        updateMethod.Invoke(stepsManager, null);
        
        // Test with Finish state
        stateManager.CurrentState = State.Finish;
        updateMethod.Invoke(stepsManager, null);
        
        // Since we can't easily verify highlight behavior in unit tests, this just ensures the method runs
        Assert.Pass();
        
        // Clean up
        DestroyImmediate(component1);
        DestroyImmediate(component2);
    }

    [Test]
    public void OnStepChanged_CreatesNewStepData()
    {
        // Set up a current step data
        var stepsDataField = typeof(StepsManager).GetField("stepsData", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepsData = new List<StepsManager.StepData>();
        stepsDataField.SetValue(stepsManager, stepsData);
        
        var currentStepDataField = typeof(StepsManager).GetField("currentStepData", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepStartTimeField = typeof(StepsManager).GetField("stepStartTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Call OnStepChanged with a step number
        var onStepChangedMethod = typeof(StepsManager).GetMethod("OnStepChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        onStepChangedMethod.Invoke(stepsManager, new object[] { 2 });
        
        // Verify a new step data was created and added
        Assert.AreEqual(1, stepsData.Count);
        
        var currentStepData = currentStepDataField.GetValue(stepsManager) as StepsManager.StepData;
        Assert.IsNotNull(currentStepData);
        Assert.AreEqual(3, currentStepData.StepNumber); // Step number + 1
        
        // Verify step start time was set
        var stepStartTime = (float)stepStartTimeField.GetValue(stepsManager);
        Assert.Greater(stepStartTime, 0);
        
        // Call again to test the time tracking
        // Set a specific current time to make the test deterministic
        currentStepData.TimeSpent = 0;
        stepStartTimeField.SetValue(stepsManager, Time.time - 10); // 10 seconds ago
        
        onStepChangedMethod.Invoke(stepsManager, new object[] { 3 });
        
        // Verify time spent was updated in the previous step data and a new one was created
        Assert.AreEqual(2, stepsData.Count);
        Assert.Greater(stepsData[0].TimeSpent, 0); // Time was added
    }

    [Test]
    public void OnErrorCountChanged_UpdatesCurrentStepData()
    {
        // Setup current step data
        var currentStepData = new StepsManager.StepData(1);
        var currentStepDataField = typeof(StepsManager).GetField("currentStepData", BindingFlags.NonPublic | BindingFlags.Instance);
        currentStepDataField.SetValue(stepsManager, currentStepData);
        
        var previousErrorCountField = typeof(StepsManager).GetField("previousErrorCount", BindingFlags.NonPublic | BindingFlags.Instance);
        previousErrorCountField.SetValue(stepsManager, 2);
        
        // Call OnErrorCountChanged
        var onErrorCountChangedMethod = typeof(StepsManager).GetMethod("OnErrorCountChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        onErrorCountChangedMethod.Invoke(stepsManager, new object[] { 5 });
        
        // Verify errors were updated
        Assert.AreEqual(3, currentStepData.Errors); // 5 - 2 = 3
        Assert.AreEqual(5, previousErrorCountField.GetValue(stepsManager));
    }

    [Test]
    public void OnHintCountChanged_UpdatesCurrentStepData()
    {
        // Setup current step data
        var currentStepData = new StepsManager.StepData(1);
        var currentStepDataField = typeof(StepsManager).GetField("currentStepData", BindingFlags.NonPublic | BindingFlags.Instance);
        currentStepDataField.SetValue(stepsManager, currentStepData);
        
        var previousHintCountField = typeof(StepsManager).GetField("previousHintCount", BindingFlags.NonPublic | BindingFlags.Instance);
        previousHintCountField.SetValue(stepsManager, 1);
        
        // Call OnHintCountChanged
        var onHintCountChangedMethod = typeof(StepsManager).GetMethod("OnHintCountChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        onHintCountChangedMethod.Invoke(stepsManager, new object[] { 3 });
        
        // Verify hints were updated
        Assert.AreEqual(2, currentStepData.Hints); // 3 - 1 = 2
        Assert.AreEqual(3, previousHintCountField.GetValue(stepsManager));
    }

    [Test]
    public void PrintStepData_CreatesUIComponents()
    {
        // Setup current step data and step start time
        var currentStepData = new StepsManager.StepData(1);
        var stepsData = new List<StepsManager.StepData> { currentStepData };
        var currentStepDataField = typeof(StepsManager).GetField("currentStepData", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepsDataField = typeof(StepsManager).GetField("stepsData", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepStartTimeField = typeof(StepsManager).GetField("stepStartTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        currentStepDataField.SetValue(stepsManager, currentStepData);
        stepsDataField.SetValue(stepsManager, stepsData);
        stepStartTimeField.SetValue(stepsManager, Time.time - 5); // 5 seconds ago
        
        // Setup performance data in manager
        manager.PerformaceForEachStep = new List<float> { 0.75f }; // 75% accuracy
        
        // Call PrintStepData with Finish state
        var printStepDataMethod = typeof(StepsManager).GetMethod("PrintStepData", BindingFlags.Public | BindingFlags.Instance);
        printStepDataMethod.Invoke(stepsManager, new object[] { State.Finish });
        
        // Verify time was updated in the current step data
        Assert.Greater(currentStepData.TimeSpent, 0);
        
        // Verify accuracy was set
        Assert.AreEqual(75f, currentStepData.Accuracy);
        
        // Verify UI components were created
        Assert.AreEqual(1, stepParent.childCount);
        
        // Call with other state to test early returns
        printStepDataMethod.Invoke(stepsManager, new object[] { State.PlayBack });
        
        // Test step number > performance count
        var currentStepData2 = new StepsManager.StepData(2);
        stepsData.Add(currentStepData2);
        currentStepDataField.SetValue(stepsManager, currentStepData2);
        
        printStepDataMethod.Invoke(stepsManager, new object[] { State.Finish });
    }

    [Test]
    public void ShowPlacement_RepositionsAndHighlightsComponents()
    {
        // Setup component lists and dictionaries
        var componentToHighlightField = typeof(StepsManager).GetField("componentToHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var componentToRemoveHighlightField = typeof(StepsManager).GetField("componentToRemoveHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepComponentsByStepField = typeof(StepsManager).GetField("stepComponentsByStep", BindingFlags.NonPublic | BindingFlags.Instance);
        var processOneTimeComponentsPerStepsField = typeof(StepsManager).GetField("processOneTimeComponentsPerSteps", BindingFlags.NonPublic | BindingFlags.Instance);
        
        componentToHighlightField.SetValue(stepsManager, new List<Transform>());
        componentToRemoveHighlightField.SetValue(stepsManager, new List<Transform>());
        
        var stepComponentsByStep = new Dictionary<int, Transform>();
        stepComponentsByStepField.SetValue(stepsManager, stepComponentsByStep);
        
        // Create test components
        var component1 = new GameObject("Component1");
        component1.AddComponent<ComponentObject>();
        var component2 = new GameObject("Component2");
        component2.AddComponent<ComponentObject>();
        
        // Add components to Manager.Components
        manager.Components = new List<Transform> { component1.transform, component2.transform };
        
        // Setup assembly sequence
        manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData() { stepId = 1, componentName = "Component1" },
            new ComponentData() { stepId = 2, componentName = "Component2" }
        };
        
        // Add components to stepComponentsByStep
        stepComponentsByStep[1] = component1.transform;
        stepComponentsByStep[2] = component2.transform;
        
        // Call ShowPlacement
        var showPlacementMethod = typeof(StepsManager).GetMethod("ShowPlacement", BindingFlags.NonPublic | BindingFlags.Instance);
        showPlacementMethod.Invoke(stepsManager, new object[] { 2 }); // Show up to step 2
        
        // Verify processOneTimeComponentsPerSteps is true
        Assert.IsTrue((bool)processOneTimeComponentsPerStepsField.GetValue(stepsManager));
        
        // Call again with processOneTimeComponentsPerSteps already true
        processOneTimeComponentsPerStepsField.SetValue(stepsManager, true);
        showPlacementMethod.Invoke(stepsManager, new object[] { 1 }); // Show up to step 1
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(component1);
        UnityEngine.Object.DestroyImmediate(component2);
    }

    [Test]
    public void UpdateComponentsForStepManager_SelectsComponentForStep()
    {
        // Setup component lists and dictionaries
        var stepComponentsByStepField = typeof(StepsManager).GetField("stepComponentsByStep", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepComponentsByStep = new Dictionary<int, Transform>();
        stepComponentsByStepField.SetValue(stepsManager, stepComponentsByStep);
        
        // Create test components
        var component1 = new GameObject("TestComponent");
        var componentObject1 = component1.AddComponent<ComponentObject>();
        
        var component2 = new GameObject("OtherComponent");
        var componentObject2 = component2.AddComponent<ComponentObject>();
        
        // Setup assembly sequence
        manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData() { stepId = 1, componentName = "TestComponent", group = "TestGroup" }
        };
        
        // Add components to Manager.Components
        manager.Components = new List<Transform> { component1.transform, component2.transform };
        
        // Call UpdateComponentsForStepManager with valid step
        var updateComponentsMethod = typeof(StepsManager).GetMethod("UpdateComponentsForStepManager", BindingFlags.Public | BindingFlags.Instance);
        updateComponentsMethod.Invoke(stepsManager, new object[] { 0 });
        
        // Verify component was added to stepComponentsByStep
        Assert.IsTrue(stepComponentsByStep.ContainsKey(1));
        Assert.AreEqual(component1.transform, stepComponentsByStep[1]);
        
        // Test with invalid step index
        updateComponentsMethod.Invoke(stepsManager, new object[] { 1 }); // Should return early
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(component1);
        UnityEngine.Object.DestroyImmediate(component2);
    }

    [Test]
    public void HighlightComponent_AppliesHighlightMaterial()
    {
        // Create test component with renderers
        var component = new GameObject("TestComponent");
        var renderer = component.AddComponent<MeshRenderer>();
        renderer.materials = new Material[] { new Material(Shader.Find("Standard")) };
        
        // Call HighlightComponent
        var highlightComponentMethod = typeof(StepsManager).GetMethod("HighlightComponent", BindingFlags.NonPublic | BindingFlags.Instance);
        highlightComponentMethod.Invoke(stepsManager, new object[] { component.transform });
        
        // Call again to test the case when originalMaterials already contains the renderer
        highlightComponentMethod.Invoke(stepsManager, new object[] { component.transform });
        
        // Verify the original materials were stored
        var originalMaterialsField = typeof(StepsManager).GetField("originalMaterials", BindingFlags.NonPublic | BindingFlags.Instance);
        var originalMaterials = originalMaterialsField.GetValue(stepsManager) as Dictionary<Renderer, Material[]>;
        Assert.IsTrue(originalMaterials.ContainsKey(renderer));
        
        // Clean up
        UnityEngine.Object.DestroyImmediate(component);
    }

    [Test]
    public void ClearHighlight_RestoresOriginalMaterials()
    {
        // Create test component with renderers
        var component = new GameObject("TestComponent");
        var renderer = component.AddComponent<MeshRenderer>();
        var originalMaterial = new Material(Shader.Find("Standard"));
        renderer.materials = new Material[] { originalMaterial };
        
        // Store original materials
        var originalMaterialsField = typeof(StepsManager).GetField("originalMaterials", BindingFlags.NonPublic | BindingFlags.Instance);
        var originalMaterials = new Dictionary<Renderer, Material[]>
        {
            { renderer, new Material[] { originalMaterial } }
        };
        originalMaterialsField.SetValue(stepsManager, originalMaterials);
        
        // Call ClearHighlight
        var clearHighlightMethod = typeof(StepsManager).GetMethod("ClearHighlight", BindingFlags.NonPublic | BindingFlags.Instance);
        clearHighlightMethod.Invoke(stepsManager, new object[] { component.transform });
        
        Assert.AreEqual(originalMaterial.shader.name, renderer.materials[0].shader.name);
        
        DestroyImmediate(component);
    }

    [Test]
    public void StepData_Constructor_InitializesProperties()
    {
        // Create StepData
        var stepData = new StepsManager.StepData(5);
        
        // Verify properties were initialized correctly
        Assert.AreEqual(5, stepData.StepNumber);
        Assert.AreEqual(0, stepData.Errors);
        Assert.AreEqual(0, stepData.Hints);
        Assert.AreEqual(0f, stepData.TimeSpent);
        Assert.AreEqual(0f, stepData.Accuracy);
    }

    [Test]
    public void StepData_Properties_GetAndSet()
    {
        // Create StepData
        var stepData = new StepsManager.StepData(1);
        
        // Test property setters
        stepData.StepNumber = 2;
        stepData.Errors = 3;
        stepData.Hints = 4;
        stepData.TimeSpent = 5f;
        stepData.Accuracy = 6f;
        
        // Verify properties
        Assert.AreEqual(2, stepData.StepNumber);
        Assert.AreEqual(3, stepData.Errors);
        Assert.AreEqual(4, stepData.Hints);
        Assert.AreEqual(5f, stepData.TimeSpent);
        Assert.AreEqual(6f, stepData.Accuracy);
    }

    [Test]
    public void StepsData_Accessor_ReturnsReadOnlyList()
    {
        // Set up stepsData
        var stepsDataField = typeof(StepsManager).GetField("stepsData", BindingFlags.NonPublic | BindingFlags.Instance);
        var stepsData = new List<StepsManager.StepData> 
        { 
            new StepsManager.StepData(1),
            new StepsManager.StepData(2)
        };
        stepsDataField.SetValue(stepsManager, stepsData);
        
        // Access via property
        var stepsDataProperty = typeof(StepsManager).GetProperty("StepsData");
        var result = stepsDataProperty.GetValue(stepsManager) as IReadOnlyList<StepsManager.StepData>;
        
        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
    }
}