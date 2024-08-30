using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

public class ModelSelectionManagerTests
{
    private ModelSelectionManager modelSelectionManager;
    private GameObject modelSelectionPanel;
    private Button confirmButton;
    private Button loadModel;
    private FileBrowserManager fileBrowserManager;
    private StateManager stateManager;


    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a new GameObject and attach the ModelSelectionManager component
        var gameObject = new GameObject();
        modelSelectionManager = gameObject.AddComponent<ModelSelectionManager>();

        // Create UI elements and assign them to the ModelSelectionManager
        modelSelectionPanel = new GameObject();
        modelSelectionManager.GetType().GetField("modelSelectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modelSelectionManager, modelSelectionPanel);

        confirmButton = new GameObject().AddComponent<Button>();
        modelSelectionManager.GetType().GetField("confirmButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modelSelectionManager, confirmButton);

        loadModel = new GameObject().AddComponent<Button>();
        modelSelectionManager.GetType().GetField("loadModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modelSelectionManager, loadModel);

        fileBrowserManager = new GameObject().AddComponent<FileBrowserManager>();
        modelSelectionManager.GetType().GetField("fileBrowserManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(modelSelectionManager, fileBrowserManager);

        fileBrowserManager.GetType().GetField("fileBrowserCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(fileBrowserManager, new GameObject());
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(modelSelectionManager.gameObject);
        Object.DestroyImmediate(modelSelectionPanel);
        Object.DestroyImmediate(confirmButton.gameObject);
        Object.DestroyImmediate(loadModel.gameObject);
        Object.DestroyImmediate(fileBrowserManager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);

    }

    [Test]
    public void TestSetPanelActive_ActivatesModelSelectionPanel_WhenStateIsChoosingModel()
    {
        // Arrange
        modelSelectionPanel.SetActive(false);

        // Act
        modelSelectionManager.SetPanelActive(State.ChoosingModel);

        // Assert
        Assert.IsTrue(modelSelectionPanel.activeSelf, "modelSelectionPanel should be active when state is ChoosingModel");
    }

    [Test]
    public void TestOnConfirmButtonClicked_UpdatesStateToSelectingMode()
    {
        // Act
        modelSelectionManager.OnConfirmButtonClicked();

        // Assert
        Assert.AreEqual(State.SelectingMode, StateManager.Instance.CurrentState);
    }

    [Test]
    public void TestShowFileBrowser_CallsShowDialog()
    {
        // Act
        modelSelectionManager.ShowFileBrowser();

    }
}