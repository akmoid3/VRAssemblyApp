using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


[System.Serializable]
public class ComponentTypeData
{
    public string componentName;
    public ComponentObject.ComponentType componentType;
}

public class JsonData
{
    public List<ComponentTypeData> components = new List<ComponentTypeData>();
}

public class InitializedDataManager : MonoBehaviour
{
    [SerializeField] Manager manager;
    [SerializeField] InitializeComponentManager initializeComponentManager;
    [SerializeField] GameObject model;
    [SerializeField] ComponentPositioner componentPositioner;

    private string directoryPath;
    //[SerializeField] private List<ComponentTypeData> components = new List<ComponentTypeData>();
    void Start()
    {
        directoryPath = Path.Combine(Application.persistentDataPath, "InitializedModels");
        if(!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void CanInitialize()
    {
       bool exist = FileChecker.DoesJsonFileExist(directoryPath, manager.Model.name);
       manager.IsInitializing = !exist;
    }

    public void SaveComponentsData()
    {
        model = componentPositioner.GetParentModel();
        if (model == null) return;

        string fileName = manager.Model.name + ".json";
        if (string.IsNullOrEmpty(fileName)) return;

        string filePath = Path.Combine(directoryPath, fileName);

        JsonData data = new JsonData();
        // Iterate through the children and save their data
        foreach (Transform child in model.transform)
        {
            ComponentObject componentObject = child.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                ComponentTypeData componentData = new ComponentTypeData
                {
                    componentName = child.name,
                    componentType = componentObject.GetComponentType()
                };

                data.components.Add(componentData);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }


    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            
        }
        return null;
    }

}
