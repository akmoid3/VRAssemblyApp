using SimpleFileBrowser;
using System;
using System.IO;
using UnityEngine;

public class FileBrowserManager : MonoBehaviour
{
    [SerializeField] private GameObject fileBrowserCanvas;
    void Start()
    {
        fileBrowserCanvas.SetActive(false);
    }

    public void ShowDialog(string dirName, string extension)
    {
        fileBrowserCanvas.SetActive(true);

        // Set filters
        SimpleFileBrowser.FileBrowser.SetFilters(false, new SimpleFileBrowser.FileBrowser.Filter("Filter", extension));

        // Set default filter that is selected when the dialog is shown 
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(extension);

        // Set excluded file extensions 
        SimpleFileBrowser.FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser 
        SimpleFileBrowser.FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        SimpleFileBrowser.FileBrowser.SingleClickMode = true;
        FileBrowser.ShowLoadDialog((paths) => { OnFilesSelected(paths, dirName); }, () => { }, FileBrowser.PickMode.Files, false, null, null, "Select Files", "Select");
    }


    void OnFilesSelected(string[] filePaths, string dirName)
    {
        string filePath = filePaths[0];

        // Destination directory
        string modelsDirectory = Path.Combine(Application.persistentDataPath, dirName);

        // Ensure the destination directory exists
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        // Generate the destination file path
        string destinationPath = Path.Combine(modelsDirectory, FileBrowserHelpers.GetFilename(filePath));

        // Check if the destination file already exists
        if (File.Exists(destinationPath))
        {
            // Delete the existing file to replace it
            File.Delete(destinationPath);
        }

        // Manually copy the file to the destination
        File.Copy(filePath, destinationPath);

        // Check if the copied file is a PDF
        if (Path.GetExtension(filePath).ToLower() == ".pdf")
        {
            // Generate a new file name for the PDF
            string newFileName = Manager.Instance.Model.name.ToString() + ".pdf"; // Replace with your desired new file name
            string newDestinationPath = Path.Combine(modelsDirectory, newFileName);

            // Check if the new destination path already exists
            if (File.Exists(newDestinationPath))
            {
                // Delete the existing file to replace it
                File.Delete(newDestinationPath);
            }

            // Move the copied file to the new file name
            File.Move(destinationPath, newDestinationPath);

        }
    }


}
