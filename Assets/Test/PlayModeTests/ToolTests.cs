using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Moq;
using System;

public class TestToolFake : Tool
{

    // The subclass is used just for testing.
}


public class TestManager : Manager
{
    public bool HoverEnterCalled { get; private set; } = false;
    public bool HoverExitCalled { get; private set; } = false;

    public override void OnHoverEnter(HoverEnterEventArgs args)
    {
        HoverEnterCalled = true;
    }

    public override void OnHoverExit(HoverExitEventArgs args)
    {
        HoverExitCalled = true;
    }
}

[TestFixture]
public class ToolTests
{
    private TestToolFake _toolFake;
    private TestManager _testManager;
    private StateManager stateManager;

    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a GameObject and add the TestTool component
        var gameObject = new GameObject();
        _toolFake = gameObject.AddComponent<TestToolFake>();
        _toolFake.ToolName = "TestTool";
        // Create a GameObject and add the TestManager component
        _testManager = new GameObject().AddComponent<TestManager>();

        // Set the manager instance to the test manager
        _toolFake.SetManager(_testManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the GameObject after each test
        GameObject.DestroyImmediate(_toolFake.gameObject);
        GameObject.DestroyImmediate(_testManager.gameObject);
        GameObject.DestroyImmediate(stateManager.gameObject);

    }

    [Test]
    public void GetNameTest()
    {
        Assert.AreEqual(_toolFake.ToolName, "TestTool");
    }
    
    [Test]
    public void SetNameTest()
    {
        _toolFake.ToolName = "TestTool2";
        Assert.AreEqual(_toolFake.ToolName, "TestTool2");
    }

    [Test]
    public void OnHoverEntered_CallsManagerOnHoverEnter()
    {
        // Arrange
        var hoverArgs = new HoverEnterEventArgs();

        // Act
        var method = _toolFake.GetType().GetMethod("OnHoverEntered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(HoverEnterEventArgs) }, null);
        method.Invoke(_toolFake, new object[] { hoverArgs });

        // Assert
        Assert.IsTrue(_testManager.HoverEnterCalled, "Manager.OnHoverEnter should have been called.");
    }

    [Test]
    public void OnHoverExited_CallsManagerOnHoverExit()
    {
        // Arrange
        var hoverArgs = new HoverExitEventArgs();

        // Act
        var method = _toolFake.GetType().GetMethod("OnHoverExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(HoverExitEventArgs) }, null);
        method.Invoke(_toolFake, new object[] { hoverArgs });

        // Assert
        Assert.IsTrue(_testManager.HoverExitCalled, "Manager.OnHoverExit should have been called.");
    }


}


