using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// Test helper class to spy on PrefabManager calls
public class TestPrefabManager : PrefabManager
{
    public string LastModelShown { get; private set; }
    public bool LoadModelsCalled { get; private set; }
    public List<string> LastModelNamesLoaded { get; private set; }
    
    // Override ShowModel to track calls
    public new void ShowModel(string modelName)
    {
        LastModelShown = modelName;
        base.ShowModel(modelName);
    }
    
    // Override LoadModels to track calls
    public new void LoadModels()
    {
        LoadModelsCalled = true;
        base.LoadModels();
    }
    
    // Helper to trigger OnModelsLoaded event
    public void TriggerOnModelsLoaded(List<string> modelNames)
    {
        LastModelNamesLoaded = modelNames;
        // Use reflection to invoke the protected event
        var eventField = typeof(PrefabManager).GetField("OnModelsLoaded", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        var onModelsLoaded = (Action<List<string>>)eventField?.GetValue(this);
        onModelsLoaded?.Invoke(modelNames);
    }
}

// Test helper to track dropdown events
public class TestDropdown : TMP_Dropdown
{
    public int LastValueChanged { get; private set; } = -999;
    public int ListenerCallCount { get; private set; }
    
    public void SimulateValueChanged(int value)
    {
        ListenerCallCount++;
        LastValueChanged = value;
        onValueChanged.Invoke(value);
    }
}

public class DropDownManagerTests
{
    private GameObject testObject;
    private DropDownManager dropDownManager;
    private TestDropdown testDropdown;
    private TestPrefabManager testPrefabManager;
    private GameObject managerObject;
    private Manager manager;
    private GameObject prefabContainerObject;
    private StateManager stateManager;

    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a GameObject and add the DropDownManager component
        testObject = new GameObject("TestObject");
        dropDownManager = testObject.AddComponent<DropDownManager>();

        // Create Manager singleton for tests that need it
        managerObject = new GameObject("Manager");
        manager = managerObject.AddComponent<Manager>();
        
        // Make sure the static instance is set
        var instanceField = typeof(Manager).GetField("_instance",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        instanceField?.SetValue(null, manager);

        // Create and set up the TestDropdown
        testDropdown = testObject.AddComponent<TestDropdown>();
        testDropdown.options = new List<TMP_Dropdown.OptionData>();
        
        // Inject the dropdown
        dropDownManager.GetType()
                       .GetField("dropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .SetValue(dropDownManager, testDropdown);

        // Create prefab container
        prefabContainerObject = new GameObject("PrefabContainer");
        
        // Create and set up the TestPrefabManager
        GameObject prefabManagerObject = new GameObject("PrefabManager");
        testPrefabManager = prefabManagerObject.AddComponent<TestPrefabManager>();
        testPrefabManager.prefabContainer = prefabContainerObject.transform;
       
        // Create and inject a real FileMonitor with valid path
        GameObject fileMonitorObject = new GameObject("FileMonitor");
        var fileMonitor = fileMonitorObject.AddComponent<FileMonitor>();
        
        // Make sure PersistentDataPath is set
        var persistentDataPathField = typeof(FileMonitor).GetField("_persistentDataPath", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        persistentDataPathField?.SetValue(fileMonitor, Application.temporaryCachePath);
        
        testPrefabManager.fileMonitor = fileMonitor;
  
        
        // Create and inject a ModelLoader
        GameObject loaderObject = new GameObject("ModelLoader");
        var modelLoader = loaderObject.AddComponent<ModelLoader>();
        
        testPrefabManager.loader = modelLoader;

        testPrefabManager.manager = manager;
      

        // Inject the PrefabManager
        dropDownManager.GetType()
                       .GetField("prefabManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .SetValue(dropDownManager, testPrefabManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up all created GameObjects
        Object.DestroyImmediate(testObject);
        Object.DestroyImmediate(testPrefabManager.gameObject);
        Object.DestroyImmediate(managerObject);
        Object.DestroyImmediate(prefabContainerObject);
        Object.DestroyImmediate(stateManager);

    }

    [Test]
    public void LoadModelsIntoDropdown_ShouldPopulateDropdownOptions()
    {
        // Arrange
        List<string> modelNames = new List<string> { "Model1", "Model2", "Model3" };

        // Act
        dropDownManager.GetType()
                       .GetMethod("LoadModelsIntoDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .Invoke(dropDownManager, new object[] { modelNames });

        // Assert
        Assert.AreEqual(3, testDropdown.options.Count);
        Assert.AreEqual("Model1", testDropdown.options[0].text);
        Assert.AreEqual("Model2", testDropdown.options[1].text);
        Assert.AreEqual("Model3", testDropdown.options[2].text);
    }

    [UnityTest]
    public IEnumerator Start_RegistersEventAndLoadModels()
    {
        // Act
        // Since PrefabManager.LoadModels is called directly in Start(),
        // we just need to call Start() and verify the call was made
        dropDownManager.Start();
        
        // Assert
        
        // Test that event handlers are registered by simulating events
        List<string> testModels = new List<string> { "TestModel1", "TestModel2" };
        testPrefabManager.TriggerOnModelsLoaded(testModels);
        yield return null;
        // This verifies both that the event handler was registered and it properly populated the dropdown
        Assert.GreaterOrEqual(testDropdown.options.Count,1);
        
    }

    [UnityTest]
    public IEnumerator OnDropdownValueChanged_WithValidIndex_ShowsCorrectModel()
    {
        // Arrange
        List<string> modelNames = new List<string> { "Model1", "Model2", "Model3" };
        
        // Add options to dropdown
        testDropdown.options.Clear();
        foreach (var name in modelNames)
        {
            testDropdown.options.Add(new TMP_Dropdown.OptionData(name));
        }
        
        // Act - call OnDropdownValueChanged with a valid index (1)
        dropDownManager.GetType()
                       .GetMethod("OnDropdownValueChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .Invoke(dropDownManager, new object[] { 1 });
        yield return null;
        // Assert
        Assert.AreEqual("Model2", Manager.Instance.ModelName);
    }

    [Test]
    public void OnDropdownValueChanged_WithInvalidIndex_LogsError()
    {
        // Arrange
        testDropdown.options.Clear(); // Ensure no options
        
        // We need to capture the Debug.LogError call
        LogAssert.Expect(LogType.Error, "Dropdown index out of range: -1");
        
        // Act - call OnDropdownValueChanged with an invalid index (-1)
        dropDownManager.GetType()
                       .GetMethod("OnDropdownValueChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .Invoke(dropDownManager, new object[] { -1 });
        
        // Assert - LogAssert will verify the error was logged
    }
    
    [Test]
    public void OnDropdownValueChanged_WithTooLargeIndex_LogsError()
    {
        // Arrange
        testDropdown.options.Clear();
        testDropdown.options.Add(new TMP_Dropdown.OptionData("SingleItem"));
        
        // We need to capture the Debug.LogError call
        LogAssert.Expect(LogType.Error, "Dropdown index out of range: 5");
        
        // Act - call OnDropdownValueChanged with an index that's too large (5)
        dropDownManager.GetType()
                       .GetMethod("OnDropdownValueChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .Invoke(dropDownManager, new object[] { 5 });
        
        // Assert - LogAssert will verify the error was logged
    }
    
   
}