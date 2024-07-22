using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FbxSharp;

public class FbxLoadImporter : MonoBehaviour
{
    public string fileName;
    public string filePath;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(filePath))
            {

                // Load the FBX file using FBXSharp
                var fbxImporter = new FbxImporter();
                fbxImporter.Name = filePath;
                var fbxScene = fbxImporter.Import(filePath);

                //Debug.Log(fbxScene.Nodes[0].ToString());
            }
            else
            {

            }
        }
    }


}
