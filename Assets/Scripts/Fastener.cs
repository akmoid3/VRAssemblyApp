using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Fastener : MonoBehaviour
{
    [SerializeField] protected float pitch = 0.1f; // Distance the fastener moves per rotation
    protected float maxAllowedDotProduct = 0.9f;

    protected float alignmentDotProductThreshold = -0.9f;
    [SerializeField] protected bool isCollidingWithTool = false;
    [SerializeField] protected bool isCollidingWithComponent = false;

    [SerializeField] protected bool canStop = false;
    [SerializeField] protected bool isStopped = false; // Flag if the fastener should stop moving

    protected Renderer fastenerRenderer;
    protected Color alignedColor = Color.green;
    protected Color notAlignedColor = Color.red;

    protected Color defaultColor = Color.white;

    protected float fastenerLength; // Length of the fastener mesh
    protected Vector3 initialZPosition; // Initial position of the fastener along the Z-axis
    protected float distanceToTravel; // Depth to stop the fastener

    protected Collider component = null;

    protected bool isAligned = false;

    // Store the original interaction layer mask
    protected int originalLayerMask;

    protected virtual void Start()
    {
      
        fastenerRenderer = GetComponent<Renderer>();
        defaultColor = fastenerRenderer.material.color;

        // Calculate the length of the fastener mesh
        fastenerLength = GetComponent<Renderer>().bounds.size.z;
        distanceToTravel = fastenerLength - 0.005f;
    }

    protected virtual void Update()
    {
        if (isCollidingWithTool && !isStopped)
        {
            HandleInteraction();
        }

        if (isCollidingWithComponent && !isAligned)
        {
            AlignWithContactPoint(component);
        }

    }

    protected abstract void HandleInteraction();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            isCollidingWithTool = true;
            OnToolCollisionEnter(other);
            canStop = true;
        }
        else if (other.CompareTag("Component"))
        {
            component = other;
            isCollidingWithComponent = true;

            // Set initial position at the point of contact
            initialZPosition = transform.localPosition;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tool"))
        {
            isCollidingWithTool = false;
            OnToolCollisionExit(other);
        }
        else if (other.CompareTag("Component"))
        {
            component = null;
            isCollidingWithComponent = false;
            isAligned = false;
            isStopped = false;
            fastenerRenderer.material.color = defaultColor;
        }
    }

    protected abstract void OnToolCollisionEnter(Collider other);
    protected abstract void OnToolCollisionExit(Collider other);

    protected void AlignWithContactPoint(Collider componentCollider)
    {
        // Calculate the closest point on the component's surface
        Vector3 closestPoint = componentCollider.ClosestPoint(transform.position);

        // Get the normal of the closest point
        Vector3 normalAtContact = (transform.position - closestPoint).normalized;

        // Check the dot product with the normal
        float dotProduct = Vector3.Dot(transform.forward, normalAtContact);

        if (dotProduct <= alignmentDotProductThreshold)
        {
            // Align the fastener's forward direction with the normal
            transform.LookAt(transform.position + (-normalAtContact));
            fastenerRenderer.material.color = alignedColor;

            isAligned = true;
        }
        else
        {
            fastenerRenderer.material.color = notAlignedColor;
            isAligned = false;
        }
    }





}
