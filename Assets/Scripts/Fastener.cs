using System.Collections.Generic;
using UnityEngine;

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

    protected List<Collider> components = new List<Collider>();

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
            AlignWithClosestComponent();
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
            if (!components.Contains(other))
            {
                components.Add(other);
                isCollidingWithComponent = true;
            }
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
            if (components.Contains(other))
            {
                components.Remove(other);
                if (components.Count == 0)
                {
                    isCollidingWithComponent = false;
                    isAligned = false;
                    isStopped = false;
                    fastenerRenderer.material.color = defaultColor;
                }
            }
        }
    }

    protected abstract void OnToolCollisionEnter(Collider other);
    protected abstract void OnToolCollisionExit(Collider other);

    protected void AlignWithClosestComponent()
    {
        Collider closestComponent = null;
        float closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        Vector3 normalAtContact = Vector3.zero;

        foreach (Collider component in components)
        {
            Vector3 point = component.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, point);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestComponent = component;
                closestPoint = point;
                normalAtContact = (transform.position - closestPoint).normalized;
            }
        }

        if (closestComponent != null)
        {
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
}
