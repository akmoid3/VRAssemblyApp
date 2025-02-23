using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SequenceReaderTests
{
    private GameObject sequenceReaderGO;
    private SequenceReader sequenceReader;
    private GameObject buildingPosition;
    private Material holographicMaterial;
    private GameObject managerGO;
    private Manager manager; // Your concrete Manager component
    private GameObject prefab; // Prefab with a child named "TestComponent"
    private string jsonDir;
    private string modelName;

    [SetUp]
    public void Setup()
    {
        // Create and set up the building position
        buildingPosition = new GameObject("BuildingPosition");
        buildingPosition.transform.position = Vector3.zero;

        // Create SequenceReader and assign required fields
        sequenceReaderGO = new GameObject("SequenceReader");
        sequenceReader = sequenceReaderGO.AddComponent<SequenceReader>();
        sequenceReader.buildingPosition = buildingPosition;
        holographicMaterial = new Material(Shader.Find("Standard"));
        sequenceReader.holographicMaterial = holographicMaterial;

        // Set up the Manager instance (using your concrete Manager)
        managerGO = new GameObject("Manager");
        manager = managerGO.AddComponent<Manager>();
        manager.ModelName = "TestModel";
        modelName = manager.ModelName;
        

        // Create a prefab GameObject with a child named "TestComponent"
        prefab = new GameObject("Prefab");
        GameObject child = new GameObject("TestComponent");
        child.transform.SetParent(prefab.transform);
        // Add required components for CopyMeshAndMaterial to work
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
        prefab.transform.localScale = Vector3.one;
        manager.Model = prefab; // Assign the prefab in the manager

        // Ensure the JSON directory exists in persistentDataPath
        jsonDir = Path.Combine(Application.persistentDataPath, "SavedBuildData");
        if (!Directory.Exists(jsonDir))
        {
            Directory.CreateDirectory(jsonDir);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (sequenceReaderGO != null) GameObject.DestroyImmediate(sequenceReaderGO);
        if (buildingPosition != null) GameObject.DestroyImmediate(buildingPosition);
        if (managerGO != null) GameObject.DestroyImmediate(managerGO);
        if (prefab != null) GameObject.DestroyImmediate(prefab);

        // Delete the test JSON file if it exists
        string jsonPath = Path.Combine(jsonDir, modelName + ".json");
        if (File.Exists(jsonPath))
            File.Delete(jsonPath);
    }

  

    [Test]
    public void Test_CreateSnapObjectFromJSON_InvalidJson()
    {
        // Write an invalid JSON file so that deserialization fails.
        string jsonPath = Path.Combine(jsonDir, modelName + ".json");
        File.WriteAllText(jsonPath, "invalid json");
        
        // Expect an exception to be thrown by JsonUtility.FromJson.
        Assert.Throws<System.ArgumentException>(() => sequenceReader.CreateSnapObjectFromJSON());
    }

    [Test]
    public void Test_CreateSnapObjectFromJSON_PrefabNull()
    {
        // Create a valid JSON file with one component entry using the updated ComponentData fields
        RootObject rootObject = new RootObject
        {
            components = new List<ComponentData>
            {
                new ComponentData
                {
                    stepId = 1,
                    componentName = "TestComponent",
                    position = new Vector3(0, 0, 0),
                    rotation = new Quaternion(0, 0, 0, 1),
                    toolName = "",
                    toolForce = 0,
                    group = "",
                    type = ComponentObject.ComponentType.None,
                    pdfIndex = 0
                }
            }
        };
        string json = JsonUtility.ToJson(rootObject);
        string jsonPath = Path.Combine(jsonDir, modelName + ".json");
        File.WriteAllText(jsonPath, json);

        // Set the Manager’s prefab reference to null to simulate a missing prefab.
        manager.Model = null;
        LogAssert.Expect(LogType.Error, $"Prefab not found at path: Prefabs/{modelName}");
        sequenceReader.CreateSnapObjectFromJSON();
    }

    [Test]
    public void Test_CreateSnapObjectFromJSON_Success()
    {
        // Create a valid JSON file with one component entry using the updated ComponentData fields
        RootObject rootObject = new RootObject
        {
            components = new List<ComponentData>
            {
                new ComponentData
                {
                    stepId = 1,
                    componentName = "TestComponent",
                    position = new Vector3(1, 2, 3),
                    rotation = new Quaternion(0, 0, 0, 1),
                    toolName = "",
                    toolForce = 0,
                    group = "",
                    type = ComponentObject.ComponentType.None,
                    pdfIndex = 0
                }
            }
        };
        string json = JsonUtility.ToJson(rootObject);
        string jsonPath = Path.Combine(jsonDir, modelName + ".json");
        File.WriteAllText(jsonPath, json);

        // Call the method under test.
        sequenceReader.CreateSnapObjectFromJSON();

        // Verify that a SnapParentObject was created in the scene.
        GameObject snapParent = GameObject.Find("SnapParentObject");
        Assert.IsNotNull(snapParent, "SnapParentObject should be created.");

        // Verify required components on the snap parent.
        Assert.IsNotNull(snapParent.GetComponent<SnapToPosition>(), "SnapParentObject should have a SnapToPosition component.");
        BoxCollider boxCollider = snapParent.GetComponent<BoxCollider>();
        Assert.IsNotNull(boxCollider, "SnapParentObject should have a BoxCollider.");
        Assert.IsTrue(boxCollider.isTrigger, "BoxCollider should be set as a trigger.");
        Assert.IsNotNull(snapParent.GetComponent<Rigidbody>(), "SnapParentObject should have a Rigidbody component.");

        // Verify that the child object ("TestComponent") exists under the snap parent.
        Transform childTransform = snapParent.transform.Find("TestComponent");
        Assert.IsNotNull(childTransform, "TestComponent should exist as a child of SnapParentObject.");

        // Check that the holographic material was assigned and that the MeshRenderer is disabled.
        MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
        Assert.IsNotNull(renderer, "TestComponent should have a MeshRenderer.");
        Assert.AreEqual(holographicMaterial, renderer.sharedMaterial, "MeshRenderer should have the holographic material assigned.");
        Assert.IsFalse(renderer.enabled, "MeshRenderer should be disabled.");
    }
}
