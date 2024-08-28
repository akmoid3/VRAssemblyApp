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
    private StateManager stateManager;
    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();

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
        GameObject.DestroyImmediate(stateManager.gameObject);

    }

    private string NormalizeJson(string json)
    {
        // Remove whitespace, new lines, and carriage returns
        return Regex.Replace(json, @"\s+", "");
    }

    private ComponentObject CreateComponentObject(GameObject component, ComponentObject.Group group, ComponentObject.ComponentType type)
    {
        ComponentObject componentObject = component.AddComponent<ComponentObject>();
        componentObject.SetGroup(group);
        componentObject.SetComponentType(type);
        return componentObject;
    }

    [UnityTest]
    public IEnumerator SaveComponent_CreatesExpectedJsonFile()
    {
        GameObject component = new GameObject("Component1");
        CreateComponentObject(component, ComponentObject.Group.None, ComponentObject.ComponentType.None);

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
                        ""toolName"": ""null"",
                        ""group"": 0,
                        ""type"": 0
                    }
                ]
            }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure.");
    }

    [UnityTest]
    public IEnumerator ModifyComponent_UpdatesJsonFileCorrectly()
    {
        // Create and save the initial component
        GameObject component = new GameObject("Component1");
        CreateComponentObject(component, ComponentObject.Group.None, ComponentObject.ComponentType.None);

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
                    ""toolName"": ""null"",
                    ""group"": 0,
                    ""type"": 0
                }
            ]
        }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure after modification.");
    }

    [UnityTest]
    public IEnumerator RemoveComponent_ResultsInEmptyJsonFile()
    {
        GameObject component = new GameObject("Component1");
        CreateComponentObject(component, ComponentObject.Group.None, ComponentObject.ComponentType.None);

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

    [Test]
    public void ObjectDataProperty_GetsAndSetsCorrectly()
    {
        // Arrange
        var newObjectData = new ObjectData();
        newObjectData.components.Add(new ComponentData
        {
            componentName = "TestComponent",
            position = new Vector3(1, 2, 3),
            rotation = Quaternion.Euler(45, 90, 0),
            toolName = "TestTool",
            group = ComponentObject.Group.None,
            type = ComponentObject.ComponentType.None
        });

        // Act
        saveSequence.ObjectData = newObjectData;

        // Assert
        Assert.AreEqual(newObjectData, saveSequence.ObjectData, "The ObjectData property did not correctly set or get the expected value.");
    }

    [Test]
    public void StartMethod_InitializesDirectoryPathCorrectly()
    {
        // Act
        saveSequence.Invoke("Start", 0f);

        // Assert
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "The directory path was not correctly initialized or created by the Start method.");
    }

    [UnityTest]
    public IEnumerator SaveComponent_SetsToolNameWhenFastenerAndToolArePresent()
    {
        // Arrange
        GameObject component = new GameObject("ComponentWithFastener");

        // Aggiungi un Renderer al GameObject se necessario
        component.AddComponent<MeshRenderer>();
        component.AddComponent<ComponentObject>();


        // Aggiungi il componente Fastener
        Fastener fastener = component.AddComponent<Screw>();

        // Crea un oggetto Tool e assegnalo al Fastener
        GameObject tool = new GameObject("Screwdriver");
        fastener.CorrectToolName = (tool.name);

        // Verifica che il tool sia stato correttamente impostato
        Assert.AreEqual(tool.name, fastener.CorrectToolName, "Il tool non è stato impostato correttamente nel Fastener.");

        CreateComponentObject(component, ComponentObject.Group.None, ComponentObject.ComponentType.None);

        fastener.Tool = tool;

        Assert.AreEqual(tool, fastener.Tool, "Il tool non è stato impostato correttamente nel Fastener.");

        // Act
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
                    ""componentName"": ""ComponentWithFastener"",
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
                    ""toolName"": ""Screwdriver"",
                    ""group"": 0,
                    ""type"": 0
                }
            ]
        }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file does not match the expected structure when Fastener and Tool are present.");
    }


    [UnityTest]
    public IEnumerator SaveComponent_DoesNotSetToolNameWhenToolIsNull()
    {
        // Arrange
        GameObject component = new GameObject("ComponentWithFastenerButNoTool");
        Fastener fastener = component.AddComponent<Fastener>();

        // Non impostare alcun tool per il Fastener
        CreateComponentObject(component, ComponentObject.Group.None, ComponentObject.ComponentType.None);

        // Act
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
                    ""componentName"": ""ComponentWithFastenerButNoTool"",
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
                    ""toolName"": ""null"",
                    ""group"": 0,
                    ""type"": 0
                }
            ]
        }";

        // Normalize and compare JSON
        Assert.AreEqual(NormalizeJson(expectedJson), NormalizeJson(json), "The JSON file should not include a tool name when Tool is null.");
    }

    [UnityTest]
    public IEnumerator Start_CreatesDirectoryIfNotExists()
    {
        // Arrange
        if (Directory.Exists(testDirectoryPath))
        {
            Directory.Delete(testDirectoryPath, true);
        }

        // Act
        saveSequence.Invoke("Start", 0f);

        yield return null;
        // Assert
        Assert.IsTrue(Directory.Exists(testDirectoryPath), "The directory should be created if it does not exist.");
    }


}
