using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;
using System.Reflection;

public class FinishManagerTests
{
    private FinishManager finishManager;
    private GameObject finishPanel;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI errorCountText;
    private TextMeshProUGUI hintCountText;
    private Button finishButton;
    private Manager manager;
    private StateManager stateManager;
    private SequenceManager sequenceManager;
    private HintManager hintManager;



    [SetUp]
    public void SetUp()
    {
        sequenceManager = new GameObject().AddComponent<SequenceManager>();
        hintManager = new GameObject().AddComponent<HintManager>();


        // Create a new GameObject and attach the FinishManager component
        var gameObject = new GameObject();
        finishManager = gameObject.AddComponent<FinishManager>();

        // Create UI elements and assign them to the FinishManager
        finishPanel = new GameObject();
        finishManager.GetType().GetField("finishPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(finishManager, finishPanel);

        timerText = new GameObject().AddComponent<TextMeshProUGUI>();
        finishManager.GetType().GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(finishManager, timerText);

        errorCountText = new GameObject().AddComponent<TextMeshProUGUI>();
        finishManager.GetType().GetField("errorCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(finishManager, errorCountText);

        hintCountText = new GameObject().AddComponent<TextMeshProUGUI>();
        finishManager.GetType().GetField("hintCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(finishManager, hintCountText);

        finishButton = new GameObject().AddComponent<Button>();
        finishManager.GetType().GetField("finishButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(finishManager, finishButton);

        // Setup mock Manager and StateManager
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();

        SetPrivateField(manager, "sequenceManager", sequenceManager);

        SetPrivateField(manager, "hintManager", hintManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(finishManager.gameObject);
        Object.DestroyImmediate(finishPanel);
        Object.DestroyImmediate(timerText.gameObject);
        Object.DestroyImmediate(errorCountText.gameObject);
        Object.DestroyImmediate(hintCountText.gameObject);
        Object.DestroyImmediate(finishButton.gameObject);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void TestSetPanelActive_ActivatesFinishPanelAndUpdatesText_WhenStateIsFinish()
    {
        // Arrange
        finishPanel.SetActive(false);
        Manager.Instance.GetType().GetProperty("ErrorCount").SetValue(Manager.Instance, 5);
        Manager.Instance.GetType().GetProperty("HintCount").SetValue(Manager.Instance, 10);

        // Act
        finishManager.SetPanelActive(State.Finish);

        // Assert
        Assert.IsTrue(finishPanel.activeSelf, "finishPanel should be active when state is Finish");
        Assert.AreEqual("5", errorCountText.text, "errorCountText should be updated with the correct error count");
        Assert.AreEqual("10", hintCountText.text, "hintCountText should be updated with the correct hint count");
    }

    [Test]
    public void TestSetPanelActive_DeactivatesFinishPanel_WhenStateIsNotFinish()
    {
        // Arrange
        finishPanel.SetActive(true);

        // Act
        finishManager.SetPanelActive(State.PlayBack);

        // Assert
        Assert.IsFalse(finishPanel.activeSelf, "finishPanel should be inactive when state is not Finish");
    }

    [Test]
    public void TestOnFinishClicked_ReloadsCurrentScene()
    {
        // Arrange
        var initialScene = SceneManager.GetActiveScene().name;

        // Act

    }

    [Test]
    public void TestUpdate_UpdatesTimerText_WhenStateIsFinishAndTimerIsZero()
    {
        // Arrange
        timerText.text = "00:00";
        Manager.Instance.GetType().GetProperty("FinishTime").SetValue(Manager.Instance, "10:45");

        StateManager.Instance.GetType().GetProperty("CurrentState").SetValue(StateManager.Instance, State.Finish);

        // Act
        finishManager.Update();

        // Assert
        Assert.AreEqual("10:45", timerText.text, "timerText should be updated with the Manager's FinishTime");
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {obj.GetType()}");
        }
    }
}
