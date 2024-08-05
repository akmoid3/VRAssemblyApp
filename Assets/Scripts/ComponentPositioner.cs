using System.Collections.Generic;
using UnityEngine;

public class ComponentPositioner : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer tableRenderer;
    [SerializeField]
    private GameObject tableRoll;
    [SerializeField]
    private float extraSpacing = 0.1f;

    private Manager manager;
    [SerializeField]
    private float scrollSpeed = 1.0f;
    [SerializeField]
    private KeyCode scrollKey = KeyCode.Space;
    [SerializeField]
    private GameObject parent;

    private List<Transform> spawnedChildren = new List<Transform>();
    private Bounds tableBounds;


    void Start()
    {
        tableRenderer = tableRoll.GetComponent<MeshRenderer>();

        tableBounds = tableRenderer.bounds;
        manager = Manager.Instance;
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

        GameObject prefab = manager.Model;

        if (prefab != null)
        {
            GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, null);
            Vector3 newPosition;
            float width;
            float height;
            float startPosition = tableBounds.min.x + 0.2f;
            float currentX = startPosition;
            float childCount = instantiatedPrefab.transform.childCount;
            // Position the components within the bounds of the table
            for (int i = 0; i < childCount; i++)
            {
                Transform child = instantiatedPrefab.transform.GetChild(i);
                child.rotation = Quaternion.identity;
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
                    child.gameObject.AddComponent<ComponentObject>();
                    child.gameObject.AddComponent<MakeGrabbable>();
                    child.tag = "Component";
                    // Deactivate if out of bounds
                    if (child.position.x > tableBounds.max.x)
                    {
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }

            Manager.Instance.Components = spawnedChildren;

            for (int i = 0; i < childCount; i++)
            {
                spawnedChildren[i].SetParent(parent.transform);
            }
            Destroy(instantiatedPrefab);
        }
    }


    private void ScrollComponents()
    {
        Transform rightmostChild = null;
        float rightmostX = float.MinValue;

        // Scroll all children and find the rightmost child
        foreach (Transform child in parent.transform)
        {
            Vector3 position = child.position;
            position.x -= scrollSpeed * Time.deltaTime;
            child.position = position;

            // Find the rightmost child
            float childRightX = position.x + child.GetComponent<Renderer>().bounds.size.x / 2;
            if (childRightX > rightmostX)
            {
                rightmostX = childRightX;
                rightmostChild = child;
            }
        }

        // Check if the rightmost child is out of the left bounds
        if (rightmostChild != null && rightmostChild.position.x < tableBounds.min.x - rightmostChild.GetComponent<Renderer>().bounds.size.x / 2)
        {
            Vector3 newPosition;
            float width;
            float height;
            float startPosition = tableBounds.max.x;
            float currentX = startPosition;
            float childCount = parent.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    width = childBounds.size.x;
                    height = childBounds.size.y;
                    newPosition = new Vector3(currentX + width / 2, tableBounds.max.y + height / 2, tableBounds.center.z);
                    child.position = newPosition;
                    currentX += width + extraSpacing;
                }
            }
        }

        foreach (Transform child in parent.transform)
        {
            if (child.position.x < tableBounds.min.x || child.position.x > tableBounds.max.x)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }
        }
    }

}

