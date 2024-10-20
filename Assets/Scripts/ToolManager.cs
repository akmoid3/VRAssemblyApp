using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> toolPrefabsList;
    private Dictionary<string, GameObject> toolPrefabs;
    private Dictionary<GameObject, GameObject> toolInstances = new Dictionary<GameObject, GameObject>();

    public Dictionary<GameObject, GameObject> ToolInstances { get => toolInstances; set => toolInstances = value; }

    private void Awake()
    {
        // Initialize the tool prefabs dictionary
        toolPrefabs = new Dictionary<string, GameObject>();
        foreach (var toolPrefab in toolPrefabsList)
        {
            toolPrefabs[toolPrefab.name] = toolPrefab;
        }
    }

    public GameObject AttachToolToComponent(GameObject component, string toolName)
    {
        // Remove any existing tool from the component
        HideToolOnComponent(component);

        if (toolPrefabs.TryGetValue(toolName, out GameObject toolPrefab))
        {
            GameObject toolInstance = Instantiate(toolPrefab);
            toolInstance.name = toolName;
            toolInstance.transform.SetParent(component.transform); // Attach tool to component
            toolInstance.transform.localPosition = Vector3.zero + new Vector3(0, 0.5f, 0); // Set local position relative to component
            toolInstance.GetComponent<Rigidbody>().isKinematic = true;
            toolInstance.transform.SetParent(null); // Detach tool from component

            // Store the tool instance for later management
            toolInstances[component] = toolInstance;
            return toolInstance;
        }

        return null;
    }

    public void HideToolOnComponent(GameObject component)
    {
        if (toolInstances.TryGetValue(component, out GameObject toolInstance))
        {
            Destroy(toolInstance);
            toolInstances.Remove(component);
        }
    }

    public void HideAllTools()
    {
        foreach (var toolInstance in toolInstances.Values)
        {
            if (toolInstance != null)
            {
                Destroy(toolInstance);
            }
        }
        toolInstances.Clear();
    }
}
