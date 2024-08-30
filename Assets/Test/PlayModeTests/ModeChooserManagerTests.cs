using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.IO;

public class ModeChooserManagerTests
{
    private ModeChooserManager modeChooserManager;
    private GameObject modeSelectionPanel;
    private Button playBackButton;
    private Button recordButton;
    private Button initializeButton;
    private Button returnBackButton;
    private Manager manager;
    private StateManager stateManager;


    [SetUp]
    public void SetUp()
    {
        manager = new GameObject().AddComponent<Manager>();
        stateManager = new GameObject().AddComponent<StateManager>();

        // Create a new GameObject and attach the ModeChooserManager component
        var gameObject = new GameObject();
        modeChooserManager = gameObject.AddComponent<ModeChooserManager>();

        // Create UI elements and assign them to the ModeChooserManager
        modeSelectionPanel = new GameObject();
        modeChooserManager.GetType().GetField("modeSelectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modeChooserManager, modeSelectionPanel);

        playBackButton = new GameObject().AddComponent<Button>();
        modeChooserManager.GetType().GetField("playBackButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modeChooserManager, playBackButton);

        recordButton = new GameObject().AddComponent<Button>();
        modeChooserManager.GetType().GetField("recordButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modeChooserManager, recordButton);

        initializeButton = new GameObject().AddComponent<Button>();
        modeChooserManager.GetType().GetField("initializeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modeChooserManager, initializeButton);

        returnBackButton = new GameObject().AddComponent<Button>();
        modeChooserManager.GetType().GetField("returnBackButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modeChooserManager, returnBackButton);


        // Set up Manager's Model mock
        var modelGameObject = new GameObject("MockModel");
        Manager.Instance.GetType().GetProperty("Model").SetValue(Manager.Instance, modelGameObject);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(modeChooserManager.gameObject);
        Object.DestroyImmediate(modeSelectionPanel);
        Object.DestroyImmediate(playBackButton.gameObject);
        Object.DestroyImmediate(recordButton.gameObject);
        Object.DestroyImmediate(initializeButton.gameObject);
        Object.DestroyImmediate(returnBackButton.gameObject);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void TestSetPanelActive_ActivatesModeSelectionPanel_WhenStateIsSelectingMode()
    {
        // Arrange
        modeSelectionPanel.SetActive(false);

        // Act
        modeChooserManager.SetPanelActive(State.SelectingMode);

        // Assert
        Assert.IsTrue(modeSelectionPanel.activeSelf, "modeSelectionPanel should be active when state is SelectingMode");
    }

    [Test]
    public void TestUpdateButtonStates_SetsButtonsInteractableBasedOnFileExistence()
    {
        // Arrange
        var initializedModelsPath = Path.Combine(Application.persistentDataPath, "InitializedModels", "MockModel.json");
        var savedBuildDataPath = Path.Combine(Application.persistentDataPath, "SavedBuildData", "MockModel.json");

        // Create fake file for Initialized Models
        File.Create(initializedModelsPath).Dispose();

        // Act
        modeChooserManager.UpdateButtonStates();

        // Assert
        Assert.IsTrue(recordButton.interactable, "recordButton should be interactable if InitializedModels file exists.");
        Assert.IsFalse(playBackButton.interactable, "playBackButton should not be interactable if SavedBuildData file does not exist.");

        // Clean up
        if (File.Exists(initializedModelsPath))
            File.Delete(initializedModelsPath);
    }

    [Test]
    public void TestOnInitializeClicked_UpdatesStateToInitialize()
    {
        // Act
        modeChooserManager.OnInitializeClicked();

        // Assert
        Assert.AreEqual(State.Initialize, StateManager.Instance.CurrentState);
    }

    [Test]
    public void TestOnPlayBackButtonClicked_UpdatesStateToPlayBack()
    {
        LogAssert.Expect(LogType.Error, "Interactor is not assigned.");
        // Act
        modeChooserManager.OnPlayBackButtonClicked();

        // Assert
        Assert.AreEqual(State.PlayBack, StateManager.Instance.CurrentState);
    }

    [Test]
    public void TestOnRecordButtonClicked_UpdatesStateToRecord()
    {
        // Act
        modeChooserManager.OnRecordButtonClicked();

        // Assert
        Assert.AreEqual(State.Record, StateManager.Instance.CurrentState);
    }

  
}
