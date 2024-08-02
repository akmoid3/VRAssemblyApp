using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Tool : XRGrabInteractable
{
    protected Manager manager;

    public virtual void Start()
    {
        manager = FindObjectOfType<Manager>();
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
