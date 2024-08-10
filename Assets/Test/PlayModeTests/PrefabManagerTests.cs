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
    public IEnumerator DestroyAllPrefabs_ClearsThePrefabInstances()
    {
        string modelName = "testModel";
        GameObject fakeModel = new GameObject(modelName);
        typeof(PrefabManager).GetField("prefabInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(prefabManager, new Dictionary<string, GameObject> { { modelName, fakeModel } });

        prefabManager.DestroyAllPrefabs();

        yield return 0;

        Assert.AreEqual(0, prefabManagerGameObject.transform.childCount);
    }
}
