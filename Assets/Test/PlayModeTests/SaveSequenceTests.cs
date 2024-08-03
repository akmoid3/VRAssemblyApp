using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SaveSequenceTests
{
    private GameObject gameObject;
    private SaveSequence saveSequence;
    private string testDirectoryPath;
    private string testFilePath;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        saveSequence = gameObject.AddComponent<SaveSequence>();

        // Initialize the directoryPath
        saveSequence.Invoke("Start", 0f);

        // Get the directory path and set the test file path
        testDirectoryPath = Path.Combine(Application.persistentDataPath, "SavedBuildData");
        testFilePath = Path.Combine(testDirectoryPath, "test_sequence.json");

        // Ensure the directory exists for the test
        if (!Directory.Exists(testDirectoryPath))
        {
            Directory.CreateDirectory(testDirectoryPath);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
        GameObject.DestroyImmediate(gameObject);
    }

    private string NormalizeJson(string json)
    {
        // Remove whitespace, new lines, and carriage returns
        return Regex.Replace(json, @"\s+", "");
    }

    [UnityTest]
    public IEnumerator TestAddComponent()
    {
        GameObject component = new GameObject("Component1");
        saveSequence.SaveComponent(component);

        saveSequence.SaveSequenceToJSON("test_sequence");

        // Wait a frame to ensure file writing is complete
        yield return null;

        // Read JSON from the file
        string json = File.ReadAllText(testFilePath);

        // Expected JSON structure
        string expectedJson =
            @"{
                ""components"": [
                    {
                        ""componentName"": ""Component1"",
                        ""position"": {
                            ""x"": 0.0,
                            ""y"": 0.0,
                            ""z"": 0.0
                        },
                        ""rotation"": {
                            ""x"": 0.0,
                            ""y"": 0.0,
                            ""z"": 0.0,
                            ""w"": 1.0
                        },
                        ""toolName"": ""null""
                    }
                ]
            }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure.");
    }

    [UnityTest]
    public IEnumerator TestModifyComponent()
    {
        // Create and save the initial component
        GameObject component = new GameObject("Component1");
        saveSequence.SaveComponent(component);

        // Modify the component's transform and rotation
        component.transform.localPosition = new Vector3(1, 2, 3);
        component.transform.localRotation = Quaternion.Euler(45, 90, 0);

        // Update the component in the SaveSequence
        saveSequence.ModifyComponent(component);

        // Save the updated sequence to JSON
        saveSequence.SaveSequenceToJSON("test_sequence");

        // Wait a frame to ensure file writing is complete
        yield return null;

        // Read the JSON from the file
        string json = File.ReadAllText(testFilePath);

        // Define the expected JSON structure with precise values
        string expectedJson = @"
    {
        ""components"": [
            {
                ""componentName"": ""Component1"",
                ""position"": {
                    ""x"": 1.0,
                    ""y"": 2.0,
                    ""z"": 3.0
                },
                ""rotation"": {
                    ""x"": 0.27059808373451235,
                    ""y"": 0.6532815098762512,
                    ""z"": -0.27059808373451235,
                    ""w"": 0.6532815098762512
                },
                ""toolName"": ""null""
            }
        ]
    }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure after modification.");
    }


    [UnityTest]
    public IEnumerator TestRemoveComponent()
    {
        GameObject component = new GameObject("Component1");
        saveSequence.SaveComponent(component);
        saveSequence.RemoveComponent(component);

        saveSequence.SaveSequenceToJSON("test_sequence");

        // Wait a frame to ensure file writing is complete
        yield return null;

        // Read JSON from the file
        string json = File.ReadAllText(testFilePath);

        // Expected JSON structure with no components
        string expectedJson =
            @"{
                ""components"": []
            }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file should be empty after removing the component.");
    }

    [UnityTest]
    public IEnumerator TestSaveSequenceToJSON()
    {
        GameObject component = new GameObject("Component1");
        saveSequence.SaveComponent(component);

        saveSequence.SaveSequenceToJSON("test_sequence");

        // Wait a frame to ensure file writing is complete
        yield return null;

        Assert.IsTrue(File.Exists(testFilePath), "The JSON file should be created");

        // Read JSON from the file
        string json = File.ReadAllText(testFilePath);

        // Expected JSON structure
        string expectedJson =
            @"{
                ""components"": [
                    {
                        ""componentName"": ""Component1"",
                        ""position"": {
                            ""x"": 0.0,
                            ""y"": 0.0,
                            ""z"": 0.0
                        },
                        ""rotation"": {
                            ""x"": 0.0,
                            ""y"": 0.0,
                            ""z"": 0.0,
                            ""w"": 1.0
                        },
                        ""toolName"": ""null""
                    }
                ]
            }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure.");
    }
}
