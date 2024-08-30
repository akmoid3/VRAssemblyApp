using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class ModelLoaderPlayModeTests
{
    [UnityTest]
    public IEnumerator TestModelLoaderWithRealFile()
    {
        var loader = new GameObject().AddComponent<ModelLoader>();
        var loadTask = loader.LoadFromFile("Assets/ModelsTest/Test.glb");

        // Wait for the load operation to complete
        yield return new WaitUntil(() => loadTask.IsCompleted);

        // Check results
        Assert.IsNotNull(loadTask.Result);
        // Further assertions as necessary
    }

    [UnityTest]
    public IEnumerator TestModelLoaderWithNonValidFile_()
    {
        var loader = new GameObject().AddComponent<ModelLoader>();
        var loadTask = loader.LoadFromFile("Assets/ModelsTest/nonvalid.glb");

        // Wait for the load operation to complete
        yield return new WaitUntil(() => loadTask.IsCompleted);

        // Check results
        Assert.IsNull(loadTask.Result);
        // Further assertions as necessary
    }
}
