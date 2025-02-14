using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Per le UI
using MeshProcess;
using TMPro;

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

    // Campi per il pannello di progresso
    [Header("Progress UI")]
    [SerializeField] private GameObject progressPanel; // Pannello intero (da attivare/disattivare)
    [SerializeField] private Slider progressBar;        // Slider per il progresso
    [SerializeField] private TextMeshProUGUI progressText;           // Testo che mostra la percentuale

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

    public async void SpawnComponents()
    {
        if (tableRenderer == null)
        {
            Debug.LogError("Table Renderer not assigned!");
            return;
        }

        GameObject prefab = Manager.Instance.Model;

        if (prefab != null)
        {
            GameObject instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity, null);
            List<Transform> allChildrenWithMesh = new List<Transform>();
            CollectChildrenWithMesh(instantiatedPrefab.transform, allChildrenWithMesh);

            RepositionComponentsOnTable(allChildrenWithMesh);

            Manager.Instance.Components = allChildrenWithMesh;

            Destroy(instantiatedPrefab);
        }

        // Per ogni componente, aggiungiamo i collider generati dalla convex decomposition
        await AddVHACDCollidersToComponentsAsync(Manager.Instance.Components);
    }

    public async Task AddVHACDCollidersToComponentsAsync(List<Transform> components)
    {
        // Attiva il pannello di progresso e azzera lo slider
        if (progressPanel != null)
            progressPanel.SetActive(true);
        if (progressBar != null)
            progressBar.value = 0;
        if (progressText != null)
            progressText.text = "Loading colliders: 0%";

        int totalComponents = components.Count;
        for (int i = 0; i < totalComponents; i++)
        {
            Transform comp = components[i];
            MeshFilter mf = comp.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                Debug.LogWarning($"Il componente {comp.name} non ha un MeshFilter o un mesh valido.");
                continue;
            }

            // Aggiungi il componente VHACD se non esiste già
            VHACD vhacd = comp.GetComponent<VHACD>();
            if (vhacd == null)
            {
                vhacd = comp.gameObject.AddComponent<VHACD>();
            }

            Mesh mesh = mf.sharedMesh;
            // Esegui la convex decomposition in background (passando mesh, vertici e triangoli)
            List<Mesh> convexMeshes = await vhacd.GenerateConvexMeshesAsync(mesh, mesh.vertices, mesh.triangles);
            if (convexMeshes == null || convexMeshes.Count == 0)
            {
                Debug.LogWarning($"VHACD non ha generato mesh per il componente {comp.name}.");
                continue;
            }

            // Rimuovi tutti i collider esistenti per evitare sovrapposizioni
            Collider[] existingColliders = comp.GetComponents<Collider>();
            foreach (Collider col in existingColliders)
            {
                Destroy(col);
            }

            // Aggiungi i MeshCollider (sul thread principale)
            foreach (Mesh convexMesh in convexMeshes)
            {
                MeshCollider meshCollider = comp.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = convexMesh;
                meshCollider.convex = true;
            }

            // Aggiorna il progresso
            float progress = (float)(i + 1) / totalComponents;
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"Loading colliders: {(int)(progress * 100)}%";

            // Rilascia il controllo per permettere l'aggiornamento dell'interfaccia
            await Task.Yield();
        }

        // Nascondi il pannello di progresso al termine
        if (progressPanel != null)
            progressPanel.SetActive(false);
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

                if (!child.GetComponent<ComponentObject>())
                    child.gameObject.AddComponent<ComponentObject>();

                if (!child.GetComponent<MakeGrabbable>())
                    child.gameObject.AddComponent<MakeGrabbable>();

                child.SetParent(parent.transform);

                // Disattiva se fuori dai limiti
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

    private void CollectChildrenWithMesh(Transform parentT, List<Transform> resultList)
    {
        foreach (Transform child in parentT)
        {
            if (child.GetComponent<MeshRenderer>())
            {
                resultList.Add(child);
            }

            // Chiamata ricorsiva per i figli
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
        // Muovi tutti i figli a sinistra e trova il più a destra
        foreach (Transform child in components)
        {
            Vector3 position = child.position;
            position.x -= scrollSpeed * Time.deltaTime;
            child.position = position;

            float childRightX = position.x + child.GetComponent<Renderer>().bounds.size.x / 2;
            if (childRightX > rightmostX)
            {
                rightmostX = childRightX;
                rightmostChild = child;
            }
        }

        // Se il figlio più a destra esce dai limiti, lo riposiziona a destra
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

        // Attiva o disattiva i figli in base alla loro posizione
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

        // Muovi tutti i figli a destra e trova il più a sinistra
        foreach (Transform child in components)
        {
            Vector3 position = child.position;
            position.x += scrollSpeed * Time.deltaTime;
            child.position = position;

            float childLeftX = position.x - child.GetComponent<Renderer>().bounds.size.x / 2;
            if (childLeftX < leftmostX)
            {
                leftmostX = childLeftX;
                leftmostChild = child;
            }
        }

        // Se il figlio più a sinistra esce dai limiti, lo riposiziona a sinistra
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

        // Attiva o disattiva i figli in base alla loro posizione
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
