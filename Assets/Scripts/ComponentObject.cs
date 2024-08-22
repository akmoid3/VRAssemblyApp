using UnityEngine;

public class ComponentObject : MonoBehaviour
{
    private bool isPlaced = false;
    private bool isReleased = false;

    
    public enum ComponentType
    {
        None,
        Screw,
        Nail,
        WoodenPin
    }

    public enum Group
    {
        None,
        Group01,
        Group02,
        Group03,
        Group04
    }

    [SerializeField] private ComponentType componentType = ComponentType.None;
    [SerializeField] private Group componentGroup = Group.None;
    

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

    public Group GetGroup()
    {
        return componentGroup;
    }

    public void SetGroup(Group group)
    {
        componentGroup = group;
    }
    public void SetComponentType(ComponentType type)
    {
        componentType = type;
    }
}
