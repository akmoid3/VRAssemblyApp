using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IFileMonitor
{
    string PersistentDataPath { get; set; }

    event Action<HashSet<string>> OnFilesChanged;

    void StartMonitoring();
}

public class FileMonitor : MonoBehaviour, IFileMonitor
{
    public event Action<HashSet<string>> OnFilesChanged; // Implementazione dell'evento

    private string directoryName = "Models";
    private string persistentDataPath;
    private HashSet<string> currentFiles = new HashSet<string>();

    private const string ModelFileExtension = "*.glb";
    private const float FileCheckInterval = 0.5f;

    public string PersistentDataPath { get => persistentDataPath; set => persistentDataPath = value; }

    private void Awake()
    {
        InitializePersistentDataPath();
        EnsureModelDirectoryExists();
        StartMonitoring();
    }

    public void StartMonitoring()
    {
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
            ForceCheckForFileChanges();

            yield return new WaitForSeconds(FileCheckInterval);
        }
    }

    public void ForceCheckForFileChanges()
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(persistentDataPath, ModelFileExtension);

        }
        catch (Exception ex)
        {
            return;
        }
        if (files == null)
            return;
        HashSet<string> newFiles = new HashSet<string>(files);

        if (!newFiles.SetEquals(currentFiles))
        {
            currentFiles = newFiles;
            OnFilesChanged?.Invoke(newFiles);
        }
    }
}
