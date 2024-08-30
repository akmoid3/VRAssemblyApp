using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class PlayBackManagerTests
{
    private PlayBackManager playBackManager;
    private GameObject playBackPanel;
    private Button finishButton;
    private Button showSolutionButton;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI errorCountText;
    private TextMeshProUGUI hintCountText;
    private TextMeshProUGUI stepsText;
    private Manager manager;
    private StateManager stateManager;
    private SequenceManager sequenceManager;



    [SetUp]
    public void SetUp()
    {
        sequenceManager = new GameObject().AddComponent<SequenceManager>();
        // Create a new GameObject and attach the PlayBackManager component
        var gameObject = new GameObject();
        playBackManager = gameObject.AddComponent<PlayBackManager>();

        // Create UI elements and assign them to the PlayBackManager
        playBackPanel = new GameObject();
        playBackManager.playBackPanel = playBackPanel;

        finishButton = new GameObject().AddComponent<Button>();
        playBackManager.finishButton = finishButton;

        showSolutionButton = new GameObject().AddComponent<Button>();
        playBackManager.showSolutionButton = showSolutionButton;

        timerText = new GameObject().AddComponent<TextMeshProUGUI>();
        playBackManager.timerText = timerText;

        errorCountText = new GameObject().AddComponent<TextMeshProUGUI>();
        playBackManager.errorCountText = errorCountText;

        hintCountText = new GameObject().AddComponent<TextMeshProUGUI>();
        playBackManager.hintCountText = hintCountText;

        stepsText = new GameObject().AddComponent<TextMeshProUGUI>();
        playBackManager.stepsText = stepsText;

        // Set up the Manager and StateManager instances
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();

        SetPrivateField(manager, "sequenceManager", sequenceManager);

    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playBackManager.gameObject);
        Object.DestroyImmediate(playBackPanel);
        Object.DestroyImmediate(finishButton.gameObject);
        Object.DestroyImmediate(showSolutionButton.gameObject);
        Object.DestroyImmediate(timerText.gameObject);
        Object.DestroyImmediate(errorCountText.gameObject);
        Object.DestroyImmediate(hintCountText.gameObject);
        Object.DestroyImmediate(stepsText.gameObject);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void TestSetPanelActive_ActivatesPlayBackPanel_WhenStateIsPlayBack()
    {
        // Arrange
        playBackPanel.SetActive(false);

        // Act
        playBackManager.SetPanelActive(State.PlayBack);

        // Assert
        Assert.IsTrue(playBackPanel.activeSelf, "playBackPanel should be active when state is PlayBack");
        Assert.IsTrue(playBackManager.IsPlayingBack, "isPlayingBack should be true when state is PlayBack");
    }

    [Test]
    public void TestSetPanelActive_StoresFinishTime_WhenStateIsFinish()
    {
        // Arrange
        timerText.text = "05:23";

        // Act
        playBackManager.SetPanelActive(State.Finish);

        // Assert
        Assert.AreEqual("05:23", Manager.Instance.FinishTime, "FinishTime should be stored when state is Finish");
    }


    [Test]
    public void TestUpdateTimerDisplay_FormatsTimeCorrectly()
    {
        // Arrange
        playBackManager.ElapsedTime = 123.45f; // 2 minutes and 3 seconds

        // Act
        playBackManager.UpdateTimerDisplay();

        // Assert
        Assert.AreEqual("02:03", timerText.text, "Timer display should correctly format minutes and seconds");
    }

    [Test]
    public void TestOnShowSolutionClicked_CallsPlaceAllComponentsGradually()
    {
        // Act
        playBackManager.OnShowSolutionClicked();

    }

    [UnityTest]
    public IEnumerator TestUpdate_UpdatesTimerWhenPlayingBack()
    {
        // Arrange
        playBackManager.SetPanelActive(State.PlayBack);

        // Act
        yield return null; // Wait for one frame

        // Assert
        Assert.Greater(playBackManager.ElapsedTime, 0, "Elapsed time should increase when isPlayingBack is true");
    }

    [Test]
    public void TestIncrementErrorCount_UpdatesErrorCountText()
    {
        // Act
        playBackManager.IncrementErrorCount(5);

        // Assert
        Assert.AreEqual("5", errorCountText.text, "Error count text should be updated correctly");
    }

    [Test]
    public void TestIncrementHintCount_UpdatesHintCountText()
    {
        // Act
        playBackManager.IncrementHintCount(3);

        // Assert
        Assert.AreEqual("3", hintCountText.text, "Hint count text should be updated correctly");
    }

    [Test]
    public void TestIncrementStepCount_UpdatesStepsText()
    {
        // Arrange
        Manager.Instance.AssemblySequence = new List<ComponentData> { new ComponentData(), new ComponentData(), new ComponentData() };

        // Act
        playBackManager.IncrementStepCount(2);

        // Assert
        Assert.AreEqual("2/3", stepsText.text, "Steps text should be updated correctly with the current and total steps");
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
