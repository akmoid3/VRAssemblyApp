using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

public class InitializedDataManagerTests
{
    private GameObject testObject;
    private InitializedDataManager initializedDataManager;
    private string testDirectoryPath;
    private string testFilePath;
    private Manager manager;
    private StateManager stateManager;

    [SetUp]
    public void Setup()
    {
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();

        // Create test GameObject and add InitializedDataManager component
        testObject = new GameObject();
        initializedDataManager = testObject.AddComponent<InitializedDataManager>();

        // Use reflection to set a custom directory for testing
        var directoryField = typeof(InitializedDataManager).GetField("directory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string customDirectory = "CustomInitializedModels";
        directoryField.SetValue(initializedDataManager, customDirectory);

        testDirectoryPath = Path.Combine(Application.persistentDataPath, customDirectory);

        var directoryPathField = typeof(InitializedDataManager).GetField("directoryPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        directoryPathField.SetValue(initializedDataManager, testDirectoryPath);

        // Make sure any existing test directory is deleted
        if (Directory.Exists(testDirectoryPath))
        {
            var files = Directory.GetFiles(testDirectoryPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(testDirectoryPath);
        }

        // Set up test components
        var child1 = new GameObject("Component1").transform;
        var child2 = new GameObject("Component2").transform;
        child1.gameObject.AddComponent<ComponentObject>();

        manager.Components = new List<Transform> { child1, child2 };
        manager.RemovedComponents = new List<Transform>();
        manager.Model = new GameObject("TestModel");
        manager.ModelName = "TestModel";
    }

    [TearDown]
    public void Teardown()
    {
        // Remove all files in test directory
        if (Directory.Exists(testDirectoryPath))
        {
            var files = Directory.GetFiles(testDirectoryPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            // Delete the directory after removing files
            Directory.Delete(testDirectoryPath);
        }

        // Destroy test objects
        Object.DestroyImmediate(testObject);
        Object.DestroyImmediate(stateManager.gameObject);
        Object.DestroyImmediate(manager.gameObject);
        
        // Destroy any remaining component objects
        var allGameObjects = Object.FindObjectsOfType<GameObject>();
        foreach (var go in allGameObjects)
        {
            if (go.name.StartsWith("Component") || go.name == "TestModel")
                Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void Start_DirectoryDoesntExist_CreatesDirectory()
    {
        // Make sure directory doesn't exist
        if (Directory.Exists(testDirectoryPath))
        {
            Directory.Delete(testDirectoryPath);
        }
        
        // Execute Start method via reflection
        initializedDataManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(initializedDataManager, null);

        // Verify directory was created
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "Directory should have been created");
    }
    
    [Test]
    public void Start_DirectoryAlreadyExists_DoesNotThrow()
    {
        // Make sure directory exists
        if (!Directory.Exists(testDirectoryPath))
        {
            Directory.CreateDirectory(testDirectoryPath);
        }
        
        // Execute Start method via reflection - should not throw
        Assert.DoesNotThrow(() => {
            initializedDataManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(initializedDataManager, null);
        });
        
        // Verify directory still exists
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "Directory should still exist");
    }

    [UnityTest]
    public IEnumerator SaveComponentsData_WithInactiveComponent_ResetsState()
    {
        // Create a test component that's inactive
        GameObject inactiveObj = new GameObject("InactiveComponent");
        ComponentObject compObj = inactiveObj.AddComponent<ComponentObject>();
        compObj.SetComponentType(ComponentObject.ComponentType.None);
        inactiveObj.SetActive(false);
        
        // Add to components list
        manager.Components.Add(inactiveObj.transform);
        
        // Execute SaveComponentsData
        initializedDataManager.SaveComponentsData();
        yield return null;
        // Verify component was temporarily activated to save data
        Assert.IsTrue(inactiveObj.activeSelf, "Component should have been reactivated during save");
        
        // Verify the file was created
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        Assert.IsTrue(File.Exists(filePath), "JSON file should have been created");
        
        // Clean up
        Object.DestroyImmediate(inactiveObj);
    }

    [UnityTest]
    public IEnumerator SaveComponentsData_WithFastenerComponent_SavesAxisData()
    {
        // Create a screw component
        GameObject screwObj = new GameObject("ScrewComponent");
        ComponentObject screwComp = screwObj.AddComponent<ComponentObject>();
        screwComp.SetComponentType(ComponentObject.ComponentType.Screw);
        
        // Set selected axis and direction
        Vector3 testAxis = new Vector3(0, 1, 0);
        screwComp.SetSelectedAxis(testAxis);
        
        
        // Add to components list
        manager.Components.Add(screwObj.transform);
        
        // Execute SaveComponentsData
        initializedDataManager.SaveComponentsData();
        yield return null;
        // Verify the file was created
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        Assert.IsTrue(File.Exists(filePath), "JSON file should have been created");
        
        // Read the file and check axis data was saved
        string jsonContent = File.ReadAllText(filePath);
        JsonData data = JsonUtility.FromJson<JsonData>(jsonContent);
        
        // Find our screw component in the saved data
        ComponentTypeData savedScrewData = null;
        foreach (var comp in data.components)
        {
            if (comp.componentName == "ScrewComponent")
            {
                savedScrewData = comp;
                break;
            }
        }
        
        Assert.IsNotNull(savedScrewData, "Screw component should be in saved data");
        Assert.AreEqual(ComponentObject.ComponentType.Screw, savedScrewData.componentType);
        Assert.AreEqual(testAxis, savedScrewData.selectedAxis);
        Assert.AreEqual(0.0, savedScrewData.selectedDirection);
        
        // Clean up
        Object.DestroyImmediate(screwObj);
    }

    [UnityTest]
    public IEnumerator SaveComponentsData_WithRemovedFastenerComponent_SavesAxisData()
    {
        // Create a removed fastener component (nail)
        GameObject removedNailObj = new GameObject("RemovedNail");
        ComponentObject removedNail = removedNailObj.AddComponent<ComponentObject>();
        removedNail.SetComponentType(ComponentObject.ComponentType.Nail);
        removedNail.IsDestroyed = true;
        
        // Set selected axis and direction
        Vector3 testAxis = new Vector3(1, 0, 0);
        removedNail.SetSelectedAxis(testAxis);
    
        
        // Add to removed components list
        manager.RemovedComponents = new List<Transform> { removedNailObj.transform };
        
        // Execute SaveComponentsData
        initializedDataManager.SaveComponentsData();
        yield return null;
        // Verify the file was created
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        Assert.IsTrue(File.Exists(filePath), "JSON file should have been created");
        
        // Read the file and check axis data was saved
        string jsonContent = File.ReadAllText(filePath);
        JsonData data = JsonUtility.FromJson<JsonData>(jsonContent);
        
        // Find our nail component in the saved data
        ComponentTypeData savedNailData = null;
        foreach (var comp in data.components)
        {
            if (comp.componentName == "RemovedNail")
            {
                savedNailData = comp;
                break;
            }
        }
        
        Assert.IsNotNull(savedNailData, "Nail component should be in saved data");
        Assert.AreEqual(ComponentObject.ComponentType.Nail, savedNailData.componentType);
        Assert.AreEqual(testAxis, savedNailData.selectedAxis);
        Assert.AreEqual(0.0, savedNailData.selectedDirection);
        Assert.IsTrue(savedNailData.destroyed);
        
        // Clean up
        Object.DestroyImmediate(removedNailObj);
    }

    [UnityTest]
    public IEnumerator LoadComponentsData_WithDestroyedComponent_MovesToRemovedList()
    {
        // Create test component that is marked as destroyed
        GameObject testObj = new GameObject("DestroyedComponent");
        manager.Components.Add(testObj.transform);
        
        // Create JSON file with this component marked as destroyed
        JsonData data = new JsonData();
        data.components.Add(new ComponentTypeData
        {
            componentName = "DestroyedComponent",
            componentType = ComponentObject.ComponentType.None,
            componentGroup = "TestGroup",
            selectedAxis = Vector3.zero,
            selectedDirection = 0f,
            destroyed = true
        });
        
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        File.WriteAllText(filePath, JsonUtility.ToJson(data));
        
        // Make sure RemovedComponents list is clear
        manager.RemovedComponents.Clear();
        
        // Execute LoadComponentsData
        initializedDataManager.LoadComponentsData();
        yield return null;
        // Verify component is now inactive
        Assert.IsFalse(testObj.activeSelf, "Destroyed component should be inactive");
        
        // Verify component was removed from Components list
        Assert.IsFalse(manager.Components.Contains(testObj.transform), 
            "Destroyed component should be removed from Components list");
            
        // Verify component was added to RemovedComponents list
        Assert.IsTrue(manager.RemovedComponents.Contains(testObj.transform),
            "Destroyed component should be added to RemovedComponents list");
    }

    [UnityTest]
    public IEnumerator LoadComponentsData_WithFastener_SetsAxisData()
    {
        // Create test fastener component (WoodenPin)
        GameObject pinObj = new GameObject("WoodenPin");
        manager.Components.Add(pinObj.transform);
        
        // Create JSON file with fastener axis data and direction
        Vector3 testAxis = new Vector3(0, 0, 1);
        float testDirection = 3.0f;
        JsonData data = new JsonData();
        data.components.Add(new ComponentTypeData
        {
            componentName = "WoodenPin",
            componentType = ComponentObject.ComponentType.WoodenPin,
            componentGroup = "Fasteners",
            selectedAxis = testAxis,
            selectedDirection = testDirection,
            destroyed = false
        });
        
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        File.WriteAllText(filePath, JsonUtility.ToJson(data));
        
        // Execute LoadComponentsData
        initializedDataManager.LoadComponentsData();
        yield return null;
        // Verify component has the correct data
        ComponentObject component = pinObj.GetComponent<ComponentObject>();
        Assert.IsNotNull(component, "ComponentObject should be added");
        Assert.AreEqual(ComponentObject.ComponentType.WoodenPin, component.GetComponentType());
        Assert.AreEqual("Fasteners", component.GetGroup());
        Assert.AreEqual(testAxis, component.GetSelectedAxis());
        
        // Check if we can verify the direction using reflection
        var directionField = typeof(ComponentObject).GetField("selectedDirection", BindingFlags.Instance | BindingFlags.NonPublic);
        if (directionField != null)
        {
            float actualDirection = (float)directionField.GetValue(component);
            Assert.AreEqual(testDirection, actualDirection, "Selected direction wasn't set correctly");
        }
    }

    [UnityTest]
    public IEnumerator LoadComponentsData_ValidJsonFileFormat_LoadsCorrectly()
    {
        // Create a sample JSON file with the exact format shown in the example
        string jsonContent = @"{
    ""components"": [
        {
            ""componentName"": ""TestComponent"",
            ""componentType"": 0,
            ""componentGroup"": ""None"",
            ""selectedAxis"": {
                ""x"": 0.0,
                ""y"": 0.0,
                ""z"": 0.0
            },
            ""selectedDirection"": 0.0,
            ""destroyed"": false
        }
    ]
}";

        manager.Model = new GameObject("jsonFormatTest");
        string filePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        Directory.CreateDirectory(testDirectoryPath);
        File.WriteAllText(filePath, jsonContent);
        
        // Create GameObject to match the one in JSON
        GameObject testComponentObj = new GameObject("TestComponent");
        manager.Components = new List<Transform> { testComponentObj.transform };
        
        // Execute LoadComponentsData
        initializedDataManager.LoadComponentsData();
        
        yield return null;
        
        // Verify component data loaded correctly
        ComponentObject component = testComponentObj.GetComponent<ComponentObject>();
        Assert.IsNotNull(component, "ComponentObject should be added to GameObject");
        Assert.AreEqual(ComponentObject.ComponentType.None, component.GetComponentType());
        Assert.AreEqual("None", component.GetGroup());
        Assert.AreEqual(Vector3.zero, component.GetSelectedAxis());
        Assert.IsFalse(component.IsDestroyed);
        
        // Clean up
        Object.DestroyImmediate(testComponentObj);
    }

    

    [Test]
    public void SaveComponentsData_NoComponents_DoesNotCreateFile()
    {
        manager.Model = new GameObject("Prova");

        // Set components to null
        manager.Components = null;

        // Execute SaveComponentsData method
        initializedDataManager.SaveComponentsData();

        // Verify that the JSON file wasn't created
        testFilePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        Assert.IsFalse(File.Exists(testFilePath), "JSON file shouldn't have been created");
    }
    

    [Test]
    public void LoadComponentsData_NoFile_DoesNotThrowException()
    {
        // Verify no JSON file exists
        manager.Model = new GameObject("DoesNotThrowException");

        testFilePath = Path.Combine(testDirectoryPath, manager.ModelName + ".json");
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        // Execute LoadComponentsData and verify no exceptions
        Assert.DoesNotThrow(() => initializedDataManager.LoadComponentsData(), "LoadComponentsData shouldn't throw exceptions if file doesn't exist");
    }

    [Test]
    public void FindChildByName_ReturnsCorrectChild()
    {
        // Setup: Add component with known name
        Transform targetChild = manager.Components[0];

        // Execute FindChildByName method
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, targetChild.name }) as Transform;

        // Verify found child is correct
        Assert.AreEqual(targetChild, foundChild, "FindChildByName didn't find the correct child");
    }

    [Test]
    public void FindChildByName_ReturnsNullIfNotFound()
    {
        // Setup: Non-existent component name
        string nonExistentName = "NonExistentComponent";

        // Execute FindChildByName method
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, nonExistentName }) as Transform;

        // Verify result is null
        Assert.IsNull(foundChild, "FindChildByName should return null for non-existent component");
    }

    [Test]
    public void FindChildByName_EmptyName_ReturnsNull()
    {
        // Execute FindChildByName with empty name
        Transform foundChild = initializedDataManager.GetType().GetMethod("FindChildByName", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(initializedDataManager, new object[] { manager.Components, "" }) as Transform;

        // Verify result is null
        Assert.IsNull(foundChild, "FindChildByName should return null for empty name");
    }

    [UnityTest]
    public IEnumerator LoadComponentsData_AddsComponentObjectIfNull()
    {
        // Preparation: Create directory and JSON file with test data
        manager.Model = new GameObject("test");
        string fileName = manager.ModelName + ".json";

        // Existing JSON to write - Updated to match the expected format
        string jsonContent = @"{
    ""components"": [
        {
            ""componentName"": ""TestComponent"",
            ""componentType"": 0,
            ""componentGroup"": ""None"",
            ""selectedAxis"": {
                ""x"": 0.0,
                ""y"": 0.0,
                ""z"": 0.0
            },
            ""selectedDirection"": 0.0,
            ""destroyed"": false
        }
    ]
}";

        var filePath = Path.Combine(testDirectoryPath, fileName);
        // Write JSON directly to file
        File.WriteAllText(filePath, jsonContent);

        // Set up GameObject without ComponentObject
        var testChild = new GameObject("TestComponent").transform;
        manager.Components = new List<Transform> { testChild };

        // Execute LoadComponentsData
        initializedDataManager.LoadComponentsData();

        yield return null;
        // Verify ComponentObject was added and data was correctly set
        var componentObject = testChild.GetComponent<ComponentObject>();
        Assert.IsNotNull(componentObject, "ComponentObject should have been added to GameObject");
        Assert.AreEqual(ComponentObject.ComponentType.None, componentObject.GetComponentType(), "Component type wasn't set correctly");
        Assert.AreEqual("None", componentObject.GetGroup(), "Component group wasn't set correctly");
    }
}