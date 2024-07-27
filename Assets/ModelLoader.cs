using UnityEngine;
using System.IO;
using System.Collections;
using GLTFast;
using Dummiesman;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.Rendering;

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
            // Construct the full path to the GLTF file
            gltfFilePath = Path.Combine(Application.persistentDataPath, fileName);
            gltfFilePath += ".glb";
            Debug.Log($"Attempting to load GLB file from: {gltfFilePath}");
            LoadFromFile();
        }
        if (Input.GetMouseButtonDown(1))
        {

            gltfFilePath = Path.Combine(Application.persistentDataPath, fileName);
            Debug.Log($"Attempting to load GLB file from: {gltfFilePath}");
            LoadOBJModel(gltfFilePath);

        }
    }

    async void LoadFromFile()
    {
        var gltf = new GLTFast.GltfImport();

        // Load the glTF
        var success = await gltf.Load("file:///" + gltfFilePath);

        if (success)
        {
            var gameObject = new GameObject("glb");
            await gltf.InstantiateMainSceneAsync(gameObject.transform);
        }
        else
        {
            Debug.LogError("Loading glb failed!");
        }
    }


    void LoadOBJModel(string objPath)
    {
        string mtlPath = objPath + ".mtl";
        objPath += ".obj";
        if (File.Exists(objPath))
        {
            // Carica il modello .obj
            GameObject loadedObject = new OBJLoader().Load(objPath);
            loadedObject.transform.parent = this.transform;

            // Applica i materiali dal file .mtl
            if (File.Exists(mtlPath))
            {
                ApplyMaterials(loadedObject, mtlPath);
            }
            else
            {
                Debug.LogWarning("File .mtl non trovato: " + mtlPath);
            }
        }
        else
        {
            Debug.LogError("File .obj non trovato: " + objPath);
        }
    }

    void ApplyMaterials(GameObject loadedObject, string mtlPath)
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

                    // Copy properties
                    if (originalMaterial.HasProperty("_Color"))
                    {
                        urpMaterial.SetColor("_BaseColor", originalMaterial.GetColor("_Color"));
                    }
                    if (originalMaterial.HasProperty("_MainTex"))
                    {
                        urpMaterial.SetTexture("_BaseMap", originalMaterial.GetTexture("_MainTex"));
                    }
                    if (originalMaterial.HasProperty("_BumpMap"))
                    {
                        urpMaterial.SetTexture("_BumpMap", originalMaterial.GetTexture("_BumpMap"));
                        urpMaterial.EnableKeyword("_NORMALMAP");
                    }
                    if (originalMaterial.HasProperty("_MetallicGlossMap"))
                    {
                        urpMaterial.SetTexture("_MetallicGlossMap", originalMaterial.GetTexture("_MetallicGlossMap"));
                        urpMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
                    }
                    if (originalMaterial.HasProperty("_OcclusionMap"))
                    {
                        urpMaterial.SetTexture("_OcclusionMap", originalMaterial.GetTexture("_OcclusionMap"));
                    }
                    if (originalMaterial.HasProperty("_EmissionMap"))
                    {
                        urpMaterial.SetTexture("_EmissionMap", originalMaterial.GetTexture("_EmissionMap"));
                        urpMaterial.SetColor("_EmissionColor", originalMaterial.GetColor("_EmissionColor"));
                        urpMaterial.EnableKeyword("_EMISSION");
                    }

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
}
