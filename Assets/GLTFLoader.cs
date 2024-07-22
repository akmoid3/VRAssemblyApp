using UnityEngine;
using UnityGLTF;
using System.IO;
using System.Collections;

public class GLTFLoader : MonoBehaviour
{
    [Tooltip("Just the name of the file")]
    [SerializeField]
    private string fileName = "Table.gltf";

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
            StartCoroutine(LoadGLTF(gltfFilePath));
        }
    }

    private IEnumerator LoadGLTF(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found at {path}");
            yield break;
        }


        // Check if GLTFComponent already exists to avoid duplicates
        GLTFComponent gltfComponent = gameObject.GetComponent<GLTFComponent>();
        if (gltfComponent == null)
        {
            gltfComponent = gameObject.AddComponent<GLTFComponent>();
        }

        // Configure GLTFComponent settings
        gltfComponent.GLTFUri = path;
        gltfComponent.Multithreaded = true;
        gltfComponent.UseStream = true;

        // Log success message
        Debug.Log($"GLB file loading started from: {path}");

        // Start the loading process
        yield return gltfComponent.Load();

        // Create a new empty GameObject with the name of the model
        string modelName = Path.GetFileNameWithoutExtension(path);
        GameObject modelParent = new GameObject(modelName);

        // Set the parent of the loaded model to the new GameObject
        gltfComponent.transform.SetParent(modelParent.transform, false);

        // Log success message after loading is complete
        Debug.Log($"GLB file loaded successfully and parented to: {modelParent.name}");

    }
}
