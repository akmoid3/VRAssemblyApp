using UnityEngine;
using UnityGLTF;
using System.IO;

public class GLTFLoader : MonoBehaviour
{
    public string gltfFileName = "Table.gltf"; // Just the name of the file, not the full path

    void Start()
    {
        // Log the persistent data path for debugging purposes
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
    }

    void Update()
    {
        // Check for mouse click (left button)
        if (Input.GetMouseButtonDown(0))
        {
            string gltfFilePath = Path.Combine(Application.persistentDataPath, gltfFileName);
            Debug.Log("Attempting to load GLTF file from: " + gltfFilePath);
            LoadGLTF(gltfFilePath);
        }
    }

    public async void LoadGLTF(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found at " + path);
            return;
        }

        try
        {
            GLTFComponent gltfComponent = gameObject.AddComponent<GLTFComponent>();
            gltfComponent.GLTFUri = path;
            gltfComponent.Multithreaded = true;
            gltfComponent.UseStream = true;


            // Log success message
            Debug.Log("GLTF file loading started from: " + path);

            await gltfComponent.Load();

            Debug.Log("GLTF file loaded successfully from: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading GLTF file: " + e.Message);
            Debug.LogError(e.StackTrace);
        }
    }
}
