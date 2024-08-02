using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

[Serializable]
public class ComponentData
{
    public string componentName;
    public TransformData position;
    public TransformData rotation;
    public string toolName;
}

[Serializable]
public class TransformData
{
    public float x;
    public float y;
    public float z;
    public float w;
}

[Serializable]
public class RootObject
{
    public List<ComponentData> components;
}

public class SequenceReader : MonoBehaviour
{
    [SerializeField] private Manager manager;
    [SerializeField] private Material holographicMaterial;
    [SerializeField] private float distanceThreshold = 0.05f;
    [SerializeField] private float angleThreshold = 10.0f;
    public void CreateSnapObjectFromJSON()
    {
        if (manager == null)
        {
            Debug.LogError("Manager is not assigned.");
            return;
        }

        string fileName = manager.Model.name;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is not set in Manager.");
            return;
        }

        // Construct the file path
        string folderPath = "Assets/SavedData";
        string filePath = System.IO.Path.Combine(folderPath, "tavolo" + ".json");

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"JSON file not found at path: {filePath}");
            return;
        }

        // Read the JSON
        string json = System.IO.File.ReadAllText(filePath);

        // Deserialize JSON data
        RootObject rootObject = JsonUtility.FromJson<RootObject>(json);

        if (rootObject == null || rootObject.components == null)
        {
            Debug.LogError("Failed to deserialize JSON data.");
            return;
        }

        // Load the prefab from the Resources folder
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + "tavolo");
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at path: {"Prefabs/" + fileName}");
            return;
        }

        // Instantiate the prefab to use its child components
        GameObject prefabInstance = Instantiate(prefab);

        // Create the parent object of the snap
        GameObject parent = new GameObject("SnapParentObject");
       
        // Create objects based on the deserialized data
        foreach (var component in rootObject.components)
        {
            if (string.IsNullOrEmpty(component.componentName))
            {
                Debug.LogWarning("Component name is empty. Skipping this component.");
                continue;
            }

            // Instantiate a new empty GameObject for the component
            GameObject obj = new GameObject(component.componentName);

            // Set position and rotation
            if (component.position != null)
            {
                obj.transform.localPosition = new Vector3(component.position.x, component.position.y, component.position.z);
            }

            if (component.rotation != null)
            {
                obj.transform.localRotation = new Quaternion(component.rotation.x, component.rotation.y, component.rotation.z, component.rotation.w);
            }

            // Set the object as a child of the parent
            obj.transform.SetParent(parent.transform);

            // Find the corresponding child in the prefab instance
            Transform prefabChild = prefabInstance.transform.Find(component.componentName);
            if (prefabChild != null)
            {
                // Copy the MeshFilter and set OlographicMaterial
                MeshFilter prefabMeshFilter = prefabChild.GetComponent<MeshFilter>();
                MeshRenderer prefabMeshRenderer = prefabChild.GetComponent<MeshRenderer>();

                if (prefabMeshFilter != null)
                {
                    MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = prefabMeshFilter.sharedMesh;
                }

                if (prefabMeshRenderer != null)
                {
                    MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = holographicMaterial;
                }
            }
            else
            {
                Debug.LogWarning($"Child with name {component.componentName} not found in prefab.");
            }

            
        }

        Destroy(prefabInstance);

        XRSnapPointSocketInteractor socket = parent.AddComponent<XRSnapPointSocketInteractor>();
        socket.DistanceThreshold = distanceThreshold;
        socket.AngleThreshold = angleThreshold;
        BoxCollider collider = parent.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(10.0f, 10.0f, 10.0f);
    }
}
