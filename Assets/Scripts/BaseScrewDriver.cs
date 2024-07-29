using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseScrewDriver : XRGrabInteractable
{
    [SerializeField] protected Transform screwDriver;
    [SerializeField] protected float speedMultiplier = 100.0f;
    protected float currentRotationSpeed;
    protected Manager manager;

    public void Start()
    {
        manager = FindObjectOfType<Manager>();
    }
    public abstract void RotateScrewDriver();

    public float GetRotationSpeed()
    {
        return currentRotationSpeed;
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
