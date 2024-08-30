using Moq;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class ElectricScrewDriverTests
{
    private ElectricScrewDriver electricScrewDriver;
    private AudioSource audioSource;
    private GameObject screwdriverObject;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject and attach the ElectricScrewDriver component
        screwdriverObject = new GameObject();
        electricScrewDriver = screwdriverObject.AddComponent<ElectricScrewDriver>();

        // Add a real AudioSource component
        audioSource = screwdriverObject.AddComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);

    }

    [UnityTest]
    public IEnumerator Start_InitializesAudioSourcePropertiesCorrectly()
    {
        yield return null;
        // Act: The Start method should have been called automatically by Unity
        // when the component was added in SetUp.

        // Assert: Check that the audio source is assigned
        Assert.IsNotNull(electricScrewDriver.AudioSource, "AudioSource should be assigned.");

        // Assert: Check that the loop property is set to true
        Assert.IsTrue(electricScrewDriver.AudioSource.loop, "AudioSource.loop should be true.");

        // Assert: Check that the volume is set to 0f
        Assert.AreEqual(0f, electricScrewDriver.AudioSource.volume, "AudioSource.volume should be 0f.");

        // Assert: Check that the pitch is set to 1f
        Assert.AreEqual(1f, electricScrewDriver.AudioSource.pitch, "AudioSource.pitch should be 1f.");
    }


    [UnityTest]
    public IEnumerator StopScrewDriverSound_StopsSoundAndResetsProperties()
    {
        // Arrange: Set up the initial state
        audioSource.Play();
        audioSource.volume = 1f;
        audioSource.pitch = 2f;
        electricScrewDriver.IsPlayingSound = true;

        yield return null;
        // Act: Call the StopScrewDriverSound method
        electricScrewDriver.StopScrewDriverSound();

        // Assert: Check that the audio source has stopped playing
        Assert.IsFalse(audioSource.isPlaying, "AudioSource should be stopped.");

        // Assert: Check that the volume and pitch have been reset
        Assert.AreEqual(0f, audioSource.volume, "AudioSource volume should be reset to 0.");
        Assert.AreEqual(1f, audioSource.pitch, "AudioSource pitch should be reset to 1.");

        // Assert: Check that the isPlayingSound flag has been set to false
        Assert.IsFalse(electricScrewDriver.IsPlayingSound, "isPlayingSound should be false.");
    }

    [Test]
    public void ActivateAudio_TriggerValueZero_StopsSound()
    {
        electricScrewDriver.AudioSource = audioSource;
        // Arrange
        audioSource.Play();
        electricScrewDriver.IsPlayingSound = true;


        // Act
        electricScrewDriver.ActivateAudio(0f);

        // Assert: Check that the audio source has stopped playing
        Assert.IsFalse(electricScrewDriver.AudioSource.isPlaying, "AudioSource should be stopped when trigger value is 0.");
        Assert.AreEqual(0f, electricScrewDriver.AudioSource.volume, "AudioSource volume should be 0 when trigger value is 0.");
        Assert.AreEqual(1f, electricScrewDriver.AudioSource.pitch, "AudioSource pitch should reset to 1 when trigger value is 0.");
    }

    [Test]
    public void ActivateAudio_TriggerValueNonZero_StartsSoundAndAdjustsProperties()
    {
        electricScrewDriver.AudioSource = audioSource;
        // Arrange
        float triggerValue = 0.5f; // Mid-range value
        electricScrewDriver.IsPlayingSound = false;

        // Act
        electricScrewDriver.ActivateAudio(triggerValue);

        // Assert: Check that the audio source is playing
        Assert.IsTrue(electricScrewDriver.AudioSource.isPlaying, "AudioSource should be playing when trigger value is greater than 0.");
        Assert.AreEqual(triggerValue, electricScrewDriver.AudioSource.volume, "AudioSource volume should match the trigger value.");
        Assert.AreEqual(Mathf.Lerp(1f, 3f, triggerValue), electricScrewDriver.AudioSource.pitch, "AudioSource pitch should be adjusted based on the trigger value.");
    }

    [Test]
    public void ActivateAudio_TriggerValueOne_StartsSoundAndAdjustsProperties()
    {
        electricScrewDriver.AudioSource = audioSource;
        // Arrange
        float triggerValue = 1f; // Mid-range value
        electricScrewDriver.IsPlayingSound = false;

        // Act
        electricScrewDriver.ActivateAudio(triggerValue);

        // Assert: Check that the audio source is playing
        Assert.IsTrue(electricScrewDriver.AudioSource.isPlaying, "AudioSource should be playing when trigger value is greater than 0.");
        Assert.AreEqual(triggerValue, electricScrewDriver.AudioSource.volume, "AudioSource volume should match the trigger value.");
        Assert.AreEqual(Mathf.Lerp(1f, 3f, triggerValue), electricScrewDriver.AudioSource.pitch, "AudioSource pitch should be adjusted based on the trigger value.");
    }


    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(screwdriverObject);
    }
}
