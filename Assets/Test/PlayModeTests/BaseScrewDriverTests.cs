using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class TestScrewDriver : BaseScrewDriver
{
    public override void RotateScrewDriver()
    {
        // Example implementation for testing purposes
        currentRotationSpeed = SpeedMultiplier * Time.deltaTime;
    }
}

public class BaseScrewDriverTests
{
    private TestScrewDriver testScrewDriver;

    [SetUp]
    public void SetUp()
    {
        GameObject testObject = new GameObject();
        testScrewDriver = testObject.AddComponent<TestScrewDriver>();

        // Set any required properties
        testScrewDriver.GetType()
                       .GetField("screwDriver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       .SetValue(testScrewDriver, new GameObject().transform);
    }

    [Test]
    public void GetRotationSpeed_ShouldReturnCurrentRotationSpeed()
    {
        // Arrange
        testScrewDriver.RotateScrewDriver();  // Simulate some action that changes the rotation speed

        // Act
        float rotationSpeed = testScrewDriver.GetRotationSpeed();

        // Assert
        Assert.AreEqual(100.0f * Time.deltaTime, rotationSpeed, "Rotation speed does not match expected value.");
    }
}

