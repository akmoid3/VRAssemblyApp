using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;


[TestFixture]
public class ComponentPositionerTests
{
    private GameObject gameObject;
    private ComponentPositioner componentPositioner;
    private AudioSource audioSource;
    private Slider progressBar;
    private TextMeshProUGUI progressText;
    private GameObject progressPanel;
    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        componentPositioner = gameObject.AddComponent<ComponentPositioner>();
        componentPositioner.TableRoll = new GameObject();
        componentPositioner.TableRoll.AddComponent<MeshRenderer>();
        componentPositioner.AudioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<CoACD>();
        
        progressPanel = new GameObject("ProgressPanel");

        GameObject progressBarObject = new GameObject("ProgressBar");
        progressBar = progressBarObject.AddComponent<Slider>();

        GameObject progressTextObject = new GameObject("ProgressText");
        progressText = progressTextObject.AddComponent<TextMeshProUGUI>();
        
        componentPositioner.GetType().GetField("progressPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(componentPositioner, progressPanel);
        
        componentPositioner.GetType().GetField("progressText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(componentPositioner, progressText);
        
        componentPositioner.GetType().GetField("progressBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(componentPositioner, progressBar);
        
        // Add an AudioSource to the gameObject
        audioSource = gameObject.AddComponent<AudioSource>();
        componentPositioner.AudioSource = audioSource;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void TestInitialization()
    {
        // Setup is executed before each test
        componentPositioner.Start();

        Assert.IsNotNull(componentPositioner.TableRoll, "TableRoll is not assigned.");
        Assert.IsNotNull(componentPositioner.AudioSource, "AudioSource is not assigned.");
        Assert.IsNotNull(componentPositioner.TableRoll.GetComponent<MeshRenderer>(), "MeshRenderer is not found in TableRoll.");
    }

    [UnityTest]
    public IEnumerator SpawnComponents_AddComponentsToTheParentObject()
    {
        // Mock the manager and set a prefab model
        var prefab = new GameObject("TestPrefab");
        var childPrefab = new GameObject("Child");
        childPrefab.transform.SetParent(prefab.transform, false);
        var childPrefab2 = new GameObject("Child2");
        childPrefab2.transform.SetParent(prefab.transform, false);
        prefab.AddComponent<MeshRenderer>();
        childPrefab.AddComponent<MeshRenderer>();
        childPrefab2.AddComponent<MeshRenderer>();

        var manager = new GameObject().AddComponent<Manager>();
        var stateManager = new GameObject().AddComponent<StateManager>();
        manager.Model = prefab;

        componentPositioner.Start();
        componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        componentPositioner.SpawnComponents();
        yield return null;

        GameObject gameObject = GameObject.Find("Parent");
        // Assert
        Assert.IsNotNull(gameObject);
        Assert.AreEqual(gameObject.transform.childCount, 2);


        
            
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(stateManager.gameObject);


    }

    private Mesh CreateTestMesh()
    {
        Mesh mesh = new Mesh();
        // Create a simple triangle mesh.
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        return mesh;
    }
    [UnityTest]
    public IEnumerator TestAddCoACDCollidersToComponentsAsync()
    {
        List<Transform> components = new List<Transform>();
        Transform gameObject = new GameObject().transform;
        MeshFilter mesh = gameObject.gameObject.AddComponent<MeshFilter>();
        mesh.sharedMesh = CreateTestMesh();
        components.Add(gameObject); 

        // Call the asynchronous method.
        
        componentPositioner.AddCoACDCollidersToComponentsAsync(components,"prova");
        
        yield return new WaitForSeconds(20f);

        // Verify that a MeshCollider has been added to the test object.
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        Assert.IsNotNull(meshCollider, "A MeshCollider should be added to the GameObject.");

        // Optionally, check if the collider is set to convex.
        Assert.IsTrue(meshCollider.convex, "The MeshCollider should be set to convex.");
    }
    
    [Test]
    public void TestProperties()
    {
        // Test ButtonRightPressed property
        componentPositioner.ButtonRightPressed = true;
        Assert.IsTrue(componentPositioner.ButtonRightPressed);
        componentPositioner.ButtonRightPressed = false;
        Assert.IsFalse(componentPositioner.ButtonRightPressed);

        // Test ButtonLeftPressed property
        componentPositioner.ButtonLeftPressed = true;
        Assert.IsTrue(componentPositioner.ButtonLeftPressed);
        componentPositioner.ButtonLeftPressed = false;
        Assert.IsFalse(componentPositioner.ButtonLeftPressed);

        // Test Parent property
        var parent = new GameObject("Parent");
        componentPositioner.Parent = parent;
        Assert.AreEqual(parent, componentPositioner.Parent);

        // Test TableRoll property
        var tableRoll = new GameObject("TableRoll");
        componentPositioner.TableRoll = tableRoll;
        Assert.AreEqual(tableRoll, componentPositioner.TableRoll);

        // Test AudioSource property
        var audioSource = gameObject.AddComponent<AudioSource>();
        componentPositioner.AudioSource = audioSource;
        Assert.AreEqual(audioSource, componentPositioner.AudioSource);

        // Test StartScrollClip property
        var startScrollClip = AudioClip.Create("StartScrollClip", 44100, 1, 44100, false);
        componentPositioner.StartScrollClip = startScrollClip;
        Assert.AreEqual(startScrollClip, componentPositioner.StartScrollClip);

        // Test LoopScrollClip property
        var loopScrollClip = AudioClip.Create("LoopScrollClip", 44100, 1, 44100, false);
        componentPositioner.LoopScrollClip = loopScrollClip;
        Assert.AreEqual(loopScrollClip, componentPositioner.LoopScrollClip);

        // Test ScrollSpeed property
        componentPositioner.ScrollSpeed = 2.0f;
        Assert.AreEqual(2.0f, componentPositioner.ScrollSpeed);

        // Test IsScrolling property
        componentPositioner.IsScrolling = true;
        Assert.IsTrue(componentPositioner.IsScrolling);
        componentPositioner.IsScrolling = false;
        Assert.IsFalse(componentPositioner.IsScrolling);
    }

    [Test]
    public void TestScrollingLeft()
    {
        componentPositioner.Parent = new GameObject();
        GameObject child1 = new GameObject();
        child1.AddComponent<MeshRenderer>();
        child1.transform.SetParent(componentPositioner.Parent.transform);
        componentPositioner.Start();
        componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        componentPositioner.ButtonLeftPressed = true;
        componentPositioner.Update();

        Assert.IsTrue(componentPositioner.AudioSource.isPlaying, "Audio should be playing when scrolling left.");
        Assert.IsTrue(componentPositioner.IsScrolling, "Scrolling should be active when left button is pressed.");
    }

    [Test]
    public void TestScrollingRight()
    {
        componentPositioner.Parent = new GameObject();
        GameObject child1 = new GameObject();
        child1.AddComponent<MeshRenderer>();
        child1.transform.SetParent(componentPositioner.Parent.transform);
        componentPositioner.Start();
        componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(1, 1, 1));

        componentPositioner.ButtonRightPressed = true;
        componentPositioner.Update();

        Assert.IsTrue(componentPositioner.AudioSource.isPlaying, "Audio should be playing when scrolling right.");
        Assert.IsTrue(componentPositioner.IsScrolling, "Scrolling should be active when right button is pressed.");
    }

    [Test]
    public void TestStopScrolling()
    {
        componentPositioner.Start();
        componentPositioner.ButtonRightPressed = false;
        componentPositioner.ButtonLeftPressed = false;
        componentPositioner.Update();

        Assert.IsFalse(componentPositioner.AudioSource.isPlaying, "Audio should stop when scrolling is inactive.");
        Assert.IsFalse(componentPositioner.IsScrolling, "Scrolling should be inactive when no button is pressed.");
    }

    [Test]
    public void TestStartScrolling()
    {
        AudioSource audioSource = componentPositioner.gameObject.AddComponent<AudioSource>();
        componentPositioner.AudioSource = audioSource;

        componentPositioner.AudioSource.Stop();
        componentPositioner.StartScrolling();

        Assert.IsTrue(componentPositioner.IsScrolling);
    }

    [Test]
    public void TestScrollSounds()
    {
        componentPositioner.StartScrolling();
        componentPositioner.StartLoopingSound();

        Assert.AreEqual(componentPositioner.LoopScrollClip, audioSource.clip);
        Assert.IsTrue(audioSource.isPlaying);
    }
}
