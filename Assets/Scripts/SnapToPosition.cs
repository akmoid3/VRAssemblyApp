using System;
using System.Collections.Generic;
using System.Linq;
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
    private float snapDistance = 0.2f;
    private float fastenerSnapDistance = 0.01f;
    private float snapAngle = 5f;
    private List<SnapPoint> snapPoints;
    private HashSet<GameObject> snappedObjects = new HashSet<GameObject>(); // Track snapped objects
    public static event Action OnComponentPlaced;
    private XRInteractionManager interactionManager;
    private bool isKinematic = true;

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



    public void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("SnapPoint");
        
       
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
        
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            grabInteractable.throwOnDetach = false;
            grabInteractable.useDynamicAttach = true;
            grabInteractable.selectMode = InteractableSelectMode.Multiple;
            grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay: " + other.name);
        // Check if the collider is on the OriginalColliders layer
        if (other.gameObject.layer != LayerMask.NameToLayer("OriginalColliders"))
            return;
        if(Manager.Instance.sequenceManager == null)
            return;
        if (other == null ||
            !((StateManager.Instance.CurrentState != State.PlayBack ||
               StateManager.Instance.CurrentState != State.Finish) &&
              Manager.Instance.CurrentStep < Manager.Instance.AssemblySequence.Count))
            return;

        // if (StateManager.Instance.CurrentState != State.PlayBack && !isKinematic)
        // {
        //     isKinematic = true;
        //     this.GetComponent<Rigidbody>().isKinematic = true;
        // }
        
        if (Manager.Instance.ComponentsThatCanSnap.Contains(other.transform.parent))
        {
            CheckSnap(other);
            
                
        }
    }

    private void CheckSnap(Collider other)
    {
        Transform parent = other.transform.parent;
        
        ComponentObject componentObject = parent.GetComponent<ComponentObject>();
        Debug.Log(componentObject.gameObject.name);
        var snapPoint = snapPoints[Manager.Instance.CurrentStep];

        if (!componentObject.IsReleased)
            return;

        if (componentObject != null)
        {
            if (parent.name == snapPoint.componentName ||
                (componentObject.GetGroup() != "None" &&
                 componentObject.GetGroup() == snapPoint.componentObject.GetGroup() &&
                 componentObject.GetType() == snapPoint.componentObject.GetType()))
            {
                float distance = Vector3.Distance(parent.transform.position, snapPoint.snapTransform.position);
                float angle = Quaternion.Angle(parent.transform.rotation, snapPoint.snapTransform.rotation);

                Fastener fastener = parent.GetComponent<Fastener>();

                if (fastener != null)
                {
                    fastener.SetSocketTransform(snapPoint.snapTransform);
                }

                if ((fastener && distance < fastenerSnapDistance) ||
                    (distance < snapDistance && angle < snapAngle && !componentObject.GetIsPlaced()))
                {
                    float performance = CalculatePerformance(parent.transform, snapPoint, fastener != null);

                    if (fastener && componentObject.GetIsPlaced() || componentObject.IsAutomaticSnap)
                        performance = 1;
                    Manager.Instance.PerformaceForEachStep.Add(performance);
                    parent.GetComponent<Rigidbody>().isKinematic = true;
                    snapPoint.meshRenderer.enabled = true;

                    parent.transform.SetPositionAndRotation(snapPoint.snapTransform.position,
                        snapPoint.snapTransform.rotation);

                    parent.GetComponent<Rigidbody>().isKinematic = true;
                    //parent.GetComponent<MeshCollider>().convex = false;

                    IXRInteractable xrInteractable = parent.GetComponent<IXRInteractable>();
                    if (xrInteractable != null)
                    {
                        interactionManager.RegisterInteractable(xrInteractable);
                        Destroy(xrInteractable as MonoBehaviour);
                    }

                    // Add the object to the snapped objects set
                    snappedObjects.Add(other.gameObject);

                    snapPoint.meshRenderer.enabled = true;

                    parent.transform.SetParent(snapPoint.snapTransform);

                    componentObject.SetIsPlaced(true);

                    // if (StateManager.Instance.CurrentState == State.PlayBack && isKinematic)
                    // {
                    //     isKinematic = false;
                    //     this.GetComponent<Rigidbody>().isKinematic = false;
                    // }
                    
                    //AddGrabbable(other as MeshCollider);
                    
                    

                    if (fastener != null)
                    {
                        snapPoint.snapTransform.GetComponent<Collider>().enabled = false;
                    }
                    else
                    {
                        TransferCollidersToSnapPoint(other.transform, snapPoint.snapTransform);
                        AddChildCollidersToParentGrabbable();
                    }
                    componentObject.PlayBuildPopSound();
                    int currentStepId = Manager.Instance.AssemblySequence[Manager.Instance.CurrentStep].stepId;

                    if (!Manager.Instance.CurrentAssembledSequence.ContainsKey(currentStepId))
                    {
                        Manager.Instance.CurrentAssembledSequence.Add(currentStepId, parent.gameObject);
                    }

                    Manager.Instance.HideHint();

                    OnComponentPlaced?.Invoke();
                }
            }
        }
    }

    private void TransferCollidersToSnapPoint(Transform snappedComponent, Transform snapPointTransform)
    {
        MeshCollider newCollider =  snapPointTransform.gameObject.GetComponent<MeshCollider>();
        newCollider.enabled = true;
        if (snappedComponent.gameObject.layer == LayerMask.NameToLayer("OriginalColliders"))
        {
            snappedComponent.gameObject.layer = LayerMask.NameToLayer("Snapped");
        }

        /*MeshCollider[] componentColliders = snappedComponent.GetComponentsInChildren<MeshCollider>();

        foreach (MeshCollider originalCollider in componentColliders)
        {
            originalCollider.gameObject.layer = LayerMask.NameToLayer("OriginalColliders");

            MeshCollider newCollider = snapPointTransform.gameObject.AddComponent<MeshCollider>();
            newCollider.sharedMesh = originalCollider.sharedMesh;
            newCollider.convex = originalCollider.convex;
            newCollider.isTrigger = originalCollider.isTrigger;

            // Cambia layer per il nuovo collider
            newCollider.gameObject.layer = LayerMask.NameToLayer("SnapPointColliders");
        }*/
    }

    private float CalculatePerformance(Transform component, SnapPoint snapPoint, bool isFastener)
    {
        float distance = Vector3.Distance(component.transform.position, snapPoint.snapTransform.position);
        float angle = Quaternion.Angle(component.transform.rotation, snapPoint.snapTransform.rotation);

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
        /*XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
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
            interactionManager?.UnregisterInteractable(grabInteractable as IXRInteractable);
        }

        grabInteractable.colliders.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null && !grabInteractable.colliders.Contains(childCollider))
            {
                grabInteractable.colliders.Add(childCollider);
            }
        }

        interactionManager?.RegisterInteractable(grabInteractable as IXRInteractable);

        grabInteractable.enabled = true;*/
    }


}