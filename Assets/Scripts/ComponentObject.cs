using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentObject : MonoBehaviour
{
    private bool isPlaced = false;
    private bool isReleased = false;

    // Define an enum to represent component types
    public enum ComponentType
    {
        None,
        Screw,
        Nail
    }

    // Field to store the component type
    [SerializeField] private ComponentType componentType = ComponentType.None;

    // Properties for component state
    public bool IsReleased { get => isReleased; set => isReleased = value; }

    public bool GetIsPlaced()
    {
        return isPlaced;
    }

    public void SetIsPlaced(bool value)
    {
        isPlaced = value;
    }

    // Method to get the component type
    public ComponentType GetComponentType()
    {
        return componentType;
    }

    // Method to set the component type
    public void SetComponentType(ComponentType type)
    {
        componentType = type;
    }
}
