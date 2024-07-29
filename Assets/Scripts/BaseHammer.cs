using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseHammer : XRGrabInteractable
{
    [SerializeField] protected float forceMultiplier = 100.0f;
    protected float currentImpactForce;
    protected Manager manager;

    // Define minimum and maximum impact force values
    [SerializeField] protected float minImpactForce = 1f;
    [SerializeField] protected float maxImpactForce = 10f;
    public void Start()
    {
        manager = FindObjectOfType<Manager>();

    }
    public float GetImpactForce()
    {
        return currentImpactForce;
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
