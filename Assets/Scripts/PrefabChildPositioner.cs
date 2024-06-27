using UnityEngine;

public class PrefabChildPositioner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject table;
    [SerializeField]
    private float extraSpacing = 0.1f; // Additional spacing between objects

    void Start()
    {
        GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        Vector3 tablePosition = table.transform.position;
        float Xsize = table.transform.localScale.x / 2;
        float Zsize = table.transform.localScale.z / 2;

        float xOffset = 0.0f;
        Vector3 newPosition = Vector3.zero;
        float width = 0.0f;
        float oldPosition = tablePosition.x - Xsize;
        for (int i = 0; i < instantiatedPrefab.transform.childCount; i++)
        {
            Transform child = instantiatedPrefab.transform.GetChild(i);

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                width = renderer.bounds.size.x;
                newPosition = new Vector3(oldPosition + xOffset, tablePosition.y * 2, tablePosition.z - Zsize/2);
                child.position = newPosition;
                oldPosition = newPosition.x;
                xOffset = width + extraSpacing;
            }
        }
    }
}
