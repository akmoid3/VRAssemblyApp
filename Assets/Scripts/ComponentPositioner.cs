using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class ComponentPositioner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private MeshRenderer tableRenderer;
    [SerializeField]
    private Object tableRoll;
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
    [SerializeField]
    private GameObject parent;

    private bool isOverTableMaxX = false;

    private List<Transform> spawnedChildren = new List<Transform>();
    private Bounds tableBounds;

    void Start()
    {
        tableRenderer = tableRoll.GetComponent<MeshRenderer>();

        tableBounds = tableRenderer.bounds;

        // Create the common parent if not assigned
        if (parent == null)
        {
            parent = new GameObject("Parent");
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
            Vector3 newPosition;
            float width;
            float height;
            float startPosition = tableBounds.min.x; // Start position for the components
            float currentX = startPosition;
            float childCount = instantiatedPrefab.transform.childCount;
            // Position the components within the bounds of the table
            for (int i = 0; i < childCount; i++)
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
                        isOverTableMaxX = true;
                    }
                    else
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }

            for (int i = 0; i < childCount; i++)
            {
                spawnedChildren[i].SetParent(parent.transform);
            }

            Destroy(instantiatedPrefab);
            prefabInstanced = true;
        }
    }
    private void ScrollComponents()
    {
        foreach (Transform child in parent.transform)
        {
            Vector3 position = child.position;
            position.x -= scrollSpeed * Time.deltaTime;

            // Check if the component has moved out of the left bounds
            if (position.x < tableBounds.min.x - child.GetComponent<Renderer>().bounds.size.x / 2)
            {
                // Find the rightmost child to determine new position
                float rightmostX = float.MinValue;
                foreach (Transform sibling in parent.transform)
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

