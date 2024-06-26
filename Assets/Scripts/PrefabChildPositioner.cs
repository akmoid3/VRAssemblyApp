using UnityEngine;

public class PrefabChildPositioner : MonoBehaviour
{
    public GameObject prefab; 
    public GameObject table; 

    void Start()
    {

        GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, transform);

        for (int i = 0; i < instantiatedPrefab.transform.childCount; i++)
        {
            Transform child = instantiatedPrefab.transform.GetChild(i);

            Vector3 tablePosition = table.transform.position;
            float xOffset = i * .5f;
            Vector3 newPosition = new Vector3(tablePosition.x + xOffset, tablePosition.y * 2, tablePosition.z);

            child.position = newPosition;
        }
    }
}
