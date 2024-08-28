using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AutomaticPlacementManagerTests
{
    private GameObject _automaticPlacementManagerGameObject;
    private AutomaticPlacementManager _automaticPlacementManager;
    private SnapToPosition _interactor;
    private ToolManager _toolManager;
    private List<ComponentData> _assemblySequence;
    private List<Transform> _components;

    [SetUp]
    public void Setup()
    {
        // Setup the AutomaticPlacementManager GameObject and component
        _automaticPlacementManagerGameObject = new GameObject();
        _automaticPlacementManager = _automaticPlacementManagerGameObject.AddComponent<AutomaticPlacementManager>();

        // Setup mock data
        _interactor = new GameObject("Interactor").AddComponent<SnapToPosition>();
        _interactor.transform.position = Vector3.zero;

        // Setup the ToolManager with any dependencies it requires
        _toolManager = new GameObject("ToolManager").AddComponent<ToolManager>();

        // Initialize ToolManager dependencies or mock them as needed here
        InitializeToolManagerDependencies(_toolManager);

        _assemblySequence = new List<ComponentData>
        {
            new ComponentData { componentName = "Component1", toolName = "Tool1" },
            new ComponentData { componentName = "Component2", toolName = "Tool2" }
        };

        _components = new List<Transform>
        {
            new GameObject("Component1").transform,
            new GameObject("Component2").transform
        };

        // Mock show solution position
        var showSolutionPositionObject = new GameObject("ShowSolutionPosition");
        _automaticPlacementManagerGameObject.transform.SetParent(showSolutionPositionObject.transform);
        typeof(AutomaticPlacementManager).GetField("showSolutionPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                         ?.SetValue(_automaticPlacementManager, showSolutionPositionObject.transform);
    }

    private void InitializeToolManagerDependencies(ToolManager toolManager)
    {
        // Initialize or mock any dependencies the ToolManager expects here.
        // For example, if ToolManager requires references to certain components or objects,
        // create those objects or mock them as necessary.
    }

    [TearDown]
    public void Teardown()
    {
        // Cleanup all created game objects after each test
        Object.DestroyImmediate(_automaticPlacementManagerGameObject);
        Object.DestroyImmediate(_interactor.gameObject);
        Object.DestroyImmediate(_toolManager.gameObject);
        foreach (var component in _components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator PlaceInitialComponent_CorrectlyPlacesFirstComponent()
    {
        // Act
        _automaticPlacementManager.PlaceInitialComponent(_assemblySequence, _components, _interactor);

        // Assert
        Transform firstComponent = _components[0];
        Assert.IsNotNull(firstComponent);
        yield return null; // wait for the coroutine to run

        Assert.AreEqual(_interactor.transform.position, firstComponent.position);
    }

    [UnityTest]
    public IEnumerator PlaceAllComponentsGradually_PlacesComponentsCorrectly()
    {
        // Act
        _automaticPlacementManager.PlaceAllComponentsGradually(0.5f, _interactor, _assemblySequence, _components, _toolManager);

        // Assert
        yield return new WaitForSeconds(1.5f); // Wait to let the coroutine run

        // Verify that the components have been moved to the correct positions
        foreach (var componentData in _assemblySequence)
        {
            var component = _components.Find(c => c.name == componentData.componentName);
            Assert.IsNotNull(component);
            Assert.AreEqual(_interactor.transform.GetChild(_assemblySequence.IndexOf(componentData)).position, component.position);
        }
    }

    [UnityTest]
    public IEnumerator SmoothMoveComponent_MovesComponentSmoothly()
    {
        // Arrange
        var component = _components[0];
        var targetPosition = new Vector3(1, 1, 1);
        var targetRotation = Quaternion.Euler(45, 45, 45);

        // Act
        yield return _automaticPlacementManager.StartCoroutine(_automaticPlacementManager.PlaceAllComponentsGraduallyCoroutine(0.5f, _interactor.gameObject, _assemblySequence, _components, _toolManager));

        // Assert
        Assert.AreEqual(targetPosition, component.position);
        Assert.AreEqual(targetRotation.eulerAngles, component.rotation.eulerAngles);
    }

    [UnityTest]
    public IEnumerator CleanupPreviousClones_CleansUpClones()
    {
        // Act
        _automaticPlacementManager.PlaceAllComponentsGradually(0.5f, _interactor, _assemblySequence, _components, _toolManager);

        // Assert
        yield return new WaitForSeconds(1.5f); // Wait to let the coroutine run

        // Manually invoke cleanup
        typeof(AutomaticPlacementManager).GetMethod("CleanupPreviousClones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                         ?.Invoke(_automaticPlacementManager, null);

        // Ensure all clones are destroyed
        Assert.IsTrue(_automaticPlacementManager.transform.childCount == 0);
    }
}
