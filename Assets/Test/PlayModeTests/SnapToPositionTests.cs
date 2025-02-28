using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
public class SnapToPositionTests
{
    private GameObject managerGO;
    private GameObject stateManagerGO;
    private GameObject xrInteractionManagerGO;
    private GameObject snapParent;       
    private SnapToPosition snapToPosition;
    private GameObject snapPointObj;     
    private GameObject snapCandidate;   

    [SetUp]
    public void Setup()
    {
        // Create dummy Manager.
        managerGO = new GameObject("Manager");
        managerGO.AddComponent<Manager>();
        SequenceManager sequenceManager = new GameObject().AddComponent<SequenceManager>();
        sequenceManager.AssemblySequence.Add(new ComponentData() { stepId = 0 });

        Manager.Instance.sequenceManager = sequenceManager;
        
        Manager.Instance.CurrentStep = 0;

        // Create dummy StateManager.
        stateManagerGO = new GameObject("StateManager");
        var sm = stateManagerGO.AddComponent<StateManager>();
        sm.CurrentState = State.Initialize;

        // Create XRInteractionManager.
        xrInteractionManagerGO = new GameObject("XRInteractionManager");
        xrInteractionManagerGO.AddComponent<XRInteractionManager>();

        // Create the parent object with SnapToPosition.
        snapParent = new GameObject("SnapParent");
        snapToPosition = snapParent.AddComponent<SnapToPosition>();

        // Create a child snap point.
        snapPointObj = new GameObject("SnapPoint");
        snapPointObj.transform.parent = snapParent.transform;
        // Add required components.
        snapPointObj.AddComponent<MeshRenderer>();
        snapPointObj.AddComponent<ComponentObject>();

        // Manually call Start() to populate the snapPoints list.
        snapToPosition.Start();

        // Create a candidate object that should snap.
        // Name it the same as the snap point (to meet the condition in CheckSnap).
        snapCandidate = new GameObject("SnapPoint");
        // Place it near the snap point.
        snapCandidate.transform.position = snapPointObj.transform.position + Vector3.one * 0.05f;
        snapCandidate.transform.rotation = Quaternion.identity;
        // Add components required by CheckSnap.
        snapCandidate.AddComponent<BoxCollider>();
        Rigidbody rb = snapCandidate.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        var candidateComp = snapCandidate.AddComponent<ComponentObject>();
        candidateComp.IsReleased = true;
        candidateComp.SetIsPlaced(false);
        // Register candidate as a component that can snap.
        Manager.Instance.ComponentsThatCanSnap.Add(snapCandidate.transform);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(managerGO);
        Object.DestroyImmediate(stateManagerGO);
        Object.DestroyImmediate(xrInteractionManagerGO);
        Object.DestroyImmediate(snapParent);
        Object.DestroyImmediate(snapCandidate);
    }

    [UnityTest]
    public IEnumerator Test_CheckSnap_SnapsObjectCorrectly()
    {
        // Retrieve the candidate's collider.
        Collider candidateCollider = snapCandidate.GetComponent<Collider>();

        // Use reflection to invoke the private CheckSnap(Collider) method.
        MethodInfo checkSnapMethod = typeof(SnapToPosition).GetMethod("CheckSnap", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(checkSnapMethod, "CheckSnap method not found on SnapToPosition.");
        checkSnapMethod.Invoke(snapToPosition, new object[] { candidateCollider });

        // Wait one frame for changes to take effect.
        yield return null;

        // Verify that the candidate's position and rotation now match the snap point.
        Assert.AreEqual(snapPointObj.transform.position, snapCandidate.transform.position, "The candidate did not snap to the correct position.");
        Assert.AreEqual(snapPointObj.transform.rotation, snapCandidate.transform.rotation, "The candidate did not snap to the correct rotation.");
        // Verify that the candidate is now parented to the snap point.
        Assert.AreEqual(snapPointObj.transform, snapCandidate.transform.parent, "The candidate is not parented to the snap point.");
        // Verify that the candidate's Rigidbody is now set to kinematic.
        Assert.IsTrue(snapCandidate.GetComponent<Rigidbody>().isKinematic, "Candidate's Rigidbody should be kinematic after snapping.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_AddChildCollidersToParentGrabbable_AddsColliders()
    {
        // Create an additional child with a collider under snapParent.
        GameObject childObj = new GameObject("ChildCollider");
        childObj.transform.parent = snapParent.transform;
        BoxCollider childCollider = childObj.AddComponent<BoxCollider>();

        // Invoke the private AddChildCollidersToParentGrabbable() method.
        MethodInfo method = typeof(SnapToPosition).GetMethod("AddChildCollidersToParentGrabbable", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "AddChildCollidersToParentGrabbable method not found.");
        method.Invoke(snapToPosition, null);

        yield return null;

        // Check that an XRGrabInteractable has been added and that its collider list includes the child's collider.
        XRGrabInteractable grabInteractable = snapParent.GetComponent<XRGrabInteractable>();
        Assert.IsNotNull(grabInteractable, "XRGrabInteractable was not added to snapParent.");
        Assert.IsTrue(grabInteractable.colliders.Contains(childCollider), "Child collider was not added to XRGrabInteractable's colliders list.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_TransferCollidersToSnapPoint_AddsNewCollider()
    {
        // Add a MeshCollider to the snap candidate.
        MeshCollider originalCollider = snapCandidate.AddComponent<MeshCollider>();
        originalCollider.convex = true;
        originalCollider.isTrigger = false;

        // Invoke the private TransferCollidersToSnapPoint(Transform, Transform) method.
        MethodInfo method = typeof(SnapToPosition).GetMethod("TransferCollidersToSnapPoint", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "TransferCollidersToSnapPoint method not found.");
        method.Invoke(snapToPosition, new object[] { snapCandidate.transform, snapPointObj.transform });

        yield return null;

        // Verify that the snap point now has an additional MeshCollider with matching properties.
        MeshCollider[] colliders = snapPointObj.GetComponents<MeshCollider>();
        bool found = false;
        foreach (var col in colliders)
        {
            if (col.sharedMesh == originalCollider.sharedMesh &&
                col.convex == originalCollider.convex &&
                col.isTrigger == originalCollider.isTrigger)
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "A new MeshCollider with the expected properties was not added to the snap point.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_CalculatePerformance_ReturnsExpectedValue()
    {
        // Use reflection to invoke the private CalculatePerformance method.
        MethodInfo method = typeof(SnapToPosition).GetMethod("CalculatePerformance", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "CalculatePerformance method not found.");

        // For perfect alignment, set the candidate exactly at the snap point.
        snapCandidate.transform.position = snapPointObj.transform.position;
        snapCandidate.transform.rotation = snapPointObj.transform.rotation;
        // isFastener is false.
        object result = method.Invoke(snapToPosition, new object[] { snapCandidate.transform, new SnapPoint { snapTransform = snapPointObj.transform }, false });
        float performance = (float)result;
        // With zero distance and rotation difference, performance should be 1.
        Assert.AreEqual(1f, performance, 0.001f, "Performance should be 1 when the candidate is perfectly aligned with the snap point.");
        yield return null;
    }
}
