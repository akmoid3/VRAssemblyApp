using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ComponentObject : MonoBehaviour
{

    [SerializeField]
    private bool isPlaced = false;
    [SerializeField]
    private bool isReleased = false;
    [SerializeField]
    private bool isDestroyed = false;
    [SerializeField]
    private bool isAutomaticSnap = false;
    
    private bool hasMoved = false;

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



    private AudioSource audioSource;
    private LineRenderer lineRenderer;

    // Properties for component state
    public bool IsReleased { get => isReleased; set => isReleased = value; }
    public bool IsDestroyed { get => isDestroyed; set => isDestroyed = value; }

    public bool IsAutomaticSnap
    {
        get => isAutomaticSnap;
        set => isAutomaticSnap = value;
    }

    public bool HasMoved
    {
        get => hasMoved;
        set => hasMoved = value;
    }


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
        if (StateManager.Instance.CurrentState == State.Initialize)
        {
            lineRenderer.enabled = true;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + transform.rotation * selectedAxis * 0.2f;

            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        else if (StateManager.Instance.CurrentState != State.Initialize)
        {

            lineRenderer.enabled = false;
        }
    }

    public void CopyFrom(ComponentObject other)
    {
        if (other == null) return;

        // Copying properties from the other ComponentObject to this one
        this.componentType = other.componentType;
        this.componentGroup = other.componentGroup;
        this.selectedAxis = other.selectedAxis;
        this.isPlaced = other.isPlaced;
        this.isReleased = other.isReleased;
        this.isDestroyed = other.isDestroyed;

    }

    public void PlayBuildPopSound()
    {
        AudioManager.Instance.PlayOneShot(audioSource, "BuildPop", 1f);
    }

    public bool GetIsPlaced()
    {
        return isPlaced;
    }

    public void SetIsPlaced(bool value)
    {
        // if(GetComponent<MeshCollider>() != null)
        //     GetComponent<MeshCollider>().convex = !value;
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