using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DropDownManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private PrefabManager prefabManager;

    void Start()
    {
        prefabManager.OnModelsLoaded += LoadModelsIntoDropdown;
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        prefabManager.LoadModels();
    }

    private void LoadModelsIntoDropdown(List<string> modelNames)
    {
        dropdown.options.Clear();
        foreach (var modelName in modelNames)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(modelName));
        }
        dropdown.RefreshShownValue();

        OnDropdownValueChanged(dropdown.value);
    }

    private void OnDropdownValueChanged(int index)
    {
        string selectedModelName = dropdown.options[index].text;
        prefabManager.ShowModel(selectedModelName);
    }
}
