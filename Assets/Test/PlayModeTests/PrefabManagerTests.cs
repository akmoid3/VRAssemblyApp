using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;
using System.IO;
using System.Collections;

public class PrefabManagerTests
{
    private GameObject prefabManagerGameObject;
    private PrefabManager prefabManager;
    private Mock<IModelLoader> mockLoader;
    private Mock<IFileMonitor> mockFileMonitor;
    private Manager manager;


    [SetUp]
    public void SetUp()
    {
        prefabManagerGameObject = new GameObject();
        prefabManager = prefabManagerGameObject.AddComponent<PrefabManager>();

        mockLoader = new Mock<IModelLoader>();
        mockFileMonitor = new Mock<IFileMonitor>();
        manager = new GameObject().AddComponent<Manager>();

        typeof(PrefabManager).GetField("loader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, mockLoader.Object);
        typeof(PrefabManager).GetField("fileMonitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, mockFileMonitor.Object);

        typeof(PrefabManager).GetField("prefabContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, prefabManagerGameObject.transform);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(prefabManagerGameObject);
        Object.DestroyImmediate(manager.gameObject);
    }

    [UnityTest]
    public IEnumerator LoadModels_InvokesOnModelsLoadedEvent()
    {
        List<string> modelNames = new List<string>();
        prefabManager.OnModelsLoaded += names => modelNames.AddRange(names);

        HashSet<string> fakeFiles = new HashSet<string> { "model1.glb", "model2.glb" };
        mockFileMonitor.Setup(fm => fm.PersistentDataPath).Returns("fake/path");
        Directory.CreateDirectory("fake/path");
        File.Create("fake/path/model1.glb").Dispose();
        File.Create("fake/path/model2.glb").Dispose();

        prefabManager.LoadModels();

        yield return null;

        Assert.AreEqual(2, modelNames.Count);
        Assert.Contains("model1", modelNames);
        Assert.Contains("model2", modelNames);

        Directory.Delete("fake/path", true);
    }

    [UnityTest]
    public IEnumerator ShowModel_DisplaysTheCorrectModel()
    {
        string modelName = "testModel";
        GameObject fakeModel = new GameObject(modelName);

        // Mock the IModelLoader to return the fake model
        mockLoader.Setup(loader => loader.LoadFromFile(It.IsAny<string>())).ReturnsAsync(fakeModel);

        // Set up the mock for IFileMonitor
        mockFileMonitor.Setup(fm => fm.PersistentDataPath).Returns("fake/path");

        // Ensure the directory exists and create a fake model file for testing
        Directory.CreateDirectory("fake/path");
        File.Create("fake/path/testModel.glb").Dispose();

        // Call the method to test
        prefabManager.ShowModel(modelName);

        yield return null;

        // Verify that the correct model is set
        Assert.AreEqual(fakeModel.name, manager.Model.name);

        // Clean up
        Directory.Delete("fake/path", true);
    }

    [UnityTest]
    public IEnumerator ShowModel_DisplaysFailedToInstantiateModel()
    {
        // Arrange
        string modelName = "nonExistentModel";
        GameObject nullModel = null;

        // Mock the IModelLoader to return null, simulating a failed instantiation
        mockLoader.Setup(loader => loader.LoadFromFile(It.IsAny<string>())).ReturnsAsync(nullModel);

        // Set up the mock for IFileMonitor
        mockFileMonitor.Setup(fm => fm.PersistentDataPath).Returns("fake/path");

        // Ensure the directory exists
        Directory.CreateDirectory("fake/path");
        File.Create("fake/path/nonExistentModel.glb").Dispose();

        // Act: Set the expectation before the action occurs
        LogAssert.Expect(LogType.Error, $"Failed to instantiate model: {modelName}");

        // Attempt to show the model which should fail
        prefabManager.ShowModel(modelName);

        yield return null;

        // Assert: No additional assertions needed since LogAssert.Expect will fail if the log is not received
        // Clean up
        Directory.Delete("fake/path", true);
    }


    [UnityTest]
    public IEnumerator DestroyAllPrefabs_ClearsThePrefabInstances()
    {
        string modelName = "testModel";
        GameObject fakeModel = new GameObject(modelName);
        typeof(PrefabManager).GetField("prefabInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, new Dictionary<string, GameObject> { { modelName, fakeModel } });

        prefabManager.DestroyAllPrefabs();

        yield return 0;

        Assert.AreEqual(0, prefabManagerGameObject.transform.childCount);
    }

    [UnityTest]
    public IEnumerator HideAllPrefabs_DisablesAllActivePrefabs()
    {
        // Arrange
        string modelName1 = "testModel1";
        string modelName2 = "testModel2";

        GameObject prefab1 = new GameObject(modelName1);
        GameObject prefab2 = new GameObject(modelName2);

        // Set the prefabs as active
        prefab1.SetActive(true);
        prefab2.SetActive(true);

        // Add prefabs to the prefabInstances dictionary
        var prefabInstances = new Dictionary<string, GameObject>
    {
        { modelName1, prefab1 },
        { modelName2, prefab2 }
    };

        // Set the prefabInstances field using reflection
        typeof(PrefabManager).GetField("prefabInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, prefabInstances);

        // Act
        var hideAllPrefabsMethod = typeof(PrefabManager).GetMethod("HideAllPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        hideAllPrefabsMethod.Invoke(prefabManager, null);

        yield return null; // Wait for the next frame

        // Assert
        Assert.IsFalse(prefab1.activeSelf, "Prefab 1 was not deactivated.");
        Assert.IsFalse(prefab2.activeSelf, "Prefab 2 was not deactivated.");

        // Clean up
        Object.DestroyImmediate(prefab1);
        Object.DestroyImmediate(prefab2);
    }


}
