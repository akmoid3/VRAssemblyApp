using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Reflection;

public class AudioManagerTests
{
    private GameObject audioManagerGO;
    private AudioManager audioManager;
    private GameObject audioSourceGO;
    private AudioSource audioSource;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject for the AudioManager and add the component
        audioManagerGO = new GameObject("AudioManager");
        audioManager = audioManagerGO.AddComponent<AudioManager>();

        // Create a GameObject with an AudioSource component
        audioSourceGO = new GameObject("AudioSource");
        audioSource = audioSourceGO.AddComponent<AudioSource>();

        // Create a dummy AudioClip (44100 samples, 1 channel, 44100 Hz)
        AudioClip dummyClip = AudioClip.Create("TestClip", 44100, 1, 44100, false);

        
        FieldInfo audioClipsField = typeof(AudioManager).GetField("audioClips", BindingFlags.NonPublic | BindingFlags.Instance);
        var clips = new Dictionary<string, AudioClip> { { "TestClip", dummyClip } };
        audioClipsField.SetValue(audioManager, clips);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(audioManagerGO);
        Object.DestroyImmediate(audioSourceGO);
    }
    

    [Test]
    public void TestPlaySound_LogsWarningWhenClipNotFound()
    {
        // LogAssert will capture the expected warning message when a clip is not found
        LogAssert.Expect(LogType.Warning, "AudioManager: Sound 'NonExistentClip' not found!");

        audioManager.PlaySound(audioSource, "NonExistentClip");
    }

    [Test]
    public void TestPlayOneShot_DoesNotThrow()
    {
        // Ensure PlayOneShot does not throw an exception when playing a valid clip
        Assert.DoesNotThrow(() => audioManager.PlayOneShot(audioSource, "TestClip"));
    }

    [Test]
    public void TestStopSound_StopsAudioSource()
    {
        // Start playing the sound
        audioManager.PlaySound(audioSource, "TestClip");
        Assert.IsTrue(audioSource.isPlaying);

        // Stop the sound and check that the AudioSource is no longer playing
        audioManager.StopSound(audioSource);
        Assert.IsFalse(audioSource.isPlaying);
    }

    [Test]
    public void TestSetPitch_ClampsCorrectly()
    {
        // Test setting pitch within the allowed range
        audioManager.SetPitch(audioSource, 2.0f);
        Assert.AreEqual(2.0f, audioSource.pitch);

        // Test setting pitch below the minimum (should clamp to 0.1)
        audioManager.SetPitch(audioSource, 0.0f);
        Assert.AreEqual(0.1f, audioSource.pitch);

        // Test setting pitch above the maximum (should clamp to 3.0)
        audioManager.SetPitch(audioSource, 4.0f);
        Assert.AreEqual(3.0f, audioSource.pitch);
    }

    [Test]
    public void TestSetVolume_ClampsCorrectly()
    {
        // Test setting volume within range
        audioManager.SetVolume(audioSource, 0.5f);
        Assert.AreEqual(0.5f, audioSource.volume);

        // Test setting volume below 0 (should clamp to 0)
        audioManager.SetVolume(audioSource, -0.5f);
        Assert.AreEqual(0f, audioSource.volume);

        // Test setting volume above 1 (should clamp to 1)
        audioManager.SetVolume(audioSource, 2.0f);
        Assert.AreEqual(1f, audioSource.volume);
    }

    [Test]
    public void TestSingletonPattern()
    {
        // Create a second AudioManager GameObject
        GameObject anotherAMGO = new GameObject("AudioManager2");
        AudioManager anotherAM = anotherAMGO.AddComponent<AudioManager>();

        // The singleton Instance should remain the first one created in SetUp
        Assert.AreEqual(audioManager, AudioManager.Instance);

        Object.DestroyImmediate(anotherAMGO);
    }
}
