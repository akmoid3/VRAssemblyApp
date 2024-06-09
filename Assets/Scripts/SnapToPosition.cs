using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapToPosition : MonoBehaviour
{
    public float snapDistance = 0.1f; // Maximum allowed distance to snap
    public float snapAngle = 5f; // Maximum allowed angle difference to snap

    private List<SnapPoint> snapPoints;

    private void Start()
    {
        // Automatically find all child objects and consider them as snap points
        snapPoints = new List<SnapPoint>();

        foreach (Transform child in transform)
        {
            SnapPoint snapPoint = new SnapPoint
            {
                snapTransform = child,
                componentName = child.name.Replace("SnapPoint", ""),
                meshRenderer = child.GetComponent<MeshRenderer>()
            };
            snapPoints.Add(snapPoint);
        }

        Debug.Log($"Initialized {snapPoints.Count} snap points.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>().isKinematic)
            other.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Object in trigger: {other.name}");

        // Check for snapping
        CheckSnap(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Object exited trigger: {other.name}");

        if (other.GetComponent<Rigidbody>().isKinematic)
            other.GetComponent<Rigidbody>().isKinematic = false;
        // Re-enable all snap point MeshRenderers
        EnableAllSnapPoints();
    }

    private void CheckSnap(Collider other)
    {
        foreach (var snapPoint in snapPoints)
        {
            Debug.Log($"Checking snap point: {snapPoint.snapTransform.name}");

            if (other.name == snapPoint.componentName)
            {
                Debug.Log($"Matching component found: {other.name}");

                float distance = Vector3.Distance(other.transform.position, snapPoint.snapTransform.position);
                float angle = Quaternion.Angle(other.transform.rotation, snapPoint.snapTransform.rotation);

                Debug.Log($"Distance: {distance}, Angle: {angle}");

                if (distance < snapDistance && angle < snapAngle)
                {
                    Debug.Log($"Snapping {other.name} to {snapPoint.snapTransform.name}");

                    other.transform.position = snapPoint.snapTransform.position;
                    other.transform.rotation = snapPoint.snapTransform.rotation;
                    other.GetComponent<Rigidbody>().isKinematic = true; // Freeze the object after snapping

                    if (snapPoint.meshRenderer != null)
                    {
                        snapPoint.meshRenderer.enabled = false; // Disable the MeshRenderer
                    }

                    break;
                }
            }
        }
    }

    private void EnableAllSnapPoints()
    {
        foreach (var snapPoint in snapPoints)
        {
            if (snapPoint.meshRenderer != null)
            {
                snapPoint.meshRenderer.enabled = true;
            }
        }
    }

    [System.Serializable]
    public class SnapPoint
    {
        public Transform snapTransform; // The transform of the snap point
        public string componentName; // The name of the component that should snap here
        public MeshRenderer meshRenderer; // The MeshRenderer of the snap point
    }
}
