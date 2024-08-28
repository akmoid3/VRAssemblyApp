using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestHammer : BaseHammer
{
    // This method could be used to simulate an action that changes the impact force
    public void ApplyForce()
    {
        // Example implementation that calculates the current impact force based on forceMultiplier
        currentImpactForce = Mathf.Clamp(forceMultiplier * Time.deltaTime, minImpactForce, maxImpactForce);
    }
}

public class BaseHammerTests
{
    private TestHammer testHammer;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject and add the TestHammer component
        GameObject testObject = new GameObject();
        testHammer = testObject.AddComponent<TestHammer>();

        // Set any required properties
        testHammer.GetType()
                  .GetField("forceMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                  .SetValue(testHammer, 100.0f);
        testHammer.GetType()
                  .GetField("minImpactForce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                  .SetValue(testHammer, 1.0f);
        testHammer.GetType()
                  .GetField("maxImpactForce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                  .SetValue(testHammer, 10.0f);
    }

    [Test]
    public void GetImpactForce_ShouldReturnCurrentImpactForce()
    {
        // Arrange
        testHammer.ApplyForce();  // Simulate some action that changes the impact force

        // Act
        float impactForce = testHammer.GetImpactForce();

        // Assert
        Assert.IsTrue(impactForce >= 1.0f && impactForce <= 10.0f, "Impact force is out of the expected range.");
    }
}