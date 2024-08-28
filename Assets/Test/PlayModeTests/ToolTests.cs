using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Moq;
using System;

public class TestTool : Tool
{

    // No need to add additional code here, the subclass is used just for testing.
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
    private TestTool _tool;
    private TestManager _testManager;
    private StateManager stateManager;

    [SetUp]
    public void SetUp()
    {
        stateManager = new GameObject().AddComponent<StateManager>();
        // Create a GameObject and add the TestTool component
        var gameObject = new GameObject();
        _tool = gameObject.AddComponent<TestTool>();

        // Create a GameObject and add the TestManager component
        _testManager = new GameObject().AddComponent<TestManager>();

        // Set the manager instance to the test manager
        _tool.SetManager(_testManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the GameObject after each test
        GameObject.DestroyImmediate(_tool.gameObject);
        GameObject.DestroyImmediate(_testManager.gameObject);
        GameObject.DestroyImmediate(stateManager.gameObject);

    }

    [Test]
    public void OnHoverEntered_CallsManagerOnHoverEnter()
    {
        // Arrange
        var hoverArgs = new HoverEnterEventArgs();

        // Act
        var method = _tool.GetType().GetMethod("OnHoverEntered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(HoverEnterEventArgs) }, null);
        method.Invoke(_tool, new object[] { hoverArgs });

        // Assert
        Assert.IsTrue(_testManager.HoverEnterCalled, "Manager.OnHoverEnter should have been called.");
    }

    [Test]
    public void OnHoverExited_CallsManagerOnHoverExit()
    {
        // Arrange
        var hoverArgs = new HoverExitEventArgs();

        // Act
        var method = _tool.GetType().GetMethod("OnHoverExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(HoverExitEventArgs) }, null);
        method.Invoke(_tool, new object[] { hoverArgs });

        // Assert
        Assert.IsTrue(_testManager.HoverExitCalled, "Manager.OnHoverExit should have been called.");
    }
}


