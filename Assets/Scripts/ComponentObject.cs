using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentObject : MonoBehaviour
{
    private bool isPlaced = false;
    private bool isReleased = false;

    
    public enum ComponentType
    {
        None,
        Screw,
        Nail
    }

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

    public ComponentType GetComponentType()
    {
        return componentType;
    }

    public void SetComponentType(ComponentType type)
    {
        componentType = type;
    }
}
