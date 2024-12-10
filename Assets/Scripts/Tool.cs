using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public abstract class Tool : XRGrabInteractable
{
    protected Manager manager = Manager.Instance;
    [SerializeField] private string toolName;

    public string ToolName { get => toolName; set => toolName = value; }

    public void SetManager(Manager customManager)
    {
        manager = customManager;
    }
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        if (manager != null)
        {
            manager.OnHoverEnter(args);
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        if (manager != null)
        {
            manager.OnHoverExit(args);
        }
    }
}
