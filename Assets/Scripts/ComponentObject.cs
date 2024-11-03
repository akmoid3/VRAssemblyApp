using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ComponentObject : MonoBehaviour
{
    [SerializeField]
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

    [SerializeField] private Vector3 selectedAxis;

    // Properties for component state
    public bool IsReleased { get => isReleased; set => isReleased = value; }

    private AudioSource audioSource;
    private LineRenderer lineRenderer;

    private void Start()
    {
        // Initialize the AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configure lineRenderer appearance
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
        lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + transform.rotation * selectedAxis * 0.5f;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
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

    public Vector3 GetSelectedAxis()
    {
        return selectedAxis;
    }

    public void SetSelectedAxis(Vector3 axis)
    {
        selectedAxis = axis;
    }

}
