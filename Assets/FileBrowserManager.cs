using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;

public class FileBrowserManager : MonoBehaviour
{
    [SerializeField] private GameObject fileBrowserCanvas;
    [SerializeField] private string directoryName = "Models";
    void Start()
    {
        fileBrowserCanvas.SetActive(false);

    }

    public void showDialog()
    {
        fileBrowserCanvas.SetActive(true);

        // Set filters
        SimpleFileBrowser.FileBrowser.SetFilters(true, new SimpleFileBrowser.FileBrowser.Filter("Models", ".glb"));

        // Set default filter that is selected when the dialog is shown 
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(".glb");

        // Set excluded file extensions 
        SimpleFileBrowser.FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser 
        SimpleFileBrowser.FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        SimpleFileBrowser.FileBrowser.SingleClickMode = true;
        FileBrowser.ShowLoadDialog((paths) => { OnFilesSelected(paths); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Files, false, null, null, "Select Files", "Select");
    }


    void OnFilesSelected(string[] filePaths)
    {
        for (int i = 0; i < filePaths.Length; i++)
        {
            Debug.Log(filePaths[i]);
        }

        string filePath = filePaths[0];

        string modelsDirectory = Path.Combine(Application.persistentDataPath, directoryName);

        // Check if the Models directory exists, if not, create it
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        string destinationPath = Path.Combine(modelsDirectory, FileBrowserHelpers.GetFilename(filePath));
        FileBrowserHelpers.CopyFile(filePath, destinationPath);
    }
}
