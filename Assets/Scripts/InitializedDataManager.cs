using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ComponentTypeData
{
    public string componentName;
    public ComponentObject.ComponentType componentType;
    public string componentGroup;
    public Vector3 selectedAxis;         
    public float selectedDirection;      
}

public class JsonData
{
    public List<ComponentTypeData> components = new List<ComponentTypeData>();
}

public class InitializedDataManager : MonoBehaviour
{
    [SerializeField] InitializeComponentManager initializeComponentManager;
    [SerializeField] List<Transform> components;
    [SerializeField] ComponentPositioner componentPositioner;
    private string directory = "InitializedModels";

    private string directoryPath;

    void Start()
    {
        directoryPath = Path.Combine(Application.persistentDataPath, directory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void SaveComponentsData()
    {
        components = Manager.Instance.Components;
        if (components == null) return;

        string fileName = Manager.Instance.Model.name + ".json";
        if (string.IsNullOrEmpty(fileName)) return;

        string filePath = Path.Combine(directoryPath, fileName);

        JsonData data = new JsonData();

        foreach (Transform child in components)
        {
            ComponentObject componentObject = child.GetComponent<ComponentObject>();

            if (componentObject != null)
            {
                ComponentTypeData componentData = new ComponentTypeData
                {
                    componentName = child.name,
                    componentType = componentObject.GetComponentType(),
                    componentGroup = componentObject.GetGroup()
                };

                // If component is a fastener, add axis and direction
                if (componentData.componentType == ComponentObject.ComponentType.Screw ||
                    componentData.componentType == ComponentObject.ComponentType.Nail ||
                    componentData.componentType == ComponentObject.ComponentType.WoodenPin)
                {
                    componentData.selectedAxis = componentObject.GetSelectedAxis();
                }

                data.components.Add(componentData);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    public void LoadComponentsData()
    {
        components = Manager.Instance.Components;
        if (components == null) return;

        string fileName = Manager.Instance.Model.name + ".json";
        if (string.IsNullOrEmpty(fileName)) return;

        string filePath = Path.Combine(directoryPath, fileName);
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        JsonData data = JsonUtility.FromJson<JsonData>(json);

        foreach (ComponentTypeData componentData in data.components)
        {
            Transform child = FindChildByName(components, componentData.componentName);
            if (child != null)
            {
                ComponentObject componentObject = child.GetComponent<ComponentObject>();
                if (componentObject == null)
                {
                    componentObject = child.gameObject.AddComponent<ComponentObject>();
                }
                componentObject.SetComponentType(componentData.componentType);
                componentObject.SetGroup(componentData.componentGroup);

                // If the component is a fastener, set axis and direction
                if (componentData.componentType == ComponentObject.ComponentType.Screw ||
                    componentData.componentType == ComponentObject.ComponentType.Nail ||
                    componentData.componentType == ComponentObject.ComponentType.WoodenPin)
                {
                    componentObject.SetSelectedAxis(componentData.selectedAxis);
                }
            }
        }
    }

    private Transform FindChildByName(List<Transform> components, string name)
    {
        foreach (Transform child in components)
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }
}
