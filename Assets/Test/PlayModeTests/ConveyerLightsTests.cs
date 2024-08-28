using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConveyerLightsTests
{
    private GameObject testObject;
    private ConveyerLights conveyerLights;
    private Material materialRed;
    private Material materialRed2;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject and add the ConveyerLights component
        testObject = new GameObject();
        conveyerLights = testObject.AddComponent<ConveyerLights>();

        // Create two test materials
        materialRed = new Material(Shader.Find("Standard"));
        materialRed2 = new Material(Shader.Find("Standard"));

        // Assign the test materials to the ConveyerLights component
        conveyerLights.GetType()
                      .GetField("materialRed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      .SetValue(conveyerLights, materialRed);
        conveyerLights.GetType()
                      .GetField("materialRed2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      .SetValue(conveyerLights, materialRed2);

        // Set a short blink interval for testing purposes
        conveyerLights.BlinkInterval = 0.1f;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.Destroy(testObject);
    }

    [UnityTest]
    public IEnumerator OnToggleLightsStarted_ShouldStartBlinking()
    {
        // Act
        conveyerLights.OnToggleLightsStarted();

        // Assert
        yield return new WaitForSeconds(0.15f); // wait a bit longer than half the blink interval

        Assert.IsFalse(materialRed.IsKeywordEnabled("_EMISSION"));
        Assert.IsTrue(materialRed2.IsKeywordEnabled("_EMISSION"));

        yield return new WaitForSeconds(0.1f); // wait for the second blink

        Assert.IsTrue(materialRed.IsKeywordEnabled("_EMISSION"));
        Assert.IsFalse(materialRed2.IsKeywordEnabled("_EMISSION"));
    }

    [UnityTest]
    public IEnumerator OnToggleLightsCanceled_ShouldStopBlinkingAndTurnOffEmission()
    {
        // Act
        conveyerLights.OnToggleLightsStarted();
        yield return new WaitForSeconds(0.2f); // allow some blinking to happen

        conveyerLights.OnToggleLightsCanceled();

        // Assert
        yield return null; // wait one frame

        Assert.IsFalse(materialRed.IsKeywordEnabled("_EMISSION"));
        Assert.IsFalse(materialRed2.IsKeywordEnabled("_EMISSION"));
    }

    [Test]
    public void BlinkInterval_SetterAndGetter_ShouldWorkCorrectly()
    {
        // Arrange
        float newBlinkInterval = 0.5f;

        // Act
        conveyerLights.BlinkInterval = newBlinkInterval;

        // Assert
        Assert.AreEqual(newBlinkInterval, conveyerLights.BlinkInterval);
    }
}
