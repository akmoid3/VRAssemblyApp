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

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(screwdriverObject);
    }
}
