using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FileCheckerTests
{
    private string testDirectory;
    private string testFileName;

    [SetUp]
    public void SetUp()
    {
        testDirectory = Path.Combine(Application.persistentDataPath, "TestDirectory");
        if (!Directory.Exists(testDirectory))
        {
            Directory.CreateDirectory(testDirectory);
        }

        testFileName = "TestFile";
    }

    [TearDown]
    public void TearDown()
    {
        string testFilePath = Path.Combine(testDirectory, testFileName + ".json");
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory);
        }
    }

    [Test]
    public void DoesJsonFileExist_FileExists_ReturnsTrue()
    {
        string filePath = Path.Combine(testDirectory, testFileName + ".json");
        File.WriteAllText(filePath, "{}");

        bool result = FileChecker.DoesJsonFileExist(testDirectory, testFileName);

        Assert.IsTrue(result, "File should exist.");
    }

    [Test]
    public void DoesJsonFileExist_FileDoesNotExist_ReturnsFalse()
    {
        bool result = FileChecker.DoesJsonFileExist(testDirectory, testFileName);

        Assert.IsFalse(result, "File should not exist.");
    }

    [UnityTest]
    public IEnumerator DoesJsonFileExist_EmptyFileName_ReturnsFalse()
    {
        LogAssert.Expect(LogType.Error, "Prefab name is null or empty.");

        bool result = FileChecker.DoesJsonFileExist(testDirectory, string.Empty);

        Assert.IsFalse(result, "File should not exist due to empty filename.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesJsonFileExist_NullFileName_ReturnsFalse()
    {
        LogAssert.Expect(LogType.Error, "Prefab name is null or empty.");

        bool result = FileChecker.DoesJsonFileExist(testDirectory, null);

        Assert.IsFalse(result, "File should not exist due to null filename.");

        yield return null;
    }

    [Test]
    public void DoesJsonFileExist_InvalidDirectory_ReturnsFalse()
    {
        bool result = FileChecker.DoesJsonFileExist("InvalidDirectory", testFileName);

        Assert.IsFalse(result, "File should not exist due to invalid directory.");
    }
}
