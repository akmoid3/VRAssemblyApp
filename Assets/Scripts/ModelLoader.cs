using UnityEngine;
using System.Threading.Tasks;

public interface IModelLoader
{
    Task<GameObject> LoadFromFile(string filePath);
}

public class ModelLoader : MonoBehaviour, IModelLoader
{

    public async Task<GameObject> LoadFromFile(string filePath)
    {
        var gltf = new GLTFast.GltfImport();

        // Load the glTF
        var success = await gltf.Load("file:///" + filePath);

        if (success)
        {
            var tempParent = new GameObject("tempParent");
            var instantiateSuccess = await gltf.InstantiateMainSceneAsync(tempParent.transform);
            GameObject gameObject = null;
            if (instantiateSuccess)
            {
                gameObject = tempParent.transform.GetChild(0).gameObject;
                Destroy(tempParent);
            }
            // Return the root GameObject of the loaded model
            return gameObject;
        }
        else
        {
            Debug.LogError("Loading glb failed!");
            return null;
        }
    }

}
