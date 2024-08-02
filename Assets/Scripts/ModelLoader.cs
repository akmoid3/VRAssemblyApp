using UnityEngine;
using System.IO;
using Dummiesman;
using SFB;
using System.Threading.Tasks;

public class ModelLoader : MonoBehaviour
{

    public async Task<GameObject> LoadFromFile(string filePath)
    {
        var gltf = new GLTFast.GltfImport();

        // Load the glTF
        var success = await gltf.Load("file:///" + filePath);

        if (success)
        {
            var tempParent = new GameObject("tempParent");
            var instantiateSuccess = await gltf.InstantiateMainSceneAsync(tempParent.transform);
            GameObject gameObject = null;
            if (instantiateSuccess)
            {
                gameObject = tempParent.transform.GetChild(0).gameObject;
                Destroy(tempParent);
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

    /* For obj files
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
    */

}
