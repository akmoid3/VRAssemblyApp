using System.Collections;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class DropDownManagerTests
{
    private GameObject testObject;
    private DropDownManager dropDownManager;
    private TMP_Dropdown dropdown;
    private PrefabManager prefabManager;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject and add the DropDownManager component
        testObject = new GameObject();
        dropDownManager = testObject.AddComponent<DropDownManager>();

        // Create and set up the TMP_Dropdown
        dropdown = testObject.AddComponent<TMP_Dropdown>();
        dropDownManager.GetType()
                       .GetField("dropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .SetValue(dropDownManager, dropdown);

        // Create and set up the PrefabManager
        GameObject prefabManagerObject = new GameObject();
        prefabManager = prefabManagerObject.AddComponent<PrefabManager>();

        // Mock the FileMonitor with a valid PersistentDataPath
        var mockFileMonitor = new Mock<IFileMonitor>();
        mockFileMonitor.Setup(fm => fm.PersistentDataPath).Returns(Application.persistentDataPath);

        // Set the mockFileMonitor into the PrefabManager
        prefabManager.GetType()
                     .GetField("fileMonitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(prefabManager, mockFileMonitor.Object);

        // Inject the PrefabManager into the DropDownManager
        dropDownManager.GetType()
                       .GetField("prefabManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .SetValue(dropDownManager, prefabManager);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(testObject);
        Object.Destroy(prefabManager.gameObject);
    }

    [Test]
    public void LoadModelsIntoDropdown_ShouldPopulateDropdownOptions()
    {
        // Arrange
        List<string> modelNames = new List<string> { "Model1", "Model2", "Model3" };

        // Act
        dropDownManager.GetType()
                       .GetMethod("LoadModelsIntoDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .Invoke(dropDownManager, new object[] { modelNames });

        // Assert
        Assert.AreEqual(3, dropdown.options.Count);
        Assert.AreEqual("Model1", dropdown.options[0].text);
        Assert.AreEqual("Model2", dropdown.options[1].text);
        Assert.AreEqual("Model3", dropdown.options[2].text);

    }


}
