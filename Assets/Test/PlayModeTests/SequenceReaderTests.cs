using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Content.Interaction;
public class SequenceReaderTests
{
    private GameObject testGameObject;
    private GameObject testGameObject2;
    private SequenceReader sequenceReader;
    private GameObject managerSequenceReaderTests;
    private Manager managerScript;
    private string directoryPath;
    private string filePath;
    private Material holographicMaterial;

    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject();
        sequenceReader = testGameObject.AddComponent<SequenceReader>();

        managerSequenceReaderTests = new GameObject("Manager2");
        managerScript = managerSequenceReaderTests.AddComponent<Manager>();

        directoryPath = Path.Combine(Application.persistentDataPath, "SavedBuildData");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string materialPath = "Assets/Materials/HolographicMaterial.mat";
        holographicMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        testGameObject2 = CreateTestPrefab();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            GameObject.DestroyImmediate(testGameObject);
        }
        if (managerSequenceReaderTests != null)
        {
            GameObject.DestroyImmediate(managerSequenceReaderTests);
        }

        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [UnityTest]
    public IEnumerator TestCreateSnapObjectFromJSON_FileNotExist()
    {
        managerScript.Model = new GameObject("TestModel");
        filePath = Path.Combine(directoryPath, "TestModel.json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        sequenceReader.CreateSnapObjectFromJSON();

        yield return null;


        GameObject parentObject = GameObject.Find("SnapParentObject");
        Assert.IsNull(parentObject, "SnapParentObject should not be created if the JSON file does not exist.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestCreateSnapObjectFromJSON_ValidFile()
    {
        managerScript.Model = testGameObject2;
        string jsonContent = "{\"components\":[{\"componentName\":\"TestComponent\",\"position\":{\"x\":1,\"y\":2,\"z\":3},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"toolName\":\"\"}]}";
        filePath = Path.Combine(directoryPath, "TestModel2.json");

        File.WriteAllText(filePath, jsonContent);

        sequenceReader.CreateSnapObjectFromJSON();

        yield return null;

        GameObject parentObject = GameObject.Find("SnapParentObject");
        Assert.IsNotNull(parentObject, "SnapParentObject was not created.");


        GameObject testComponent = parentObject.transform.Find("TestComponent")?.gameObject;
        Assert.IsNotNull(testComponent, "TestComponent was not created.");
        Assert.AreEqual(new Vector3(1, 2, 3), testComponent.transform.localPosition, "TestComponent has incorrect position.");
        Assert.AreEqual(new Quaternion(0, 0, 0, 1), testComponent.transform.localRotation, "TestComponent has incorrect rotation.");

        var socket = parentObject.GetComponent<XRSnapPointSocketInteractor>();
        Assert.IsNotNull(socket, "XRSnapPointSocketInteractor component is missing.");
        Assert.AreEqual(0.05f, socket.DistanceThreshold, "DistanceThreshold is incorrect.");
        Assert.AreEqual(10.0f, socket.AngleThreshold, "AngleThreshold is incorrect.");

        var collider = parentObject.GetComponent<BoxCollider>();
        Assert.IsNotNull(collider, "BoxCollider component is missing.");
        Assert.IsTrue(collider.isTrigger, "BoxCollider should be a trigger.");
        Assert.AreEqual(new Vector3(10.0f, 10.0f, 10.0f), collider.size, "BoxCollider size is incorrect.");

        var meshFilter = testComponent.GetComponent<MeshFilter>();
        Assert.IsNotNull(meshFilter, "MeshFilter is missing on TestComponent.");

        var meshRenderer = testComponent.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Assert.AreEqual(holographicMaterial, meshRenderer.sharedMaterial, "Material is incorrect on TestComponent.");
        }
        

        yield return null;
    }

    private GameObject CreateTestPrefab()
    {
        GameObject prefab = new GameObject("TestModel2");
        GameObject component = new GameObject("TestComponent");
        component.transform.SetParent(prefab.transform);
        component.AddComponent<MeshFilter>();
        component.AddComponent<MeshRenderer>();

        return prefab;
    }

}
