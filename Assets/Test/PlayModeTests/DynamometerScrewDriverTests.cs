using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VRTemplate;
using UnityEngine.TestTools;

public class DynamometerScrewDriverTests
{
    private GameObject dynamometerGO;
    private DynamometerScrewDriver dynamometerScrewDriver;
    private AudioSource audioSource;
    private GameObject audioManagerGO;
    private AudioManager audioManager;
    private TextMeshProUGUI forceText;

    [SetUp]
    public void SetUp()
    {
        audioManagerGO = new GameObject("AudioManager");
        audioManager = audioManagerGO.AddComponent<AudioManager>();

        AudioClip dummyDrillClip = AudioClip.Create("Drill", 44100, 1, 44100, false);
        FieldInfo audioClipsField =
            typeof(AudioManager).GetField("audioClips", BindingFlags.NonPublic | BindingFlags.Instance);
        var clips = new Dictionary<string, AudioClip> { { "Drill", dummyDrillClip } };
        audioClipsField.SetValue(audioManager, clips);


        dynamometerGO = new GameObject("DynamometerScrewDriver");
        dynamometerScrewDriver = dynamometerGO.AddComponent<DynamometerScrewDriver>();

        GameObject textGO = new GameObject("ForceText", typeof(TextMeshProUGUI));
        forceText = textGO.GetComponent<TextMeshProUGUI>();
        forceText.text = "0";
        forceText.color = Color.white;
        FieldInfo forceTextField =
            typeof(DynamometerScrewDriver).GetField("forceText", BindingFlags.NonPublic | BindingFlags.Instance);
        forceTextField.SetValue(dynamometerScrewDriver, forceText);

        dynamometerScrewDriver.Start();
        audioSource = dynamometerGO.GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource, "AudioSource should be attached after Start.");
    }

    [TearDown]
    public void TearDown()
    {
        if (audioManagerGO != null)
            GameObject.DestroyImmediate(audioManagerGO);
        GameObject.DestroyImmediate(dynamometerGO);
    }

    [Test]
    public void TestStart_AddsAudioSourceIfMissing()
    {
        Assert.IsNotNull(audioSource);
    }

    [Test]
    public void TestSetForce_UpdatesForceAndForceText()
    {
        GameObject knobGO = new GameObject("DummyKnob");
        XRKnob dummyKnob = knobGO.AddComponent<XRKnob>();
        dummyKnob.value = 0.5f; // Expect approximately 60: Mathf.Lerp(0,120,0.5f)

        dynamometerScrewDriver.SetForce(dummyKnob);

        int expectedForce = Mathf.RoundToInt(Mathf.Lerp(0, 120, dummyKnob.value));
        Assert.AreEqual(expectedForce, dynamometerScrewDriver.Force);
        Assert.AreEqual(expectedForce.ToString(), forceText.text);

        GameObject.DestroyImmediate(knobGO);
    }

    [Test]
    public void TestActivateAudio_WithPositiveTrigger_StartsSound()
    {
        float triggerVal = 0.5f;
        dynamometerScrewDriver.TriggerValue = triggerVal;
        dynamometerScrewDriver.ActivateAudio(triggerVal);

        // Expect that the sound is now playing and the "Drill" clip is assigned.
        Assert.IsTrue(dynamometerScrewDriver.IsPlayingSound);
        Assert.IsNotNull(audioSource.clip);
        Assert.AreEqual("Drill", audioSource.clip.name);
    }

    [Test]
    public void TestActivateAudio_WithZeroTrigger_StopsSound()
    {
        // Simulate that sound is already playing.
        dynamometerScrewDriver.IsPlayingSound = true;
        audioSource.Play();

        // Calling ActivateAudio with zero trigger should stop the sound.
        dynamometerScrewDriver.ActivateAudio(0f);

        Assert.IsFalse(dynamometerScrewDriver.IsPlayingSound);
        Assert.IsFalse(audioSource.isPlaying);
    }

    [Test]
    public void TestStopScrewDriverSound_StopsSoundWhenPlaying()
    {
        // Simulate playing state.
        dynamometerScrewDriver.IsPlayingSound = true;
        audioSource.Play();

        dynamometerScrewDriver.StopScrewDriverSound();

        Assert.IsFalse(dynamometerScrewDriver.IsPlayingSound);
        Assert.IsFalse(audioSource.isPlaying);
    }

    [Test]
    public void TestProcessInteractable_NotSelected_StopsSound()
    {
        dynamometerScrewDriver.IsPlayingSound = true;
        audioSource.Play();

        dynamometerScrewDriver.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

        Assert.IsFalse(dynamometerScrewDriver.IsPlayingSound);
        Assert.IsFalse(audioSource.isPlaying);
    }


    [UnityTest]
    public IEnumerator TestShowCorrectForceTemporarily_ResetsForceTextAfterDuration()
    {
        forceText.text = "Initial";
        forceText.color = Color.white;

        dynamometerScrewDriver.ShowCorrectForceTemporarily(100, 0.1f);

        Assert.AreEqual("100", forceText.text);
        Assert.AreEqual(Color.red, forceText.color);

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("Initial", forceText.text);
        Assert.AreEqual(Color.white, forceText.color);
    }
}