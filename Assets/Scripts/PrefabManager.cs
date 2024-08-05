using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private Transform prefabContainer;
    [SerializeField] private ModelLoader loader;
    [SerializeField] private FileMonitor fileMonitor;
    private Manager manager;
    private Dictionary<string, GameObject> prefabInstances = new Dictionary<string, GameObject>();

    public delegate void ModelsLoadedHandler(List<string> modelNames);
    public event ModelsLoadedHandler OnModelsLoaded;

    private void Start()
    {
        fileMonitor.OnFilesChanged += OnFilesChanged;
        manager = Manager.Instance;
    }

    public void LoadModels()
    {
        OnFilesChanged(new HashSet<string>(Directory.GetFiles(fileMonitor.PersistentDataPath, "*.glb")));
    }

    private void OnFilesChanged(HashSet<string> newFiles)
    {
        List<string> modelNames = new List<string>();
        foreach (var file in newFiles)
        {
            modelNames.Add(Path.GetFileNameWithoutExtension(file));
        }
        OnModelsLoaded?.Invoke(modelNames);
    }

    public async void ShowModel(string modelName)
    {
        HideAllPrefabs();

        if (!prefabInstances.TryGetValue(modelName, out GameObject instance))
        {
            string modelPath = Path.Combine(fileMonitor.PersistentDataPath, modelName + ".glb");
            if (File.Exists(modelPath))
            {
                instance = await loader.LoadFromFile(modelPath);
                if (instance != null)
                {
                    instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    instance.transform.SetParent(prefabContainer, false);
                    prefabInstances[modelName] = instance;

                    GameObject modelClone = Instantiate(instance);
                    modelClone.name = instance.name;

                    if (manager.Model != null)
                    {
                        Destroy(manager.Model);
                    }
                    manager.Model = modelClone;
                    modelClone.SetActive(false);
                }
                else
                {
                    Debug.LogError($"Failed to instantiate model: {modelName}");
                }
            }
            else
            {
                Debug.LogWarning($"Model {modelName} not found.");
            }
        }

        if (instance != null)
        {
            instance.SetActive(true);
            GameObject modelClone = Instantiate(instance);
            modelClone.name = instance.name;

            if (manager.Model != null)
            {
                Destroy(manager.Model);
            }
            manager.Model = modelClone;
            modelClone.SetActive(false);
        }
    }


    private void HideAllPrefabs()
    {
        foreach (var prefabInstance in prefabInstances.Values)
        {
            if (prefabInstance != null && prefabInstance.activeSelf)
            {
                prefabInstance.SetActive(false);
            }
        }
    }

    public void DestroyAllPrefabs()
    {
        foreach (var prefabInstance in prefabInstances.Values)
        {
            if (prefabInstance != null)
            {
                Destroy(prefabInstance);
            }
        }
        prefabInstances.Clear();
    }
}
