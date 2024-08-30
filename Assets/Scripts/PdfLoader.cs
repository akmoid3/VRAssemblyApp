using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PdfLoader : MonoBehaviour
{
    [SerializeField] private GameObject pdfPanel;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool overWrite = true;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private int dpi = 600;

    private bool canActivePanel = true;

    private List<Texture2D> pages = new List<Texture2D>();
    private int currentPageIndex = 0;
    private RawImage pageImage;

    public List<Texture2D> Pages { get => pages; set => pages = value; }

    // Start is called before the first frame update
    void Start()
    {
        nextPageButton.onClick.AddListener(NextPage);
        previousPageButton.onClick.AddListener(PreviousPage);

        StateManager.OnStateChanged += HandleStateChanged;

        HandleStateChanged(StateManager.Instance.CurrentState);
    }


    private void OnDestroy()
    {
        if(nextPageButton != null)
        nextPageButton.onClick.RemoveListener(NextPage);
        if (previousPageButton != null)
            previousPageButton.onClick.RemoveListener(PreviousPage);

        StateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(State newState)
    {
        bool shouldActivatePanel = (newState == State.Record || newState == State.PlayBack) && canActivePanel;
        pdfPanel.SetActive(shouldActivatePanel);
    }

    public virtual void LoadPDF(string fileName)
    {
        // Append ".pdf" extension if not already present
        fileName = fileName.EndsWith(".pdf") ? fileName : fileName + ".pdf";

        var inputDir = Path.Combine(Application.persistentDataPath, "Instructions");
        var inputPath = Path.Combine(inputDir, fileName);
        var outputPath = Path.Combine(inputDir, Path.GetFileNameWithoutExtension(inputPath));

        // Check if the PDF file exists
        if (!File.Exists(inputPath))
        {
            canActivePanel = false;

            return; // Exit the method if the file does not exist
        }
        if (!IsPdfAlreadyConverted(outputPath, fileName))
        {
            Directory.CreateDirectory(outputPath);
            RenderPdfToFile(inputPath, outputPath);
        }

        LoadFileToTextures(outputPath);


        // Instantiate the image prefab and set the initial page
        var pageObject = Instantiate(imagePrefab, spawnPoint, false);
        pageImage = pageObject.GetComponent<RawImage>();
        ShowPage(currentPageIndex);
    }

    private void RenderPdfToFile(string pdfFilename, string outputDir)
    {
        using (var doc = PdfiumViewer.PdfDocument.Load(pdfFilename))
        {               // Load PDF Document from file
            for (int page = 0; page < doc.PageCount; page++)
            {          // Loop through pages
                using (var img = doc.Render(page, dpi, dpi, false))
                {   // Render with dpi and with forPrinting false
                    var tempImagePath = Path.Combine(outputDir, $"{page}_temp.png");
                    img.Save(tempImagePath);     // Save rendered image to temporary path

                    var outputName = Path.Combine(outputDir, $"{page}.png");
                    if (overWrite && File.Exists(outputName))
                    {
                        File.Delete(outputName);
                    }
                    File.Move(tempImagePath, outputName);  // Move to final output name
                }
            }
        }
    }

    private void LoadFileToTextures(string path)
    {
        var dir = new DirectoryInfo(path);

        // Get all PNG files and sort them by the number in the file name
        var sortedFiles = dir.GetFiles("*.png")
                             .OrderBy(file => ExtractPageNumber(file.Name))
                             .ToList();

        foreach (var file in sortedFiles)
        {
            byte[] fileData = File.ReadAllBytes(file.FullName);
            Texture2D tex = new Texture2D(2, 2); // Create a texture (size doesn't matter)
            tex.LoadImage(fileData); // Load the image data
            pages.Add(tex);
        }
    }

    private int ExtractPageNumber(string filename)
    {
        // Extract the page number from the filename (assuming the format is "pageNumber.png")
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        int pageNumber;
        if (int.TryParse(fileNameWithoutExtension, out pageNumber))
        {
            return pageNumber;
        }
        return int.MaxValue; // Put files without a valid number at the end
    }

    private void ShowPage(int pageIndex)
    {
        if (pages.Count == 0 || pageIndex < 0 || pageIndex >= pages.Count)
        {
            Debug.LogWarning("Invalid page index or no pages loaded.");
            return;
        }

        if (pageImage)
            pageImage.texture = pages[pageIndex];
    }

    private void NextPage()
    {
        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            ShowPage(currentPageIndex);
        }
    }

    private void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowPage(currentPageIndex);
        }
    }

    private bool IsPdfAlreadyConverted(string outputDir, string filename)
    {
        if (Directory.Exists(outputDir))
        {
            var pngFiles = Directory.GetFiles(outputDir, "*.png");

            // Attempt to determine the number of pages from an existing output
            using (var doc = PdfiumViewer.PdfDocument.Load(Path.Combine(Application.persistentDataPath, "Instructions", filename)))
            {
                return pngFiles.Length == doc.PageCount;
            }
        }

        return false;
    }
}
