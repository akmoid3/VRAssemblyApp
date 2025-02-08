using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class SnapPoint
{
    public Transform snapTransform;
    public string componentName;
    public MeshRenderer meshRenderer;
    public ComponentObject componentObject;
}

public class SnapToPosition : MonoBehaviour
{
    private float snapDistance = 0.1f;
    private float fastenerSnapDistance = 0.01f;
    private float snapAngle = 5f;
    private List<SnapPoint> snapPoints;
    private HashSet<GameObject> snappedObjects = new HashSet<GameObject>(); // Track snapped objects
    public static event Action OnComponentPlaced;
    private XRInteractionManager interactionManager;

    public float SnapDistance
    {
        get => snapDistance;
        set => snapDistance = value;
    }

    public float SnapAngle
    {
        get => snapAngle;
        set => snapAngle = value;
    }

    private void Awake()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();
    }


    private void Start()
    {
        snapPoints = new List<SnapPoint>();

        foreach (Transform child in transform)
        {
            // Try to get the required components
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            ComponentObject componentObject = child.GetComponent<ComponentObject>();

            if (componentObject == null)
            {
                componentObject = child.gameObject.AddComponent<ComponentObject>();
            }

            SnapPoint snapPoint = new SnapPoint
            {
                snapTransform = child,
                componentName = child.name,
                meshRenderer = meshRenderer,
                componentObject = componentObject
            };

            snapPoints.Add(snapPoint);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other == null ||
            !((StateManager.Instance.CurrentState != State.PlayBack ||
               StateManager.Instance.CurrentState != State.Finish) &&
              Manager.Instance.CurrentStep < Manager.Instance.AssemblySequence.Count))
            return;

        if (Manager.Instance.ComponentsThatCanSnap.Contains(other.transform))
        {
            CheckSnap(other);
        }
    }

    private void CheckSnap(Collider other)
    {
        ComponentObject componentObject = other.GetComponent<ComponentObject>();
        var snapPoint = snapPoints[Manager.Instance.CurrentStep];

        if (!componentObject.IsReleased)
            return;

        if (componentObject != null)
        {
            if (other.name == snapPoint.componentName ||
                (componentObject.GetGroup() != "None" &&
                 componentObject.GetGroup() == snapPoint.componentObject.GetGroup() &&
                 componentObject.GetType() == snapPoint.componentObject.GetType()))
            {
                float distance = Vector3.Distance(other.transform.position, snapPoint.snapTransform.position);
                float angle = Quaternion.Angle(other.transform.rotation, snapPoint.snapTransform.rotation);

                Fastener fastener = other.GetComponent<Fastener>();

                if (fastener != null)
                {
                    fastener.SetSocketTransform(snapPoint.snapTransform);
                }

                if ((fastener && distance < fastenerSnapDistance) ||
                    (distance < snapDistance && angle < snapAngle && !componentObject.GetIsPlaced()))
                {
                    float performance = CalculatePerformance(other.transform, snapPoint, fastener != null);
                    Manager.Instance.PerformaceForEachStep.Add(performance);
                    other.attachedRigidbody.isKinematic = true;
                    snapPoint.meshRenderer.enabled = true;

                    other.transform.SetPositionAndRotation(snapPoint.snapTransform.position,
                        snapPoint.snapTransform.rotation);

                    other.GetComponent<Rigidbody>().isKinematic = true;
                    IXRInteractable xrInteractable = other.GetComponent<IXRInteractable>();
                    if (xrInteractable != null)
                    {
                        interactionManager.RegisterInteractable(xrInteractable);
                        Destroy(xrInteractable as MonoBehaviour);
                    }

                    // Add the object to the snapped objects set
                    snappedObjects.Add(other.gameObject);

                    snapPoint.meshRenderer.enabled = false;

                    other.transform.SetParent(snapPoint.snapTransform);


                    componentObject.SetIsPlaced(true);

                    AddGrabbable(other as MeshCollider);

                    componentObject.PlayBuildPopSound();

                    int currentStepId = Manager.Instance.AssemblySequence[Manager.Instance.CurrentStep].stepId;

                    if (!Manager.Instance.CurrentAssembledSequence.ContainsKey(currentStepId))
                    {
                        Manager.Instance.CurrentAssembledSequence.Add(currentStepId, other.gameObject);
                    }

                    Manager.Instance.HideHint();

                    OnComponentPlaced?.Invoke();
                }
            }
        }
    }

    private float CalculatePerformance(Transform component, SnapPoint snapPoint, bool isFastener)
    {
        float distance = Vector3.Distance(component.transform.position, snapPoint.snapTransform.position);
        float angle = Quaternion.Angle(component.transform.rotation, snapPoint.snapTransform.rotation);

        // Use the appropriate snap distance for the calculation
        float effectiveSnapDistance = isFastener ? fastenerSnapDistance : snapDistance;

        // Calculate distance performance
        float distancePerformance = 1.0f - Mathf.Clamp01(distance / effectiveSnapDistance);

        // Calculate rotation performance only if not a fastener
        float rotationPerformance = isFastener ? 1.0f : (1.0f - Mathf.Clamp01(angle / snapAngle));

        // Calculate overall performance
        float overallPerformance = isFastener
            ? distancePerformance
            : (distancePerformance + rotationPerformance) / 2.0f;


        Debug.Log($"Precision Performance: {overallPerformance * 100f}% {component.name}");
        return overallPerformance;
        // Log details for debugging
        
    }


    private void AddGrabbable(MeshCollider collider)
    {
        XRGrabInteractable xrGrabInteractable = GetComponent<XRGrabInteractable>();
        MakeGrabbable makeGrabbable = collider.GetComponent<MakeGrabbable>();
        makeGrabbable.DestroyInteractables();
        if (xrGrabInteractable)
        {
            // Unregister the interactable from the interaction manager
            interactionManager.UnregisterInteractable(xrGrabInteractable as IXRInteractable);


            xrGrabInteractable.throwOnDetach = false;
            xrGrabInteractable.useDynamicAttach = true;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            if (!xrGrabInteractable.colliders.Contains(collider))
                xrGrabInteractable.colliders.Add(collider);
            // Re-register the interactable
            interactionManager.RegisterInteractable(xrGrabInteractable as IXRInteractable);
        }
        else
        {
            // If no interactable is present, add one
            xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.throwOnDetach = false;
            xrGrabInteractable.useDynamicAttach = true;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            if (!xrGrabInteractable.colliders.Contains(collider))
                xrGrabInteractable.colliders.Add(collider);

            // Register the newly added interactable
            interactionManager.RegisterInteractable(xrGrabInteractable as IXRInteractable);
        }


        xrGrabInteractable.enabled = true;
    }
}