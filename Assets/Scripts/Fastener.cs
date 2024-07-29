using UnityEngine;

public abstract class Fastener : MonoBehaviour
{
    protected float maxAllowedDotProduct = 0.9f;
    protected float alignmentDotProductThreshold = 0.95f;

    [SerializeField] protected bool isCollidingWithTool = false;
    [SerializeField] protected bool isCollidingWithComponent = false;

    [SerializeField] protected bool canStop = false;
    [SerializeField] protected bool isStopped = false; // Flag if the fastener should stop moving
    [SerializeField] protected float headSize = 0.005f;
    [SerializeField] protected float rayLength = 0.15f;

    [SerializeField] protected Transform socketTransform;
    protected Vector3 initialSocketPosition;
    protected GameObject tool;
    protected Renderer fastenerRenderer;
    protected Color alignedColor = Color.green;
    protected Color notAlignedColor = Color.red;

    protected Color defaultColor = Color.white;

    protected Vector3 initialZPosition; // Initial position of the fastener along the Z-axis
    protected float distanceToTravel; // Depth to stop the fastener

    protected Collider colliderComponent;

    [SerializeField] protected bool isAligned = false;
    protected float fastenerLength;

    protected ComponentObject componentObject;
    public GameObject getTool()
    {
        return tool;
    }

    protected virtual void Start()
    {
        fastenerRenderer = GetComponent<Renderer>();
        defaultColor = fastenerRenderer.material.color;

        // Length of the mesh
        fastenerLength = GetComponent<Renderer>().bounds.size.z;
        distanceToTravel = fastenerLength - headSize;
    }

    protected virtual void Update()
    {
        if (isCollidingWithTool && !isStopped)
        {
            HandleInteraction();
        }

        if (!canStop && !isStopped)
            PerformComponentRaycast();
    }

    protected abstract void HandleInteraction();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            tool = other.gameObject;
            isCollidingWithTool = true;
            OnToolCollisionEnter(other);
            canStop = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            isCollidingWithTool = false;
            OnToolCollisionExit(other);
        }
    }

    protected abstract void OnToolCollisionEnter(Collider other);
    protected abstract void OnToolCollisionExit(Collider other);

    protected void PerformComponentRaycast()
    {
        isCollidingWithComponent = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Component"))
            {
                isCollidingWithComponent = true;

                // Check alignment with dot product
                Vector3 normalAtContact = hit.normal;
                float dotProduct = Vector3.Dot(transform.forward, -normalAtContact);
                componentObject = GetComponent<ComponentObject>();
                if (dotProduct >= alignmentDotProductThreshold)
                {
                    fastenerRenderer.material.color = alignedColor;
                    if (componentObject && componentObject.IsReleased)
                        AlignWithComponent(hit.point, normalAtContact);
                }
                else
                {
                    fastenerRenderer.material.color = notAlignedColor;
                    isAligned = false;
                }
            }
        }
        else
        {
            isStopped = false;
            canStop = false;
            isCollidingWithComponent = false;
            isAligned = false;
            fastenerRenderer.material.color = defaultColor;

        }
    }

    protected void AlignWithComponent(Vector3 contactPoint, Vector3 contactNormal)
    {
        Quaternion targetRotation = Quaternion.LookRotation(-contactNormal);
        Vector3 directionToMove = -transform.forward;
        Vector3 targetPosition = contactPoint + directionToMove * fastenerLength / 2.0f;

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        // Set initial position at the point of contact
        initialZPosition = transform.localPosition;

        isAligned = true;
    }

    public void SetSocketTransform(Transform socket)
    {
        socketTransform = socket;
        initialSocketPosition = socket.localPosition;

    }
}
