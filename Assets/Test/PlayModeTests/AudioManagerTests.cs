using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AudioManagerTests
{
    private GameObject _audioManagerGameObject;
    private AudioManager _audioManager;
    private AudioSource _audioSource;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject and attach the AudioManager component to it
        _audioManagerGameObject = new GameObject();
        _audioManager = _audioManagerGameObject.AddComponent<AudioManager>();

        // Add an AudioSource to the GameObject for testing
        _audioSource = _audioManagerGameObject.AddComponent<AudioSource>();

        // Access private fields and set them up for testing
        typeof(AudioManager).GetField("audioSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(_audioManager, _audioSource);
    }

    [TearDown]
    public void Teardown()
    {
        // Destroy the GameObject after each test
        Object.DestroyImmediate(_audioManagerGameObject);
    }

    [Test]
    public void SingletonPattern_IsImplementedCorrectly()
    {
        // Act: Create a second instance
        var secondGameObject = new GameObject();
        var secondAudioManager = secondGameObject.AddComponent<AudioManager>();

        // Assert: Ensure the first instance remains the singleton and the second is destroyed
        Assert.AreEqual(AudioManager.Instance, _audioManager);
        Assert.IsTrue(secondAudioManager == null || AudioManager.Instance != secondAudioManager);

        // Clean up the second GameObject if it wasn't destroyed
        Object.DestroyImmediate(secondGameObject);
    }


    [Test]
    public void PlayScrewSound_SetsCorrectAudioClipAndPlays()
    {
        // Arrange
        var screwClip = AudioClip.Create("ScrewSound", 44100, 1, 44100, false);
        typeof(AudioManager).GetField("screwSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(_audioManager, screwClip);

        // Act
        _audioManager.PlayScrewSound();

        // Assert
        Assert.AreEqual(screwClip, _audioSource.clip);
        Assert.IsTrue(_audioSource.isPlaying);
        Assert.IsTrue(_audioSource.loop);
        Assert.AreEqual(0.5f, _audioSource.volume);
    }

    [Test]
    public void PlayPopSound_SetsCorrectAudioClipAndPlays()
    {
        // Arrange
        var popClip = AudioClip.Create("PopSound", 44100, 1, 44100, false);
        typeof(AudioManager).GetField("popSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(_audioManager, popClip);

        // Act
        _audioManager.PlayPopSound();

        // Assert
        Assert.AreEqual(popClip, _audioSource.clip);
        Assert.IsTrue(_audioSource.isPlaying);
        Assert.IsFalse(_audioSource.loop);
        Assert.AreEqual(1.0f, _audioSource.volume);
    }

    [Test]
    public void StopSound_StopsPlayingAudio()
    {
        // Arrange
        _audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        _audioSource.Play();

        // Act
        _audioManager.StopSound();

        // Assert
        Assert.IsFalse(_audioSource.isPlaying);
    }

    [Test]
    public void SetPitch_AdjustsPitchCorrectly()
    {
        // Act
        _audioManager.SetPitch(2.0f);

        // Assert
        Assert.AreEqual(2.0f, _audioSource.pitch);

        // Test lower boundary
        _audioManager.SetPitch(0.0f);
        Assert.AreEqual(0.1f, _audioSource.pitch);

        // Test upper boundary
        _audioManager.SetPitch(5.0f);
        Assert.AreEqual(3.0f, _audioSource.pitch);
    }
}
