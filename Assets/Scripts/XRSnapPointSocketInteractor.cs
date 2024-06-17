using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// Socket Interactor for holding a group of Interactables at predefined snap points.
    /// </summary>
    /// <remarks>
    /// During Awake, it sets up the snap points based on predefined Transforms.
    /// </remarks>
    public class XRSnapPointSocketInteractor : XRSocketInteractor
    {
        [Space]
        [SerializeField]
        [Tooltip("Maximum distance within which an object can snap to a snap point.")]
        private float snapDistance = 0.1f;

        [SerializeField]
        [Tooltip("Maximum angle difference (in degrees) within which an object can snap to a snap point.")]
        private float snapAngle = 15f;

        private List<Transform> snapPoints = new List<Transform>();
        private readonly HashSet<Transform> usedSnapPoints = new HashSet<Transform>();
        private readonly Dictionary<IXRInteractable, Transform> snapPointByInteractable = new Dictionary<IXRInteractable, Transform>();

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            GatherSnapPoints();
        }

        private void GatherSnapPoints()
        {
            snapPoints.Clear();
            foreach (Transform child in transform)
            {
                snapPoints.Add(child);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            var closestSnapPoint = GetClosestSnapPoint(args.interactableObject);
            if (closestSnapPoint != null)
            {
                usedSnapPoints.Add(closestSnapPoint);
                snapPointByInteractable.Add(args.interactableObject, closestSnapPoint);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (snapPointByInteractable.TryGetValue(args.interactableObject, out var closestSnapPoint))
            {
                usedSnapPoints.Remove(closestSnapPoint);
                snapPointByInteractable.Remove(args.interactableObject);
            }

            base.OnSelectExiting(args);
        }

        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return IsSelecting(interactable)
                   || (HasAvailableSnapPoint(interactable) && !interactable.isSelected && !usedSnapPoints.Contains(GetClosestSnapPoint(interactable)));
        }

        /// <inheritdoc />
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable)
                   && !usedSnapPoints.Contains(GetClosestSnapPoint(interactable));
        }

        /// <inheritdoc />
        public override Transform GetAttachTransform(IXRInteractable interactable)
        {
            if (snapPointByInteractable.TryGetValue(interactable, out var interactableSnapPoint))
                return interactableSnapPoint;

            return GetClosestSnapPoint(interactable);
        }

        private bool HasAvailableSnapPoint(IXRInteractable interactable)
        {
            foreach (var snapPoint in snapPoints)
            {
                if (!usedSnapPoints.Contains(snapPoint) && snapPoint.name == interactable.transform.name)
                {
                    return true;
                }
            }
            return false;
        }

        private Transform GetClosestSnapPoint(IXRInteractable interactable)
        {
            Transform closestSnapPoint = null;
            float closestDistance = snapDistance;
            float closestAngle = snapAngle;

            foreach (var snapPoint in snapPoints)
            {
                if (usedSnapPoints.Contains(snapPoint) || snapPoint.name != interactable.transform.name)
                    continue;

                float distance = Vector3.Distance(interactable.GetAttachTransform(this).position, snapPoint.position);
                if (distance > closestDistance)
                    continue;

                float angle = Quaternion.Angle(interactable.GetAttachTransform(this).rotation, snapPoint.rotation);
                if (angle > closestAngle)
                    continue;

                closestSnapPoint = snapPoint;
                closestDistance = distance;
                closestAngle = angle;
            }

            return closestSnapPoint;
        }
    }
}
