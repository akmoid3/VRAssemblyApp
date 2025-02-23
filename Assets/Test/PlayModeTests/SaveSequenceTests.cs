using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Collections;

public class SaveSequenceTests
{
    private GameObject saveSequenceGO;
    private SaveSequence saveSequence;
    private string directoryPath;
    private GameObject managerGO;
    private AudioManager audioManager;
    private StateManager stateManager;


    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        managerGO = new GameObject("Manager");
        Manager manager = managerGO.AddComponent<Manager>();
        PdfLoader loader = managerGO.AddComponent<PdfLoader>();
        manager.LoaderPDF = loader;
        loader.CanActivePanel = false; // so that pdfIndex remains -1
        loader.CurrentPageIndex = -1;

        audioManager = new GameObject().AddComponent<AudioManager>();

        saveSequenceGO = new GameObject("SaveSequence");
        saveSequence = saveSequenceGO.AddComponent<SaveSequence>();

        saveSequence.Start();
        directoryPath = Path.Combine(Application.persistentDataPath, "SavedBuildData");
    }

    [TearDown]
    public void TearDown()
    {
        if (saveSequenceGO != null)
            Object.DestroyImmediate(saveSequenceGO);
        if (managerGO != null)
            Object.DestroyImmediate(managerGO);
        Object.DestroyImmediate(audioManager.gameObject);
        if(stateManager != null)
        Object.DestroyImmediate(stateManager.gameObject);

    }

    [Test]
    public void TestStart_CreatesDirectory()
    {
        Assert.IsTrue(Directory.Exists(directoryPath), $"Directory {directoryPath} should exist after Start.");
    }

    [Test]
    public void TestSaveComponent_AddsComponentDataWithoutFastener()
    {
        GameObject componentGO = new GameObject("TestComponent");
        componentGO.transform.localPosition = new Vector3(1, 2, 3);
        componentGO.transform.localRotation = Quaternion.Euler(10, 20, 30);
        componentGO.AddComponent<ComponentObject>();

        int initialCount = saveSequence.ObjectData.components.Count;
        saveSequence.SaveComponent(componentGO);
        Assert.AreEqual(initialCount + 1, saveSequence.ObjectData.components.Count);

        ComponentData data = saveSequence.ObjectData.components[saveSequence.ObjectData.components.Count - 1];
        Assert.AreEqual(componentGO.name, data.componentName);
        Assert.AreEqual(componentGO.transform.localPosition, data.position);
        Assert.AreEqual(componentGO.transform.localRotation, data.rotation);

        Assert.AreEqual("null", data.toolName);
        Assert.AreEqual(0, data.toolForce);

        Assert.AreEqual(-1, data.pdfIndex);

        Object.DestroyImmediate(componentGO);
    }

    [Test]
    public void TestSaveComponent_AddsComponentDataWithFastener()
    {
        GameObject componentGO = new GameObject("TestComponentWithTool");
        componentGO.transform.localPosition = new Vector3(4, 5, 6);
        componentGO.transform.localRotation = Quaternion.Euler(40, 50, 60);
        componentGO.AddComponent<ComponentObject>();

        Fastener fastener = componentGO.AddComponent<Screw>();
        DynamometerScrewDriver tool = componentGO.AddComponent<DynamometerScrewDriver>();

        tool.Force = 99;
        tool.ToolName = "TestTool";
        fastener.Tool = tool;


        int initialCount = saveSequence.ObjectData.components.Count;
        saveSequence.SaveComponent(componentGO);
        Assert.AreEqual(initialCount + 1, saveSequence.ObjectData.components.Count);

        ComponentData data = saveSequence.ObjectData.components[saveSequence.ObjectData.components.Count - 1];
        Assert.AreEqual(componentGO.name, data.componentName);
        Assert.AreEqual(componentGO.transform.localPosition, data.position);
        Assert.AreEqual(componentGO.transform.localRotation, data.rotation);
        Assert.AreEqual("TestTool", data.toolName);
        Assert.AreEqual(99, data.toolForce);

        Object.DestroyImmediate(componentGO);
    }

    [Test]
    public void TestModifyComponent_UpdatesComponentData()
    {
        GameObject componentGO = new GameObject("ModComponent");
        componentGO.transform.localPosition = Vector3.zero;
        componentGO.transform.localRotation = Quaternion.identity;
        componentGO.AddComponent<ComponentObject>();

        saveSequence.SaveComponent(componentGO);
        int countBefore = saveSequence.ObjectData.components.Count;
        ComponentData dataBefore = saveSequence.ObjectData.components[saveSequence.ObjectData.components.Count - 1];

        componentGO.transform.localPosition = new Vector3(7, 8, 9);
        componentGO.transform.localRotation = Quaternion.Euler(70, 80, 90);
        saveSequence.ModifyComponent(componentGO);

        ComponentData dataAfter = saveSequence.ObjectData.components[saveSequence.ObjectData.components.Count - 1];
        Assert.AreEqual(new Vector3(7, 8, 9), dataAfter.position);
        Assert.AreEqual(Quaternion.Euler(70, 80, 90), dataAfter.rotation);

        Object.DestroyImmediate(componentGO);
    }

    [Test]
    public void TestRemoveComponent_RemovesLastOccurrence()
    {
        GameObject componentGO = new GameObject("RemovableComponent");
        componentGO.AddComponent<ComponentObject>();

        saveSequence.SaveComponent(componentGO);
        saveSequence.SaveComponent(componentGO);
        int countBefore = saveSequence.ObjectData.components.Count;

        saveSequence.RemoveComponent(componentGO);
        Assert.AreEqual(countBefore - 1, saveSequence.ObjectData.components.Count);

        saveSequence.RemoveComponent(componentGO);
        Assert.AreEqual(countBefore - 2, saveSequence.ObjectData.components.Count);

        Object.DestroyImmediate(componentGO);
    }

    [UnityTest]
    public IEnumerator TestSaveSequenceToJSON_WritesFile()
    {
        GameObject componentGO = new GameObject("JsonComponent");
        componentGO.AddComponent<ComponentObject>();
        saveSequence.SaveComponent(componentGO);

        string fileName = "TestSaveSequence";
        string filePath = Path.Combine(directoryPath, fileName + ".json");
        if (File.Exists(filePath))
            File.Delete(filePath);

        saveSequence.SaveSequenceToJSON(fileName);

        yield return null;

        Assert.IsTrue(File.Exists(filePath), $"File {filePath} should exist after SaveSequenceToJSON.");
        string jsonContent = File.ReadAllText(filePath);
        Assert.IsFalse(string.IsNullOrEmpty(jsonContent));

        File.Delete(filePath);
        Object.DestroyImmediate(componentGO);
    }
}