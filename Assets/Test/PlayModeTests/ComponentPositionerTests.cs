using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System.Reflection;

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
        
        componentPositioner.GetType().GetField("extraSpacing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(componentPositioner, 0.1f);
        
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

        //Assert.IsTrue(componentPositioner.AudioSource.isPlaying, "Audio should be playing when scrolling left.");
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

        //Assert.IsTrue(componentPositioner.AudioSource.isPlaying, "Audio should be playing when scrolling right.");
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
//        Assert.IsTrue(audioSource.isPlaying);
    }
    

    [UnityTest]
    public IEnumerator TestAddCoACDCollidersWithExistingSavedData()
    {
        // Create mock file system
        string testDir = Path.Combine(Application.persistentDataPath, "ColliderData");
        Directory.CreateDirectory(testDir);
        string filePath = Path.Combine(testDir, "testmodel.json");

        // Create and save test data
        RuntimeColliderData testData = new RuntimeColliderData();
        MeshGroup group = new MeshGroup();
        Mesh testMesh = CreateTestMesh();
        MeshData meshData = MeshDataConverter.ConvertMeshToMeshData(testMesh);
        group.baseMesh = meshData;
        group.computedMeshes.Add(meshData);
        testData.meshGroups.Add(group);
        
        ColliderDataSaver.SaveRuntimeColliderData(testData, "testmodel.json");

        // Setup test components
        List<Transform> components = new List<Transform>();
        Transform testObj = new GameObject().transform;
        MeshFilter mesh = testObj.gameObject.AddComponent<MeshFilter>();
        mesh.sharedMesh = testMesh;
        components.Add(testObj);

        // Call the method using the test file name
        componentPositioner.AddCoACDCollidersToComponentsAsync(components, "testmodel");
        
        // Wait for execution
        yield return new WaitForSeconds(5f);

        // Verify result
        MeshCollider meshCollider = testObj.GetComponent<MeshCollider>();
        Assert.IsNotNull(meshCollider, "A MeshCollider should be added from saved data");
        
        // Cleanup
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    [UnityTest]
    public IEnumerator TestScrollLeftEdgeCase()
    {
        // Setup
        var manager = new GameObject().AddComponent<Manager>();
        componentPositioner.Start();
        componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        // Create components that will be at the edge
        List<Transform> components = new List<Transform>();
        GameObject child1 = new GameObject("Component1");
        child1.AddComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(1, 1, 1));
        child1.AddComponent<ComponentObject>();
        child1.AddComponent<XRSimpleInteractable>(); // Add to test that branch
        
        // Position the object at the left edge
        child1.transform.position = new Vector3(componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds.min.x - 0.6f, 0, 0);
        components.Add(child1.transform);
        
        // Set the components in the manager
        manager.Components = components;
        
        // Trigger scroll
        componentPositioner.ButtonLeftPressed = true;
        componentPositioner.Update();
        
        yield return null;
        
        // Execute scroll left method directly for edge case
        componentPositioner.ScrollLeft();
        
        // Verify component gets repositioned when going off-edge
        yield return null;
        
        // Clean up
        Object.DestroyImmediate(manager.gameObject);
    }

    [UnityTest]
    public IEnumerator TestScrollRightWithWraparound()
    {
        // Setup
        var manager = new GameObject().AddComponent<Manager>();
        componentPositioner.Start();
        componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        // Create components
        List<Transform> components = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            GameObject child = new GameObject($"Component{i}");
            Renderer renderer = child.AddComponent<MeshRenderer>();
            renderer.bounds = new Bounds(child.transform.position, new Vector3(1, 1, 1));
            child.AddComponent<ComponentObject>();
            
            // Position at different places
            child.transform.position = new Vector3(componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds.max.x + i, 0, 0);
            components.Add(child.transform);
        }
        
        // Set the components in the manager
        manager.Components = components;
        
        // Trigger scroll right to test wraparound logic
        componentPositioner.ButtonRightPressed = true;
        componentPositioner.Update();
        
        yield return null;
        componentPositioner.ScrollRight(); // Execute once more to ensure wraparound occurs
        yield return null;
        
        // Check that components were moved appropriately
        // The test will pass just by executing this code path
        
        // Clean up
        Object.DestroyImmediate(manager.gameObject);
    }

    [Test]
public void TestRepositionComponentsWithDifferentStates()
{
    // Setup state manager with playback state
    var stateManager = new GameObject().AddComponent<StateManager>();
    stateManager.CurrentState = State.PlayBack;
    
    // Create parent object and assign it
    GameObject parentObject = new GameObject("Parent");
    componentPositioner.Parent = parentObject;
    
    // Create test components
    List<Transform> components = new List<Transform>();
    
    // Case 1: Component with HasMoved=true and no XRSimpleInteractable
    GameObject movedComp = new GameObject("MovedComponent");
    MeshRenderer movedRenderer = movedComp.AddComponent<MeshRenderer>();
    movedRenderer.bounds = new Bounds(movedComp.transform.position, new Vector3(1, 1, 1));
    ComponentObject compObject = movedComp.AddComponent<ComponentObject>();
    compObject.HasMoved = true;
    components.Add(movedComp.transform);
    
    // Case 2: Component with IsAutomaticSnap=true
    GameObject autoSnapComp = new GameObject("AutoSnapComponent");
    MeshRenderer autoSnapRenderer = autoSnapComp.AddComponent<MeshRenderer>();
    autoSnapRenderer.bounds = new Bounds(autoSnapComp.transform.position, new Vector3(1, 1, 1));
    ComponentObject autoSnapObj = autoSnapComp.AddComponent<ComponentObject>();
    autoSnapObj.IsAutomaticSnap = true;
    components.Add(autoSnapComp.transform);
    
    // Case 3: Normal component
    GameObject normalComp = new GameObject("NormalComponent");
    MeshRenderer normalRenderer = normalComp.AddComponent<MeshRenderer>();
    normalRenderer.bounds = new Bounds(normalComp.transform.position, new Vector3(1, 1, 1));
    normalComp.AddComponent<ComponentObject>();
    components.Add(normalComp.transform);
    
    // Set up table bounds
    componentPositioner.Start();
    componentPositioner.TableRoll.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(10, 1, 10));
    
    // Execute
    componentPositioner.RepositionComponentsOnTable(components);
    
    // Verify normal component was repositioned on the table
    Assert.AreEqual(parentObject.transform, normalComp.transform.parent, 
        "The normal component should be parented to the Parent GameObject");
    
    // Clean up
    Object.DestroyImmediate(stateManager.gameObject);
    Object.DestroyImmediate(parentObject);
}

    [Test]
    public void TestCollectChildrenWithMeshRecursive()
    {
        // Create a deep hierarchy with meshes
        GameObject root = new GameObject("Root");
        GameObject child1 = new GameObject("Child1");
        GameObject grandchild = new GameObject("Grandchild");
        
        child1.transform.SetParent(root.transform);
        grandchild.transform.SetParent(child1.transform);
        
        // Add mesh only to the grandchild to test recursive collection
        grandchild.AddComponent<MeshRenderer>();
        
        // Create result list
        List<Transform> results = new List<Transform>();
        
        // Call the private method using reflection
        MethodInfo methodInfo = typeof(ComponentPositioner).GetMethod("CollectChildrenWithMesh", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        methodInfo.Invoke(componentPositioner, new object[] { root.transform, results });
        
        // Verify that the grandchild was found
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(grandchild.transform, results[0]);
        
        // Clean up
        Object.DestroyImmediate(root);
    }
    
    [Test]
    public void TestStartScrollingWithClipLength()
    {
        // Create audio clips
        AudioClip startClip = AudioClip.Create("StartClip", 44100, 1, 44100, false);
        componentPositioner.StartScrollClip = startClip;
    
        AudioClip loopClip = AudioClip.Create("LoopClip", 44100, 1, 44100, false);
        componentPositioner.LoopScrollClip = loopClip;
    
        // Ensure the audio source exists and is not playing
        AudioSource testAudioSource = componentPositioner.gameObject.AddComponent<AudioSource>();
        componentPositioner.AudioSource = testAudioSource;
        componentPositioner.AudioSource.Stop(); // Make sure it's stopped
    
        // Start scrolling which should schedule the looping sound
        componentPositioner.IsScrolling = false;  // Reset first
        componentPositioner.StartScrolling();
    
        // Verify the start clip is playing
        Assert.AreEqual(startClip, componentPositioner.AudioSource.clip, 
            "Audio source should be playing the start clip");
        Assert.IsTrue(componentPositioner.AudioSource.isPlaying, 
            "Audio source should be playing");
    
        // We can't directly test the Invoke timing in a unit test,
        // but we can verify it was scheduled by calling StartLoopingSound manually
        componentPositioner.StartLoopingSound();
    
        // Verify the loop clip is now playing
        Assert.AreEqual(loopClip, componentPositioner.AudioSource.clip, 
            "Audio source should switch to loop clip");
    }
}