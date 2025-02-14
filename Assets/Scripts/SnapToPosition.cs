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

                    if (fastener && componentObject.GetIsPlaced() || componentObject.IsAutomaticSnap)
                        performance = 1;
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

                    snapPoint.meshRenderer.enabled = true;

                    other.transform.SetParent(snapPoint.snapTransform);


                    componentObject.SetIsPlaced(true);

                    //AddGrabbable(other as MeshCollider);
                    TransferCollidersToSnapPoint(other.transform, snapPoint.snapTransform);
                    AddChildCollidersToParentGrabbable();
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

    private void TransferCollidersToSnapPoint(Transform snappedComponent, Transform snapPointTransform)
    {
        // Ottieni tutti i MeshCollider presenti sul componente (e nei suoi figli)
        MeshCollider[] componentColliders = snappedComponent.GetComponentsInChildren<MeshCollider>();
    
        foreach (MeshCollider originalCollider in componentColliders)
        {
            // Cambia layer per il collider originale
            originalCollider.gameObject.layer = LayerMask.NameToLayer("OriginalColliders");

            // Clona il collider nel gameObject dello snap point
            MeshCollider newCollider = snapPointTransform.gameObject.AddComponent<MeshCollider>();
            newCollider.sharedMesh = originalCollider.sharedMesh;
            newCollider.convex = originalCollider.convex;
            newCollider.isTrigger = originalCollider.isTrigger;

            // Cambia layer per il nuovo collider
            newCollider.gameObject.layer = LayerMask.NameToLayer("SnapPointColliders");
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


        return overallPerformance;
        
    }


    private void AddChildCollidersToParentGrabbable()
    {
        // Try to get or add an XRGrabInteractable on the parent (this object)
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            grabInteractable.throwOnDetach = false;
            grabInteractable.useDynamicAttach = true;
            grabInteractable.selectMode = InteractableSelectMode.Multiple;
            grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }
        else
        {
            // Unregister if needed before updating
            interactionManager?.UnregisterInteractable(grabInteractable as IXRInteractable);
        }

        // Optionally clear any existing colliders from the XRGrabInteractable
        grabInteractable.colliders.Clear();

        // Retrieve all colliders from the children of this GameObject.
        // This will also include the parent's collider if it exists.
        Collider[] childColliders = GetComponentsInChildren<Collider>();

        // If you want only the children's colliders, you can remove the parent's own collider:
        // Collider parentCollider = GetComponent<Collider>();
        // childColliders = childColliders.Where(c => c != parentCollider).ToArray();

        // Add each child collider to the XRGrabInteractable's collider list.
        foreach (Collider col in childColliders)
        {
            if (!grabInteractable.colliders.Contains(col))
            {
                grabInteractable.colliders.Add(col);
            }
        }

        // Re-register with the interaction manager, if necessary.
        interactionManager?.RegisterInteractable(grabInteractable as IXRInteractable);

        // Enable the XRGrabInteractable component.
        grabInteractable.enabled = true;
    }

}