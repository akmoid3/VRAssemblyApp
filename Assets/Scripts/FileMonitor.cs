using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileMonitor : MonoBehaviour
{
    public delegate void FilesChangedHandler(HashSet<string> newFiles);
    public event FilesChangedHandler OnFilesChanged;

    private string directoryName = "Models";
    private string persistentDataPath;
    private HashSet<string> currentFiles = new HashSet<string>();

    private const string ModelFileExtension = "*.glb";
    private const float FileCheckInterval = 0.5f;

    public string PersistentDataPath { get => persistentDataPath; private set => persistentDataPath = value; }

    private void Awake()
    {
        InitializePersistentDataPath();
        EnsureModelDirectoryExists();
        StartCoroutine(CheckForFileChanges());
    }


    private void InitializePersistentDataPath()
    {
        persistentDataPath = Path.Combine(Application.persistentDataPath, directoryName);
    }

    private void EnsureModelDirectoryExists()
    {
        if (!Directory.Exists(persistentDataPath))
        {
            Directory.CreateDirectory(persistentDataPath);
        }
    }

    private IEnumerator CheckForFileChanges()
    {
        while (true)
        {
            string[] files = Directory.GetFiles(persistentDataPath, ModelFileExtension);
            HashSet<string> newFiles = new HashSet<string>(files);

            if (!newFiles.SetEquals(currentFiles))
            {
                currentFiles = newFiles;
                OnFilesChanged?.Invoke(newFiles);
            }

            yield return new WaitForSeconds(FileCheckInterval);
        }
    }
}
