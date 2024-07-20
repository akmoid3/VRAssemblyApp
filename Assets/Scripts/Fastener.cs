using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Fastener : MonoBehaviour
{
    protected float maxAllowedDotProduct = 0.9f;

    protected float alignmentDotProductThreshold = -0.9f;
    [SerializeField] protected bool isCollidingWithTool = false;
    [SerializeField] protected bool isCollidingWithComponent = false;

    [SerializeField] protected bool canStop = false;
    [SerializeField] protected bool isStopped = false; // Flag if the fastener should stop moving
    [SerializeField] protected float headSize = 0.005f;

    protected GameObject tool;

    protected Renderer fastenerRenderer;
    protected Color alignedColor = Color.green;
    protected Color notAlignedColor = Color.red;

    protected Color defaultColor = Color.white;

    protected Vector3 initialZPosition; // Initial position of the fastener along the Z-axis
    protected float distanceToTravel; // Depth to stop the fastener

    protected Collider component = null;

    protected bool isAligned = false;
    protected float fastenerLength;


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
            tool = other.gameObject;
            isCollidingWithTool = true;
            OnToolCollisionEnter(other);
            canStop = true;
        }
        else if (other.CompareTag("Component"))
        {
            component = other;
            isCollidingWithComponent = true;
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
        // Get the closest point on the component's surface
        Vector3 closestPoint = componentCollider.ClosestPoint(transform.position);

        // Get the normal of the closest point
        Vector3 normalAtContact = (transform.position - closestPoint).normalized;

        // Check the dot product with the normal
        float dotProduct = Vector3.Dot(transform.forward, normalAtContact);

        if (dotProduct <= alignmentDotProductThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-normalAtContact);
            Vector3 directionToMove = -transform.forward;
            Vector3 targetPosition = closestPoint + directionToMove * fastenerLength / 2.0f;

            transform.SetPositionAndRotation(targetPosition, targetRotation);

            // Set initial position at the point of contact
            initialZPosition = transform.localPosition;

            isAligned = true;
            fastenerRenderer.material.color = alignedColor;
        }
        else
        {
            fastenerRenderer.material.color = notAlignedColor;
            isAligned = false;
        }
    }



}