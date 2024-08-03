using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSequence : MonoBehaviour
{
    // A class to hold the data for each component
    [System.Serializable]
    public class ComponentData
    {
        public string componentName;
        public Vector3 position;
        public Quaternion rotation;
        public string toolName;
    }

    // A class to hold the data for all components
    [System.Serializable]
    public class ObjectData
    {
        public List<ComponentData> components = new List<ComponentData>();
    }

    private ObjectData objectData = new ObjectData();

    private string folderName = "SavedBuildData";
    private string directoryPath;
    private void Start()
    {
        directoryPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
    public void ModifyComponent(GameObject component)
    {
        // Gets the last data of the component
        ComponentData existingData = objectData.components.FindLast(data => data.componentName == component.name);

        if (existingData != null)
        {
            // Update existing component data
            existingData.position = component.transform.localPosition;
            existingData.rotation = component.transform.localRotation;
        }
        else
        {
            // Add new component data
            SaveComponent(component);
        }
    }

    // Method to save components
    public void SaveComponent(GameObject component)
    {
        string name = "null";
        Fastener fastener = component.GetComponent<Fastener>();
        if (fastener != null)
        {
            GameObject tool = fastener.getTool();
            if (tool != null)
            {
                name = tool.name;
            }

        }
        // Create new component data
        ComponentData newData = new ComponentData
        {
            componentName = component.name,
            position = component.transform.localPosition,
            rotation = component.transform.localRotation,
            toolName = name
        };
        objectData.components.Add(newData);
    }

    // Method to remove the last occurrence of component data with a given name
    public void RemoveComponent(GameObject component)
    {
        // Find the last occurrence of the component data with the given component name
        int index = objectData.components.FindLastIndex(data => data.componentName == component.name);

        // If found, remove it from the list
        if (index != -1)
        {
            objectData.components.RemoveAt(index);
        }
    }

    public void SaveSequenceToJSON(string name)
    {

        /* DA USARE PER LA BUILD
        string json = JsonUtility.ToJson(objectData, true);

        
        File.WriteAllText(path, json);

        Debug.Log($"Components and sequence saved to {path}");

        */
        // Serialize objectData to JSON

        string json = JsonUtility.ToJson(objectData, true);
        string path = Path.Combine(directoryPath, name + ".json");
        // Define the path to the custom folder within Assets
       
       

        // Save JSON to a file within the custom folder
        //string filePath = Path.Combine(folderPath, folderPath ,name + ".json");
        File.WriteAllText(path, json);


    }
}
