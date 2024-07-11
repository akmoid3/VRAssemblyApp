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
    }

    // A class to hold the data for all components
    [System.Serializable]
    public class ObjectData
    {
        public List<ComponentData> components = new List<ComponentData>();
    }

    private ObjectData objectData = new ObjectData();

    public void SaveComponentOrModify(GameObject component)
    {
        // Check if the component already exists in the list
        ComponentData existingData = objectData.components.Find(data => data.componentName == component.name);

        if (existingData != null)
        {
            // Update existing component data
            existingData.position = component.transform.localPosition;
            existingData.rotation = component.transform.localRotation;
        }
        else
        {
            // Add new component data
            ComponentData newData = new ComponentData
            {
                componentName = component.name,
                position = component.transform.localPosition,
                rotation = component.transform.localRotation,
            };
            objectData.components.Add(newData);
        }
    }

    public void SaveSequenceToJSON(string name)
    {

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
        string filePath = Path.Combine(folderPath, name + ".json");
        File.WriteAllText(filePath, json);

        // Refresh the Asset Database to show changes in Unity Editor
        UnityEditor.AssetDatabase.Refresh();


    }
}
