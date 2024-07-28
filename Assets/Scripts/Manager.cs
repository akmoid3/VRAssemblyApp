using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


public class Manager : MonoBehaviour
{
    [SerializeField] private string currentPrefabName;
    [SerializeField] private SaveSequence saveSequence;
    [SerializeField] private GameObject currentSelectedComponent;
    [SerializeField] private bool isRecording = false;
    [SerializeField] private bool isPlayBacking = false;
    [SerializeField] private bool isModelConfirmed = false;
    [SerializeField] private bool finishedRecording = false;
    [SerializeField] private bool isRealeased = false;
    [SerializeField] private bool canHover = true;
    [SerializeField] private List<GameObject> sequenceOrderList = new List<GameObject>();

    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1.0f;

    public bool IsRealeased { get => isRealeased; set => isRealeased = value; }

    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        currentSelectedComponent = args.interactableObject.transform.gameObject;
        ResetParentIfNotGroup(currentSelectedComponent);
        canHover = false;
    }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
    public void OnSelectExit(SelectExitEventArgs args)
    {
        ResetParentIfNotGroup(currentSelectedComponent);
        SetComponentReleasedState(currentSelectedComponent, true);
        canHover = true;
    }

    private void ResetParentIfNotGroup(GameObject component)
    {
        Transform parent = component.transform.parent;
        if (parent != null && parent.name != "Group")
        {
            component.transform.SetParent(null);
        }
    }

    private void SetComponentReleasedState(GameObject component, bool isReleased)
    {
        ComponentObject componentObject = component.GetComponent<ComponentObject>();
        if (componentObject != null)
        {
            componentObject.IsReleased = isReleased;
        }
    }


    private void ConfigureOutline(GameObject gameObject, bool enable)
    {
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
        }
        outline.enabled = enable;
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (canHover)
            ConfigureOutline(args.interactableObject.transform.gameObject, true);
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        ConfigureOutline(args.interactableObject.transform.gameObject, false);
    }


    public void SetRecording(bool recording)
    {
        isRecording = recording;
    }

    public bool GetRecording()
    {
        return isRecording;
    }

    public void SetPlayBacking(bool playBacking)
    {
        isPlayBacking = playBacking;
    }

    public bool GetPlayBacking()
    {
        return isPlayBacking;
    }


    public bool GtModelConfirmed()
    {
        return isModelConfirmed;
    }

    public void SetCurrentSelectedPrefabName(string name)
    {
        currentPrefabName = name;
    }

    public string GetCurrentSelectedPrefabName()
    {
        return currentPrefabName;
    }

    public GameObject GetCurrentSelectedComponent()
    {
        return currentSelectedComponent;
    }

    public void SetCurrentSelectedComponent(GameObject gameObject)
    {
        currentSelectedComponent = gameObject;
    }

    public void SaveBuildingSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.SaveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(currentPrefabName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    public void ModifyBuildingSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.ModifyComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(currentPrefabName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

    public void RemoveComponentFromSequence()
    {
        if (saveSequence != null)
        {
            saveSequence.RemoveComponent(currentSelectedComponent);
            saveSequence.SaveSequenceToJSON(currentPrefabName);
        }
        else
        {
            Debug.LogError("SaveSequence reference is not set.");
        }
    }

}
