
using System.Reflection;
using NUnit.Framework;
using UnityEngine;


public class SimpleHammerTest
{
    private GameObject hammerObject;
    private SimpleHammer simpleHammer;
    private AudioSource audioSource;

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    [SetUp]
    public void SetUp()
    {
        hammerObject = new GameObject();
        simpleHammer = hammerObject.AddComponent<SimpleHammer>();

        audioSource = hammerObject.AddComponent<AudioSource>();

        AudioClip clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        audioSource.clip = clip;

        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f;
        audioSource.mute = false;

        SetPrivateField(simpleHammer, "forceMultiplier", 10f);
        SetPrivateField(simpleHammer, "minImpactForce", 5f);
        SetPrivateField(simpleHammer, "maxImpactForce", 50f);

        // Initialize the SimpleHammer
        simpleHammer.Start();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(hammerObject);
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
}
