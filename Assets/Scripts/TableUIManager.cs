using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;


public class TableUIManager : MonoBehaviour
{
    public GameObject panelContainer;
    public TMP_Dropdown dropdownComponentTypes;
    public Transform prefabButtonsContainer;
    public GameObject prefabButtonPrefab;
    public Transform attributesContainer;
    public GameObject dropdownPrefab;
    public Transform spawnPoint;
    public AudioSource audioSource;

    public TableComponentListSO componentListSO;

    private TableComponentDataSO selectedComponent;
    private readonly List<GameObject> dynamicDropdowns = new List<GameObject>();

    private void Awake()
    {
        StateManager.OnStateChanged += HandleStateChanged;
    }

    private void Start()
    {
        PopulateComponentDropdown();
        dropdownComponentTypes.onValueChanged.AddListener(delegate { OnComponentSelected(); });
    }

    private void HandleStateChanged(State newState)
    {
        if (panelContainer)
            panelContainer.SetActive(newState == State.Record);
    }

    private void PopulateComponentDropdown()
    {
        dropdownComponentTypes.ClearOptions();
        List<string> componentOptions = new List<string> { "None" };
        foreach (var component in componentListSO.components)
        {
            if (component != null)
            {
                string componentTypeLower = component.type.ToLower(); 
                if (!componentOptions.Contains(componentTypeLower))
                    componentOptions.Add(componentTypeLower);
            }
        }
        dropdownComponentTypes.AddOptions(componentOptions);
        OnComponentSelected();
    }

    private void OnComponentSelected()
    {
        string selectedType = dropdownComponentTypes.options[dropdownComponentTypes.value].text.ToLower();

        prefabButtonsContainer.parent.gameObject.SetActive(selectedType != "none");

        selectedComponent = componentListSO.components.Find(c => c.type.ToLower() == selectedType);
        PopulateAttributesUI(selectedType);
        PopulatePrefabButtons(selectedType);
    }

    private void PopulateAttributesUI(string selectedType)
    {
        foreach (var dropdown in dynamicDropdowns)
        {
            Destroy(dropdown.gameObject);
        }
        dynamicDropdowns.Clear();

        Dictionary<string, GameObject> existingDropdowns = new Dictionary<string, GameObject>();

        if (selectedComponent != null)
        {
            List<TableComponentDataSO> filteredComponents = componentListSO.components.FindAll(c => c.type.ToLower() == selectedType);

            foreach (var component in filteredComponents)
            {
                foreach (var attribute in component.attributes)
                {
                    string attributeKeyLower = attribute.key.ToLower(); 
                    string attributeValueLower = attribute.value.ToLower(); 

                    if (!existingDropdowns.ContainsKey(attributeKeyLower))
                    {
                        GameObject dropdownObject = Instantiate(dropdownPrefab, attributesContainer);
                        TMP_Dropdown dropdown = dropdownObject.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
                        TMP_Text label = dropdownObject.transform.Find("Text").GetComponent<TMP_Text>();
                        label.text = attributeKeyLower; // Mostra la key in minuscolo

                        dropdown.ClearOptions();
                        dropdown.AddOptions(new List<string> { "None", attributeValueLower }); 

                        existingDropdowns[attributeKeyLower] = dropdownObject;
                        dynamicDropdowns.Add(dropdownObject);

                        dropdown.onValueChanged.AddListener(delegate { UpdatePrefabButtons(selectedType); });
                    }
                    else
                    {
                        GameObject existingDropdown = existingDropdowns[attributeKeyLower];
                        TMP_Dropdown dropdown = existingDropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
                        if (!dropdown.options.Exists(option => option.text == attributeValueLower))
                        {
                            dropdown.options.Add(new TMP_Dropdown.OptionData(attributeValueLower));
                        }
                    }
                }
            }
        }
    }

    private void PopulatePrefabButtons(string selectedType)
    {
        foreach (Transform child in prefabButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        List<TableComponentDataSO> filteredComponents = componentListSO.components.FindAll(c => c.type.ToLower() == selectedType);

        foreach (var component in filteredComponents)
        {
            GameObject buttonObject = Instantiate(prefabButtonPrefab, prefabButtonsContainer);
            Button button = buttonObject.GetComponent<Button>();

            TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
            buttonText.text = component.id.ToLower();

            button.onClick.AddListener(() => SpawnPrefabById(component.id));
        }
    }

    private void SpawnPrefabById(string componentId)
    {
        TableComponentDataSO component = componentListSO.components.Find(c => c.id.ToLower() == componentId.ToLower());
        if (component != null && component.prefab != null)
        {
            GameObject spawnedObject = Instantiate(component.prefab, spawnPoint.position, Quaternion.identity, spawnPoint);
            spawnedObject.name = component.prefab.name;

            MakeGrabbable makeGrabbable = spawnedObject.AddComponent<MakeGrabbable>();
            makeGrabbable.MakeObjectGrabbable();
            InitializeComponentType(spawnedObject);
            AudioManager.Instance.PlayOneShot(audioSource, "TemplateSpawn", 1.0f);

        }
        else
        {
            Debug.LogError(component == null ? $"Component with ID {componentId} not found." : "Prefab is null in the component data.");
        }
    }


    private void UpdatePrefabButtons(string selectedType)
    {
        foreach (Transform child in prefabButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        List<TableComponentDataSO> filteredComponents = componentListSO.components;

        if (selectedType != "none")
        {
            filteredComponents = filteredComponents.FindAll(component => component.type.ToLower() == selectedType);
        }

        foreach (var dropdown in dynamicDropdowns)
        {
            if (dropdown != null && dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options.Count > 0)
            {
                string selectedValue = dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().options[dropdown.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().value].text.ToLower(); 

                if (selectedValue != "none")
                {
                    filteredComponents = filteredComponents.FindAll(component =>
                        component.attributes.Exists(attr => attr.value.ToLower() == selectedValue));
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

    public void InitializeComponentType(GameObject component)
    {
        ComponentObject componentObject = component.GetComponent<ComponentObject>();

        if (componentObject != null)
        {
            switch (componentObject.GetComponentType())
            {
                case ComponentObject.ComponentType.Screw:
                    component.gameObject.AddComponent<Screw>();
                    break;
                case ComponentObject.ComponentType.Nail:
                    component.gameObject.AddComponent<Nail>();
                    break;
                case ComponentObject.ComponentType.WoodenPin:
                    component.gameObject.AddComponent<WoodenPin>();
                    break;
                default:
                    break;
            }
        }
    }
}