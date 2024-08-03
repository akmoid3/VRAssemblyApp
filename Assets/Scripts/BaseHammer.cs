using UnityEngine;

public abstract class BaseHammer : Tool
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
