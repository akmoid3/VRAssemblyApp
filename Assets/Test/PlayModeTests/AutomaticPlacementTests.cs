using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AutomaticPlacementManagerTests
{
    private GameObject managerObject;
    private AutomaticPlacementManager placementManager;
    private GameObject componentObject;      
    private GameObject snapInteractor;      
    private Transform snapPoint;             
    private StateManager stateManager;
    [SetUp]
    public void Setup()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        
        managerObject = new GameObject("PlacementManager");
        placementManager = managerObject.AddComponent<AutomaticPlacementManager>();

        snapInteractor = new GameObject("Interactor");
        snapInteractor.AddComponent<SnapToPosition>();

        GameObject snapPointGO = new GameObject("SnapPoint");
        snapPointGO.transform.position = new Vector3(5f, 0f, 0f);
        snapPointGO.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        snapPointGO.transform.SetParent(snapInteractor.transform);

        snapPoint = snapPointGO.transform;

        

        
        componentObject = new GameObject("ComponentToPlace");
        componentObject.AddComponent<ComponentObject>();

        componentObject.transform.position = Vector3.zero;
        componentObject.transform.rotation = Quaternion.identity;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(managerObject);
        Object.DestroyImmediate(snapInteractor);
        Object.DestroyImmediate(componentObject);
        Object.DestroyImmediate(stateManager);
    }

    [UnityTest]
    public IEnumerator TestPlaceCurrentStepComponent_MovesComponentOverTime()
    {
        int stepIndex = 0;
        float timeMovement = 0.5f;

        placementManager.PlaceCurrentStepComponent(stepIndex, componentObject.transform, 
            snapInteractor.GetComponent<SnapToPosition>(), timeMovement);

        
        yield return new WaitForSeconds(0.6f);

        Assert.AreEqual(snapPoint.position, componentObject.transform.position, "Component should be at the snap point position after placement.");
        Assert.AreEqual(snapPoint.rotation, componentObject.transform.rotation, "Component should have the snap point rotation after placement.");

        ComponentObject compObj = componentObject.GetComponent<ComponentObject>();
        Assert.IsTrue(compObj.IsReleased, "ComponentObject.IsReleased should be true after placement.");
    }
    
    [UnityTest]
    public IEnumerator TestPlaceStepComponent_MovesComponentImmediately()
    {
        int stepIndex = 0; 

        placementManager.PlaceStepComponent(stepIndex, componentObject.transform, 
            snapInteractor.GetComponent<SnapToPosition>());

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(snapPoint.position, componentObject.transform.position, "Component should be moved immediately to the snap point position.");
        Assert.AreEqual(snapPoint.rotation, componentObject.transform.rotation, "Component should be rotated immediately to the snap point rotation.");

        ComponentObject compObj = componentObject.GetComponent<ComponentObject>();
        Assert.IsTrue(compObj.IsReleased, "ComponentObject.IsReleased should be true after placement.");
    }

    [UnityTest]
    public IEnumerator TestSmoothMoveComponent_MovesComponentGradually()
    {
        componentObject.transform.position = Vector3.zero;
        componentObject.transform.rotation = Quaternion.identity;
        float duration = 1.0f;

        yield return placementManager.SmoothMoveComponent(componentObject.transform, snapPoint, duration);

        // Assert:
        Assert.AreEqual(snapPoint.position, componentObject.transform.position, "After smooth move, component position should match the snap point position.");
        Assert.AreEqual(snapPoint.rotation, componentObject.transform.rotation, "After smooth move, component rotation should match the snap point rotation.");
    }

    [Test]
    public void TestMoveComponent_SetsPositionAndRotation()
    {
        componentObject.transform.position = new Vector3(10f, 10f, 10f);
        componentObject.transform.rotation = Quaternion.identity;

        placementManager.MoveComponent(componentObject.transform, snapPoint);

        Assert.AreEqual(snapPoint.position, componentObject.transform.position, "MoveComponent should set the component's position to the snap point's position.");
        Assert.AreEqual(snapPoint.rotation, componentObject.transform.rotation, "MoveComponent should set the component's rotation to the snap point's rotation.");
    }
}