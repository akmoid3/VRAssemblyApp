using UnityEngine;

public abstract class BaseScrewDriver : Tool
{
    [SerializeField] protected Transform screwDriver;
    [SerializeField] private float speedMultiplier = 100.0f;
    protected float currentRotationSpeed;

    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }

    public abstract void RotateScrewDriver();

    public float GetRotationSpeed()
    {
        return currentRotationSpeed;
    }
}
