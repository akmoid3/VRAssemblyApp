using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

// A class to hold the data for each component
[System.Serializable]
public class ComponentData
{
    public int stepId;
    public string componentName;
    public Vector3 position;
    public Quaternion rotation;
    public string toolName;
    public int toolForce;
    public string group;
    public ComponentObject.ComponentType type;
    public int pdfIndex;
}

// A class to hold the data for all components
[System.Serializable]
public class ObjectData
{
    public List<ComponentData> components = new List<ComponentData>();
}

public class SaveSequence : MonoBehaviour
{
    private ObjectData objectData = new ObjectData();

    private string folderName = "SavedBuildData";
    private string directoryPath;
    private Dictionary<GameObject, int> componentIdMap = new Dictionary<GameObject, int>();
    private static int stepCounter = 0;

    public ObjectData ObjectData { get => objectData; set => objectData = value; }

    private void Start()
    {
        directoryPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
    public virtual void ModifyComponent(GameObject component)
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
    public virtual void SaveComponent(GameObject component)
    {
        string toolName = "null";
        int force = 0;

        Fastener fastener = component.GetComponent<Fastener>();
        if (fastener != null)
        {
            Tool tool = fastener.getTool();
            if (tool != null)
            {
                toolName = tool.ToolName;
                DynamometerScrewDriver dyn = tool as DynamometerScrewDriver;
                if(dyn != null)
                {
                    force = dyn.Force;
                }
            }
        }

        ComponentObject componentObject = component.GetComponent<ComponentObject>();
        if (componentObject == null)
            return;

        // Check if the GameObject already has an assigned stepId in the dictionary
        if (!componentIdMap.TryGetValue(component, out int stepId))
        {
            // If not, assign a new unique ID and store it in the dictionary
            stepId = stepCounter++;
            componentIdMap[component] = stepId;
        }

        int index = -1;
        if (Manager.Instance.LoaderPDF.CanActivePanel)
        {
            index = Manager.Instance.LoaderPDF.CurrentPageIndex;
        }
            
        // Create new component data using the determined stepId
        ComponentData newData = new ComponentData
        {
            stepId = stepId,
            componentName = component.name,
            position = component.transform.localPosition,
            rotation = component.transform.localRotation,
            toolName = toolName,
            toolForce = force,
            group = componentObject.GetGroup(),
            type = componentObject.GetComponentType(),
            pdfIndex = index
        };

        objectData.components.Add(newData);
    }

    // Method to remove the last occurrence of component data with a given name
    public virtual void RemoveComponent(GameObject component)
    {
        // Find the last occurrence of the component data with the given component name
        int index = objectData.components.FindLastIndex(data => data.componentName == component.name);

        // If found, remove it from the list
        if (index != -1)
        {
            objectData.components.RemoveAt(index);
        }
    }

    public virtual void SaveSequenceToJSON(string name)
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
