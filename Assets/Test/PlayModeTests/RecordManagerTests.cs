using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class RecordManagerTests
{
    private RecordManager recordManager;
    private GameObject recordPanel;
    private StateManager stateManager;
    private Button button;



    [SetUp]
    public void SetUp()
    {
        button = new GameObject().AddComponent<Button>();
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a new GameObject and attach the RecordManager component
        var gameObject = new GameObject();
        recordManager = gameObject.AddComponent<RecordManager>();

        // Create a new GameObject for the recordPanel and assign it to the RecordManager
        recordPanel = new GameObject();
        recordManager.GetType().GetField("recordPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(recordManager, recordPanel);
        recordManager.GetType().GetField("finishButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(recordManager, button);

    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(recordManager.gameObject);
        Object.DestroyImmediate(recordPanel);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void TestSetPanelActive_ActivatesPanelWhenStateIsRecord()
    {
        // Arrange
        recordPanel.SetActive(false); // Ensure it's initially inactive

        // Act
        recordManager.SetPanelActive(State.Record);

        // Assert
        Assert.IsTrue(recordPanel.activeSelf, "recordPanel should be active when state is Record");
    }

    [Test]
    public void TestSetPanelActive_DeactivatesPanelWhenStateIsNotRecord()
    {
        // Arrange
        recordPanel.SetActive(true); // Ensure it's initially active

        // Act
        recordManager.SetPanelActive(State.PlayBack);


        // Assert
        Assert.IsFalse(recordPanel.activeSelf, "recordPanel should be inactive when state is not Record");
    }
}
