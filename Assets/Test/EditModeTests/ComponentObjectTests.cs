using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ComponentObjectTests
{
    private GameObject gameObject;
    private ComponentObject componentObject;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        componentObject = gameObject.AddComponent<ComponentObject>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(gameObject);
    }

    [Test]
    public void TestInitialValues()
    {
        Assert.IsFalse(componentObject.GetIsPlaced(), "Initial isPlaced value should be false");
        Assert.IsFalse(componentObject.IsReleased, "Initial isReleased value should be false");
        Assert.AreEqual(ComponentObject.ComponentType.None, componentObject.GetComponentType(), "Initial componentType should be None");
    }

    [Test]
    public void TestSetIsPlaced()
    {
        componentObject.SetIsPlaced(true);
        Assert.IsTrue(componentObject.GetIsPlaced(), "isPlaced value should be true after setting it to true");
    }

    [Test]
    public void TestSetIsReleased()
    {
        componentObject.IsReleased = true;
        Assert.IsTrue(componentObject.IsReleased, "isReleased value should be true after setting it to true");
    }

    [Test]
    public void TestSetComponentType()
    {
        componentObject.SetComponentType(ComponentObject.ComponentType.Screw);
        Assert.AreEqual(ComponentObject.ComponentType.Screw, componentObject.GetComponentType(), "componentType should be Screw after setting it to Screw");
    }
}
