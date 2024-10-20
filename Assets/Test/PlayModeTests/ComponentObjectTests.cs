/*using NUnit.Framework;
using UnityEngine;

public class ComponentObjectTests
{
    private GameObject _componentObjectGameObject;
    private ComponentObject _componentObject;

    [SetUp]
    public void SetUp()
    {
        // Create a new GameObject with the ComponentObject script
        _componentObjectGameObject = new GameObject();
        _componentObject = _componentObjectGameObject.AddComponent<ComponentObject>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.Destroy(_componentObjectGameObject);
    }

    [Test]
    public void GetIsPlaced_ReturnsFalseByDefault()
    {
        Assert.IsFalse(_componentObject.GetIsPlaced());
    }

    [Test]
    public void SetIsPlaced_SetsIsPlacedCorrectly()
    {
        _componentObject.SetIsPlaced(true);

        Assert.IsTrue(_componentObject.GetIsPlaced());
    }

    [Test]
    public void GetComponentType_ReturnsDefaultNone()
    {
        Assert.AreEqual(ComponentObject.ComponentType.None, _componentObject.GetComponentType());
    }

    [Test]
    public void SetComponentType_SetsComponentTypeCorrectly()
    {
        _componentObject.SetComponentType(ComponentObject.ComponentType.Screw);

        Assert.AreEqual(ComponentObject.ComponentType.Screw, _componentObject.GetComponentType());
    }

    [Test]
    public void GetGroup_ReturnsDefaultNone()
    {
        Assert.AreEqual(ComponentObject.Group.None, _componentObject.GetGroup());
    }

    [Test]
    public void SetGroup_SetsGroupCorrectly()
    {
        _componentObject.SetGroup(ComponentObject.Group.Group01);

        Assert.AreEqual(ComponentObject.Group.Group01, _componentObject.GetGroup());
    }

    [Test]
    public void IsReleased_PropertyWorksCorrectly()
    {
        _componentObject.IsReleased = true;

        Assert.IsTrue(_componentObject.IsReleased);
    }
}
*/