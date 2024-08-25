using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FileBrowserTests
{
    private FileBrowserManager fileBrowserManager;
    private GameObject fileBrowserCanvas;
    private string testDirectory;
    private string testFilePath;

    [SetUp]
    public void SetUp()
    {
        GameObject gameObject = new GameObject("FileBrowserManager");
        fileBrowserManager = gameObject.AddComponent<FileBrowserManager>();

        fileBrowserCanvas = new GameObject("FileBrowserCanvas");
        fileBrowserManager.GetType().GetField("fileBrowserCanvas", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fileBrowserManager, fileBrowserCanvas);

        testDirectory = Path.Combine(Application.persistentDataPath, "Models");
        if (!Directory.Exists(testDirectory))
        {
            Directory.CreateDirectory(testDirectory);
        }


        testFilePath = Path.Combine(Application.persistentDataPath, "test.glb");
        File.WriteAllText(testFilePath, "test content");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        if (File.Exists(Path.Combine(Application.persistentDataPath, "Models", "test.glb")))
            File.Delete(Path.Combine(Application.persistentDataPath, "Models", "test.glb"));

        if (Directory.Exists(Path.Combine(Application.persistentDataPath, "NewModels")))
        {
            Directory.Delete(Path.Combine(Application.persistentDataPath, "NewModels"), true);
        }
    }

    [UnityTest]
    public IEnumerator Start_FileBrowserCanvasIsHidden()
    {
        MethodInfo startMethod = fileBrowserManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(fileBrowserManager, null);

        yield return null;

        Assert.IsFalse(fileBrowserCanvas.activeSelf, "File browser canvas should be hidden at start.");
    }

    [UnityTest]
    public IEnumerator ShowDialog_FileSelection_CopiesFileToModelsDirectory()
    {
        MethodInfo startMethod = fileBrowserManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(fileBrowserManager, null);

        fileBrowserManager.ShowDialog("Models",".glb");


        string[] filePaths = new string[] { testFilePath };
        string destinationPath = Path.Combine(testDirectory, "test.glb");

        yield return new WaitForSeconds(1f);
        MethodInfo onFilesSelectedMethod = fileBrowserManager.GetType().GetMethod("OnFilesSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        onFilesSelectedMethod.Invoke(fileBrowserManager, new object[] { filePaths });

        Assert.IsTrue(fileBrowserCanvas.activeSelf, "File browser canvas should be active after ShowDialog method.");
        Assert.IsTrue(File.Exists(destinationPath), "File should be copied to the Models directory.");
        Assert.AreEqual(File.ReadAllText(testFilePath), File.ReadAllText(destinationPath), "File contents should match.");
    }

    [UnityTest]
    public IEnumerator OnFilesSelected_DirectoryCreation()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "NewModels");
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        FieldInfo directoryNameField = fileBrowserManager.GetType().GetField("directoryName", BindingFlags.NonPublic | BindingFlags.Instance);
        directoryNameField.SetValue(fileBrowserManager, "NewModels");

        MethodInfo onFilesSelectedMethod = fileBrowserManager.GetType().GetMethod("OnFilesSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        onFilesSelectedMethod.Invoke(fileBrowserManager, new object[] { new string[] { testFilePath } });

        yield return null;

        string modelsDirectory = Path.Combine(Application.persistentDataPath, "NewModels");
        Assert.IsTrue(Directory.Exists(modelsDirectory), "Models directory should be created.");
    }
}
