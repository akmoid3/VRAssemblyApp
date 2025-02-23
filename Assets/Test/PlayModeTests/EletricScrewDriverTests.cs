using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Reflection;

public class ElectricScrewDriverTests
{
    private GameObject screwdriverGO;
    private ElectricScrewDriver electricScrewDriver;
    private AudioSource audioSource;
    private GameObject audioManagerGO;
    private AudioManager audioManager;

    [SetUp]
    public void SetUp()
    {
        audioManagerGO = new GameObject("AudioManager");
        audioManager = audioManagerGO.AddComponent<AudioManager>();

        // Inject a dummy "Drill" clip into AudioManager using reflection.
        AudioClip dummyDrillClip = AudioClip.Create("Drill", 44100, 1, 44100, false);
        FieldInfo audioClipsField =
            typeof(AudioManager).GetField("audioClips", BindingFlags.NonPublic | BindingFlags.Instance);
        var clips = new Dictionary<string, AudioClip> { { "Drill", dummyDrillClip } };
        audioClipsField.SetValue(audioManager, clips);


        // Create a GameObject for ElectricScrewDriver.
        screwdriverGO = new GameObject("ElectricScrewDriver");
        electricScrewDriver = screwdriverGO.AddComponent<ElectricScrewDriver>();

        // Manually invoke Start to ensure AudioSource is added.
        electricScrewDriver.Start();

        audioSource = screwdriverGO.GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource, "AudioSource should be attached after Start.");
    }

    [TearDown]
    public void TearDown()
    {
        if (audioManagerGO != null)
            GameObject.DestroyImmediate(audioManagerGO);
        GameObject.DestroyImmediate(screwdriverGO);
    }

    [Test]
    public void TestStart_AddsAudioSourceIfMissing()
    {
        // After Start(), an AudioSource should be attached.
        Assert.IsNotNull(audioSource);
    }

    [Test]
    public void TestActivateAudio_WithPositiveTrigger_StartsSound()
    {
        float triggerVal = 0.5f;
        electricScrewDriver.TriggerValue = triggerVal;
        electricScrewDriver.ActivateAudio(electricScrewDriver.TriggerValue);

        // Expect that the sound is now playing.
        Assert.IsTrue(electricScrewDriver.IsPlayingSound);
    }

    [Test]
    public void TestActivateAudio_WithZeroTrigger_StopsSound()
    {
        // Simulate that sound is already playing.
        electricScrewDriver.IsPlayingSound = true;
        audioSource.Play();

        // Calling ActivateAudio with zero trigger should stop the sound.
        electricScrewDriver.ActivateAudio(0f);

        Assert.IsFalse(electricScrewDriver.IsPlayingSound);
        Assert.IsFalse(audioSource.isPlaying);
    }

    [Test]
    public void TestStopScrewDriverSound_StopsSoundWhenPlaying()
    {
        // Simulate playing state.
        electricScrewDriver.IsPlayingSound = true;
        audioSource.Play();

        electricScrewDriver.StopScrewDriverSound();
        Assert.IsFalse(electricScrewDriver.IsPlayingSound);
        Assert.IsFalse(audioSource.isPlaying);
    }
    
    [Test]
    public void TestRotateScrewDriver()
    {
        Transform screwTip = new GameObject().transform;
        // Simulate playing state.
        FieldInfo firstInteractorField = typeof(BaseScrewDriver).GetField("screwDriver", BindingFlags.NonPublic | BindingFlags.Instance);
        firstInteractorField.SetValue(electricScrewDriver, screwTip);
        electricScrewDriver.TriggerValue = 100f;
        electricScrewDriver.RotateScrewDriver();

        Assert.AreNotEqual(screwTip.rotation, Quaternion.identity);
    }
    

    

}