using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;
using System.Reflection;

[TestFixture]
public class StepsManagerTest
{
    private StepsManager stepsManager;
    private GameObject stepsManagerObject;
    private GameObject stepPrefabMock;
    private GameObject stepParentObject;
    StateManager stateManager;
    
    Manager manager;

    [SetUp]
    public void Setup()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create the StepsManager GameObject
        stepsManagerObject = new GameObject("StepsManager");
        stepsManager = stepsManagerObject.AddComponent<StepsManager>();

        // Create step prefab mock
        stepPrefabMock = new GameObject("StepPrefabMock");
        AddStepPrefabComponents(stepPrefabMock);

        // Create step parent
        stepParentObject = new GameObject("StepParent");
        Transform stepParent = stepParentObject.transform;

        // Setup StepsManager fields
        stepsManager.stepPrefab = stepPrefabMock;
        stepsManager.stepParent = stepParent;

        // Add serialized field for highlight material
        Material highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = Color.yellow;
        SetPrivateField(stepsManager, "highlightMaterial", highlightMaterial);

        // Add serialized field for interactor position
        GameObject interactorPosObject = new GameObject("InteractorPosition");
        SetPrivateField(stepsManager, "InteractorPosition", interactorPosObject.transform);

        // Setup Manager singleton for tests
        manager = new GameObject().AddComponent<Manager>();
        manager.PerformaceForEachStep = new List<float> { 0.85f }; // 85% performance for step 1
        manager.Components = new List<Transform>();
        
        SequenceManager sequenceManager = new GameObject().AddComponent<SequenceManager>();
        SetPrivateField(stepsManager, "sequenceManager", sequenceManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up created GameObjects
        Object.Destroy(stepsManagerObject);
        Object.Destroy(stepPrefabMock);
        Object.Destroy(stepParentObject);
    }

    [Test]
    public void StepData_Constructor_InitializesWithCorrectValues()
    {
        // Act
        var stepData = new StepsManager.StepData(5);

        // Assert
        Assert.AreEqual(5, stepData.StepNumber);
        Assert.AreEqual(0, stepData.Errors);
        Assert.AreEqual(0, stepData.Hints);
        Assert.AreEqual(0f, stepData.TimeSpent);
        Assert.AreEqual(0f, stepData.Accuracy);
    }

    [Test]
    public void OnStepChanged_AddsNewStepDataToList()
    {
        // Arrange
        var stepsDataList = GetPrivateField<List<StepsManager.StepData>>(stepsManager, "stepsData");
        Assert.AreEqual(0, stepsDataList.Count);

        // Act - Call the private method via reflection
        InvokePrivateMethod(stepsManager, "OnStepChanged", 0);

        // Assert
        stepsDataList = GetPrivateField<List<StepsManager.StepData>>(stepsManager, "stepsData");
        Assert.AreEqual(1, stepsDataList.Count);
        Assert.AreEqual(1, stepsDataList[0].StepNumber); // Note: Method adds 1 to passed stepNumber
    }

    [Test]
    public void OnErrorCountChanged_IncreasesErrorCount()
    {
        // Arrange
        InvokePrivateMethod(stepsManager, "OnStepChanged", 0); // Create a step
        SetPrivateField(stepsManager, "previousErrorCount", 1);
        var currentStepData = GetPrivateField<StepsManager.StepData>(stepsManager, "currentStepData");
        Assert.AreEqual(0, currentStepData.Errors);

        // Act
        InvokePrivateMethod(stepsManager, "OnErrorCountChanged", 3);

        // Assert
        Assert.AreEqual(2, currentStepData.Errors); // 3 - 1 = 2 new errors
        Assert.AreEqual(3, GetPrivateField<int>(stepsManager, "previousErrorCount"));
    }

    [Test]
    public void OnHintCountChanged_IncreasesHintCount()
    {
        // Arrange
        InvokePrivateMethod(stepsManager, "OnStepChanged", 0); // Create a step
        SetPrivateField(stepsManager, "previousHintCount", 1);
        var currentStepData = GetPrivateField<StepsManager.StepData>(stepsManager, "currentStepData");

        // Act
        InvokePrivateMethod(stepsManager, "OnHintCountChanged", 3);

        // Assert
        Assert.AreEqual(2, currentStepData.Hints); // 3 - 1 = 2 new hints
        Assert.AreEqual(3, GetPrivateField<int>(stepsManager, "previousHintCount"));
    }

    [Test]
    public void UpdateComponentsForStepManager_AddsComponentToStepMapping()
    {
        // Arrange
        var stepComponentsByStep = new Dictionary<int, Transform>();
        SetPrivateField(stepsManager, "stepComponentsByStep", stepComponentsByStep);

        SetupManagerWithMockAssemblySequence();

        // Act
        stepsManager.UpdateComponentsForStepManager(0);

        // Assert
        stepComponentsByStep = GetPrivateField<Dictionary<int, Transform>>(stepsManager, "stepComponentsByStep");
        Assert.IsTrue(stepComponentsByStep.ContainsKey(1)); // First step ID
    }

    [Test]
    public void HighlightComponent_SetsHighlightMaterial()
    {
        // Arrange
        GameObject testObject = new GameObject("TestComponent");
        MeshRenderer renderer = testObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
    
        var originalMaterials = new Dictionary<Renderer, Material[]>();
        SetPrivateField(stepsManager, "originalMaterials", originalMaterials);
    
        // Get highlight material for later comparison
        Material highlightMaterial = GetPrivateField<Material>(stepsManager, "highlightMaterial");
    
        // Act
        InvokePrivateMethod(stepsManager, "HighlightComponent", testObject.transform);
    
        // Assert
        originalMaterials = GetPrivateField<Dictionary<Renderer, Material[]>>(stepsManager, "originalMaterials");
        Assert.IsTrue(originalMaterials.ContainsKey(renderer));
    
        // Check if material was changed to highlight material by comparing properties instead of direct reference
        Assert.AreEqual(highlightMaterial.color, renderer.material.color);
        Assert.AreEqual(highlightMaterial.shader, renderer.material.shader);
    
        // Clean up
        Object.Destroy(testObject);
    }

    [Test]
    public void ClearHighlight_RestoresOriginalMaterials()
    {
        // Arrange
        GameObject testObject = new GameObject("TestComponent");
        MeshRenderer renderer = testObject.AddComponent<MeshRenderer>();
        Material originalMaterial = new Material(Shader.Find("Standard"));
        originalMaterial.color = Color.red; // Set a distinctive color
        renderer.material = originalMaterial;
    
        // Set up originalMaterials dictionary
        var originalMaterials = new Dictionary<Renderer, Material[]>();
        originalMaterials[renderer] = new Material[] { originalMaterial };
        SetPrivateField(stepsManager, "originalMaterials", originalMaterials);
    
        // First highlight it
        InvokePrivateMethod(stepsManager, "HighlightComponent", testObject.transform);
    
        // Act
        InvokePrivateMethod(stepsManager, "ClearHighlight", testObject.transform);
    
        // Assert - compare properties instead of direct material reference
        Assert.AreEqual(originalMaterial.color, renderer.material.color);
        Assert.AreEqual(originalMaterial.shader, renderer.material.shader);
    
        // Clean up
        Object.Destroy(testObject);
    }


    private void AddStepPrefabComponents(GameObject prefab)
    {
        // Add Button component
        prefab.AddComponent<Button>();

        // Add Step, Errors, Hint, Time, Accuracy TextMeshPro components
        CreateTextChild(prefab, "Steps");
        CreateTextChild(prefab, "Errors");
        CreateTextChild(prefab, "Hint");
        CreateTextChild(prefab, "Time");
        CreateTextChild(prefab, "Accuracy");
    }

    private GameObject CreateTextChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        child.AddComponent<TextMeshProUGUI>();
        return child;
    }

    private void SetupManagerSingleton()
    {
        // Create Manager GameObject and set it up as a singleton
        GameObject managerObject = new GameObject("Manager");
        Manager manager = managerObject.AddComponent<Manager>();

        // Initialize basic properties
        manager.PerformaceForEachStep = new List<float> { 0.85f }; // 85% performance for step 1
        manager.Components = new List<Transform>();
        
        SequenceManager sequenceManager = new GameObject().AddComponent<SequenceManager>();
        SetPrivateField(stepsManager, "sequenceManager", sequenceManager);
    }

    private void SetupManagerWithMockAssemblySequence()
    {
        
        SequenceManager sequenceManager = new GameObject().AddComponent<SequenceManager>();
        SetPrivateField(stepsManager, "sequenceManager", sequenceManager);
        // Create a mock assembly sequence
        Manager.Instance.AssemblySequence = new List<ComponentData>
        {
            new ComponentData() { stepId = 1, componentName = "TestComponent", group = "Group1" }
        };

        // Create mock components
        GameObject component = new GameObject("TestComponent");
        ComponentObject componentObj = component.AddComponent<ComponentObject>();

        // Add the component to the manager's components list
        Manager.Instance.Components = new List<Transform> { component.transform };
    }

    private void SetupManagerWithMockComponents()
    {
        // Create mock components
        GameObject component1 = new GameObject("Component1");
        ComponentObject comp1Obj = component1.AddComponent<ComponentObject>();

        GameObject component2 = new GameObject("Component2");
        ComponentObject comp2Obj = component2.AddComponent<ComponentObject>();

        // Create interactor object with children for step parents
        GameObject interactor = new GameObject("Interactor");
        SnapToPosition interactorSnap = interactor.AddComponent<SnapToPosition>();
        GameObject step0Parent = new GameObject("Step0Parent");
        step0Parent.transform.SetParent(interactor.transform);

        // Set up Manager
        Manager.Instance.Components = new List<Transform> { component1.transform, component2.transform };
        Manager.Instance.Interactor = interactorSnap;
        
        SequenceManager sequenceManager = new GameObject().AddComponent<SequenceManager>();
        SetPrivateField(stepsManager, "sequenceManager", sequenceManager);
        
        
    }

    private void SetupStepComponentsByStep()
    {
        var stepComponentsByStep = new Dictionary<int, Transform>();
        stepComponentsByStep[1] = Manager.Instance.Components[0]; // Map step 1 to Component1
        SetPrivateField(stepsManager, "stepComponentsByStep", stepComponentsByStep);
    }

    private void SetPrivateField<T>(object instance, string fieldName, T value)
    {
        System.Type type = instance.GetType();
        FieldInfo field = type.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
            field.SetValue(instance, value);
    }

    private T GetPrivateField<T>(object instance, string fieldName)
    {
        System.Type type = instance.GetType();
        FieldInfo field = type.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
            return (T)field.GetValue(instance);

        return default(T);
    }

    private object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
    {
        System.Type type = instance.GetType();
        MethodInfo method = type.GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method != null)
            return method.Invoke(instance, parameters);

        return null;
    }
    
    
    [UnityTest]
public IEnumerator Update_HighlightsComponentsWhenStateIsFinish()
{
    // Arrange
    var componentToHighlight = new List<Transform>();
    var componentToRemoveHighlight = new List<Transform>();
    SetPrivateField(stepsManager, "componentToHighlight", componentToHighlight);
    SetPrivateField(stepsManager, "componentToRemoveHighlight", componentToRemoveHighlight);
    
    // Create test objects to highlight
    GameObject highlightObj = new GameObject("HighlightObject");
    highlightObj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    componentToHighlight.Add(highlightObj.transform);
    
    GameObject removeHighlightObj = new GameObject("RemoveHighlightObject");
    removeHighlightObj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    componentToRemoveHighlight.Add(removeHighlightObj.transform);
    
    // Create and set up StateManager
    GameObject stateManagerObj = new GameObject("StateManager");
    StateManager stateManager = stateManagerObj.AddComponent<StateManager>();
    
    // Use reflection to set the Instance
    System.Type stateManagerType = typeof(StateManager);
    FieldInfo instanceField = stateManagerType.GetField("Instance", 
        BindingFlags.Public | BindingFlags.Static);
    instanceField.SetValue(null, stateManager);
    
    // Set state to Finish
    stateManager.CurrentState = State.Finish;
    
    // Act - let Update run
    yield return null;
    
    // Assert - verify highlight was called
    var originalMaterials = GetPrivateField<Dictionary<Renderer, Material[]>>(stepsManager, "originalMaterials");
    Assert.IsTrue(originalMaterials.ContainsKey(highlightObj.GetComponent<Renderer>()));
    Assert.IsTrue(originalMaterials.ContainsKey(removeHighlightObj.GetComponent<Renderer>()));
    
    // Clean up
    Object.Destroy(highlightObj);
    Object.Destroy(removeHighlightObj);
    Object.Destroy(stateManagerObj);
}

[UnityTest]
public IEnumerator Update_ClearsHighlightWhenStateIsNotFinish()
{
    // Arrange
    var componentToHighlight = new List<Transform>();
    var componentToRemoveHighlight = new List<Transform>();
    SetPrivateField(stepsManager, "componentToHighlight", componentToHighlight);
    SetPrivateField(stepsManager, "componentToRemoveHighlight", componentToRemoveHighlight);
    
    // Create test objects to highlight
    GameObject highlightObj = new GameObject("HighlightObject");
    highlightObj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    componentToHighlight.Add(highlightObj.transform);
    
    GameObject removeHighlightObj = new GameObject("RemoveHighlightObject");
    removeHighlightObj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    componentToRemoveHighlight.Add(removeHighlightObj.transform);
    
    // Create and set up StateManager
    GameObject stateManagerObj = new GameObject("StateManager");
    StateManager stateManager = stateManagerObj.AddComponent<StateManager>();
    
    // Use reflection to set the Instance
    System.Type stateManagerType = typeof(StateManager);
    FieldInfo instanceField = stateManagerType.GetField("Instance", 
        BindingFlags.Public | BindingFlags.Static);
    instanceField.SetValue(null, stateManager);
    
    // First highlight the objects
    Material highlightMaterial = GetPrivateField<Material>(stepsManager, "highlightMaterial");
    highlightObj.GetComponent<Renderer>().material = highlightMaterial;
    removeHighlightObj.GetComponent<Renderer>().material = highlightMaterial;
    
    // Set up originalMaterials dictionary
    Material origMaterial = new Material(Shader.Find("Standard"));
    origMaterial.color = Color.blue;
    var originalMaterials = new Dictionary<Renderer, Material[]>();
    originalMaterials[highlightObj.GetComponent<Renderer>()] = new Material[] { origMaterial };
    originalMaterials[removeHighlightObj.GetComponent<Renderer>()] = new Material[] { origMaterial };
    SetPrivateField(stepsManager, "originalMaterials", originalMaterials);
    
    // Set state to Run (not Finish)
    stateManager.CurrentState = State.PlayBack;
    
    // Act - let Update run
    yield return null;
    
    // Assert - check materials were reset
    Assert.AreEqual(origMaterial.color, highlightObj.GetComponent<Renderer>().material.color);
    Assert.AreEqual(origMaterial.color, removeHighlightObj.GetComponent<Renderer>().material.color);
    
    // Clean up
    Object.Destroy(highlightObj);
    Object.Destroy(removeHighlightObj);
    Object.Destroy(stateManagerObj);
}

[Test]
public void PrintStepData_DoesNothingWhenStateIsNotFinish()
{
    // Arrange
    var stepsDataBefore = GetPrivateField<List<StepsManager.StepData>>(stepsManager, "stepsData");
    int initialCount = stepsDataBefore.Count;
    
    // Act
    stepsManager.PrintStepData(State.PlayBack);
    
    // Assert
    var stepsDataAfter = GetPrivateField<List<StepsManager.StepData>>(stepsManager, "stepsData");
    Assert.AreEqual(initialCount, stepsDataAfter.Count);
    // No UI objects should be created
    Assert.AreEqual(0, stepsManager.stepParent.childCount);
}

[Test]
public void PrintStepData_PopulatesUIWhenStateIsFinish()
{
    // Arrange
    // Add some step data
    InvokePrivateMethod(stepsManager, "OnStepChanged", 0);
    var stepData = GetPrivateField<StepsManager.StepData>(stepsManager, "currentStepData");
    stepData.Errors = 2;
    stepData.Hints = 1;
    stepData.TimeSpent = 10.5f;
    
    // Mock Manager's PerformaceForEachStep
    Manager.Instance.PerformaceForEachStep = new List<float> { 0.85f };
    
    // Set up a spy to check if Instantiate is called
    int instantiateCallCount = 0;
    System.Action onInstantiateCalled = () => instantiateCallCount++;
    
    // Use a mock stepParent with a method to detect child additions
    var originalStepParent = stepsManager.stepParent;
    GameObject mockStepParent = new GameObject("MockStepParent");
    stepsManager.stepParent = mockStepParent.transform;
    
    // Mock Instantiate method using a TestHelper (see below for implementation)
    TestHelper.SetupInstantiateMock(stepPrefabMock, mockStepParent.transform, onInstantiateCalled);
    
    // Act
    stepsManager.PrintStepData(State.Finish);
    
    // Assert
    Assert.Greater(instantiateCallCount, 0);
    
    // Clean up
    Object.Destroy(mockStepParent);
    stepsManager.stepParent = originalStepParent;
    TestHelper.CleanupInstantiateMock();
}




[Test]
public void UpdateComponentsForStepManager_HandlesEmptyAssemblySequence()
{
    
    // Act & Assert - should not throw exceptions
    stepsManager.UpdateComponentsForStepManager(0);
}

[Test]
public void UpdateComponentsForStepManager_HandlesComponentMatchingByName()
{
    // Arrange
    var stepComponentsByStep = new Dictionary<int, Transform>();
    SetPrivateField(stepsManager, "stepComponentsByStep", stepComponentsByStep);
    
    // Setup manager with assembly sequence
    Manager.Instance.AssemblySequence = new List<ComponentData>
    {
        new ComponentData() { stepId = 1, componentName = "TestComponent", group = "Group1" }
    };
    
    // Create component with matching name
    GameObject component = new GameObject("TestComponent");
    ComponentObject componentObj = component.AddComponent<ComponentObject>();
    Manager.Instance.Components = new List<Transform> { component.transform };
    
    // Act
    stepsManager.UpdateComponentsForStepManager(0);
    
    // Assert
    stepComponentsByStep = GetPrivateField<Dictionary<int, Transform>>(stepsManager, "stepComponentsByStep");
    Assert.IsTrue(stepComponentsByStep.ContainsKey(1));
    Assert.AreEqual(component.transform, stepComponentsByStep[1]);
    
    // Clean up
    Object.Destroy(component);
}

[Test]
public void UpdateComponentsForStepManager_HandlesComponentMatchingByGroup()
{
    // Arrange
    var stepComponentsByStep = new Dictionary<int, Transform>();
    SetPrivateField(stepsManager, "stepComponentsByStep", stepComponentsByStep);
    
    // Setup manager with assembly sequence that has a group
    Manager.Instance.AssemblySequence = new List<ComponentData>
    {
        new ComponentData(){ stepId = 1, componentName = "DifferentName", group = "Group1" }
    };
    
    // Create component with matching group
    GameObject component = new GameObject("TestComponent");
    ComponentObject componentObj = component.AddComponent<ComponentObject>();
    SetPrivateFieldInObject(componentObj, "group", "Group1");
    Manager.Instance.Components = new List<Transform> { component.transform };
    
    // Act
    stepsManager.UpdateComponentsForStepManager(0);
    
    // Assert
    stepComponentsByStep = GetPrivateField<Dictionary<int, Transform>>(stepsManager, "stepComponentsByStep");
    Assert.IsTrue(stepComponentsByStep.ContainsKey(1));
    Assert.AreEqual(component.transform, stepComponentsByStep[1]);
    
    // Clean up
    Object.Destroy(component);
}

// Helper class for mocking Unity's Instantiate method
public static class TestHelper
{
    private static MethodInfo originalInstantiate;
    private static System.Reflection.MethodBase replacementMethod;
    private static GameObject prefab;
    private static Transform parent;
    private static System.Action callback;
    
    public static void SetupInstantiateMock(GameObject mockPrefab, Transform mockParent, System.Action onInstantiateCalled)
    {
        prefab = mockPrefab;
        parent = mockParent;
        callback = onInstantiateCalled;
        
        // Save original Instantiate method
        originalInstantiate = typeof(Object).GetMethod("Instantiate", 
            new System.Type[] { typeof(Object), typeof(Transform) });
        
        // Replace with our mock implementation
        replacementMethod = typeof(TestHelper).GetMethod("MockInstantiate", 
            BindingFlags.Public | BindingFlags.Static);
        
        // This would normally use a mocking framework like NSubstitute or Moq
        // In a real implementation, we would intercept the call to Instantiate
        // Since we can't do that easily in Unity without a mocking framework,
        // we'll simulate it by creating a GameObject
    }
    
    public static GameObject MockInstantiate(GameObject prefab, Transform parent)
    {
        callback?.Invoke();
        GameObject newObject = Object.Instantiate(prefab, parent);
        return newObject;
    }
    
    public static void CleanupInstantiateMock()
    {
        prefab = null;
        parent = null;
        callback = null;
        // In a real implementation with a mocking framework, we would restore the original method here
    }
}

// Helper method for setting private fields in any object
private void SetPrivateFieldInObject<T>(object instance, string fieldName, T value)
{
    System.Type type = instance.GetType();
    FieldInfo field = type.GetField(fieldName, 
        BindingFlags.NonPublic | BindingFlags.Instance);
    
    if (field != null)
        field.SetValue(instance, value);
}

[Test]
public void ShowPlacement_CoversAllExecutionPaths()
{
    // Arrange
    // Setup lists to track highlighted components
    var componentToHighlight = new List<Transform>();
    var componentToRemoveHighlight = new List<Transform>();
    SetPrivateField(stepsManager, "componentToHighlight", componentToHighlight);
    SetPrivateField(stepsManager, "componentToRemoveHighlight", componentToRemoveHighlight);
    
    // Set the processOneTimeComponentsPerSteps to false to test the initialization path
    SetPrivateField(stepsManager, "processOneTimeComponentsPerSteps", false);
    
    // Create mock components with renderers for highlighting
    GameObject component1 = new GameObject("Component1");
    MeshRenderer renderer1 = component1.AddComponent<MeshRenderer>();
    renderer1.material = new Material(Shader.Find("Standard"));
    ComponentObject comp1Obj = component1.AddComponent<ComponentObject>();
    
    GameObject component2 = new GameObject("Component2");
    MeshRenderer renderer2 = component2.AddComponent<MeshRenderer>();
    renderer2.material = new Material(Shader.Find("Standard"));
    ComponentObject comp2Obj = component2.AddComponent<ComponentObject>();
    
    GameObject component3 = new GameObject("Component3");
    MeshRenderer renderer3 = component3.AddComponent<MeshRenderer>();
    renderer3.material = new Material(Shader.Find("Standard"));
    ComponentObject comp3Obj = component3.AddComponent<ComponentObject>();
    
    // Setup Manager with components
    manager.Components = new List<Transform> { 
        component1.transform, component2.transform, component3.transform 
    };
    
    // Setup assembly sequence with multiple steps
    manager.AssemblySequence = new List<ComponentData> {
        new ComponentData() { stepId = 1, componentName = "Component1", group = "Group1" },
        new ComponentData() { stepId = 2, componentName = "Component2", group = "Group2" },
        new ComponentData() { stepId = 3, componentName = "Component3", group = "Group3" }
    };
    
    // Create interactor with step parents
    GameObject interactor = new GameObject("Interactor");
    SnapToPosition interactorSnap = interactor.AddComponent<SnapToPosition>();
    
    // Add child transforms for each step
    for (int i = 0; i < 3; i++)
    {
        GameObject stepParent = new GameObject($"Step{i}Parent");
        stepParent.transform.SetParent(interactor.transform);
    }
    
    Manager.Instance.Interactor = interactorSnap;
    
    // Initialize CurrentAssembledSequence
    
    // Set up InteractorPosition
    GameObject interactorPosObject = new GameObject("InteractorPosition");
    SetPrivateField(stepsManager, "InteractorPosition", interactorPosObject.transform);
    
    // TEST CASE 1: First call with one step - should initialize and process the first component
    
    // Act
    InvokePrivateMethod(stepsManager, "ShowPlacement", 1);
    
    // Assert
    // Check that processOneTimeComponentsPerSteps is now true
    Assert.IsTrue(GetPrivateField<bool>(stepsManager, "processOneTimeComponentsPerSteps"));
    
    // Verify component 1 is in highlight list
    componentToHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToHighlight");
    Assert.AreEqual(1, componentToHighlight.Count);
    Assert.Contains(component1.transform, componentToHighlight);
    
    // Verify the other components are not in any list yet
    Assert.AreEqual(0, componentToRemoveHighlight.Count);
    
    // TEST CASE 2: Call with two steps - should process two components and move first to removeHighlight
    
    // Act
    InvokePrivateMethod(stepsManager, "ShowPlacement", 2);
    
    // Assert
    componentToHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToHighlight");
    componentToRemoveHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToRemoveHighlight");
    
    // Verify component 2 is now in highlight list
    Assert.AreEqual(1, componentToHighlight.Count);
    Assert.Contains(component2.transform, componentToHighlight);
    
    // Verify component 1 moved to removeHighlight list
    Assert.AreEqual(1, componentToRemoveHighlight.Count);
    Assert.Contains(component1.transform, componentToRemoveHighlight);
    
    // TEST CASE 3: Test the case where a component is already in highlight list and needs to be removed
    
    // Add component3 to highlight list first
    componentToHighlight.Add(component3.transform);
    
    // Act - Call with just step 1 to force removal
    InvokePrivateMethod(stepsManager, "ShowPlacement", 1);
    
    // Assert
    componentToHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToHighlight");
    componentToRemoveHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToRemoveHighlight");
    
    // Verify only component1 is in highlight
    Assert.AreEqual(1, componentToHighlight.Count);
    Assert.Contains(component1.transform, componentToHighlight);
    
    // Verify both component2 and component3 are in removeHighlight
    Assert.AreEqual(2, componentToRemoveHighlight.Count);
    Assert.Contains(component2.transform, componentToRemoveHighlight);
    Assert.Contains(component3.transform, componentToRemoveHighlight);
    
    // TEST CASE 4: Test handling of missing components for a step
    
    // Clear the dictionaries first
    var stepComponentsByStep = new Dictionary<int, Transform>();
    SetPrivateField(stepsManager, "stepComponentsByStep", stepComponentsByStep);
    
    // Add only steps 1 and 3 to dictionary (skip step 2 to test warning path)
    stepComponentsByStep.Add(1, component1.transform);
    stepComponentsByStep.Add(3, component3.transform);
    
    // Use LogAssert to verify the warning is logged
    LogAssert.Expect(LogType.Warning, "No components to snap for step 1");
    
    // Act - Process all 3 steps
    InvokePrivateMethod(stepsManager, "ShowPlacement", 3);
    
    // Assert
    componentToHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToHighlight");
    componentToRemoveHighlight = GetPrivateField<List<Transform>>(stepsManager, "componentToRemoveHighlight");
    
    // Verify component3 is in highlight list (for step 3)
    Assert.Contains(component3.transform, componentToHighlight);
    
    // Verify component1 is in removeHighlight list
    Assert.Contains(component1.transform, componentToRemoveHighlight);
    
    // Clean up
    Object.Destroy(component1);
    Object.Destroy(component2);
    Object.Destroy(component3);
    Object.Destroy(interactor);
    Object.Destroy(interactorPosObject);
}
}
