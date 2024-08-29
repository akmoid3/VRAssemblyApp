using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FileBrowserTests
{
    private FileBrowserManager fileBrowserManager;
    private GameObject fileBrowserCanvas;
    private string testDirectory;
    private string testFilePath;
    private Manager mockManager;
    private StateManager stateManager;

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

        // Set up a mock for Manager.Instance.Model.name
        mockManager = new GameObject().AddComponent<Manager>();
        mockManager.Model = new GameObject("modello");

        stateManager = new GameObject().AddComponent<StateManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(mockManager);
        Object.DestroyImmediate(stateManager);

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

    [Test]
    public void Start_FileBrowserCanvasIsHidden()
    {
        MethodInfo startMethod = fileBrowserManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(fileBrowserManager, null);


        Assert.IsFalse(fileBrowserCanvas.activeSelf, "File browser canvas should be hidden at start.");
    }

    [Test]
    public void ShowDialog_FileSelection_CopiesFileToModelsDirectory()
    {
        // Manually invoke Start to initialize the file browser manager
        MethodInfo startMethod = fileBrowserManager.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        startMethod.Invoke(fileBrowserManager, null);

        // Simulate showing the dialog
        string directoryName = "Models";
        fileBrowserManager.ShowDialog(directoryName, ".glb");

        // Simulate file selection
        string[] filePaths = new string[] { testFilePath };
        string destinationPath = Path.Combine(testDirectory, "test.glb");


        // Invoke OnFilesSelected using reflection, passing the correct parameters
        MethodInfo onFilesSelectedMethod = fileBrowserManager.GetType().GetMethod("OnFilesSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(onFilesSelectedMethod, "OnFilesSelected method not found.");
        onFilesSelectedMethod.Invoke(fileBrowserManager, new object[] { filePaths, directoryName });

        // Validate that the file was copied
        Assert.IsTrue(fileBrowserCanvas.activeSelf, "File browser canvas should be active after ShowDialog method.");
        Assert.IsTrue(File.Exists(destinationPath), "File should be copied to the Models directory.");
        Assert.AreEqual(File.ReadAllText(testFilePath), File.ReadAllText(destinationPath), "File contents should match.");
    }


    [Test]
    public void OnFilesSelected_DirectoryCreation()
    {
        string directoryName = "NewModels";
        string directoryPath = Path.Combine(Application.persistentDataPath, directoryName);

        // Ensure the directory does not exist before the test
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        // Manually invoke ShowDialog to simulate the user opening the dialog
        fileBrowserManager.ShowDialog(directoryName, ".glb");

        // Simulate file selection
        string[] filePaths = new string[] { testFilePath };
        MethodInfo onFilesSelectedMethod = fileBrowserManager.GetType().GetMethod("OnFilesSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(onFilesSelectedMethod, "OnFilesSelected method not found.");
        onFilesSelectedMethod.Invoke(fileBrowserManager, new object[] { filePaths, directoryName });


        // Verify that the directory was created
        Assert.IsTrue(Directory.Exists(directoryPath), "Models directory should be created.");
    }

    [Test]
    public void OnFilesSelected_CopiesAndRenamesPdfFile()
    {
        // Arrange
        string directoryName = "PDFTest";
        string pdfFileName = "test.pdf";
        string pdfFilePath = Path.Combine(Application.persistentDataPath, pdfFileName);


        // Make sure the destination directory does not exist before the test
        string directoryPath = Path.Combine(Application.persistentDataPath, directoryName);
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }


        // Act
        string[] filePaths = new string[] { pdfFilePath };
        MethodInfo onFilesSelectedMethod = fileBrowserManager.GetType().GetMethod("OnFilesSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(onFilesSelectedMethod, "OnFilesSelected method not found.");
        onFilesSelectedMethod.Invoke(fileBrowserManager, new object[] { filePaths, directoryName });



        var finalPath = Path.Combine(directoryPath, mockManager.Model.name + ".pdf");

        // Check if the original PDF was copied and renamed correctly
        Assert.IsTrue(File.Exists(finalPath), "PDF file should be renamed and moved to the Models directory." + finalPath);
        Assert.AreEqual(File.ReadAllText(pdfFilePath), File.ReadAllText(finalPath), "PDF file contents should match after renaming.");

        
        if (File.Exists(directoryPath + "modello.pdf"))
        {
            File.Delete(directoryPath + "modello.pdf");
        }
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }


    }

}
