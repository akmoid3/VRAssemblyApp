using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

[System.Serializable]
public class TableComponentData
{
    public string id;
    public string type;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public string prefab;
}

[System.Serializable]
public class TableComponentList
{
    public List<TableComponentData> components;
}

public class TableUIManager : MonoBehaviour
{
    public GameObject panelContainer;
    public TMP_Dropdown dropdownComponentTypes;
    public Transform prefabButtonsContainer;
    public GameObject prefabButtonPrefab;
    public Transform attributesContainer;
    public GameObject dropdownPrefab;
    public Transform spawnPoint;


    private TableComponentList componentList;
    private TableComponentData selectedComponent;
    private List<GameObject> dynamicDropdowns = new List<GameObject>();


    private void Awake()
    {
        StateManager.OnStateChanged += HandleStateChanged;

    }
    void Start()
    {
        LoadComponentData();
        PopulateComponentDropdown();
        dropdownComponentTypes.onValueChanged.AddListener(delegate { OnComponentSelected(); });
    }

    private void HandleStateChanged(State newState)
    {
        if(panelContainer)
            panelContainer.SetActive(newState == State.Record);
    }

    void LoadComponentData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("TableUIComponents/components");
        componentList = JsonConvert.DeserializeObject<TableComponentList>(jsonText.text);
    }

    void PopulateComponentDropdown()
    {
        dropdownComponentTypes.ClearOptions();
        List<string> componentOptions = new List<string> { "None" };
        foreach (var component in componentList.components)
        {
            if (!componentOptions.Contains(component.type))
                componentOptions.Add(component.type);
        }
        dropdownComponentTypes.AddOptions(componentOptions);
        OnComponentSelected();
    }

    void OnComponentSelected()
    {
        string selectedType = dropdownComponentTypes.options[dropdownComponentTypes.value].text;

        prefabButtonsContainer.parent.gameObject.SetActive(selectedType != "None");

        selectedComponent = componentList.components.Find(c => c.type == selectedType);
        PopulateAttributesUI(selectedType);
        PopulatePrefabButtons(selectedType);
    }


    void PopulateAttributesUI(string selectedType)
    {
        // Clear any previous dropdowns
        foreach (var dropdown in dynamicDropdowns)
        {
            Destroy(dropdown.gameObject);
        }
        dynamicDropdowns.Clear();

        Dictionary<string, GameObject> existingDropdowns = new Dictionary<string, GameObject>();

        if (selectedComponent != null)
        {
            List<TableComponentData> filteredComponents = componentList.components.FindAll(c => c.type == selectedType);

            foreach (var component in filteredComponents)
            {
                foreach (var attribute in component.attributes)
                {
                    // Check if a dropdown for this attribute already exists
                    if (!existingDropdowns.ContainsKey(attribute.Key))
                    {
                        // Instantiate the dropdown prefab
                        GameObject dropdownObject = Instantiate(dropdownPrefab, attributesContainer);
                        TMP_Dropdown dropdown = dropdownObject.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();

                        // Access the label inside the prefab
                        TMP_Text label = dropdownObject.transform.Find("Text").GetComponent<TMP_Text>();
                        label.text = attribute.Key; // Set the label text to the attribute key

                        dropdown.ClearOptions();
                        dropdown.AddOptions(new List<string> { "None", attribute.Value });

                        // Store the dropdown in the dictionary and in the list
                        existingDropdowns[attribute.Key] = dropdownObject;
                        dynamicDropdowns.Add(dropdownObject);

                        // Add a listener to update the prefab buttons when the dropdown value changes
                        dropdown.onValueChanged.AddListener(delegate { UpdatePrefabButtons(selectedType); });
                    }
                    else
                    {
                        // If dropdown for this attribute already exists, add new value if necessary
                        GameObject existingDropdown = existingDropdowns[attribute.Key];
                        if (!existingDropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options.Exists(option => option.text == attribute.Value))
                        {
                            existingDropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(attribute.Value));
                        }
                    }
                }
            }
        }
    }

    void PopulatePrefabButtons(string selectedType)
    {
        foreach (Transform child in prefabButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        List<TableComponentData> filteredComponents = componentList.components.FindAll(c => c.type == selectedType);

        foreach (var component in filteredComponents)
        {
            GameObject buttonObject = Instantiate(prefabButtonPrefab, prefabButtonsContainer);
            Button button = buttonObject.GetComponent<Button>();

            TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
            buttonText.text = component.id;

            button.onClick.AddListener(() => SpawnPrefabById(component.id));
        }
    }

    void SpawnPrefabById(string componentId)
    {
        TableComponentData component = componentList.components.Find(c => c.id == componentId);
        if (component != null)
        {
            GameObject prefab = Resources.Load<GameObject>("TableUIComponents/" + component.prefab);
            if (prefab != null)
            {
                Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }
    void UpdatePrefabButtons(string selectedType)
    {
        foreach (Transform child in prefabButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        List<TableComponentData> filteredComponents = componentList.components;

        if (selectedType != "None")
        {
            filteredComponents = filteredComponents.FindAll(component => component.type == selectedType);
        }

        foreach (var dropdown in dynamicDropdowns)
        {
            if (dropdown != null && dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options.Count > 0)
            {
                string selectedValue = dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options[dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().value].text;

                if (selectedValue != "None")
                {
                    filteredComponents = filteredComponents.FindAll(component =>
                        component.attributes.ContainsValue(selectedValue));
                }
            }
        }

        foreach (var component in filteredComponents)
        {
            GameObject buttonObject = Instantiate(prefabButtonPrefab, prefabButtonsContainer);
            Button button = buttonObject.GetComponent<Button>();

            TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
            buttonText.text = component.id;

            button.onClick.AddListener(() => SpawnPrefabById(component.id));
        }
    }


}
