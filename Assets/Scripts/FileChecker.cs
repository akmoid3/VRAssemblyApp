using System.IO;
using UnityEngine;

public static class FileChecker
{
    // Method to check if a JSON file exists
    public static bool DoesJsonFileExist(string directory,string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("Prefab name is null or empty.");
            return false;
        }

        string filePath = Path.Combine(directory, prefabName + ".json");

        return File.Exists(filePath);
    }
}
