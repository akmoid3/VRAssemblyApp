using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ToolManagerTests
{
    private GameObject _toolManagerObject;
    private ToolManager _toolManager;

    private GameObject _component;
    private GameObject _hammerPrefab;
    private GameObject _drillPrefab;
    private GameObject _screwDriverPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create a new GameObject to hold the ToolManager component
        _toolManagerObject = new GameObject();
        _toolManager = _toolManagerObject.AddComponent<ToolManager>();

        // Create mock GameObjects for the tools and a component
        _hammerPrefab = new GameObject("Hammer");
        _drillPrefab = new GameObject("Drill");
        _screwDriverPrefab = new GameObject("ScrewDriver");

        // Add Rigidbody components to the prefabs
        _hammerPrefab.AddComponent<Rigidbody>();
        _drillPrefab.AddComponent<Rigidbody>();
        _screwDriverPrefab.AddComponent<Rigidbody>();

        // Assign the prefabs to the ToolManager
        _toolManager.HammerPrefab = _hammerPrefab;
        _toolManager.DrillPrefab = _drillPrefab;
        _toolManager.ScrewDriverPrefab = _screwDriverPrefab;

        // Initialize the component
        _component = new GameObject("Component");

        // Manually call Start to initialize the toolPrefabs dictionary
        _toolManager.Start();
    }


    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.Destroy(_toolManagerObject);
        Object.Destroy(_component);
        Object.Destroy(_hammerPrefab);
        Object.Destroy(_drillPrefab);
        Object.Destroy(_screwDriverPrefab);
    }

    [Test]
    public void AttachToolToComponent_AttachesHammer()
    {
        GameObject toolInstance = _toolManager.AttachToolToComponent(_component, "Hammer");

        Assert.IsNotNull(toolInstance);
        Assert.AreEqual("Hammer", toolInstance.name);
        Assert.AreEqual(Vector3.zero + new Vector3(0, 0.5f, 0), toolInstance.transform.localPosition);
        Assert.IsTrue(toolInstance.GetComponent<Rigidbody>().isKinematic);
        Assert.IsTrue(_toolManager.ToolInstances.ContainsKey(_component));
    }

    [Test]
    public void AttachToolToComponent_AttachesDrill()
    {
        GameObject toolInstance = _toolManager.AttachToolToComponent(_component, "Drill");

        Assert.IsNotNull(toolInstance);
        Assert.AreEqual("Drill", toolInstance.name);
        Assert.AreEqual(Vector3.zero + new Vector3(0, 0.5f, 0), toolInstance.transform.localPosition);
        Assert.IsTrue(toolInstance.GetComponent<Rigidbody>().isKinematic);
        Assert.IsTrue(_toolManager.ToolInstances.ContainsKey(_component));
    }

    [Test]
    public void AttachToolToComponent_AttachesNull()
    {
        GameObject toolInstance = _toolManager.AttachToolToComponent(_component, "Null");

        Assert.IsNull(toolInstance);
    }

    [Test]
    public void AttachToolToComponent_ReplacesExistingTool()
    {
        _toolManager.AttachToolToComponent(_component, "Hammer");

        GameObject toolInstance = _toolManager.AttachToolToComponent(_component, "Drill");

        Assert.IsNotNull(toolInstance);
        Assert.AreEqual("Drill", toolInstance.name);
        Assert.IsFalse(_toolManager.ToolInstances[_component].name == "Hammer");
    }

    [Test]
    public void HideToolOnComponent_RemovesTool()
    {
        _toolManager.AttachToolToComponent(_component, "Hammer");

        _toolManager.HideToolOnComponent(_component);

        Assert.IsFalse(_toolManager.ToolInstances.ContainsKey(_component));
    }

    [Test]
    public void HideAllTools_RemovesAllTools()
    {
        _toolManager.AttachToolToComponent(_component, "Hammer");
        var anotherComponent = new GameObject("AnotherComponent");
        _toolManager.AttachToolToComponent(anotherComponent, "Drill");

        _toolManager.HideAllTools();

        Assert.IsEmpty(_toolManager.ToolInstances);
        Object.Destroy(anotherComponent);
    }
}
