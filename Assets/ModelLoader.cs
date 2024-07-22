using UnityEngine;
using System.IO;
using System.Collections;
using GLTFast;

public class ModelLoader : MonoBehaviour
{
    [Tooltip("Just the name of the file")]
    [SerializeField]
    private string fileName = "Table";

    private string gltfFilePath;

    void Start()
    {
        // Construct the full path to the GLTF file
        gltfFilePath = Path.Combine(Application.persistentDataPath, fileName);
        gltfFilePath += ".glb";

        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
    }

    void Update()
    {
        // Check for mouse click (left button)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Attempting to load GLB file from: {gltfFilePath}");
            LoadFromFile();
        }
    }

    async void LoadFromFile()
    {
        var gltf = new GLTFast.GltfImport();

        // Load the glTF
        var success = await gltf.Load("file:///"  + gltfFilePath);

        if (success)
        {
            var gameObject = new GameObject("glTF");
            await gltf.InstantiateMainSceneAsync(gameObject.transform);
        }
        else
        {
            Debug.LogError("Loading glTF failed!");
        }
    }
}
