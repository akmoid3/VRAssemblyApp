using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseHammer : XRGrabInteractable
{
    [SerializeField] protected float forceMultiplier = 100.0f;
    protected float currentImpactForce;

    // Define minimum and maximum impact force values
    [SerializeField] protected float minImpactForce = 1f;
    [SerializeField] protected float maxImpactForce = 10f;

    public float GetImpactForce()
    {
        return currentImpactForce;
    }
}
