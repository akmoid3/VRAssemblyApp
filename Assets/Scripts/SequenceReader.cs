using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    [SerializeField] private GameObject buildingPosition;
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
        string filePath = Path.Combine(Application.persistentDataPath, "SavedBuildData");

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
        parent.transform.SetPositionAndRotation(buildingPosition.transform.position, Quaternion.identity);

        // List to hold the positions of all child objects for calculating the center
        List<Vector3> childPositions = new List<Vector3>();

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

            GameObject prefabChild = prefab.transform.Find(component.componentName).gameObject;
            CopyMeshAndMaterial(prefabChild, obj);

            // Add the child's position to the list
            childPositions.Add(obj.transform.localPosition);
        }

        // Calculate the center of all child objects
        Vector3 center = CalculateCenter(childPositions);

        // Adjust each child's position to center them around the parent
        foreach (Transform child in parent.transform)
        {
            child.localPosition -= center;
        }

        return parent;
    }

    private Vector3 CalculateCenter(List<Vector3> positions)
    {
        if (positions.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var pos in positions)
        {
            sum += pos;
        }
        return sum / positions.Count;
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

    private void CopyMeshAndMaterial(GameObject prefabChild, GameObject obj)
    {
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
                meshRenderer.enabled = false;
            }
        }
    }

    private void InitializeSnapParent(GameObject parent)
    {
        SnapToPosition socket = parent.AddComponent<SnapToPosition>();
        BoxCollider collider = parent.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(5.0f, 5.0f, 5.0f);
        socket.enabled = true;
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }
}
