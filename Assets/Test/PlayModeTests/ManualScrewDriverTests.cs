using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

public class ManualScrewDriverTests
{
    private ManualScrewDriver manualScrewDriver;
    private GameObject testObject;

    [SetUp]
    public void Setup()
    {
        // Create a GameObject and attach the ManualScrewDriver component to it
        testObject = new GameObject();
        manualScrewDriver = testObject.AddComponent<ManualScrewDriver>();

        // Create a dummy screwdriver object
        GameObject screwDriverObject = new GameObject();
        screwDriverObject.transform.parent = testObject.transform;

        // Assign the dummy screwdriver to the ManualScrewDriver
        manualScrewDriver.ScrewDriver = screwDriverObject.transform;

        // Set initial conditions for the test
        manualScrewDriver.SpeedMultiplier = 100.0f;
    }

    [Test]
    public void Start_SetsLastRotation()
    {
        // Act
        manualScrewDriver.Start();

        // Assert
        Assert.AreEqual(testObject.transform.localRotation, GetPrivateField<Quaternion>(manualScrewDriver, "lastRotation"), "Last rotation should be set to the initial local rotation of the object.");
    }

    [UnityTest]
    public IEnumerator CalculateRotationSpeed_ClockwiseRotation_SetsPositiveSpeed()
    {
        manualScrewDriver.Start();
        Quaternion initialRotation = testObject.transform.localRotation;

        float rotationAmount = 10f; // Rotate by 10 degrees each frame

        // Simulate rotation over several frames
        for (int i = 0; i < 5; i++) 
        {
            Quaternion newRotation = testObject.transform.localRotation * Quaternion.Euler(0, rotationAmount, 0);
            testObject.transform.localRotation = newRotation;

            manualScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

            manualScrewDriver.CalculateRotationSpeed();


            yield return null; // Wait for the next frame
        }

        Debug.Log($"Calculated rotation speed after continuous rotation: {manualScrewDriver.GetRotationSpeed()}");

        // Since it's a clockwise rotation, the speed should be negative
        Assert.Greater(manualScrewDriver.GetRotationSpeed(), 0, "Rotation speed should be positive for clockwise rotation.");
    }

    [Test]
    public void RotateScrewDriver_RotatesScrewDriver()
    {
        // Arrange
        manualScrewDriver.Start();
        Quaternion initialRotation = manualScrewDriver.ScrewDriver.localRotation;
        SetPrivateField(manualScrewDriver, "currentRotationSpeed", 10.0f);

        // Act
        manualScrewDriver.RotateScrewDriver();

        // Assert
        Assert.AreNotEqual(initialRotation, manualScrewDriver.ScrewDriver.localRotation, "Screwdriver should have rotated.");
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up after each test
        Object.DestroyImmediate(testObject);
    }

    // Helper method to get private field value using reflection
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)field.GetValue(obj);
    }

    // Helper method to set private field value using reflection
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
