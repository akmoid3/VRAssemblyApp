using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;
using System.Reflection;



public class TableUIManagerTests
{
    private GameObject uiManagerGO;
    private TableUIManager uiManager;

    // Dummy UI objects.
    private GameObject panelContainer;
    private TMP_Dropdown dropdownComponentTypes;
    private Transform prefabButtonsContainer;
    private GameObject prefabButtonPrefab;
    private Transform attributesContainer;
    private GameObject dropdownPrefab;
    private Transform spawnPoint;
    private AudioSource audioSource;
    private TableComponentListSO componentListSO;
    private AudioManager audioManager;
    private Manager manager;

    [SetUp]
    public void Setup()
    {
        manager = new GameObject().AddComponent<Manager>();
        manager.ComponentsThatCanSnap = new List<Transform>();
        // Create a GameObject and attach the TableUIManager component.
        uiManagerGO = new GameObject("TableUIManager");
        uiManager = uiManagerGO.AddComponent<TableUIManager>();
        uiManagerGO.AddComponent<AudioSource>();

        panelContainer = new GameObject("PanelContainer");
        uiManager.panelContainer = panelContainer;
        audioManager = new GameObject("AudioManager").AddComponent<AudioManager>();
        
        var dropdownGO = new GameObject("DropdownComponentTypes");
        dropdownComponentTypes = dropdownGO.AddComponent<TMP_Dropdown>();
        uiManager.dropdownComponentTypes = dropdownComponentTypes;

        var prefabButtonsContainerGO = new GameObject("PrefabButtonsContainer");
        var parentGO = new GameObject("ParentContainer");
        prefabButtonsContainerGO.transform.parent = parentGO.transform;
        uiManager.prefabButtonsContainer = prefabButtonsContainerGO.transform;

        prefabButtonPrefab = new GameObject("PrefabButtonPrefab");
        prefabButtonPrefab.AddComponent<Button>();
        var tmpText = prefabButtonPrefab.AddComponent<TextMeshProUGUI>();
        tmpText.text = "";
        uiManager.prefabButtonPrefab = prefabButtonPrefab;

        // Create and assign attributes container.
        var attributesContainerGO = new GameObject("AttributesContainer");
        attributesContainer = attributesContainerGO.transform;
        uiManager.attributesContainer = attributesContainer;

        dropdownPrefab = new GameObject("DropdownPrefab");
        
        var dropdownChild = new GameObject("Dropdown");
        dropdownChild.transform.parent = dropdownPrefab.transform;
        dropdownChild.AddComponent<TMP_Dropdown>();

        var textChild = new GameObject("Text");
        textChild.transform.parent = dropdownPrefab.transform;
        textChild.AddComponent<TextMeshProUGUI>();
        uiManager.dropdownPrefab = dropdownPrefab;

        var spawnPointGO = new GameObject("SpawnPoint");
        spawnPoint = spawnPointGO.transform;
        uiManager.spawnPoint = spawnPoint;

        audioSource = spawnPointGO.AddComponent<AudioSource>();
        uiManager.audioSource = audioSource;

        componentListSO = ScriptableObject.CreateInstance<TableComponentListSO>();
        componentListSO.components = new List<TableComponentDataSO>();

        TableComponentDataSO componentData1 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        componentData1.id = "Component1";
        componentData1.type = "Screw";
        componentData1.prefab = new GameObject("ScrewPrefab");
        componentData1.attributes = new List<AttributeData>
        {
            new AttributeData { key = "Color", value = "Red" }
        };
        componentListSO.components.Add(componentData1);

        TableComponentDataSO componentData2 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        componentData2.id = "Component2";
        componentData2.type = "Nail";
        componentData2.prefab = new GameObject("NailPrefab");
        componentData2.attributes = new List<AttributeData>
        {
            new AttributeData { key = "Size", value = "Small" }
        };
        componentListSO.components.Add(componentData2);

        uiManager.componentListSO = componentListSO;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(uiManagerGO);
        Object.DestroyImmediate(panelContainer);
        Object.DestroyImmediate(dropdownComponentTypes.gameObject);
        Object.DestroyImmediate(uiManager.prefabButtonsContainer.gameObject);
        Object.DestroyImmediate(prefabButtonPrefab);
        Object.DestroyImmediate(attributesContainer.gameObject);
        Object.DestroyImmediate(dropdownPrefab);
        Object.DestroyImmediate(spawnPoint.gameObject);
        Object.DestroyImmediate(componentListSO);
        Object.DestroyImmediate(audioManager);

    }

    [UnityTest]
    public IEnumerator Start_PopulatesComponentDropdown()
    {
        yield return null;

        Assert.IsTrue(dropdownComponentTypes.options.Count > 0);
        Assert.AreEqual("None", dropdownComponentTypes.options[0].text);

        bool hasScrew = false;
        bool hasNail = false;
        foreach (var option in dropdownComponentTypes.options)
        {
            if (option.text == "screw") hasScrew = true;
            if (option.text == "nail") hasNail = true;
        }
        Assert.IsTrue(hasScrew);
        Assert.IsTrue(hasNail);
    }

    [UnityTest]
    public IEnumerator OnComponentSelected_UpdatesUIElements()
    {
        yield return null; 

        int screwIndex = dropdownComponentTypes.options.FindIndex(opt => opt.text == "screw");
        dropdownComponentTypes.value = screwIndex;
        dropdownComponentTypes.RefreshShownValue();

        uiManager.OnComponentSelected();

        Assert.IsTrue(uiManager.prefabButtonsContainer.parent.gameObject.activeSelf);

        var dynDropdownsField = typeof(TableUIManager).GetField("dynamicDropdowns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        List<GameObject> dynamicDropdowns = (List<GameObject>)dynDropdownsField.GetValue(uiManager);
        Assert.IsTrue(dynamicDropdowns.Count > 0);

        Assert.IsTrue(uiManager.prefabButtonsContainer.childCount > 0);

        yield return null;
    }

    [UnityTest]
    public IEnumerator SpawnPrefabById_CreatesPrefabInstance()
    {
        yield return null; 

        Assert.IsNotNull(componentListSO.components.Find(c => c.id.ToLower() == "component1"));

        int initialCount = GameObject.FindObjectsOfType<MakeGrabbable>().Length;

        uiManager.SpawnPrefabById("Component1");

        yield return null;

        int newCount = GameObject.FindObjectsOfType<MakeGrabbable>().Length;
        Assert.Greater(newCount, initialCount);
    }

    [UnityTest]
    public IEnumerator InitializeComponentType_AddsCorrectComponent()
    {
        yield return null;
        var dummyGO = new GameObject("DummyComponent");
        var dummyComponent = dummyGO.AddComponent<ComponentObject>();
        dummyComponent.SetComponentType(ComponentObject.ComponentType.Screw);

        uiManager.InitializeComponentType(dummyGO);

        var screw = dummyGO.GetComponent<Screw>();
        Assert.IsNotNull(screw);

        Object.DestroyImmediate(dummyGO);
        yield return null;
    }
    
    
     [UnityTest]
    public IEnumerator UpdatePrefabButtons_WithNoneSelectedType_CreatesButtonsForAllComponents()
    {
      
        TableComponentDataSO comp1 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        comp1.id = "Component1";
        comp1.type = "hammer";
        comp1.attributes = new List<AttributeData>
        {
            new AttributeData { key = "color", value = "red" }
        };

        TableComponentDataSO comp2 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        comp2.id = "Component2";
        comp2.type = "nail";
        comp2.attributes = new List<AttributeData>
        {
            new AttributeData { key = "size", value = "small" }
        };

        componentListSO.components.Clear();
        componentListSO.components.Add(comp1);
        componentListSO.components.Add(comp2);

        foreach (Transform child in uiManager.prefabButtonsContainer)
        {
            Object.Destroy(child.gameObject);
        }

        uiManager.UpdatePrefabButtons("none");

        yield return null;

     
        Assert.AreEqual(2, uiManager.prefabButtonsContainer.childCount);

        List<string> buttonIds = new List<string>();
        foreach (Transform child in uiManager.prefabButtonsContainer)
        {
            TMP_Text btnText = child.GetComponentInChildren<TMP_Text>();
            buttonIds.Add(btnText.text);
        }
        CollectionAssert.Contains(buttonIds, "Component1");
        CollectionAssert.Contains(buttonIds, "Component2");
    }

    [UnityTest]
    public IEnumerator UpdatePrefabButtons_WithDynamicDropdown_FiltersComponents()
    {
        TableComponentDataSO comp1 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        comp1.id = "Component1";
        comp1.type = "hammer";
        comp1.attributes = new List<AttributeData>
        {
            new AttributeData { key = "color", value = "red" }
        };

        TableComponentDataSO comp2 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        comp2.id = "Component2";
        comp2.type = "hammer";
        comp2.attributes = new List<AttributeData>
        {
            new AttributeData { key = "color", value = "blue" }
        };

        // Also add a component of a different type.
        TableComponentDataSO comp3 = ScriptableObject.CreateInstance<TableComponentDataSO>();
        comp3.id = "Component3";
        comp3.type = "nail";
        comp3.attributes = new List<AttributeData>
        {
            new AttributeData { key = "color", value = "red" }
        };

        componentListSO.components.Clear();
        componentListSO.components.Add(comp1);
        componentListSO.components.Add(comp2);
        componentListSO.components.Add(comp3);


        GameObject dynDropdown = new GameObject("DynamicDropdown");
        GameObject dropdownChild = new GameObject("Dropdown");
        dropdownChild.transform.parent = dynDropdown.transform;
        TMP_Dropdown dynTMPDropdown = dropdownChild.AddComponent<TMP_Dropdown>();
        dynTMPDropdown.ClearOptions();

        dynTMPDropdown.AddOptions(new List<string> { "none", "red" });
        dynTMPDropdown.value = 1;
        dynTMPDropdown.RefreshShownValue();

        FieldInfo dynamicDropdownsField = typeof(TableUIManager).GetField("dynamicDropdowns", BindingFlags.NonPublic | BindingFlags.Instance);
        List<GameObject> dynamicDropdowns = (List<GameObject>)dynamicDropdownsField.GetValue(uiManager);
        dynamicDropdowns.Clear();
        dynamicDropdowns.Add(dynDropdown);

        foreach (Transform child in uiManager.prefabButtonsContainer)
        {
            Object.Destroy(child.gameObject);
        }


        uiManager.UpdatePrefabButtons("hammer");
        yield return null; 

        Assert.AreEqual(1, uiManager.prefabButtonsContainer.childCount);
        TMP_Text createdButtonText = uiManager.prefabButtonsContainer.GetChild(0).GetComponentInChildren<TMP_Text>();
        Assert.AreEqual("Component1", createdButtonText.text);
    }
    
}
