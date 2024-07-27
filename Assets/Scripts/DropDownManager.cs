using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class DropDownManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Transform prefabContainer;
    [SerializeField] private Manager manager;
    private Dictionary<string, GameObject> prefabInstances = new Dictionary<string, GameObject>();
    [SerializeField] private ModelLoader loader;
    private string persistentDataPath;
    private HashSet<string> currentFiles = new HashSet<string>();

    void Start()
    {
        persistentDataPath = Application.persistentDataPath;
        LoadModelsIntoDropdown();
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });

        // Start monitoring the persistent data path for changes
        StartCoroutine(CheckForFileChanges());
    }

    IEnumerator CheckForFileChanges()
    {
        while (true)
        {
            string[] files = Directory.GetFiles(persistentDataPath, "*.glb");
            HashSet<string> newFiles = new HashSet<string>(files);

            if (!newFiles.SetEquals(currentFiles))
            {
                currentFiles = newFiles;
                LoadModelsIntoDropdown();
                DropdownValueChanged(dropdown);
               /* if (dropdown.options.Count > 0)
                {
                    ShowModel(dropdown.options[0].text);
                    manager.SetCurrentSelectedPrefabName(dropdown.options[0].text);
                }*/
            }

            // Wait for a second before checking again
            yield return new WaitForSeconds(0.5f);
        }
    }

    void LoadModelsIntoDropdown()
    {
        dropdown.options.Clear();

        // Get all model files in the persistent data path
        string[] files = Directory.GetFiles(persistentDataPath, "*.glb");
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            dropdown.options.Add(new TMP_Dropdown.OptionData(fileName));
        }

        // Refresh the dropdown
        dropdown.RefreshShownValue();
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        string selectedModelName = dropdown.options[change.value].text;
        ShowModel(selectedModelName);
    }

    async void ShowModel(string modelName)
    {
        HideAllPrefabs();

        // Check if the model instance is already created and stored in the dictionary
        if (prefabInstances.TryGetValue(modelName, out GameObject instance))
        {
            // If the instance exists, set it active
            instance.SetActive(true);
        }
        else
        {
            string modelPath = Path.Combine(persistentDataPath, modelName + ".glb");
            if (File.Exists(modelPath))
            {
                // Load the model and get the instantiated GameObject
                var instantiatedObject = await loader.LoadFromFile(modelPath);
                if (instantiatedObject != null)
                {
                    instantiatedObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    instantiatedObject.transform.SetParent(prefabContainer,false);
                    prefabInstances[modelName] = instantiatedObject;
                }
                else
                {
                    Debug.LogError("Failed to instantiate model: " + modelName);
                }
            }
            else
            {
                Debug.LogWarning("Model " + modelName + " not found.");
            }
        }

        manager.SetCurrentSelectedPrefabName(modelName);
    }

    public void DestroyPrefabs()
    {
        foreach (Transform child in prefabContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void HideAllPrefabs()
    {
        foreach (var prefabInstance in prefabInstances.Values)
        {
            if (prefabInstance != null && prefabInstance.activeSelf)
            {
                prefabInstance.SetActive(false);
            }
        }
    }
}
