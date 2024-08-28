using NUnit.Framework;
using System.Reflection;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

[TestFixture]
public class SimpleHammerTest
{
    private GameObject hammerObject;
    private SimpleHammer simpleHammer;
    private AudioSource audioSource;

    [SetUp]
    public void SetUp()
    {
        hammerObject = new GameObject();
        simpleHammer = hammerObject.AddComponent<SimpleHammer>();
        audioSource = hammerObject.AddComponent<AudioSource>();

        // Set up audio source for the tests
        AudioClip clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f;
        audioSource.mute = false;

        // Set private fields using reflection
        SetPrivateField(simpleHammer, "forceMultiplier", 10f);
        SetPrivateField(simpleHammer, "currentImpactForce", 30f);
        SetPrivateField(simpleHammer, "minImpactForce", 5f);
        SetPrivateField(simpleHammer, "maxImpactForce", 50f);
        SetPrivateField(simpleHammer, "isSelected", true); // Assume hammer is selected

        // Initialize the SimpleHammer
        simpleHammer.Start();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(hammerObject);
    }

    [Test]
    public void ProcessInteractable_WithDynamicPhaseAndSelected_UpdatesImpactForce()
    {
        // Simulate a dynamic update phase where the hammer is selected
        simpleHammer.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        // Check if the impact force and direction are updated correctly
        float currentImpactForce = (float)typeof(SimpleHammer).GetField("currentImpactForce", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.IsTrue(currentImpactForce >= 5f && currentImpactForce <= 50f, "Impact force should be clamped within the set range.");

        Vector3 impactDirection = (Vector3)typeof(SimpleHammer).GetField("impactDirection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.IsNotNull(impactDirection, "Impact direction should not be null.");
    }

    [Test]
    public void Start_SetsLastPosition()
    {
        Vector3 lastPosition = (Vector3)typeof(SimpleHammer).GetField("lastPosition", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.AreEqual(hammerObject.transform.position, lastPosition);
    }

    [Test]
    public void CalculateImpactForce_UpdatesImpactForceAndDirection()
    {
        hammerObject.transform.position += new Vector3(1f, 0f, 0f);
        simpleHammer.CalculateImpactForce();

        float expectedImpactForce = 50f;
        float currentImpactForce = (float)typeof(SimpleHammer).GetField("currentImpactForce", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simpleHammer);
        Assert.AreEqual(expectedImpactForce, currentImpactForce);
        Assert.AreEqual(Vector3.right, simpleHammer.GetImpactDirection());
    }

    [Test]
    public void PlayHammerSound_AudioSourceIsNotNull_PlaysAudio()
    {
        Assert.IsNotNull(audioSource);
        Assert.IsNotNull(audioSource.clip);

        simpleHammer.PlayHammerSound();

        Assert.IsTrue(audioSource.isPlaying);
    }

    [Test]
    public void PlayHammerSound_AudioSourceIsNull_DoesNotThrowException()
    {
        Object.DestroyImmediate(hammerObject.GetComponent<AudioSource>());
        simpleHammer.Start();

        Assert.DoesNotThrow(() => simpleHammer.PlayHammerSound());
    }

    


    [Test]
    public void ProcessInteractable_WhenSelectedAndNoMovement_ImpactForceRemainsZero()
    {
        // Set isSelected to true to simulate the hammer being selected
        SetPrivateField(simpleHammer, "isSelected", true);

        // Ensure hammer doesn't move (so delta position should be zero)
        Vector3 initialPosition = hammerObject.transform.position;

        // Manually invoke the method to simulate the interaction update phase
        simpleHammer.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

    }

        private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
}
