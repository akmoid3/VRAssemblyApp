using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> toolsList;      
    [SerializeField] private Material toolHighlightMaterial;  

    private Dictionary<Renderer, Material[]> originalToolMaterials = new Dictionary<Renderer, Material[]>();

 
    public void HighlightToolByName(string correctToolName)
    {
        if(toolsList == null)
            return;
        foreach (GameObject tool in toolsList)
        {
            if (tool == null)
                continue;

            Tool toolScript = tool.GetComponent<Tool>();
            if (toolScript == null)
                continue;

            Renderer[] renderers = tool.GetComponentsInChildren<Renderer>();

            if (toolScript.ToolName == correctToolName)
            {
                foreach (Renderer rend in renderers)
                {
                    if (rend == null)
                        continue;

                    if (!originalToolMaterials.ContainsKey(rend))
                    {
                        originalToolMaterials[rend] = rend.materials;
                    }

                    Material[] highlightMats = new Material[rend.materials.Length];
                    for (int i = 0; i < highlightMats.Length; i++)
                    {
                        highlightMats[i] = toolHighlightMaterial;
                    }
                    rend.materials = highlightMats;
                }
            }
            else
            {
                foreach (Renderer rend in renderers)
                {
                    if (rend == null)
                        continue;
                    ClearToolHighlight(rend);
                }
            }
        }
    }


    public void ClearToolHighlight(Renderer rend)
    {
        if (originalToolMaterials.TryGetValue(rend, out Material[] origMaterials))
        {
            rend.materials = origMaterials;
            originalToolMaterials.Remove(rend);
        }
    }


    public void ClearAllToolHighlights()
    {
        foreach (var pair in originalToolMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.materials = pair.Value;
            }
        }
        originalToolMaterials.Clear();
    }
}
