using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Moq;
using System.Collections;
using UnityEngine.TestTools;
using System.Drawing;

public class PdfLoaderTests
{
    private PdfLoader _pdfLoader;
    private GameObject _pdfPanel;
    private GameObject _imagePrefab;
    private Transform _spawnPoint;
    private Button _nextPageButton;
    private Button _previousPageButton;
    private StateManager stateManager;

    [SetUp]
    public void SetUp()
    {
        var gameObject = new GameObject();
        _pdfLoader = gameObject.AddComponent<PdfLoader>();

        _pdfPanel = new GameObject("PdfPanel");
        _imagePrefab = new GameObject("ImagePrefab");
        _imagePrefab.AddComponent<RawImage>();
        _spawnPoint = new GameObject("SpawnPoint").transform;
        _nextPageButton = new GameObject("NextPageButton").AddComponent<Button>();
        _previousPageButton = new GameObject("PreviousPageButton").AddComponent<Button>();

        _pdfLoader.GetType().GetField("pdfPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, _pdfPanel);
        _pdfLoader.GetType().GetField("imagePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, _imagePrefab);
        _pdfLoader.GetType().GetField("spawnPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, _spawnPoint);
        _pdfLoader.GetType().GetField("nextPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, _nextPageButton);
        _pdfLoader.GetType().GetField("previousPageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, _previousPageButton);

        stateManager = new GameObject().AddComponent<StateManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Distruggi i gameObject dopo ogni test
        Object.DestroyImmediate(_pdfLoader.gameObject);
        Object.DestroyImmediate(_pdfPanel);
        Object.DestroyImmediate(_imagePrefab);
        Object.DestroyImmediate(_spawnPoint.gameObject);
        Object.DestroyImmediate(_nextPageButton.gameObject);
        Object.DestroyImmediate(_previousPageButton.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);
    }

    [Test]
    public void LoadPDF_FileDoesNotExist_CanActivePanelIsFalse()
    {
        string invalidFileName = "nonexistent.pdf";

        _pdfLoader.LoadPDF(invalidFileName);

        var canActivePanel = (bool)_pdfLoader.GetType().GetField("canActivePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);
        Assert.IsFalse(canActivePanel);
    }

    [Test]
    public void LoadPDF_ValidFileFromResources_CanActivePanelIsTrue()
    {
        string pdfFileName = "test.pdf";
        int expectedPageCount = 8;

        string outputDir = Path.Combine(Application.persistentDataPath, "Instructions", "test");
        string inputDir = Path.Combine(Application.persistentDataPath, "Instructions", pdfFileName);

        Directory.CreateDirectory(outputDir);


        _pdfLoader.LoadPDF(pdfFileName);
        var canActivePanel = (bool)_pdfLoader.GetType().GetField("canActivePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);
        Assert.IsTrue(canActivePanel);

        _pdfLoader.GetType().GetMethod("RenderPdfToFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 .Invoke(_pdfLoader, new object[] { inputDir, outputDir });

        // Assert
        for (int i = 0; i < expectedPageCount; i++)
        {
            string expectedFilePath = Path.Combine(outputDir, $"{i}.png");
            Assert.IsTrue(File.Exists(expectedFilePath), $"File {expectedFilePath} should have been created.");
        }
    }

    [UnityTest]
    public IEnumerator NextPage_UpdatesCurrentPageIndex_CreatesCorrectNumberOfImages()
    {
        var page1 = new Texture2D(2, 2);
        var page2 = new Texture2D(2, 2);
        var pages = new List<Texture2D> { page1, page2 };

        _pdfLoader.Pages = pages;

        _pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, 0);

        CollectionAssert.AreEqual(pages, _pdfLoader.Pages);

        _nextPageButton.onClick.Invoke(); // Simula il click sul pulsante "Next Page"

        yield return null;

        var currentPageIndex = (int)_pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);

        Assert.AreEqual(1, currentPageIndex);
    }


    [UnityTest]
    public IEnumerator PreviousPage_UpdatesCurrentPageIndex()
    {
        var page1 = new Texture2D(2, 2);
        var page2 = new Texture2D(2, 2);
        var pages = new List<Texture2D> { page1, page2 };

        _pdfLoader.Pages = pages;

        _pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, 1);

        CollectionAssert.AreEqual(pages, _pdfLoader.Pages);

        _previousPageButton.onClick.Invoke(); // Simula il click sul pulsante "Next Page"

        yield return null;
        var currentPageIndex = (int)_pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);

        // Assicurati che l'indice della pagina sia aggiornato
        Assert.AreEqual(0, currentPageIndex);
    }

    [Test]
    public void NextPage_UpdatesCurrentPageIndex_MethodInvoke()
    {
        var page1 = new Texture2D(2, 2);
        var page2 = new Texture2D(2, 2);
        var pages = new List<Texture2D> { page1, page2 };

        _pdfLoader.Pages = pages;

        _pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, 1);

        CollectionAssert.AreEqual(pages, _pdfLoader.Pages);

        var nextPageMethod = _pdfLoader.GetType().GetMethod("NextPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nextPageMethod.Invoke(_pdfLoader, null);


        var currentPageIndex = (int)_pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);

        Assert.AreEqual(1, currentPageIndex);
    }

    [Test]
    public void PreviousPage_UpdatesCurrentPageIndex_MethodInvoke()
    {
        var page1 = new Texture2D(2, 2);
        var page2 = new Texture2D(2, 2);
        var pages = new List<Texture2D> { page1, page2 };

        _pdfLoader.Pages = pages;

        _pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_pdfLoader, 1);

        CollectionAssert.AreEqual(pages, _pdfLoader.Pages);

        var previousPageMethod = _pdfLoader.GetType().GetMethod("PreviousPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        previousPageMethod.Invoke(_pdfLoader, null);


        var currentPageIndex = (int)_pdfLoader.GetType().GetField("currentPageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdfLoader);

        Assert.AreEqual(0, currentPageIndex);
    }

    [Test]
    public void ShowPage_InvalidIndex_ShouldLogWarning()
    {
    
        // Act: Invoke ShowPage with invalid index
        var showPageMethod = _pdfLoader.GetType().GetMethod("ShowPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Invalid index: no pages loaded (index 0)
        LogAssert.Expect(LogType.Warning, "Invalid page index or no pages loaded.");
        showPageMethod.Invoke(_pdfLoader, new object[] { 0 });

        // Invalid index: negative index
        LogAssert.Expect(LogType.Warning, "Invalid page index or no pages loaded.");
        showPageMethod.Invoke(_pdfLoader, new object[] { -1 });

        // Invalid index: index greater than pages count
        LogAssert.Expect(LogType.Warning, "Invalid page index or no pages loaded.");
        showPageMethod.Invoke(_pdfLoader, new object[] { 2 });  // Index 2 is out of bounds
    }



}
