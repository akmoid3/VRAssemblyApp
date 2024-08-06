using System;
using System.Collections.Generic;
using System.IO;
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
        manager = Manager.Instance;
        if (manager == null)
        {
            Debug.LogError("Manager is not assigned.");
            return;
        }

        string fileName = manager.Model.name;
        string filePath = Path.Combine(Application.persistentDataPath,"SavedBuildData");

        if (FileChecker.DoesJsonFileExist(filePath, fileName))
        {
            string json = ReadJson(filePath, fileName);
            RootObject rootObject = DeserializeJson(json);

            if (rootObject == null || rootObject.components == null)
            {
                Debug.LogError("Failed to deserialize JSON data.");
                return;
            }

            GameObject prefab = manager.Model;
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at path: Prefabs/{fileName}");
                return;
            }

            GameObject parent = CreateSnapParentObject(prefab, rootObject);
            Destroy(prefab);
            InitializeSnapParent(parent);

            // Initialize the sequence in the manager
            manager.InitializeSequence(rootObject.components);
        }
    }

    private string ReadJson(string filePath, string fileName)
    {
        string fullPath = Path.Combine(filePath, fileName + ".json");
        return File.ReadAllText(fullPath);
    }

    private RootObject DeserializeJson(string json)
    {
        return JsonUtility.FromJson<RootObject>(json);
    }

    private GameObject CreateSnapParentObject(GameObject prefab, RootObject rootObject)
    {
        GameObject parent = new GameObject("SnapParentObject");

        foreach (var component in rootObject.components)
        {
            if (string.IsNullOrEmpty(component.componentName))
            {
                Debug.LogWarning("Component name is empty. Skipping this component.");
                continue;
            }

            GameObject obj = new GameObject(component.componentName);
            SetTransform(obj, component);
            obj.transform.SetParent(parent.transform);
            CopyMeshAndMaterial(prefab, obj, component.componentName);
        }

        return parent;
    }

    private void SetTransform(GameObject obj, ComponentData component)
    {
        if (component.position != null)
        {
            obj.transform.localPosition = new Vector3(component.position.x, component.position.y, component.position.z);
        }

        if (component.rotation != null)
        {
            obj.transform.localRotation = new Quaternion(component.rotation.x, component.rotation.y, component.rotation.z, component.rotation.w);
        }
    }

    private void CopyMeshAndMaterial(GameObject prefabInstance, GameObject obj, string componentName)
    {
        Transform prefabChild = prefabInstance.transform.Find(componentName);
        if (prefabChild != null)
        {
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
            Debug.LogWarning($"Child with name {componentName} not found in prefab.");
        }
    }

    private void InitializeSnapParent(GameObject parent)
    {
        XRSnapPointSocketInteractor socket = parent.AddComponent<XRSnapPointSocketInteractor>();
        socket.enabled = false;
        socket.DistanceThreshold = distanceThreshold;
        socket.AngleThreshold = angleThreshold;
        BoxCollider collider = parent.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(10.0f, 10.0f, 10.0f);
        socket.enabled = true;

    }
}