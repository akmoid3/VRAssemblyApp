using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveSequence : MonoBehaviour
{
    // A class to hold the data for each component
    [System.Serializable]
    public class ComponentData
    {
        public string componentName;
        public Vector3 position;
        public Quaternion rotation;
        public int sequence;
    }

    // A class to hold the data for all components
    [System.Serializable]
    public class ObjectData
    {
        public List<ComponentData> components = new List<ComponentData>();
    }

    private ObjectData objectData = new ObjectData();

    public void SaveComponentsSequence(List<GameObject> selectedObjects)
    {
        objectData.components.Clear();

        // Iterate through each selected object and save its data
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            GameObject obj = selectedObjects[i];
            ComponentData data = new ComponentData
            {
                componentName = obj.name,
                position = obj.transform.localPosition,
                rotation = obj.transform.localRotation,
                sequence = i // Use the index as sequence number
            };
            objectData.components.Add(data);
        }
        /* DA USARE PER LA BUILD
        string json = JsonUtility.ToJson(objectData, true);

        string path = Path.Combine(Application.persistentDataPath, "componentsData.json");
        File.WriteAllText(path, json);

        Debug.Log($"Components and sequence saved to {path}");

        */
        // Serialize objectData to JSON
        string json = JsonUtility.ToJson(objectData, true);

        // Define the path to the custom folder within Assets
        string folderPath = "Assets/SavedData";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Save JSON to a file within the custom folder
        string filePath = Path.Combine(folderPath, "componentsData.json");
        File.WriteAllText(filePath, json);

        // Refresh the Asset Database to show changes in Unity Editor
        UnityEditor.AssetDatabase.Refresh();

        Debug.Log($"Components and sequence saved to {filePath}");
    }
}
