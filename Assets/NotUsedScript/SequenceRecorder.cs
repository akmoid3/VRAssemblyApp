using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SequenceRecorder : MonoBehaviour
{
    public List<string> buildOrder = new List<string>();
    public bool isRecording = false;

    public void RecordAction(string objectName)
    {
        // Check if recording is enabled
        if (!isRecording)
        {
            Debug.Log("Recording is not enabled.");
            return;
        }

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

    public void StartRecording()
    {
        isRecording = true;
        Debug.Log("Recording started.");
    }
}
