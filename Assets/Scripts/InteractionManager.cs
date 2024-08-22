using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    [SerializeField] private bool canHover = true;
    [SerializeField] private GameObject currentSelectedComponent;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1.0f;


    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        if (args.interactorObject as XRBaseControllerInteractor)
        {
            currentSelectedComponent = args.interactableObject.transform.gameObject;
            SetComponentReleasedState(currentSelectedComponent, false);
            if (StateManager.Instance.CurrentState != State.Initialize)
                ResetParentIfNotGroup(currentSelectedComponent);
            canHover = false;

            if (currentSelectedComponent.GetComponent<Fastener>())
            {
                currentSelectedComponent.GetComponent<Fastener>().IsAligned = false;
                currentSelectedComponent.GetComponent<Fastener>().CanStop = false;
                currentSelectedComponent.GetComponent<Fastener>().IsStopped = false;
            }
        }
    }

    public void OnSelectExit(SelectExitEventArgs args)
    {
        if (args.interactorObject as XRBaseControllerInteractor)
        {
            if (StateManager.Instance.CurrentState != State.Initialize && StateManager.Instance.CurrentState != State.PlayBack)
                ResetParentIfNotGroup(currentSelectedComponent);
            SetComponentReleasedState(currentSelectedComponent, true);
            canHover = true;

            //if (StateManager.Instance.CurrentState == State.PlayBack)
               //ValidateComponent(currentSelectedComponent);
        }
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



    public GameObject GetCurrentSelectedComponent()
    {
        return currentSelectedComponent;
    }

    public void SetCurrentSelectedComponent(GameObject gameObject)
    {
        currentSelectedComponent = gameObject;
    }

}
