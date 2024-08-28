using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
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
    private StateManager stateManager;
    private GameObject buildPosition;

    [SetUp]
    public void SetUp()
    {
        buildPosition = new GameObject();
        testGameObject = new GameObject();
        sequenceReader = testGameObject.AddComponent<SequenceReader>();

        stateManager = new GameObject().AddComponent<StateManager>();
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

        var buildingPositionField = typeof(SequenceReader).GetField("buildingPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (buildingPositionField != null)
        {
            buildingPositionField.SetValue(sequenceReader, buildPosition);
        }
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
        GameObject.DestroyImmediate(stateManager.gameObject);


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
        string jsonContent = @"
    {
        ""components"": [
            {
                ""componentName"": ""TestComponent"",
                ""position"": {
                    ""x"": 1.0,
                    ""y"": 2.0,
                    ""z"": 3.0
                },
                ""rotation"": {
                    ""x"": 0,
                    ""y"": 0,
                    ""z"": 0,
                    ""w"": 1
                },
                ""toolName"": ""null"",
                ""group"": 0,
                ""type"": 0
            }
        ]
    }";
        filePath = Path.Combine(directoryPath, "TestModel2.json");

        File.WriteAllText(filePath, jsonContent);

        sequenceReader.CreateSnapObjectFromJSON();

        yield return null;

        GameObject parentObject = GameObject.Find("SnapParentObject");
        Assert.IsNotNull(parentObject, "SnapParentObject was not created.");


       
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
