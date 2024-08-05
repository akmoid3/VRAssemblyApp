using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// Socket Interactor for holding a group of Interactables using predefined snappoints.
    /// </summary>
    /// <remarks>
    /// The snappoints are child objects of a parent object.
    /// </remarks>
    public class XRSnapPointSocketInteractor : XRSocketInteractor
    {
        [Space]
        [SerializeField]
        [Tooltip("The parent transform that holds the snappoints.")]

        List<Transform> snappoints = new List<Transform>();
        [SerializeField]
        float distanceThreshold = 0.5f;
        [SerializeField]
        float angleThreshold = 10f;

        readonly HashSet<Transform> usedSnappoints = new HashSet<Transform>();
        readonly Dictionary<IXRInteractable, Transform> snappointByInteractable = new Dictionary<IXRInteractable, Transform>();

        bool hasEmptySnappoint => usedSnappoints.Count < snappoints.Count;

         // Getter and Setter for distanceThreshold
        public float DistanceThreshold
        {
            get => distanceThreshold;
            set => distanceThreshold = value;
        }

        // Getter and Setter for angleThreshold
        public float AngleThreshold
        {
            get => angleThreshold;
            set => angleThreshold = value;
        }
        void CollectSnappoints()
        {
            snappoints.Clear();
            foreach (Transform child in this.transform)
            {
                snappoints.Add(child);
            }
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            CollectSnappoints();

            // The same material is used on both situations
            interactableCantHoverMeshMaterial = interactableHoverMeshMaterial;
        }


        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            var snappoint = GetAttachTransform(args.interactableObject);
            usedSnappoints.Add(snappoint);
            snappointByInteractable.Add(args.interactableObject, snappoint);
            GameObject gameObject = args.interactableObject.transform.gameObject;
            XRGrabInteractable xr = gameObject.GetComponent<XRGrabInteractable>();
            xr.selectMode = InteractableSelectMode.Single;
            snappoint.GetComponent<MeshRenderer>().enabled = false;

            Fastener fastener = gameObject.GetComponent<Fastener>();
            if (fastener != null)
            {
                fastener.SetSocketTransform(snappoint);
            }
        }


        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            var snappoint = snappointByInteractable[args.interactableObject];
            usedSnappoints.Remove(snappoint);
            snappointByInteractable.Remove(args.interactableObject);
            GameObject gameObject = args.interactableObject.transform.gameObject;
            XRGrabInteractable xr = gameObject.GetComponent<XRGrabInteractable>();
            xr.selectMode = InteractableSelectMode.Multiple;
            snappoint.GetComponent<MeshRenderer>().enabled = true;


            Fastener fastener = gameObject.GetComponent<Fastener>();
            if (fastener != null)
            {
                fastener.SetSocketTransform(null);
            }

            base.OnSelectExiting(args);
        }


        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            var attachTransform = GetAttachTransform(interactable);
            if (attachTransform != null)
            {

                return IsSelecting(interactable)
                       || (hasEmptySnappoint && !interactable.isSelected && !usedSnappoints.Contains(attachTransform));
            }
            else return false;
        }

        /// <inheritdoc />
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            var attachTransform = GetAttachTransform(interactable);
            if(attachTransform != null)
            {
                return base.CanHover(interactable)
                  && !usedSnappoints.Contains(attachTransform);
            }else return false;
        }

        /// <inheritdoc />
        public override Transform GetAttachTransform(IXRInteractable interactable)
        {
            if (snappointByInteractable.TryGetValue(interactable, out var interactableSnappoint))
                return interactableSnappoint;

            var interactableName = interactable.transform.name;
            Transform matchingSnappoint = null;

            foreach (var snappoint in snappoints)
            {
                if (snappoint.name.Equals(interactableName))
                {
                    // Check if the snappoint is not already used
                    if (!usedSnappoints.Contains(snappoint))
                    {
                        float distance = Vector3.Distance(interactable.GetAttachTransform(this).position, snappoint.position);
                        if (distance < distanceThreshold)
                        {
                            float angle = Quaternion.Angle(interactable.GetAttachTransform(this).rotation, snappoint.rotation);
                            if (angle < angleThreshold)
                            {
                                matchingSnappoint = snappoint;
                                break;
                            }
                        }
                    }
                }
            }

            return matchingSnappoint;
        }

    }
}
