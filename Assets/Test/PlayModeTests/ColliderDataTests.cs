using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class ColliderDataTests
{
    [Test]
    public void TestMeshDataConverter_ConvertMeshToMeshData()
    {
        Mesh mesh = new Mesh();
        mesh.name = "TestMesh";
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2 };

        MeshData meshData = MeshDataConverter.ConvertMeshToMeshData(mesh);

        Assert.AreEqual(mesh.name, meshData.name);
        Assert.AreEqual(mesh.vertices.Length, meshData.vertices.Count);
        Assert.AreEqual(mesh.triangles.Length, meshData.triangles.Count);

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Assert.AreEqual(mesh.vertices[i], meshData.vertices[i]);
        }
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            Assert.AreEqual(mesh.triangles[i], meshData.triangles[i]);
        }
    }

    [Test]
    public void TestMeshDataConverter_ConvertMeshDataToMesh()
    {
        MeshData data = new MeshData();
        data.name = "TestMeshData";
        data.vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        };
        data.triangles = new List<int> { 0, 1, 2 };

        Mesh mesh = MeshDataConverter.ConvertMeshDataToMesh(data);

        Assert.AreEqual(data.name, mesh.name);
        Assert.AreEqual(data.vertices.Count, mesh.vertices.Length);
        Assert.AreEqual(data.triangles.Count, mesh.triangles.Length);

        for (int i = 0; i < data.vertices.Count; i++)
        {
            Assert.AreEqual(data.vertices[i], mesh.vertices[i]);
        }
        for (int i = 0; i < data.triangles.Count; i++)
        {
            Assert.AreEqual(data.triangles[i], mesh.triangles[i]);
        }
    }

    [Test]
    public void TestColliderDataSaverAndLoader()
    {
        RuntimeColliderData originalData = new RuntimeColliderData();
        MeshGroup group = new MeshGroup();

        MeshData baseMeshData = new MeshData();
        baseMeshData.name = "BaseMesh";
        baseMeshData.vertices = new List<Vector3>
        {
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(0,1,0)
        };
        baseMeshData.triangles = new List<int> { 0, 1, 2 };
        group.baseMesh = baseMeshData;

        MeshData computedMeshData = new MeshData();
        computedMeshData.name = "ComputedMesh";
        computedMeshData.vertices = new List<Vector3>
        {
            new Vector3(1,1,1),
            new Vector3(2,1,1),
            new Vector3(1,2,1)
        };
        computedMeshData.triangles = new List<int> { 0, 1, 2 };
        group.computedMeshes.Add(computedMeshData);

        originalData.meshGroups.Add(group);

        string fileName = "testColliderData.json";
        ColliderDataSaver.SaveRuntimeColliderData(originalData, fileName);

        string directoryPath = Path.Combine(Application.persistentDataPath, "ColliderData");
        string filePath = Path.Combine(directoryPath, fileName);

        Assert.IsTrue(File.Exists(filePath), "Saved JSON file should exist at " + filePath);

        RuntimeColliderData loadedData = ColliderDataLoader.LoadRuntimeColliderData(filePath);

        Assert.IsNotNull(loadedData);
        Assert.AreEqual(originalData.meshGroups.Count, loadedData.meshGroups.Count);

        MeshGroup loadedGroup = loadedData.meshGroups[0];
        Assert.AreEqual(group.baseMesh.name, loadedGroup.baseMesh.name);
        Assert.AreEqual(group.baseMesh.vertices.Count, loadedGroup.baseMesh.vertices.Count);
        Assert.AreEqual(group.baseMesh.triangles.Count, loadedGroup.baseMesh.triangles.Count);

        Assert.AreEqual(group.computedMeshes.Count, loadedGroup.computedMeshes.Count);
        MeshData loadedComputedMesh = loadedGroup.computedMeshes[0];
        Assert.AreEqual(computedMeshData.name, loadedComputedMesh.name);
        Assert.AreEqual(computedMeshData.vertices.Count, loadedComputedMesh.vertices.Count);
        Assert.AreEqual(computedMeshData.triangles.Count, loadedComputedMesh.triangles.Count);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
