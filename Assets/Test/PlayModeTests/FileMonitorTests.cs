using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[TestFixture]
public class FileMonitorTests
{
    private IFileMonitor fileMonitor;
    private string testDirectoryPath;
    private string testFilePath1;
    private string testFilePath2;

    [SetUp]
    public void SetUp()
    {
        // Create a dedicated test directory
        testDirectoryPath = Path.Combine(Application.persistentDataPath, "TestDirectory");
        Directory.CreateDirectory(testDirectoryPath);

        // Create and configure the FileMonitor
        GameObject gameObject = new GameObject();
        fileMonitor = gameObject.AddComponent<FileMonitor>();
        fileMonitor.PersistentDataPath = testDirectoryPath;

        // Set up test file paths
        testFilePath1 = Path.Combine(testDirectoryPath, "file1.glb");
        testFilePath2 = Path.Combine(testDirectoryPath, "file2.glb");

        // Start monitoring
        fileMonitor.StartMonitoring();
    }

    [Test]
    public void OnFilesChanged_ShouldInvokeEvent()
    {
        // Arrange
        var expectedFiles = new HashSet<string>
        {
            Path.GetFullPath(testFilePath1).ToLowerInvariant(),
            Path.GetFullPath(testFilePath2).ToLowerInvariant()
        };

        fileMonitor.OnFilesChanged += files =>
        {
            var actualFiles = new HashSet<string>(files.Select(f => Path.GetFullPath(f).ToLowerInvariant()));
            Debug.Log($"Files Changed: {string.Join(", ", actualFiles)}");
            Assert.IsTrue(actualFiles.SetEquals(expectedFiles), "Files did not match expected values.");
        };

        // Simulate file creation
        File.WriteAllText(testFilePath1, "test");
        File.WriteAllText(testFilePath2, "test");

    }

    [TearDown]
    public void TearDown()
    {
        // Remove the test directory and its contents
        if (Directory.Exists(testDirectoryPath))
        {
            Directory.Delete(testDirectoryPath, true);
        }
    }
}
