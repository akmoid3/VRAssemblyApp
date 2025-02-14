using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// Serializable container for storing runtime collider data.
[Serializable]
public class RuntimeColliderData
{
    public List<MeshGroup> meshGroups = new List<MeshGroup>();
}

/// Represents a group of meshes: the original mesh and its computed convex meshes.
[Serializable]
public class MeshGroup
{
    // The original mesh data.
    public MeshData baseMesh;

    // The list of computed convex meshes.
    public List<MeshData> computedMeshes = new List<MeshData>();
}

/// A serializable representation of a Mesh.
[Serializable]
public class MeshData
{
    public string name;
    public List<Vector3> vertices;
    public List<int> triangles;
}

/// Helper class to convert between Mesh and MeshData.
public static class MeshDataConverter
{
    public static MeshData ConvertMeshToMeshData(Mesh mesh)
    {
        MeshData data = new MeshData();
        data.name = mesh.name;
        data.vertices = new List<Vector3>(mesh.vertices);
        data.triangles = new List<int>(mesh.triangles);
        return data;
    }

    public static Mesh ConvertMeshDataToMesh(MeshData data)
    {
        Mesh mesh = new Mesh();
        mesh.name = data.name;
        mesh.vertices = data.vertices.ToArray();
        mesh.triangles = data.triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

/// Static class for saving the runtime collider data to a JSON file.
public static class ColliderDataSaver
{
    /// Saves the given RuntimeColliderData as JSON to Application.persistentDataPath.
    public static void SaveRuntimeColliderData(RuntimeColliderData data, string fileName)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "ColliderData");
        string filePath = Path.Combine(directoryPath, fileName);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonData);
    
        Debug.Log($"Dati collider salvati in: {filePath}");
    }

}

/// Static class for loading the runtime collider data from a JSON file.
public static class ColliderDataLoader
{
    /// Loads the RuntimeColliderData from a JSON file in Application.persistentDataPath.
    public static RuntimeColliderData LoadRuntimeColliderData(string filePath)
    {
        string json = File.ReadAllText(filePath);
        RuntimeColliderData data = JsonUtility.FromJson<RuntimeColliderData>(json);
        Debug.Log("Runtime collider data loaded from: " + filePath);
        return data;
    }
    
}

