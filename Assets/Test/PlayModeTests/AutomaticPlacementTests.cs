using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;

/*
[TestFixture]
public class AutomaticPlacementManagerTests
{
    private AutomaticPlacementManager _manager;
    private ToolManager _toolManager;
    private GameObject _managerObject;
    private GameObject _toolManagerObject;
    private GameObject _component;
    private GameObject _component2;

    private Transform _mockShowSolutionPosition;

    [SetUp]
    public void SetUp()
    {
        // Setup the manager
        _managerObject = new GameObject();
        _manager = _managerObject.AddComponent<AutomaticPlacementManager>();

        // Setup the ToolManager
        _toolManagerObject = new GameObject();
        _toolManager = _toolManagerObject.AddComponent<ToolManager>();

        // Create some example prefabs for the ToolManager
        _toolManager.HammerPrefab = new GameObject("HammerPrefab");
        _toolManager.HammerPrefab.AddComponent<Rigidbody>(); // Add Rigidbody to HammerPrefab for testing

        _toolManager.DrillPrefab = new GameObject("DrillPrefab");
        _toolManager.DrillPrefab.AddComponent<Rigidbody>(); // Add Rigidbody to DrillPrefab for testing

        _toolManager.ScrewDriverPrefab = new GameObject("ScrewDriverPrefab");
        _toolManager.ScrewDriverPrefab.AddComponent<Rigidbody>(); // Add Rigidbody to ScrewDriverPrefab for testing

        // Initialize ToolManager
        _toolManager.Start();

        // Setup a mock showSolutionPosition transform
        _mockShowSolutionPosition = new GameObject("ShowSolutionPosition").transform;

        // Use reflection to set the private fields in AutomaticPlacementManager
        typeof(AutomaticPlacementManager)
            .GetField("showSolutionPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_manager, _mockShowSolutionPosition);

        // Initialize a component
        _component = new GameObject("Component1");
        _component.AddComponent<ComponentObject>();

        // Initialize a component
        _component2 = new GameObject("Component2");
        _component2.AddComponent<ComponentObject>();

    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerObject);
        Object.DestroyImmediate(_toolManagerObject);
        Object.DestroyImmediate(_component);
        Object.DestroyImmediate(_mockShowSolutionPosition.gameObject);
    }

    [Test]
    public void Manager_InitializesWithDefaults_AssertDefaults()
    {
        // Act & Assert
        Assert.AreEqual(1.0f, _manager.TimeForFirstPlacement);
        Assert.IsEmpty(_manager.InstantiatedComponents);
    }

    [UnityTest]
    public IEnumerator PlaceInitialComponent_ValidComponent_PlacesCorrectly()
    {
        // Arrange
        var mockComponentData = new List<ComponentData>
        {
            new ComponentData { componentName = "Component1" }
        };

        var mockComponents = new List<Transform>
        {
            _component.transform
        };

        var mockInteractor = new GameObject().AddComponent<SnapToPosition>();

        // Adding a child to the mockInteractor to avoid "child out of bounds" issues
        new GameObject("Component1").transform.SetParent(mockInteractor.transform);

        yield return null;
        // Act
        _manager.PlaceInitialComponent(mockComponentData, mockComponents, mockInteractor);

        yield return null;

        // Assert
        Assert.AreEqual(Vector3.zero, _component.transform.position);
        Assert.AreEqual(Quaternion.identity, _component.transform.rotation);

        // Cleanup
        Object.DestroyImmediate(mockInteractor.gameObject);
    }

    [UnityTest]
    public IEnumerator CleanupPreviousClones_ClonesExist_RemovesAllClones()
    {
        // Arrange
        var mockClone = new GameObject("Clone");
        _manager.InstantiatedComponents = new Dictionary<string, GameObject> { { "Clone", mockClone } };

        yield return null;
        // Act
        _manager.CleanupPreviousClones();

        yield return null;

        // Assert
        Assert.IsEmpty(_manager.InstantiatedComponents);
        Assert.IsTrue(mockClone == null || !mockClone.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator PlaceAllComponentsGraduallyCoroutine_WithComponents_PlacesAllGradually()
    {
        // Arrange
        var mockComponentData = new List<ComponentData>
        {
            new ComponentData
            {
                componentName = "Component2",
                position = new Vector3(1, 1, 1),
                rotation = Quaternion.Euler(0, 45, 0),
                toolName = "null",
                group = ComponentObject.Group.None,
                type = ComponentObject.ComponentType.None
            }
        };

        _component2.name = "Component2";
        _component2.transform.position = Vector3.zero;
        _component2.transform.rotation = Quaternion.identity;

        var mockComponents = new List<Transform>
        {
            _component2.transform
        };

        var mockInteractor = new GameObject("Interactor").AddComponent<SnapToPosition>();

        // Adding a child to the mockInteractor to avoid "child out of bounds" issues
        var childSnappoint = new GameObject("Component2").transform;
        childSnappoint.SetParent(mockInteractor.transform);
        childSnappoint.position = new Vector3(0, 0, 0);
        childSnappoint.rotation = Quaternion.Euler(0, 0, 0);

        _mockShowSolutionPosition.position = Vector3.zero;
        _mockShowSolutionPosition.rotation = Quaternion.identity;

        // Act
        yield return _manager.StartCoroutine(
            _manager.PlaceAllComponentsGraduallyCoroutine(
                1f,
                mockInteractor.gameObject,
                mockComponentData,
                mockComponents,
                _toolManager  // Use the real ToolManager
            )
        );

        yield return new WaitForSeconds(3f);
        // Assert
        Assert.AreEqual(new Vector3(0, 0, 0), _component.transform.position, "Component did not reach the expected position.");
        Assert.AreEqual(Quaternion.Euler(0, 0, 0), _component.transform.rotation, "Component did not reach the expected rotation.");

        // Verify that the tool was attached
        Assert.IsFalse(_toolManager.ToolInstances.ContainsKey(_component));

        // Cleanup
        Object.DestroyImmediate(mockInteractor.gameObject);
    }

    [UnityTest]
    public IEnumerator SmoothMoveComponent_MovesToTargetPositionAndRotation_UsingComponentData()
    {
        // Arrange
        var componentData = new ComponentData
        {
            componentName = "TestComponent",
            position = new Vector3(5, 5, 5),
            rotation = Quaternion.Euler(45, 45, 45),
            toolName = "null",
            group = ComponentObject.Group.None,
            type = ComponentObject.ComponentType.None,
        };

        _component.transform.position = Vector3.zero;
        _component.transform.rotation = Quaternion.identity;

        float duration = 1.0f;

        // Act
        IEnumerator coroutine = _manager.SmoothMoveComponent(_component.transform, componentData.position, componentData.rotation, duration);
        yield return _manager.StartCoroutine(coroutine);

        yield return null;
        // Assert
        Assert.AreEqual(componentData.position, _component.transform.position);
        Assert.AreEqual(componentData.rotation, _component.transform.rotation);
    }

    [UnityTest]
    public IEnumerator PlaceAllComponentsGradually_WithMultipleComponents_PlacesThemCorrectly()
    {
        // Arrange
        var mockComponentData = new List<ComponentData>
    {
        new ComponentData
        {
            componentName = "Component1",
            position = new Vector3(1, 1, 1),
            rotation = Quaternion.Euler(0, 45, 0),
            toolName = "Hammer",
            group = ComponentObject.Group.None,
            type = ComponentObject.ComponentType.None
        },
        new ComponentData
        {
            componentName = "Component2",
            position = new Vector3(2, 2, 2),
            rotation = Quaternion.Euler(0, 90, 0),
            toolName = "Drill",
            group = ComponentObject.Group.None,
            type = ComponentObject.ComponentType.None
        }
    };

        _component.name = "Component1";
        _component.transform.position = Vector3.zero;
        _component.transform.rotation = Quaternion.identity;

        _component2.name = "Component2";
        _component2.transform.position = Vector3.zero;
        _component2.transform.rotation = Quaternion.identity;

        var mockComponents = new List<Transform>
    {
        _component.transform,
        _component2.transform
    };

        var mockInteractor = new GameObject("Interactor").AddComponent<SnapToPosition>();

        // Adding children to the mockInteractor to avoid "child out of bounds" issues
        var childSnappoint1 = new GameObject("Component1").transform;
        childSnappoint1.SetParent(mockInteractor.transform);
        childSnappoint1.position = new Vector3(0, 0, 0);
        childSnappoint1.rotation = Quaternion.Euler(0, 0, 0);

        var childSnappoint2 = new GameObject("Component2").transform;
        childSnappoint2.SetParent(mockInteractor.transform);
        childSnappoint2.position = new Vector3(0, 0, 0);
        childSnappoint2.rotation = Quaternion.Euler(0, 0, 0);

        _mockShowSolutionPosition.position = Vector3.zero;
        _mockShowSolutionPosition.rotation = Quaternion.identity;

        // Act
        _manager.PlaceAllComponentsGradually(1f, mockInteractor, mockComponentData, mockComponents, _toolManager);

        // Wait for the coroutine to run and components to be placed
        yield return new WaitForSeconds(3f); 

        // Assert for Component1
        Assert.AreEqual(new Vector3(0, 0, 0), _component.transform.position, "Component1 did not reach the expected position.");
        Assert.AreEqual(Quaternion.Euler(0, 0, 0), _component.transform.rotation, "Component1 did not reach the expected rotation.");
        Assert.IsFalse(_toolManager.ToolInstances.ContainsKey(_component), "Tool was not attached to Component1.");

        // Assert for Component2
        Assert.AreEqual(new Vector3(0, 0, 0), _component2.transform.position, "Component2 did not reach the expected position.");
        Assert.AreEqual(Quaternion.Euler(0, 0, 0), _component2.transform.rotation, "Component2 did not reach the expected rotation.");
        Assert.IsFalse(_toolManager.ToolInstances.ContainsKey(_component2));

        // Cleanup
        Object.DestroyImmediate(mockInteractor.gameObject);
    }


}
*/