using UnityEngine;

public abstract class Fastener : MonoBehaviour
{
    protected float maxAllowedDotProduct = 0.9f;
    protected float alignmentDotProductThreshold = 0.95f;

    [SerializeField] protected bool isCollidingWithTool = false;
    [SerializeField] protected bool isCollidingWithComponent = false;

    [SerializeField] protected bool canStop = false;
    [SerializeField] protected bool isStopped = false;
    [SerializeField] protected float headSize = 0.003f;
    [SerializeField] protected float rayLength = 0.15f;

    [SerializeField] protected Transform socketTransform;
    protected Vector3 initialSocketPosition;
    protected GameObject tool;
    protected Renderer fastenerRenderer;
    protected Color alignedColor = Color.green;
    protected Color notAlignedColor = Color.red;

    protected Color defaultColor = Color.white;

    protected Vector3 initialZPosition;
    protected float distanceToTravel;

    protected Collider colliderComponent;

    [SerializeField] protected bool isAligned = false;
    protected float fastenerLength;

    protected ComponentObject componentObject;

    protected bool isFirstError = true;

    private string correctToolName;
    public bool IsAligned { get => isAligned; set => isAligned = value; }
    public bool IsStopped { get => isStopped; set => isStopped = value; }
    public bool CanStop { get => canStop; set => canStop = value; }
    public string CorrectToolName { get => correctToolName; set => correctToolName = value; }

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

    protected virtual void FixedUpdate()
    {
        if (isCollidingWithTool && !isStopped)
        {
            HandleInteraction();
        }

        if (!canStop && !isStopped && !IsAligned && StateManager.Instance.CurrentState == State.Record)
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

            if(StateManager.Instance.CurrentState == State.PlayBack)
            {
                if (tool.name != CorrectToolName && isFirstError)
                {
                    Manager.Instance.IncrementCurrentError();
                    isFirstError = false;
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            isCollidingWithTool = false;
            OnToolCollisionExit(other);

            if (StateManager.Instance.CurrentState == State.PlayBack)
            {
                if (!isFirstError)
                {
                    isFirstError = true;
                }
            }
        }
    }

    protected abstract void OnToolCollisionEnter(Collider other);
    protected abstract void OnToolCollisionExit(Collider other);

    protected void PerformComponentRaycast()
    {
        isCollidingWithComponent = false;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength))
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
                    if (componentObject && componentObject.IsReleased && !isAligned)
                        AlignWithComponent(hit.point, normalAtContact);
                }
                else
                {
                    fastenerRenderer.material.color = notAlignedColor;
                    isStopped = false;
                    canStop = false;
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

        Vector3 boundsCenter = fastenerRenderer.bounds.center;

        Vector3 pivotPoint = transform.position;

        Vector3 targetPosition;

        bool isPivotBeforeCenter = Vector3.Dot(transform.forward, boundsCenter - pivotPoint) > 0;

        bool isPivotAfterCenter = Vector3.Dot(transform.forward, boundsCenter - pivotPoint) < 0;

        if (isPivotBeforeCenter)
        {
            targetPosition = contactPoint + directionToMove * (fastenerLength / 2.0f + Vector3.Distance(boundsCenter, pivotPoint));
        }
        else if (isPivotAfterCenter)
        {
            targetPosition = contactPoint + directionToMove * (fastenerLength / 2.0f - Vector3.Distance(boundsCenter, pivotPoint));


        }
        else
        {
            targetPosition = contactPoint + directionToMove * (fastenerLength / 2.0f);
        }


        transform.SetPositionAndRotation(targetPosition, targetRotation);

        // Set initial position at the point of contact
        initialZPosition = transform.localPosition;

        isAligned = true;
    }


    public void SetSocketTransform(Transform socket)
    {
        socketTransform = socket;
        if (socketTransform != null)
            initialSocketPosition = socket.localPosition;
    }

   
}