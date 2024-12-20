using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private GameObject parent;

    private List<Transform> spawnedChildren = new List<Transform>();
    private Bounds tableBounds;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip startScrollClip;
    [SerializeField]
    private AudioClip loopScrollClip;

    private bool isScrolling = false;
    private bool isScrollingLeft = false; // Tracks whether we are scrolling left or right

    private bool buttonRightPressed = false;
    private bool buttonLeftPressed = false;


    public bool ButtonRightPressed { get => buttonRightPressed; set => buttonRightPressed = value; }
    public bool ButtonLeftPressed { get => buttonLeftPressed; set => buttonLeftPressed = value; }
    public GameObject Parent { get => parent; set => parent = value; }
    public GameObject TableRoll { get => tableRoll; set => tableRoll = value; }
    public AudioSource AudioSource { get => audioSource; set => audioSource = value; }
    public AudioClip StartScrollClip { get => startScrollClip; set => startScrollClip = value; }
    public AudioClip LoopScrollClip { get => loopScrollClip; set => loopScrollClip = value; }
    public float ScrollSpeed { get => scrollSpeed; set => scrollSpeed = value; }
    public bool IsScrolling { get => isScrolling; set => isScrolling = value; }
    public AudioClip LoopScrollClip1 { get => loopScrollClip; set => loopScrollClip = value; }

    public void Start()
    {
        tableRenderer = tableRoll.GetComponent<MeshRenderer>();
        tableBounds = tableRenderer.bounds;
        manager = Manager.Instance;

        audioSource = GetComponent<AudioSource>();

        if (parent == null)
        {
            parent = new GameObject("Parent");
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
        }
    }

    public void Update()
    {
        if (buttonLeftPressed)
        {
            isScrollingLeft = true;
            StartScrolling();
        }
        else if (buttonRightPressed)
        {
            isScrollingLeft = false;
            StartScrolling();
        }
        else
        {
            StopScrolling();
        }

        if (isScrolling)
        {
            if (isScrollingLeft)
            {
                ScrollLeft();
            }
            else
            {
                ScrollRight();
            }
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
            List<Transform> allChildrenWithMesh = new List<Transform>();
            CollectChildrenWithMesh(instantiatedPrefab.transform, allChildrenWithMesh);

            RepositionComponentsOnTable(allChildrenWithMesh);

            Manager.Instance.Components = allChildrenWithMesh;

           

            Destroy(instantiatedPrefab);
        }
    }

    public void RepositionComponentsOnTable(List<Transform> components)
    {
        Vector3 newPosition;
        float width;
        float height;
        float startPosition = tableBounds.min.x + 0.2f;
        float currentX = startPosition;

        foreach (Transform child in components)
        {
            child.SetParent(null);
            child.rotation = Quaternion.identity;
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds childBounds = renderer.bounds;
                Vector3 pivotOffset = child.position - childBounds.center;

                width = childBounds.size.x;
                height = childBounds.size.y;
                newPosition = new Vector3(
                    currentX + width / 2,
                    tableBounds.max.y + height / 2 + 0.01f,
                    tableBounds.center.z
                );

                newPosition += pivotOffset;

                child.position = newPosition;
                currentX += width + extraSpacing;

                if(!child.GetComponent<ComponentObject>())
                    child.gameObject.AddComponent<ComponentObject>();

                if(!child.GetComponent<MakeGrabbable>())
                    child.gameObject.AddComponent<MakeGrabbable>();

                child.SetParent(parent.transform);

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
    }

    public void RepositionComponentOnTable(Transform child)
    {
        Vector3 newPosition;
        float width;
        float height;
        float startPosition = tableBounds.min.x + 0.2f;
        float currentX = startPosition;

            child.rotation = Quaternion.identity;
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds childBounds = renderer.bounds;
                Vector3 pivotOffset = child.position - childBounds.center;

                width = childBounds.size.x;
                height = childBounds.size.y;
                newPosition = new Vector3(
                    currentX + width / 2,
                    tableBounds.max.y + height / 2 + 0.01f,
                    tableBounds.center.z
                );

                newPosition += pivotOffset;

                child.position = newPosition;
                currentX += width + extraSpacing;

                if (!child.GetComponent<ComponentObject>())
                    child.gameObject.AddComponent<ComponentObject>();

                if (!child.GetComponent<MakeGrabbable>())
                    child.gameObject.AddComponent<MakeGrabbable>();

                child.SetParent(parent.transform);

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

    private void CollectChildrenWithMesh(Transform parent, List<Transform> resultList)
    {
        foreach (Transform child in parent)
        {
            if (child.GetComponent<MeshRenderer>())
            {
                resultList.Add(child);
            }

            // Recursive call for nested children
            if (child.childCount > 0)
            {
                CollectChildrenWithMesh(child, resultList);
            }
        }
    }

    public void ScrollLeft()
    {
        Transform rightmostChild = null;
        float rightmostX = float.MinValue;
        var components = Manager.Instance.Components;
        // Scroll all children to the left and find the rightmost child
        foreach (Transform child in components)
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

        // If the rightmost child goes out of bounds, reposition it to the right
        if (rightmostChild != null && rightmostChild.position.x < tableBounds.min.x - rightmostChild.GetComponent<Renderer>().bounds.size.x / 2)
        {
            float startPosition = tableBounds.max.x;
            float currentX = startPosition;
            int childCount = components.Count;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = components[i];
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    Vector3 pivotOffset = child.position - childBounds.center;

                    float width = childBounds.size.x;
                    float height = childBounds.size.y;
                    Vector3 newPosition = new Vector3(currentX + width / 2, tableBounds.max.y + height / 2 + 0.01f, tableBounds.center.z);

                    newPosition += pivotOffset;
                    child.position = newPosition;
                    currentX += width + extraSpacing;
                }
            }
        }

        // Activate or deactivate children based on their position relative to the table bounds
        foreach (Transform child in components)
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

    public void ScrollRight()
    {
        Transform leftmostChild = null;
        float leftmostX = float.MaxValue;
        var components = Manager.Instance.Components;

        // Scroll all children to the right and find the leftmost child
        foreach (Transform child in components)
        {
            Vector3 position = child.position;
            position.x += scrollSpeed * Time.deltaTime;
            child.position = position;

            // Find the leftmost child
            float childLeftX = position.x - child.GetComponent<Renderer>().bounds.size.x / 2;
            if (childLeftX < leftmostX)
            {
                leftmostX = childLeftX;
                leftmostChild = child;
            }
        }

        // If the leftmost child goes out of bounds, reposition it to the left
        if (leftmostChild != null && leftmostChild.position.x > tableBounds.max.x + leftmostChild.GetComponent<Renderer>().bounds.size.x / 2)
        {
            float startPosition = tableBounds.min.x;
            float currentX = startPosition;
            int childCount = components.Count;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = components[i];
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    Vector3 pivotOffset = child.position - childBounds.center;

                    float width = childBounds.size.x;
                    float height = childBounds.size.y;
                    Vector3 newPosition = new Vector3(currentX - width / 2, tableBounds.max.y + height / 2 + 0.01f, tableBounds.center.z);

                    newPosition += pivotOffset;
                    child.position = newPosition;
                    currentX -= width + extraSpacing;
                }
            }
        }

        // Activate or deactivate children based on their position relative to the table bounds
        foreach (Transform child in components)
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

    public void StartScrolling()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = startScrollClip;
            audioSource.Play();

            // Schedule the looped clip to start after the start clip
            if (startScrollClip != null)
                Invoke("StartLoopingSound", startScrollClip.length);
        }

        isScrolling = true;
    }

    public void StopScrolling()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        isScrolling = false;
    }

    public void StartLoopingSound()
    {
        if (isScrolling && audioSource != null)
        {
            audioSource.clip = loopScrollClip;
            audioSource.Play();
        }
    }
}
