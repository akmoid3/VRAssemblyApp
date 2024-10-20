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

    [SerializeField] private ComponentType componentType = ComponentType.None;
    [SerializeField] private string componentGroup = "None";

    // Properties for component state
    public bool IsReleased { get => isReleased; set => isReleased = value; }

    private AudioSource audioSource;

    private void Start()
    {
        // Initialize the AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayBuildPopSound()
    {
        AudioManager.Instance.PlaySound(audioSource, "BuildPop", false, 1f);
    }

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

    public string GetGroup()
    {
        return componentGroup;
    }

    public void SetGroup(string group)
    {
        componentGroup = group;
    }

    public void SetComponentType(ComponentType type)
    {
        componentType = type;
    }
}
