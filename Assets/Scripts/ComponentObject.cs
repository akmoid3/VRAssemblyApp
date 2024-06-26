using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentObject : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Manager manager;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        manager = FindObjectOfType<Manager>();

        grabInteractable.selectEntered.AddListener(manager.OnSelectEnter);
        grabInteractable.selectExited.AddListener(manager.OnSelectExit);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(manager.OnSelectEnter);
        grabInteractable.selectExited.RemoveListener(manager.OnSelectExit);
    }
}
