using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapToPosition : MonoBehaviour
{
    public float snapDistance = 0.1f;
    public float snapAngle = 5f;
    private List<SnapPoint> snapPoints;
    private HashSet<GameObject> snappedObjects = new HashSet<GameObject>(); // Track snapped objects
    public static event Action OnComponentPlaced;
    private XRInteractionManager interactionManager;

    private void Awake()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();

    }
    private void Start()
    {
        snapPoints = new List<SnapPoint>();

        foreach (Transform child in transform)
        {
            SnapPoint snapPoint = new SnapPoint
            {
                snapTransform = child,
                componentName = child.name,
                meshRenderer = child.GetComponent<MeshRenderer>(),
                componentObject = child.GetComponent<ComponentObject>()
            };
            snapPoints.Add(snapPoint);
        }

    }

    private void OnTriggerStay(Collider other)
    {

        // Check if the object has already been snapped
        if (snappedObjects.Contains(other.gameObject))
            return;

        if (StateManager.Instance.CurrentState == State.PlayBack)
        {
            CheckSnap(other);
        }
    }

    private void CheckSnap(Collider other)
    {
        if (other == null)
            return;
        foreach (var snapPoint in snapPoints)
        {
            ComponentObject componentObject = other.GetComponent<ComponentObject>();
            if (componentObject != null )
            {

                if (other.name == snapPoint.componentName || (componentObject.GetGroup() != ComponentObject.Group.None && componentObject.GetGroup() == snapPoint.componentObject.GetGroup()))
                {

                    float distance = Vector3.Distance(other.transform.position, snapPoint.snapTransform.position);
                    float angle = Quaternion.Angle(other.transform.rotation, snapPoint.snapTransform.rotation);

                    Fastener fastener = other.GetComponent<Fastener>();

                    if (fastener != null)
                    {
                        fastener.SetSocketTransform(snapPoint.snapTransform);
                    }

                    if ((distance < snapDistance && angle < snapAngle) || (fastener && distance < snapDistance))
                    {
                        other.attachedRigidbody.isKinematic = false;

                        snapPoint.meshRenderer.enabled = true;

                        other.transform.SetPositionAndRotation(snapPoint.snapTransform.position, snapPoint.snapTransform.rotation);

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

                        AddGrabbable(other);

                        OnComponentPlaced?.Invoke();


                        break;
                    }
                }
            }
        }



    }

    private void AddGrabbable(Collider collider)
    {
        XRGrabInteractable xrGrabInteractable = GetComponent<XRGrabInteractable>();

        if (xrGrabInteractable)
        {
            // Unregister the interactable from the interaction manager
            interactionManager.UnregisterInteractable(xrGrabInteractable as IXRInteractable);

            // Reconfigure the existing component instead of destroying it
            xrGrabInteractable.throwOnDetach = false;
            xrGrabInteractable.useDynamicAttach = true;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
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

            // Register the newly added interactable
            interactionManager.RegisterInteractable(xrGrabInteractable as IXRInteractable);
        }

        xrGrabInteractable.enabled = true;
    }


    [System.Serializable]
    public class SnapPoint
    {
        public Transform snapTransform;
        public string componentName;
        public MeshRenderer meshRenderer;
        public ComponentObject componentObject;

    }
}
