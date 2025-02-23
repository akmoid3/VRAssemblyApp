using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolFake : Tool
{
    
}

public class ToolManagerTests
{
    private ToolManager toolManager;
    private Material highlightMaterial;

    [SetUp]
    public void Setup()
    {
        var tmGO = new GameObject("ToolManager");
        toolManager = tmGO.AddComponent<ToolManager>();

        highlightMaterial = new Material(Shader.Find("Standard"));

        var toolsListField = typeof(ToolManager).GetField("toolsList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        toolsListField.SetValue(toolManager, new List<GameObject>());

        var highlightMatField = typeof(ToolManager).GetField("toolHighlightMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        highlightMatField.SetValue(toolManager, highlightMaterial);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(toolManager.gameObject);
    }

    private string GetBaseMaterialName(Material mat)
    {
        return mat.name.Replace(" (Instance)", "");
    }

    [Test]
    public void HighlightToolByName_HighlightsMatchingTool()
    {
        var toolGO = new GameObject("Tool1");
        var toolScript = toolGO.AddComponent<ToolFake>();
        toolScript.ToolName = "Hammer";

        var renderer = toolGO.AddComponent<MeshRenderer>();
        Material originalMat = new Material(Shader.Find("Standard"));
        renderer.materials = new Material[] { originalMat };

        var toolsListField = typeof(ToolManager).GetField("toolsList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        List<GameObject> toolsList = (List<GameObject>)toolsListField.GetValue(toolManager);
        toolsList.Add(toolGO);

        toolManager.HighlightToolByName("Hammer");
        //toolManager.HighlightToolByName("Screwdriver");

        Assert.IsTrue(Array.TrueForAll(renderer.materials, 
            m => GetBaseMaterialName(m) == GetBaseMaterialName(highlightMaterial)));

        // Also check that the original material was stored.
        var origMatsField = typeof(ToolManager).GetField("originalToolMaterials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var originalMaterials = (Dictionary<Renderer, Material[]>)origMatsField.GetValue(toolManager);
        Assert.IsTrue(originalMaterials.ContainsKey(renderer));
        Assert.AreEqual(GetBaseMaterialName(originalMat), GetBaseMaterialName(originalMaterials[renderer][0]));

        UnityEngine.Object.DestroyImmediate(toolGO);
    }

    [Test]
    public void ClearToolHighlight_RestoresOriginalMaterials()
    {
        var toolGO = new GameObject("Tool2");
        var renderer = toolGO.AddComponent<MeshRenderer>();
        Material originalMat = new Material(Shader.Find("Standard"));
        renderer.materials = new Material[] { originalMat };

        // Manually add the renderer and its original material to the internal dictionary.
        var origMatsField = typeof(ToolManager).GetField("originalToolMaterials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var originalMaterials = (Dictionary<Renderer, Material[]>)origMatsField.GetValue(toolManager);
        originalMaterials[renderer] = new Material[] { originalMat };
        toolManager.HighlightToolByName("Screwdriver");

        renderer.materials = new Material[] { highlightMaterial };

        toolManager.ClearToolHighlight(renderer);

        Assert.AreEqual(GetBaseMaterialName(originalMat), GetBaseMaterialName(renderer.materials[0]));
        Assert.IsFalse(originalMaterials.ContainsKey(renderer));

        UnityEngine.Object.DestroyImmediate(toolGO);
    }

    [Test]
    public void ClearAllToolHighlights_RestoresAllRenderers()
    {
        var toolGO1 = new GameObject("Tool3");
        var renderer1 = toolGO1.AddComponent<MeshRenderer>();
        Material originalMat1 = new Material(Shader.Find("Standard"));
        renderer1.materials = new Material[] { originalMat1 };
        var toolScript = toolGO1.AddComponent<ToolFake>();
        toolScript.ToolName = "Hammer";
        
        var toolsListField = typeof(ToolManager).GetField("toolsList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        List<GameObject> toolsList = (List<GameObject>)toolsListField.GetValue(toolManager);
        toolsList.Add(toolGO1);
        
        toolManager.HighlightToolByName("Screwdriver");

        
        var toolGO2 = new GameObject("Tool4");
        var renderer2 = toolGO2.AddComponent<MeshRenderer>();
        Material originalMat2 = new Material(Shader.Find("Standard"));
        renderer2.materials = new Material[] { originalMat2 };

        var origMatsField = typeof(ToolManager).GetField("originalToolMaterials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var originalMaterials = (Dictionary<Renderer, Material[]>)origMatsField.GetValue(toolManager);
        originalMaterials[renderer1] = new Material[] { originalMat1 };
        originalMaterials[renderer2] = new Material[] { originalMat2 };

        renderer1.materials = new Material[] { highlightMaterial };
        renderer2.materials = new Material[] { highlightMaterial };

        toolManager.ClearAllToolHighlights();

        // Both renderers should have their original materials restored.
        Assert.AreEqual(GetBaseMaterialName(originalMat1), GetBaseMaterialName(renderer1.materials[0]));
        Assert.AreEqual(GetBaseMaterialName(originalMat2), GetBaseMaterialName(renderer2.materials[0]));
        Assert.IsEmpty(originalMaterials);

        UnityEngine.Object.DestroyImmediate(toolGO1);
        UnityEngine.Object.DestroyImmediate(toolGO2);
    }
}
