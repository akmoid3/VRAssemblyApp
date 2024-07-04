using UnityEngine;

public class PrefabPositioner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject table;
    [SerializeField]
    private float extraSpacing = 0.1f; // Additional spacing between objects
    [SerializeField]
    private Manager manager;
    [SerializeField]
    private bool prefabInstanced = false;

    private void Update()
    {
        string prefabName = manager.GetCurrentSelectedPrefabName();
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

        if (manager.GetModelConfirmed() && !prefabInstanced)
        {
            GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, null);
            Vector3 tablePosition = table.transform.position;
            float Xsize = table.transform.localScale.x / 2;
            float Zsize = table.transform.localScale.z / 2;

            float xOffset = 0.0f;
            Vector3 newPosition = Vector3.zero;
            float width = 0.0f;
            float height = 0.0f;
            float oldPosition = tablePosition.x - Xsize;
            Transform child = null;
            // Iterate over all children of instantiatedPrefab
            for (int i = 0; i < instantiatedPrefab.transform.childCount; i++)
            {
                child = instantiatedPrefab.transform.GetChild(i);
                
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    width = renderer.bounds.size.x;
                    height = renderer.bounds.size.y;
                    newPosition = new Vector3(oldPosition + xOffset, tablePosition.y * 2 + height / 2, tablePosition.z - Zsize / 2);
                    child.position = newPosition;
                    oldPosition = newPosition.x;
                    xOffset = width + extraSpacing;
                }
            }

            prefabInstanced = true;
        }
    }
}
