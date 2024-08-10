using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity;

public class ComponentPositionerTests
{
    private GameObject gameObject;
    private ComponentPositioner componentPositioner;
    private GameObject manager;
    private Manager managerScript;


    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        componentPositioner = gameObject.AddComponent<ComponentPositioner>();

        // Arrange
        var tableRenderer = new GameObject().AddComponent<MeshRenderer>();
        var tableRoll = tableRenderer.gameObject;
        manager = new GameObject();
        managerScript = manager.AddComponent<Manager>();
        managerScript.Model = new GameObject();

        componentPositioner.GetType().GetField("tableRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(componentPositioner, tableRenderer);
        componentPositioner.GetType().GetField("tableRoll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(componentPositioner, tableRoll);
        componentPositioner.GetType().GetField("manager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(componentPositioner, managerScript);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(gameObject);
        Object.Destroy(manager);
    }

    [UnityTest]
    public IEnumerator SpawnComponents_AddComponentsToTheParentObject()
    {
        // Act
        componentPositioner.SpawnComponents();
        yield return null;

        GameObject gameObject = GameObject.Find("Parent");
        // Assert
        Assert.IsNotNull(gameObject);
        var spawnedChildren = (List<Transform>)componentPositioner.GetType().GetField("spawnedChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(componentPositioner);
        Assert.AreEqual(gameObject.transform.childCount, spawnedChildren.Count);
    }

}
