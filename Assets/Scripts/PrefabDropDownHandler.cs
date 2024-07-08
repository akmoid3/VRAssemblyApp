using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TMPDropdownPopulator : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Transform prefabContainer;
    [SerializeField] private Manager manager;
    private Dictionary<string, GameObject> prefabInstances = new Dictionary<string, GameObject>();

    void Start()
    {
        LoadPrefabsIntoDropdown();
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });

        // Initialize by showing the first prefab
        if (dropdown.options.Count > 0)
        {
            ShowPrefab(dropdown.options[0].text);
            manager.SetCurrentSelectedPrefabName(dropdown.options[0].text);
        }
    }

    void LoadPrefabsIntoDropdown()
    {
        Object[] prefabs = Resources.LoadAll("Prefabs", typeof(GameObject));
        foreach (Object prefab in prefabs)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(prefab.name));
        }
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        string selectedPrefabName = dropdown.options[change.value].text;
        ShowPrefab(selectedPrefabName);
    }

    void ShowPrefab(string prefabName)
    {
        HideAllPrefabs();

        // Check if the prefab instance is already created and stored in the dictionary
        if (prefabInstances.TryGetValue(prefabName, out GameObject instance))
        {
            // If the instance exists set active
            instance.SetActive(true);
        }
        else
        {
            // Instantiate the selected prefab
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
            if (prefab != null)
            {
                // Instantiate the prefab and store the instance in the dictionary
                instance = Instantiate(prefab, prefabContainer);
                prefabInstances[prefabName] = instance;

                instance.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("Prefab " + prefabName + " not found.");
            }
        }

        manager.SetCurrentSelectedPrefabName(prefabName);
    }

    void Update()
    {
        if (manager.GetModelConfirmed())
        {
            foreach (Transform child in prefabContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void HideAllPrefabs()
    {
        foreach (var prefabInstance in prefabInstances.Values)
        {
            if (prefabInstance != null && prefabInstance.activeSelf == true)
            {
                prefabInstance.SetActive(false);
            }
        }
    }
}
