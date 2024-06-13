using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SequenceRecorder : MonoBehaviour
{
    public List<string> buildOrder = new List<string>();

    public void RecordAction(string objectName)
    {
        // Check if the component already exists in the sequence
        if (!buildOrder.Contains(objectName))
        {
            buildOrder.Add(objectName);
            Debug.Log($"Recorded action: {objectName}");
        }
        else
        {
            Debug.Log($"Component '{objectName}' already exists in the sequence. Skipping recording.");
        }
    }

    public void SaveSequenceToJson(string filePath)
    {
        string json = JsonUtility.ToJson(this);
        File.WriteAllText(filePath, json);
        Debug.Log($"Build sequence saved to: {filePath}");
    }

    public void ClearRecordedActions()
    {
        buildOrder.Clear();
    }
}
