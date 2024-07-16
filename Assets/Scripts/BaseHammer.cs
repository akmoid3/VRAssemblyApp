using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseHammer : XRGrabInteractable
{
    [SerializeField] protected float forceMultiplier = 100.0f;
    protected float currentImpactForce;


    public float GetImpactForce()
    {
        return currentImpactForce;
    }
}
