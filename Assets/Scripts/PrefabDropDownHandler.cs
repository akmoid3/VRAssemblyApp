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
        // Remove the previous instances from the container
        foreach (Transform child in prefabContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the selected prefab
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if (prefab != null)
        {
            // Instantiate the prefab and store the instance in the dictionary
            GameObject instance = Instantiate(prefab, prefabContainer);
            prefabInstances[prefabName] = instance;

            // Set the position of the instance to (0, 0, 0) relative to the container
            instance.transform.localPosition = Vector3.zero;

            manager.SetCurrentSelectedPrefabName(prefabName);
        }
        else
        {
            Debug.LogWarning("Prefab " + prefabName + " not found.");
        }
    }

    void Update()
    {
        if (manager.GetModelConfirmed())
        {
            HideAllPrefabs();
        }
    }

    void HideAllPrefabs()
    {
        foreach (var prefabInstance in prefabInstances.Values)
        {
            if (prefabInstance != null)
            {
                prefabInstance.SetActive(false);
            }
        }

        // Remove the previous instances from the container
        foreach (Transform child in prefabContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
