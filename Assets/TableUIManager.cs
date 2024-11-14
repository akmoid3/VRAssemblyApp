using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;

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
    public TMP_Dropdown dropdownComponentTypes;
    public Transform prefabButtonsContainer;
    public GameObject prefabButtonPrefab;
    public Transform attributesContainer;
    public GameObject dropdownPrefab;
    public Transform spawnPoint;


    private TableComponentList componentList;
    private TableComponentData selectedComponent;
    private List<TMP_Dropdown> dynamicDropdowns = new List<TMP_Dropdown>();


    void Start()
    {
        LoadComponentData();
        PopulateComponentDropdown();
        dropdownComponentTypes.onValueChanged.AddListener(delegate { OnComponentSelected(); });
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
        selectedComponent = componentList.components.Find(c => c.type == selectedType);
        PopulateAttributesUI(selectedType);
        PopulatePrefabButtons(selectedType);
    }

    void PopulateAttributesUI(string selectedType)
    {
        foreach (var dropdown in dynamicDropdowns)
        {
            Destroy(dropdown.gameObject);
        }
        dynamicDropdowns.Clear();

        Dictionary<string, TMP_Dropdown> existingDropdowns = new Dictionary<string, TMP_Dropdown>();

        if (selectedComponent != null)
        {
            List<TableComponentData> filteredComponents = componentList.components.FindAll(c => c.type == selectedType);

            foreach (var component in filteredComponents)
            {
                foreach (var attribute in component.attributes)
                {
                    if (!existingDropdowns.ContainsKey(attribute.Key))
                    {
                        GameObject dropdownObject = Instantiate(dropdownPrefab, attributesContainer);
                        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();

                        TMP_Text label = dropdownObject.transform.Find("Label").GetComponent<TMP_Text>();
                        label.text = attribute.Key;

                        dropdown.ClearOptions();
                        dropdown.AddOptions(new List<string> { "None", attribute.Value });

                        existingDropdowns[attribute.Key] = dropdown;
                        dynamicDropdowns.Add(dropdown);
                        dropdown.onValueChanged.AddListener(delegate { UpdatePrefabButtons(selectedType); });
                    }
                    else
                    {
                        TMP_Dropdown existingDropdown = existingDropdowns[attribute.Key];

                        if (!existingDropdown.options.Exists(option => option.text == attribute.Value))
                        {
                            existingDropdown.options.Add(new TMP_Dropdown.OptionData(attribute.Value));
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
            if (dropdown != null && dropdown.options.Count > 0)
            {
                string selectedValue = dropdown.options[dropdown.value].text;

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
