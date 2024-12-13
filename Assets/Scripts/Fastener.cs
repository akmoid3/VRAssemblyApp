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
    private Tool tool;
    protected Renderer fastenerRenderer;
    protected Color alignedColor = Color.green;
    protected Color notAlignedColor = Color.red;

    protected Color defaultColor = Color.white;

    private Vector3 initialPosition;
    protected float distanceToTravel;

    protected Collider colliderComponent;

    [SerializeField] protected bool isAligned = false;
    protected float fastenerLength;

    protected ComponentObject componentObject;

    protected bool isFirstError = true;

    [SerializeField] private string correctToolName;

    [SerializeField] private int correctToolForce;
    public bool IsAligned { get => isAligned; set => isAligned = value; }
    public bool IsStopped { get => isStopped; set => isStopped = value; }
    public bool CanStop { get => canStop; set => canStop = value; }
    public string CorrectToolName { get => correctToolName; set => correctToolName = value; }
    public Tool Tool { get => tool; set => tool = value; }
    public Renderer FastenerRenderer { get => fastenerRenderer; set => fastenerRenderer = value; }
    public int CorrectToolForce { get => correctToolForce; set => correctToolForce = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }

    protected Vector3 selectedAxisDirRaw;
    protected Vector3 selectedAxisDirection;
    protected float fastenerLengthAlongAxis;

    public Tool getTool()
    {
        return tool;
    }

    protected AudioSource audioSource;

    protected virtual void Start()
    {
        fastenerRenderer = GetComponent<Renderer>();
        defaultColor = fastenerRenderer.material.color;

        // Initialize the AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        componentObject = GetComponent<ComponentObject>();

        if (componentObject == null)
        {
            Debug.LogError("ComponentObject is not found on this Fastener.");
        }

        selectedAxisDirRaw = componentObject.GetSelectedAxis();

        // Calculate the length of the fastener along the selected axis

        fastenerLengthAlongAxis = Mathf.Abs(Vector3.Dot(selectedAxisDirRaw, fastenerRenderer.bounds.size));

        distanceToTravel = fastenerLengthAlongAxis - headSize;


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
            tool = other.gameObject.GetComponent<Tool>();
            if (tool == null)
                tool = other.gameObject.GetComponentInParent<Tool>();
            isCollidingWithTool = true;
            OnToolCollisionEnter(other);
            canStop = true;

            if (StateManager.Instance.CurrentState == State.PlayBack)
            {
                DynamometerScrewDriver dyn = tool as DynamometerScrewDriver;
                if (dyn && tool.ToolName == CorrectToolName && dyn.Force != CorrectToolForce)
                {
                    Manager.Instance.IncrementCurrentError();
                    isFirstError = false;
                    return;
                }
                
                if (tool.ToolName != CorrectToolName && isFirstError)
                {
                    Manager.Instance.IncrementCurrentError();
                    isFirstError = false;
                    return;
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
        if (componentObject == null) return;

        isCollidingWithComponent = false;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position;

        // Use the selected axis and direction from the ComponentObject
        Vector3 rayDirection = MapSelectedAxisToTransformDirection(componentObject.GetSelectedAxis());

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Component"))
            {
                isCollidingWithComponent = true;

                Vector3 normalAtContact = hit.normal;
                float dotProduct = Vector3.Dot(rayDirection, -normalAtContact);

                if (dotProduct >= alignmentDotProductThreshold)
                {
                    fastenerRenderer.material.color = alignedColor;

                    if (componentObject.IsReleased && !isAligned)
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

    public static Quaternion OmniLookRotation(
             Vector3 exactAxis, Vector3 exactTarget,
             Vector3 approximateAxis, Vector3 approximateTarget
)
    {
        // Compute a rotation that takes the z+ and y+ axes to our custom axes.
        var zyToCustom = Quaternion.LookRotation(exactAxis, approximateAxis);
        // Invert this, to map our custom axes to z+ and y+.
        var customToZY = Quaternion.Inverse(zyToCustom);

        // Compute a rotation that takes the z+ and y+ axes to our target directions.
        var zyToTarget = Quaternion.LookRotation(exactTarget, approximateTarget);

        // Chain these two rotations so that exactAxis maps to exactTarget,
        // and approximateAxis maps as closely as it can to approximateTarget.
        var customToTarget = zyToTarget * customToZY;

        return customToTarget;
    }

    protected void AlignWithComponent(Vector3 contactPoint, Vector3 contactNormal)
    {
        if (componentObject == null) return;

        // Map the selected axis to the actual transform direction
        selectedAxisDirection = MapSelectedAxisToTransformDirection(componentObject.GetSelectedAxis());

        
        // Calculate the correct target rotation so the selected axis aligns with the contact normal
        //Quaternion targetRotation = Quaternion.LookRotation(contactNormal, selectedAxisDirection);
        Quaternion targetRotation;

        if (componentObject.GetSelectedAxis() == Vector3.up)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                transform.right, transform.forward
            );
        }
        else if (componentObject.GetSelectedAxis() == Vector3.down)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                transform.right, -transform.forward
            );
        }
        else if (componentObject.GetSelectedAxis() == Vector3.right)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                transform.up, transform.forward
            );
        }
        else if (componentObject.GetSelectedAxis() == Vector3.left)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                -transform.up, transform.forward
            );
        }
        else if (componentObject.GetSelectedAxis() == Vector3.forward)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                transform.up, transform.right
            );
        }
        else if (componentObject.GetSelectedAxis() == Vector3.back)
        {
            targetRotation = OmniLookRotation(
                componentObject.GetSelectedAxis(), -contactNormal,
                transform.up, -transform.right
            );
        }
        else
        {
            return; // Unsupported axis
        }


        // Esegue una transizione verso la rotazione target
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            4 * Time.deltaTime
        );
        // Calculate the direction to move, based on selected axis and direction
        Vector3 directionToMove = selectedAxisDirection ;

        // Center of the fastener's bounds, used for alignment
        Vector3 boundsCenter = fastenerRenderer.bounds.center;
        Vector3 pivotPoint = transform.position;

        // Calculate the distance from the pivot point to the center along the selected axis
        float distanceFromPivotToCenter = Mathf.Abs(Vector3.Dot(boundsCenter - pivotPoint, selectedAxisDirection));

        // Determine the target position based on whether the pivot point is before or after the center
        Vector3 targetPosition;
        bool isPivotBeforeCenter = Vector3.Dot(selectedAxisDirection, boundsCenter - pivotPoint) > 0;
        bool isPivotAfterCenter = Vector3.Dot(selectedAxisDirection, boundsCenter - pivotPoint) < 0;
        float offSet = 0.005f;

        if (isPivotBeforeCenter)
        {
            targetPosition = contactPoint - directionToMove * (fastenerLengthAlongAxis / 2.0f + distanceFromPivotToCenter + offSet);
        }
        else if (isPivotAfterCenter)
        {
            targetPosition = contactPoint - directionToMove * (fastenerLengthAlongAxis / 2.0f - distanceFromPivotToCenter + offSet);
        }
        else
        {
            targetPosition = contactPoint - directionToMove * (fastenerLengthAlongAxis / 2.0f + offSet);
        }

        // Set the position and rotation of the fastener
        transform.SetPositionAndRotation(targetPosition, targetRotation);

        // Store the initial position for further movement along the selected axis
        initialPosition = transform.localPosition;
        isAligned = true;
    }



    public void SetSocketTransform(Transform socket)
    {
        socketTransform = socket;
        if (socketTransform != null)
            initialSocketPosition = socket.localPosition;
    }

    public Transform GetSocketTransform()
    {
        return socketTransform;
    }

    protected void PlaySound(string clipName, bool loop = false, float volume = 1.0f)
    {
        AudioManager.Instance.PlaySound(audioSource, clipName, loop, volume);
    }

    protected void StopSound()
    {
        AudioManager.Instance.StopSound(audioSource);
    }

    protected void SetPitch(float pitch)
    {
        AudioManager.Instance.SetPitch(audioSource, pitch);
    }

    public void PlayBuildPopSound()
    {
        AudioManager.Instance.PlayOneShot(audioSource, "BuildPop", 1f);
    }

    public Vector3 MapSelectedAxisToTransformDirection(Vector3 selectedAxis)
    {
        if (selectedAxis == Vector3.forward)
            return transform.forward;
        else if (selectedAxis == Vector3.right)
            return transform.right;
        else if (selectedAxis == Vector3.up)
            return transform.up;
        if (selectedAxis == Vector3.forward * -1.0f)
            return transform.forward * -1.0f;
        else if (selectedAxis == Vector3.right * -1.0f)
            return transform.right * -1.0f;
        else if (selectedAxis == Vector3.up * -1.0f)
            return transform.up * -1.0f;


        return transform.forward;
    }

}
