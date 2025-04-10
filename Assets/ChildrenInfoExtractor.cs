using UnityEngine;
using System.Collections.Generic;
using System;

// This script should be attached to a parent GameObject that has children you want to analyze
public class ChildrenInfoExtractor : MonoBehaviour
{
    [Serializable]
    public class ComponentData
    {
        public string componentName;
        public Vector3Serializable position;
        public QuaternionSerializable rotation;
    }

    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }

    [Serializable]
    public class QuaternionSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionSerializable(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
    }

    [Serializable]
    public class ComponentsWrapper
    {
        public List<ComponentData> components = new List<ComponentData>();
    }

    // Reference to save the output to a text file
    public bool saveToFile = false;
    public string fileName = "children_info.json";

    // Generate the JSON data for children with mesh components
    public void GenerateChildrenInfo()
    {
        ComponentsWrapper wrapper = new ComponentsWrapper();
        int stepId = 0;
        
        // Process each child
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Only include children that have a mesh component
            if (HasMeshComponent(child))
            {
                ComponentData data = new ComponentData
                {
                    componentName = child.name,
                    position = new Vector3Serializable(child.position),
                    rotation = new QuaternionSerializable(child.rotation)
                };
                
                wrapper.components.Add(data);
                stepId++;
            }
        }
        
        // Convert to JSON
        string json = JsonUtility.ToJson(wrapper, true);
        
        // Print to console
        Debug.Log(json);
        
        // Save to file if enabled
        if (saveToFile)
        {
            System.IO.File.WriteAllText(Application.dataPath + "/" + fileName, json);
            Debug.Log("Saved to " + Application.dataPath + "/" + fileName);
        }
    }
    
    // Check if the transform has a mesh component
    private bool HasMeshComponent(Transform transform)
    {
        // Check for MeshFilter or MeshRenderer
        return transform.GetComponent<MeshFilter>() != null || 
               transform.GetComponent<MeshRenderer>() != null;
    }
    
    // Editor button to generate data
    [ContextMenu("Generate Children Info")]
    public void GenerateChildrenInfoContextMenu()
    {
        GenerateChildrenInfo();
    }
}