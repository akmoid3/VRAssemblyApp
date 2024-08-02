using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseScrewDriver : Tool
{
    [SerializeField] protected Transform screwDriver;
    [SerializeField] protected float speedMultiplier = 100.0f;
    protected float currentRotationSpeed;

    public abstract void RotateScrewDriver();

    public float GetRotationSpeed()
    {
        return currentRotationSpeed;
    }
}
