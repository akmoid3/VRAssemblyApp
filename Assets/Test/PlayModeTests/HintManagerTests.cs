/*using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class HintManagerTests
{
    private HintManager hintManager;
    private GameObject componentGameObject;
    private Material highlightMaterial;
    private SnapToPosition interactor;
    private GameObject snapPoint;
    private List<Transform> components;

    [SetUp]
    public void SetUp()
    {
        // Create a new GameObject and attach the HintManager component
        var gameObject = new GameObject();
        hintManager = gameObject.AddComponent<HintManager>();

        // Set the highlight material
        highlightMaterial = new Material(Shader.Find("Standard"));
        hintManager.GetType().GetField("highlightMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(hintManager, highlightMaterial);

        // Create a mock component GameObject
        componentGameObject = new GameObject("Component");
        componentGameObject.AddComponent<MeshRenderer>();

        // Create a mock SnapToPosition with child objects as snap points
        var interactorObject = new GameObject();
        interactor = interactorObject.AddComponent<SnapToPosition>();

        // Create a snap point and set it as a child of the interactor
        snapPoint = new GameObject("SnapPoint");
        snapPoint.AddComponent<MeshRenderer>().enabled = false;
        snapPoint.transform.SetParent(interactor.transform);

        // Initialize component list
        components = new List<Transform> { snapPoint.transform };
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(hintManager.gameObject);
        Object.DestroyImmediate(componentGameObject);
        Object.DestroyImmediate(interactor.gameObject);
        Object.DestroyImmediate(snapPoint);
    }

    [Test]
    public void TestShowHint_DoesNotProceedIfHintAlreadyShown()
    {
        // Arrange
        var assemblySequence = new List<ComponentData> {
            new ComponentData { componentName = "Component1" }
        };
        var components = new List<Transform>();
        hintManager.GetType().GetField("hintShownForStep", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(hintManager, new Dictionary<int, bool> { { 0, true } });

        // Act
        hintManager.ShowHint(assemblySequence, 0, components, null);

        // Assert
        var hintCount = (int)hintManager.GetType().GetField("hintCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(hintManager);
        Assert.AreEqual(0, hintCount, "Hint count should not increment if the hint is already shown.");
    }

    [Test]
    public void TestShowHint_LogsErrorIfInteractorIsNull()
    {
        // Arrange
        var assemblySequence = new List<ComponentData> {
            new ComponentData { componentName = "Component1" }
        };
        var components = new List<Transform>();

        // Act
        hintManager.ShowHint(assemblySequence, 0, components, null);

    }

    [UnityTest]
    public IEnumerator TestHandleHintCooldown_SetsIsWaitingCorrectly()
    {
        // Arrange
        var coroutine = hintManager.HandleHintCooldown(componentGameObject);

        // Act
        yield return hintManager.StartCoroutine(coroutine);

        // Assert
        var isWaiting = (bool)hintManager.GetType()
            .GetField("isWaiting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(hintManager);

        Assert.IsFalse(isWaiting, "isWaiting should be false after HandleHintCooldown coroutine completes.");
    }

    [UnityTest]
    public IEnumerator TestChangeColorTemporarily_ChangesMaterialAndRevertsBack()
    {
        // Arrange
        var meshRenderer = componentGameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        var originalMaterial = meshRenderer.material;

        // Act
        yield return hintManager.StartCoroutine(hintManager.ChangeColorTemporarily(componentGameObject));

        // Assert
        Assert.AreEqual(originalMaterial, meshRenderer.materials[0], "Material should revert back to the original after cooldown.");
    }

    [Test]
    public void TestHintCountProperty_SetAndGet()
    {
        // Act
        hintManager.HintCount = 5;

        // Assert
        Assert.AreEqual(5, hintManager.HintCount, "HintCount should be set and returned correctly.");
    }

    [Test]
    public void TestShowSnapPoint_EnablesCorrectMeshRenderer()
    {
        // Act
        hintManager.ShowSnapPoint(interactor,0);
        // Assert
        Assert.IsTrue(snapPoint.GetComponent<MeshRenderer>().enabled, "The MeshRenderer should be enabled for the correct snap point.");
    }

    [UnityTest]
    public IEnumerator TestHighlightComponentToPlace_StartsHandleHintCooldown()
    {
        // Arrange
        var assemblySequence = new List<ComponentData> {
            new ComponentData { componentName = "SnapPoint", group = ComponentObject.Group.None }
        };

        // Act
        hintManager.HighlightComponentToPlace(assemblySequence, 0, components);

        // Wait for the coroutine to complete
        yield return new WaitForSeconds(hintManager.HintCount > 0 ? hintManager.HintCount : 1.0f);

        // Assert
        // Since we're testing if the coroutine started, we should expect that isWaiting becomes true at first and then false after the coroutine ends
        var isWaiting = (bool)hintManager.GetType().GetField("isWaiting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(hintManager);
        Assert.IsTrue(isWaiting);
    }
}*/