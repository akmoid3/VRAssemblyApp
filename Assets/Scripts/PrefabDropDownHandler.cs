using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TMPDropdownPopulator : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Transform prefabContainer;
    private Dictionary<string, GameObject> prefabInstances = new Dictionary<string, GameObject>();

    void Start()
    {
        LoadPrefabsIntoDropdown();
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });

        // Initialize by showing the first prefab
        if (dropdown.options.Count > 0)
        {
            ShowPrefab(dropdown.options[0].text);
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
        // Check if the prefab instance exists and is active
        if (prefabInstances.ContainsKey(prefabName) && prefabInstances[prefabName] != null)
        {
            prefabInstances[prefabName].SetActive(false);
        }

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
        }
        else
        {
            Debug.LogWarning("Prefab " + prefabName + " not found.");
        }
    }
}