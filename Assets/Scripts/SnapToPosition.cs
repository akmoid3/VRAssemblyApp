using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapToPosition : MonoBehaviour
{
    public float snapDistance = 0.1f;
    public float snapAngle = 5f;
    private List<SnapPoint> snapPoints;
    private HashSet<GameObject> snappedObjects = new HashSet<GameObject>(); // Track snapped objects
    public SequenceRecorder recorder; // Reference to the BuildSequenceRecorder
    private bool isSequenceSaved = false; // Flag to track if the sequence is already saved

    private void Start()
    {
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

        // Check if the object has already been snapped
        if (snappedObjects.Contains(other.gameObject))
            return;

        CheckSnap(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Object exited trigger: {other.name}");

        // Remove the object from the snapped objects set
        snappedObjects.Remove(other.gameObject);

        if (other.GetComponent<Rigidbody>().isKinematic)
            other.GetComponent<Rigidbody>().isKinematic = false;
        EnableAllSnapPoints();
    }

    private void CheckSnap(Collider other)
    {
        foreach (var snapPoint in snapPoints)
        {

            if (other.name == snapPoint.componentName)
            {

                float distance = Vector3.Distance(other.transform.position, snapPoint.snapTransform.position);
                float angle = Quaternion.Angle(other.transform.rotation, snapPoint.snapTransform.rotation);

                if (distance < snapDistance && angle < snapAngle)
                {

                    other.transform.position = snapPoint.snapTransform.position;
                    other.transform.rotation = snapPoint.snapTransform.rotation;
                    other.GetComponent<Rigidbody>().isKinematic = true;

                    // Disable the MeshRenderer
                    if (snapPoint.meshRenderer != null)
                    {
                        snapPoint.meshRenderer.enabled = false;
                    }

                    // Record the snapped component's name
                    recorder.RecordAction(other.name);

                    // Add the object to the snapped objects set
                    snappedObjects.Add(other.gameObject);

                    break;
                }
            }
        }
    }

    private bool AreAllSnapPointsFilled()
    {
        foreach (var snapPoint in snapPoints)
        {
            if (snapPoint.meshRenderer != null && snapPoint.meshRenderer.enabled)
            {
                return false; // At least one snap point is still empty
            }
        }
        return true; // All snap points are filled
    }

    private void SaveSequence()
    {
        recorder.SaveSequenceToJson("Assets/BuildSequence.json"); // Save the sequence to JSON
        isSequenceSaved = true; // Update the flag to indicate that the sequence is saved
        Debug.Log("Sequence saved.");
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

    private void Update()
    {
        // Check if all snap points are complete and the sequence is not saved yet
        if (!isSequenceSaved && AreAllSnapPointsFilled())
        {
            SaveSequence();
        }
    }

    [System.Serializable]
    public class SnapPoint
    {
        public Transform snapTransform;
        public string componentName;
        public MeshRenderer meshRenderer;
    }
}
