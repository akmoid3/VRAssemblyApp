using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[TestFixture]
public class FinishManagerTests
{
    private FinishManager finishManager;
    private GameObject gameObject;
    private GameObject panelObject;
    private Button buttonObject;
    private TextMeshProUGUI timerTextObject;
    private TextMeshProUGUI errorCountTextObject;
    private TextMeshProUGUI hintCountTextObject;
    private TextMeshProUGUI averagePerformanceTextObject;
    private GameObject managerObject;
    private Manager manager;

    private StateManager stateManager;

    [SetUp]
    public void Setup()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        SetupManagerSingleton();
        // Create necessary GameObject setup
        gameObject = new GameObject("FinishManager");
        finishManager = gameObject.AddComponent<FinishManager>();

        // Create and set up finish panel
        panelObject = new GameObject("FinishPanel");
        panelObject.transform.SetParent(gameObject.transform);

        // Create button
        var buttonGO = new GameObject("FinishButton");
        buttonGO.transform.SetParent(panelObject.transform);
        buttonObject = buttonGO.AddComponent<Button>();

        // Create text objects
        var timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(panelObject.transform);
        timerTextObject = timerGO.AddComponent<TextMeshProUGUI>();
        timerTextObject.text = "00:00";

        var errorGO = new GameObject("ErrorCountText");
        errorGO.transform.SetParent(panelObject.transform);
        errorCountTextObject = errorGO.AddComponent<TextMeshProUGUI>();

        var hintGO = new GameObject("HintCountText");
        hintGO.transform.SetParent(panelObject.transform);
        hintCountTextObject = hintGO.AddComponent<TextMeshProUGUI>();

        var performanceGO = new GameObject("AveragePerformanceText");
        performanceGO.transform.SetParent(panelObject.transform);
        averagePerformanceTextObject = performanceGO.AddComponent<TextMeshProUGUI>();

        // Set serialized fields via reflection
        SetPrivateField(finishManager, "finishPanel", panelObject);
        SetPrivateField(finishManager, "finishButton", buttonObject);
        SetPrivateField(finishManager, "timerText", timerTextObject);
        SetPrivateField(finishManager, "errorCountText", errorCountTextObject);
        SetPrivateField(finishManager, "hintCountText", hintCountTextObject);
        SetPrivateField(finishManager, "averagePerformanceText", averagePerformanceTextObject);


        manager.PerformaceForEachStep = new List<float> { 0.0f, 0.5f, 0.7f, 0.9f };
        manager.ErrorCount = 3;
        manager.HintCount = 2;
        manager.FinishTime = "01:30";
    }

    private void SetupManagerSingleton()
    {
        managerObject = new GameObject("Manager");
        manager = managerObject.AddComponent<Manager>();

        manager.sequenceManager = new GameObject().AddComponent<SequenceManager>();
        manager.sequenceManager.AssemblySequence = new List<ComponentData>();
        manager.hintManager = new GameObject().AddComponent<HintManager>();
        // Initialize Manager properties
        manager.Components = new List<Transform>();
        manager.PerformaceForEachStep = new List<float>();
        manager.CurrentAssembledSequence = new Dictionary<int, GameObject>();
        manager.Interactor = new GameObject("Interactor").AddComponent<SnapToPosition>();
        var childObject = new GameObject("Child0").transform;
        childObject.SetParent(manager.Interactor.transform);
    }

    [TearDown]
    public void TearDown()
    {
        StateManager.OnStateChanged -= finishManager.SetPanelActive;
        if (gameObject)
            Object.DestroyImmediate(gameObject);

        if (manager != null)
            Object.DestroyImmediate(manager);

        if (stateManager != null)
            Object.DestroyImmediate(stateManager);
    }

    // Helper method to set private fields via reflection
    private void SetPrivateField<T>(FinishManager target, string fieldName, T value)
    {
        var field = typeof(FinishManager).GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    [Test]
    public void TestAwake()
    {
        // Act - Manually call Awake through reflection
        var awakeMethod = typeof(FinishManager).GetMethod("Awake",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(finishManager, null);

        stateManager.UpdateState(State.Finish);

        // Check if panel is active, which would indicate the event subscription worked
        Assert.IsTrue(panelObject.activeSelf);
    }

    [Test]
    public void TestUpdate_WhenStateIsFinish()
    {
        timerTextObject.text = "00:00";

        stateManager.UpdateState(State.Finish);

        // Act
        finishManager.Update();

        // Assert
        Assert.AreEqual($"Time: {manager.FinishTime}", timerTextObject.text);
    }

    [Test]
    public void TestUpdate_WhenStateIsNotFinish()
    {
        timerTextObject.text = "00:00";

        // Act
        finishManager.Update();

        // Assert
        Assert.AreEqual("00:00", timerTextObject.text);
    }

    [Test]
    public void TestSetPanelActive_WhenStateIsFinish()
    {
        // Act
        finishManager.SetPanelActive(State.Finish);

        // Assert
        Assert.IsTrue(panelObject.activeSelf);
        Assert.AreEqual($"Errors: {Manager.Instance.ErrorCount}", errorCountTextObject.text);
        Assert.AreEqual($"Hints: {Manager.Instance.HintCount}", hintCountTextObject.text);

        // This will test line 43 which calculates average performance
        float expectedAvg = finishManager.CalculateAveragePerformance();
        string expectedText = $"Accuracy: {expectedAvg * 100:F2}";
        Assert.AreEqual(expectedText, averagePerformanceTextObject.text);
    }

    [Test]
    public void TestSetPanelActive_WhenStateIsNotFinish()
    {
        // Arrange
        panelObject.SetActive(true);

        // Act
        finishManager.SetPanelActive(State.PlayBack);

        // Assert
        Assert.IsFalse(panelObject.activeSelf);
    }

    [Test]
    public void TestCalculateAveragePerformance()
    {
        float result = finishManager.CalculateAveragePerformance();

        Assert.AreEqual(0.699999988f, result);
    }

    [Test]
    public void TestCalculateAveragePerformance_WithNoElements()
    {
        // Arrange
        Manager.Instance.PerformaceForEachStep = new List<float>() { 0.0f };

        // Act
        float result = finishManager.CalculateAveragePerformance();

        // Assert
        Assert.AreEqual(0.0f, result);
    }
}