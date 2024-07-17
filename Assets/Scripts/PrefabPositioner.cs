using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class PrefabPositioner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private MeshRenderer tableRenderer;
    [SerializeField]
    private float extraSpacing = 0.1f;
    [SerializeField]
    private Manager manager;
    [SerializeField]
    private bool prefabInstanced = false;
    [SerializeField]
    private float scrollSpeed = 1.0f;
    [SerializeField]
    private KeyCode scrollKey = KeyCode.Space;

    private List<Transform> spawnedChildren = new List<Transform>();
    private Bounds tableBounds;
    private float totalWidth = 0.0f;

    void Start()
    {
        if (tableRenderer != null)
        {
            tableBounds = tableRenderer.bounds;
        }
        else
        {
            Debug.LogError("Table Renderer not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetKey(scrollKey))
        {
            ScrollComponents();
        }
    }

    public void SpawnComponents()
    {
        if (tableRenderer == null)
        {
            Debug.LogError("Table Renderer not assigned!");
            return;
        }

        string prefabName = manager.GetCurrentSelectedPrefabName();
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

        if (prefab != null)
        {
            GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, null);
            Vector3 newPosition = Vector3.zero;
            float width;
            float height;
            float startPosition = tableBounds.min.x; // Start position for the components
            float currentX = startPosition;

            // Position the components within the bounds of the table
            for (int i = 0; i < instantiatedPrefab.transform.childCount; i++)
            {
                Transform child = instantiatedPrefab.transform.GetChild(i);
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    width = childBounds.size.x;
                    height = childBounds.size.y;
                    newPosition = new Vector3(currentX + width / 2, tableBounds.max.y + height / 2, tableBounds.center.z);
                    child.position = newPosition;
                    currentX += width + extraSpacing;

                    spawnedChildren.Add(child);
                    // Deactivate if out of bounds
                    if (child.position.x > tableBounds.max.x)
                    {
                        child.gameObject.SetActive(false);
                    }else
                    child.gameObject.SetActive(true);
                }
            }

            prefabInstanced = true;
        }
    }

    private void ScrollComponents()
    {
        foreach (Transform child in spawnedChildren)
        {
            Vector3 position = child.position;
            position.x -= scrollSpeed * Time.deltaTime;

            // Check if the component has moved out of the left bounds
            if (position.x < tableBounds.min.x - child.GetComponent<Renderer>().bounds.size.x / 2)
            {
                // Find the rightmost child to determine new position
                float rightmostX = float.MinValue;
                foreach (Transform sibling in spawnedChildren)
                {
                    float siblingRightX = sibling.position.x + sibling.GetComponent<Renderer>().bounds.size.x / 2;
                    if (siblingRightX > rightmostX)
                    {
                        rightmostX = siblingRightX;
                    }
                }

                // Position this child behind the rightmost child
                position.x = rightmostX + child.GetComponent<Renderer>().bounds.size.x;
            }

            // Deactivate if out of bounds
            if (position.x < tableBounds.min.x || position.x > tableBounds.max.x)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }

            child.position = position;
        }
    }
}
