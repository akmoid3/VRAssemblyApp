using UnityEngine;

public abstract class BaseScrewDriver : Tool
{
    [SerializeField] private Transform screwDriver;
    [SerializeField] private float speedMultiplier = 100.0f;
    protected float currentRotationSpeed;

    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }
    public Transform ScrewDriver { get => screwDriver; set => screwDriver = value; }

    public abstract void RotateScrewDriver();

    public float GetRotationSpeed()
    {
        return currentRotationSpeed;
    }
}
