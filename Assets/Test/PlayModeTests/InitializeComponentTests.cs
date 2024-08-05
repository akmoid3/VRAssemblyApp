using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;
using System;

public class InitializeComponentTests
{
    private InitializeComponentManager initializeComponentManager;
    private GameObject testGameObject;
    private GameObject managerGameObject;

    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject("TestObject");
        initializeComponentManager = testGameObject.AddComponent<InitializeComponentManager>();

        GameObject dropdownGameObject = new GameObject("Dropdown");
        TMP_Dropdown dropdown = dropdownGameObject.AddComponent<TMP_Dropdown>();
        initializeComponentManager.GetType().GetField("componentDropdown", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(initializeComponentManager, dropdown);

        GameObject canvasInit = new GameObject("CanvasInit");
        initializeComponentManager.GetType().GetField("canvasInit", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(initializeComponentManager, canvasInit);
        GameObject canvasMod = new GameObject("CanvasMod");
        initializeComponentManager.GetType().GetField("canvasMod", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(initializeComponentManager, canvasMod);

        GameObject textGameObject = new GameObject("ComponentNameText");
        TextMeshProUGUI componentName = textGameObject.AddComponent<TextMeshProUGUI>();
        initializeComponentManager.GetType().GetField("componentName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(initializeComponentManager, componentName);

        managerGameObject = new GameObject("Manager");
        Manager manager = managerGameObject.AddComponent<Manager>();

        // Create and setup the finishedButton
        GameObject buttonGameObject = new GameObject("FinishedButton");
        Button finishedButton = buttonGameObject.AddComponent<Button>();
        initializeComponentManager.GetType().GetField("finishedButton", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(initializeComponentManager, finishedButton);


    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(testGameObject);
        GameObject.DestroyImmediate(managerGameObject);
    }

    [UnityTest]
    public IEnumerator InitializeSelectedComponent_NoComponentSelected_LogsError()
    {
        LogAssert.Expect(LogType.Error, "No component selected.");

        initializeComponentManager.InitializeSelectedComponent(null);

        yield return null;
    }

    [UnityTest]
    public IEnumerator InitializeSelectedComponent_ValidComponent_AddsCorrectScript()
    {
        GameObject selectedComponent = new GameObject("SelectedComponent");
        selectedComponent.AddComponent<MeshRenderer>();
        ComponentObject componentObject = selectedComponent.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);

        initializeComponentManager.InitializeSelectedComponent(selectedComponent);
        yield return null;

        Assert.IsNotNull(selectedComponent.GetComponent<Screw>(), "Screw component should be added.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator InitializeSelectedComponent_ChangesComponentType_RemovesOldScript()
    {
        GameObject selectedComponent = new GameObject("SelectedComponent");
        selectedComponent.AddComponent<MeshRenderer>();
        ComponentObject componentObject = selectedComponent.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        selectedComponent.AddComponent<Screw>();

        componentObject.SetComponentType(ComponentObject.ComponentType.Nail);
        initializeComponentManager.InitializeSelectedComponent(selectedComponent);

        yield return null;


        Assert.IsNull(selectedComponent.GetComponent<Screw>(), "Screw component should be removed.");
        Assert.IsNotNull(selectedComponent.GetComponent<Nail>(), "Nail component should be added.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator InitializeSelectedComponent_SetToNone_SetsTagToComponent()
    {
        GameObject selectedComponent = new GameObject("SelectedComponent");
        selectedComponent.AddComponent<MeshRenderer>();
        ComponentObject componentObject = selectedComponent.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.None);

        initializeComponentManager.InitializeSelectedComponent(selectedComponent);
        yield return null;

        Assert.AreEqual("Component", selectedComponent.tag, "Tag should be set to 'Component'.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator InitializeSelectedComponent_RemovesAllExistingScripts()
    {
        GameObject selectedComponent = new GameObject("SelectedComponent");
        selectedComponent.AddComponent<MeshRenderer>();
        ComponentObject componentObject = selectedComponent.AddComponent<ComponentObject>();
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        selectedComponent.AddComponent<Screw>();
        selectedComponent.AddComponent<Nail>();

        initializeComponentManager.InitializeSelectedComponent(selectedComponent);

        yield return null;

        Assert.IsNull(selectedComponent.GetComponent<Nail>(), "Nail component should be removed.");
        Assert.IsNotNull(selectedComponent.GetComponent<Screw>(), "Screw component should be added.");
        yield return null;
    }
}
