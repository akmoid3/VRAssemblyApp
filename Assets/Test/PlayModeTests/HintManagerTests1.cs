using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HintManagerTests1
{
    private GameObject hintManagerObject;
    private HintManager hintManager;
    private GameObject testComponent;
    private GameObject interactorObject;
    private SnapToPosition interactor;
    
    [SetUp]
    public void Setup()
    {
        // Create HintManager GameObject
        hintManagerObject = new GameObject("HintManager");
        hintManager = hintManagerObject.AddComponent<HintManager>();
        
        // Create a test component with MeshRenderer
        testComponent = new GameObject("TestComponent");
        testComponent.AddComponent<MeshRenderer>();
        
        // Create a snap interactor with child snap points
        interactorObject = new GameObject("Interactor");
        interactor = interactorObject.AddComponent<SnapToPosition>();
        
        // Add snap points as children to the interactor
        for (int i = 0; i < 3; i++)
        {
            GameObject snapPoint = new GameObject($"SnapPoint_{i}");
            snapPoint.transform.SetParent(interactorObject.transform);
            MeshRenderer renderer = snapPoint.AddComponent<MeshRenderer>();
            renderer.enabled = false;
        }
        
        // Mock StateManager for tests
        if (StateManager.Instance == null)
        {
            GameObject stateManagerObj = new GameObject("StateManager");
            StateManager stateManager = stateManagerObj.AddComponent<StateManager>();
            // Use reflection or test-specific setter if available to set Instance
        }
        
        // Mock Manager for tests
        if (Manager.Instance == null)
        {
            GameObject managerObj = new GameObject("Manager");
            Manager manager = managerObj.AddComponent<Manager>();
            manager.ComponentsThatCanSnap = new List<Transform>() { testComponent.transform };
            // Use reflection or test-specific setter if available to set Instance
        }
    }
    
    [TearDown]
    public void Teardown()
    {
        Object.Destroy(hintManagerObject);
        Object.Destroy(testComponent);
        Object.Destroy(interactorObject);
        
        // Destroy mock managers
        if (StateManager.Instance != null)
        {
            Object.Destroy(StateManager.Instance.gameObject);
        }
        
        if (Manager.Instance != null)
        {
            Object.Destroy(Manager.Instance.gameObject);
        }
    }
    
    [UnityTest]
    public IEnumerator ShowHint_IncrementsHintCount()
    {
        // Arrange
        int initialHintCount = hintManager.HintCount;
        int currentStep = 0;
        bool hintCountChanged = false;
        int newHintCount = 0;
        
        HintManager.OnHintCountChanged += (count) => 
        {
            hintCountChanged = true;
            newHintCount = count;
        };
        
        // Act
        hintManager.ShowHint(currentStep, testComponent.transform, interactor);
        
        // Need to wait a frame for coroutines to start
        yield return null;
        
        // Assert
        Assert.IsTrue(hintCountChanged, "HintCountChanged event should be fired");
        Assert.AreEqual(initialHintCount + 1, newHintCount, "Hint count should increment by 1");
        Assert.AreEqual(initialHintCount + 1, hintManager.HintCount, "HintCount property should be updated");
        
        // Cleanup
    }
    
    [UnityTest]
    public IEnumerator ShowHint_DoesNotIncrementCountWhenAlreadyShown()
    {
        // Arrange
        int currentStep = 1;
        int callCount = 0;
        
        HintManager.OnHintCountChanged += (count) => 
        {
            callCount++;
        };
        
        // Act - first call should increment
        hintManager.ShowHint(currentStep, testComponent.transform, interactor);
        yield return null;
        
        int countAfterFirstCall = hintManager.HintCount;
        
        // Act - second call for same step should not increment
        hintManager.ShowHint(currentStep, testComponent.transform, interactor);
        yield return null;
        
        // Assert
        Assert.AreEqual(1, callCount, "Event should only be fired once");
        Assert.AreEqual(countAfterFirstCall, hintManager.HintCount, "Hint count should not change on second call");
        
        // Cleanup
    }
    
    [UnityTest]
    public IEnumerator ShowSnapPoint_EnablesCorrectSnapPointRenderer()
    {
        // Arrange
        int currentStep = 1;
        
        // Act
        hintManager.ShowSnapPoint(interactor, currentStep);
        yield return null;
        
        // Assert
        MeshRenderer snapPointRenderer = interactorObject.transform.GetChild(currentStep).GetComponent<MeshRenderer>();
        Assert.IsTrue(snapPointRenderer.enabled, "The correct snap point renderer should be enabled");
    }
    
    [UnityTest]
    public IEnumerator HideHints_DisablesAllSnapPointRenderers()
    {
        // Arrange - first enable some snap points
        hintManager.ShowSnapPoint(interactor, 0);
        hintManager.ShowSnapPoint(interactor, 1);
        yield return null;
        
        // Act
        hintManager.HideHints(interactor);
        yield return null;
        
        // Assert
        bool anyEnabled = false;
        foreach (Transform child in interactorObject.transform)
        {
            if (child.GetComponent<MeshRenderer>().enabled)
            {
                anyEnabled = true;
                break;
            }
        }
        
        Assert.IsFalse(anyEnabled, "All snap point renderers should be disabled");
    }
    
    [UnityTest]
    public IEnumerator HighlightComponentToPlace_ChangesMaterialTemporarily()
    {
        // Arrange
        MeshRenderer meshRenderer = testComponent.GetComponent<MeshRenderer>();
        Material originalMaterial = new Material(Shader.Find("Standard"));
        string originalMaterialName = originalMaterial.name;
        Material[] originalMaterials = new Material[] { originalMaterial };
        meshRenderer.materials = originalMaterials;
        
        // Create and assign highlight material to the HintManager
        Material highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = Color.yellow; // Make it distinctly different
        
        // Use reflection to set the private field
        System.Reflection.FieldInfo fieldInfo = typeof(HintManager).GetField("highlightMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo.SetValue(hintManager, highlightMaterial);
        
        List<Transform> components = new List<Transform> { testComponent.transform };
        
        // Act
        hintManager.HighlightComponentToPlace(components);
        
        // Let the coroutine start
        yield return null;
        
        // Check if material changed to highlight material
        bool materialChanged = meshRenderer.sharedMaterials[0].color == highlightMaterial.color;
        
        // Wait for the cooldown to complete 
        float cooldownTime = 1.0f; // Default value from the class
        yield return new WaitForSeconds(cooldownTime * 1.1f); // Add a small buffer
        
        // Assert
        Assert.IsTrue(materialChanged, "Material should change to highlight material");
        
        // Check if material color reverted (instead of comparing material instances)
        Color finalColor = meshRenderer.sharedMaterials[0].color;
        bool colorReverted = finalColor != highlightMaterial.color;
        
        Assert.IsTrue(colorReverted, "Material should revert after cooldown period");
    }
    
    [Test]
    public void HintCount_PropertyWorks()
    {
        // Arrange
        int testValue = 42;
        
        // Act
        hintManager.HintCount = testValue;
        
        // Assert
        Assert.AreEqual(testValue, hintManager.HintCount, "HintCount property should work correctly");
    }
}