using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

[TestFixture]
public class SnapToPositionTests
{
    private GameObject snapToPositionObject;
    private SnapToPosition snapToPosition;
    private XRInteractionManager interactionManager;
    private GameObject testComponent;
    private ComponentObject componentObject;
    private Collider testCollider;
    private bool componentPlacedEventCalled;

    [SetUp]
    public void Setup()
    {
        // Create the interaction manager
        var interactionManagerObject = new GameObject("InteractionManager");
        interactionManager = interactionManagerObject.AddComponent<XRInteractionManager>();

        // Create SnapToPosition object with child snap points
        snapToPositionObject = new GameObject("SnapToPosition");
        snapToPosition = snapToPositionObject.AddComponent<SnapToPosition>();

        // Create a test snap point
        var snapPoint = new GameObject("TestSnapPoint");
        snapPoint.transform.SetParent(snapToPositionObject.transform);
        snapPoint.AddComponent<MeshRenderer>();

        // Create a test component that should snap
        testComponent = new GameObject("TestComponent");
        testComponent.AddComponent<Rigidbody>();
        testCollider = testComponent.AddComponent<BoxCollider>();
        componentObject = testComponent.AddComponent<ComponentObject>();

        // Reset the event call flag
        componentPlacedEventCalled = false;
        SnapToPosition.OnComponentPlaced += HandleComponentPlaced;

        // Setup Manager for testing
        SetupManager();
    }

    [TearDown]
    public void TearDown()
    {
        // Unregister from event
        SnapToPosition.OnComponentPlaced -= HandleComponentPlaced;

        // Destroy test objects
        Object.DestroyImmediate(snapToPositionObject);
        Object.DestroyImmediate(testComponent);
        Object.DestroyImmediate(interactionManager.gameObject);
        
        // Clean up the manager
        if (Manager.Instance != null)
        {
            Object.DestroyImmediate(Manager.Instance.gameObject);
        }
    }

    private void HandleComponentPlaced()
    {
        componentPlacedEventCalled = true;
    }

    private void SetupManager()
    {
        // Create and setup Manager singleton for testing
        var managerObject = new GameObject("Manager");
        var manager = managerObject.AddComponent<Manager>();
        
        // Setup sequence manager
        var sequenceManagerObject = new GameObject("SequenceManager");
        var sequenceManager = sequenceManagerObject.AddComponent<SequenceManager>();
        sequenceManagerObject.transform.SetParent(managerObject.transform);
        manager.sequenceManager = sequenceManager;
        
        // Setup test assembly sequence
        manager.AssemblySequence = new List<ComponentData>
        {
            new ComponentData() { stepId = 1, componentName = "TestSnapPoint" }
        };
        
        manager.CurrentStep = 0;
        manager.PerformaceForEachStep = new List<float>();
        manager.CurrentAssembledSequence = new Dictionary<int, GameObject>();
        manager.ComponentsThatCanSnap = new List<Transform> { testComponent.transform };
    }

    [Test]
    public void StartMethod_InitializesSnapPoints()
    {
        // Act
        snapToPosition.Start();

        // Assert - Use reflection to access private field
        var snapPointsField = typeof(SnapToPosition).GetField("snapPoints", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var snapPoints = snapPointsField.GetValue(snapToPosition) as List<SnapPoint>;

        Assert.IsNotNull(snapPoints);
        Assert.AreEqual(1, snapPoints.Count);
        Assert.AreEqual("TestSnapPoint", snapPoints[0].componentName);
    }

    [Test]
    public void SnapDistance_GetSet_WorksCorrectly()
    {
        // Arrange
        float newValue = 0.5f;

        // Act
        snapToPosition.SnapDistance = newValue;
        float result = snapToPosition.SnapDistance;

        // Assert
        Assert.AreEqual(newValue, result);
    }

    [Test]
    public void SnapAngle_GetSet_WorksCorrectly()
    {
        // Arrange
        float newValue = 10f;

        // Act
        snapToPosition.SnapAngle = newValue;
        float result = snapToPosition.SnapAngle;

        // Assert
        Assert.AreEqual(newValue, result);
    }

    [UnityTest]
    public IEnumerator OnTriggerStay_WithValidComponent_SnapsCorrectly()
    {
        // Arrange
        snapToPosition.Start();
        
        // Set up component for snapping
        componentObject.IsReleased = true;
        StateManager state = new GameObject().AddComponent<StateManager>();
        
        // Position the component near the snap point
        testComponent.transform.position = snapToPositionObject.transform.GetChild(0).position + new Vector3(0.05f, 0, 0);
        testComponent.transform.rotation = snapToPositionObject.transform.GetChild(0).rotation;

        // Act - Simulate a trigger stay event
        snapToPosition.SendMessage("OnTriggerStay", testCollider);
        
        // Wait a frame for processing
        yield return null;

        // Assert - Check if component is snapped
        Assert.IsTrue(componentObject.GetIsPlaced());
        Assert.IsTrue(componentPlacedEventCalled);
        
        GameObject.Destroy(state);
    }

    [UnityTest]
    public IEnumerator CheckSnap_WithFastener_UsesCorrectSnapDistance()
    {
        // Arrange
        snapToPosition.Start();
        
        // Add Fastener component
        var fastener = testComponent.AddComponent<Fastener>();
        
        // Set up component for snapping
        componentObject.IsReleased = true;

        
        // Position the fastener very close to snap point - within fastenerSnapDistance
        testComponent.transform.position = snapToPositionObject.transform.GetChild(0).position + new Vector3(0.005f, 0, 0);
        testComponent.transform.rotation = snapToPositionObject.transform.GetChild(0).rotation;

        // Act - Simulate a trigger stay event
        snapToPosition.SendMessage("OnTriggerStay", testCollider);
        
        // Wait a frame for processing
        yield return null;

        // Assert
        Assert.IsTrue(componentObject.GetIsPlaced());
        Assert.IsTrue(componentPlacedEventCalled);
    }

    [Test]
    public void CalculatePerformance_ReturnsCorrectValue()
    {
        // Arrange
        snapToPosition.Start();
        float distanceFromIdeal = 0.05f;
        float angleFromIdeal = 2.5f;
        bool isFastener = false;
        
        // Position test component at known distance/angle from snap point
        var snapPointTransform = snapToPositionObject.transform.GetChild(0);
        testComponent.transform.position = snapPointTransform.position + new Vector3(distanceFromIdeal, 0, 0);
        testComponent.transform.rotation = Quaternion.Euler(angleFromIdeal, 0, 0) * snapPointTransform.rotation;
        
        // Act - Call CalculatePerformance via reflection
        var methodInfo = typeof(SnapToPosition).GetMethod("CalculatePerformance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var snapPointsField = typeof(SnapToPosition).GetField("snapPoints", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var snapPoints = snapPointsField.GetValue(snapToPosition) as List<SnapPoint>;
        
        float performance = (float)methodInfo.Invoke(snapToPosition, 
            new object[] { testComponent.transform, snapPoints[0], isFastener });
        
        // Assert - Check if performance calculation is working
        // Performance should be something between 0 and 1
        Assert.Greater(performance, 0);
        Assert.LessOrEqual(performance, 1);
    }

    [Test]
    public void AddChildCollidersToParentGrabbable_AddsCollidersCorrectly()
    {
        // Arrange
        snapToPosition.Start();
        
        // Act - Call the method via reflection
        var methodInfo = typeof(SnapToPosition).GetMethod("AddChildCollidersToParentGrabbable", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(snapToPosition, null);
        
        // Assert
        var grabInteractable = snapToPositionObject.GetComponent<XRGrabInteractable>();
        Assert.IsNotNull(grabInteractable);
        Assert.AreEqual(1, grabInteractable.colliders.Count);
    }

    [Test]
    public void TransferCollidersToSnapPoint_TransfersCollidersCorrectly()
    {
        // Arrange
        snapToPosition.Start();
        
        // Add a MeshCollider to test component
        MeshCollider meshCollider = testComponent.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = new Mesh();
        meshCollider.convex = true;
        
        var snapPointTransform = snapToPositionObject.transform.GetChild(0);
        
        // Act - Call the method via reflection
        var methodInfo = typeof(SnapToPosition).GetMethod("TransferCollidersToSnapPoint", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo.Invoke(snapToPosition, new object[] { testComponent.transform, snapPointTransform });
        
        // Assert
        var newCollider = snapPointTransform.GetComponent<MeshCollider>();
        Assert.IsNotNull(newCollider);
        Assert.AreEqual(LayerMask.NameToLayer("SnapPointColliders"), snapPointTransform.gameObject.layer);
    }
}