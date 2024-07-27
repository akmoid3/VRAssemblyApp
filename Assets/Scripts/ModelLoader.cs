using UnityEngine;
using System.IO;
using Dummiesman;
using SFB;
using System.Threading.Tasks;

public class ModelLoader : MonoBehaviour
{
    private string filePath;
    public void LoadGLB()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open GLB File", "", "glb", false);
        if (paths.Length > 0)
        {
            filePath = paths[0];
            SaveFileToPersistentDataPath(filePath);
        }
    }

    public void LoadOBJ()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open OBJ File", "", "obj", false);
        if (paths.Length > 0)
        {
            filePath = paths[0];
            SaveFileToPersistentDataPath(filePath);
            LoadOBJModel(filePath);
        }
    }

    public async Task<GameObject> LoadFromFile(string filePath)
    {
        var gltf = new GLTFast.GltfImport();

        // Load the glTF
        var success = await gltf.Load("file:///" + filePath);

        if (success)
        {
            var gameObject = new GameObject("tempParent");
            var instantiateSuccess = await gltf.InstantiateMainSceneAsync(gameObject.transform);
            if (instantiateSuccess)
            {
                gameObject = gameObject.transform.GetChild(0).gameObject;
            }
            // Return the root GameObject of the loaded model
            return gameObject;
        }
        else
        {
            Debug.LogError("Loading glb failed!");
            return null;
        }
    }

    void LoadOBJModel(string objPath)
    {
        string mtlPath = objPath.Replace(".obj", ".mtl");
        if (File.Exists(objPath))
        {
            // Load the .obj model
            GameObject loadedObject = new OBJLoader().Load(objPath);
            loadedObject.transform.parent = this.transform;

            // Apply materials from the .mtl file
            if (File.Exists(mtlPath))
            {
                ApplyMaterials(loadedObject, mtlPath);
            }
            else
            {
                Debug.LogWarning("File .mtl not found: " + mtlPath);
            }
        }
        else
        {
            Debug.LogError("File .obj not found: " + objPath);
        }
    }

    private void ApplyMaterials(GameObject loadedObject, string mtlPath)
    {
        var materials = new MTLLoader().Load(mtlPath);
        Debug.Log("Materials loaded: " + materials.Count);
        var renderers = loadedObject.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            var sharedMaterials = renderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                string materialName = sharedMaterials[i].name.Replace(" (Instance)", "");
                if (materials.ContainsKey(materialName))
                {
                    Material originalMaterial = materials[materialName];
                    Material urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    urpMaterial.CopyPropertiesFromMaterial(originalMaterial);
                    sharedMaterials[i] = urpMaterial;
                }
                else
                {
                    Debug.LogWarning("Material not found: " + materialName);
                }
            }
            renderer.sharedMaterials = sharedMaterials;
        }
    }

    private void SaveFileToPersistentDataPath(string sourceFilePath)
    {
        // Create a unique file name based on the original file name
        string fileName = Path.GetFileName(sourceFilePath);
        string destinationFilePath = Path.Combine(Application.persistentDataPath, fileName);

        // Copy the file to the persistent data path
        File.Copy(sourceFilePath, destinationFilePath, true);


        // Save the file path in PlayerPrefs
        PlayerPrefs.SetString("SavedModelPath", destinationFilePath);
        PlayerPrefs.Save();

        Debug.Log($"File saved to: {destinationFilePath}");
    }
}
