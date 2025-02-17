using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentPositioner : MonoBehaviour
{
    [SerializeField] private MeshRenderer tableRenderer;
    [SerializeField] private GameObject tableRoll;
    [SerializeField] private float extraSpacing = 0.1f;

    [SerializeField] private float scrollSpeed = 1.0f;

    [SerializeField] private GameObject parent;

    private List<Transform> spawnedChildren = new List<Transform>();
    private Bounds tableBounds;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startScrollClip;
    [SerializeField] private AudioClip loopScrollClip;
    private CancellationTokenSource cancellationTokenSource;

    // Campi per il pannello di progresso
    [Header("Progress UI")] [SerializeField]
    private GameObject progressPanel; 

    [SerializeField] private Slider progressBar; 
    [SerializeField] private TextMeshProUGUI progressText;

    private bool isScrolling = false;
    private bool isScrollingLeft = false; 

    private bool buttonRightPressed = false;
    private bool buttonLeftPressed = false;

    public bool ButtonRightPressed
    {
        get => buttonRightPressed;
        set => buttonRightPressed = value;
    }

    public bool ButtonLeftPressed
    {
        get => buttonLeftPressed;
        set => buttonLeftPressed = value;
    }

    public GameObject Parent
    {
        get => parent;
        set => parent = value;
    }

    public GameObject TableRoll
    {
        get => tableRoll;
        set => tableRoll = value;
    }

    public AudioSource AudioSource
    {
        get => audioSource;
        set => audioSource = value;
    }

    public AudioClip StartScrollClip
    {
        get => startScrollClip;
        set => startScrollClip = value;
    }

    public AudioClip LoopScrollClip
    {
        get => loopScrollClip;
        set => loopScrollClip = value;
    }

    public float ScrollSpeed
    {
        get => scrollSpeed;
        set => scrollSpeed = value;
    }

    public bool IsScrolling
    {
        get => isScrolling;
        set => isScrolling = value;
    }

    public AudioClip LoopScrollClip1
    {
        get => loopScrollClip;
        set => loopScrollClip = value;
    }

    public void Start()
    {
        tableRenderer = tableRoll.GetComponent<MeshRenderer>();
        tableBounds = tableRenderer.bounds;

        audioSource = GetComponent<AudioSource>();

        if (parent == null)
        {
            parent = new GameObject("Parent");
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
        }

        progressPanel.SetActive(false);
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

        AddCoACDCollidersToComponentsAsync(Manager.Instance.Components);
    }

    /// Processes each component, generates convex collider meshes using CoACD, assigns MeshColliders,
    /// and gathers all generated mesh data to save at runtime.
    public async void AddCoACDCollidersToComponentsAsync(List<Transform> components)
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        string fileName = Manager.Instance.ModelName + ".json";
        string directoryPath = Path.Combine(Application.persistentDataPath, "ColliderData");
        string filePath = Path.Combine(directoryPath, fileName);

        if (File.Exists(filePath))
        {
            // Se esiste un file salvato lo usiamo per ricostruire i collider
            RuntimeColliderData loadedData = ColliderDataLoader.LoadRuntimeColliderData(filePath);
            if (loadedData != null)
            {
                Debug.Log("Using saved collider data...");
                for (int i = 0; i < components.Count; i++)
                {
                    Transform comp = components[i];
                    MeshGroup group = loadedData.meshGroups[i];

                    // Rimuovo eventuali collider esistenti
                    foreach (Collider col in comp.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }

                    // Ricostruisco e assegno i MeshCollider per ciascuna mesh convesso
                    foreach (MeshData mData in group.computedMeshes)
                    {
                        Mesh convexMesh = MeshDataConverter.ConvertMeshDataToMesh(mData);
                        MeshCollider meshCollider = comp.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = convexMesh;
                        meshCollider.convex = true;
                    }
                }

                if (progressPanel != null)
                    progressPanel.SetActive(false);
                return;
            }
            else
            {
                Debug.LogWarning("Saved collider data does not match the current components. Recomputing colliders.");
            }
        }

        // Mostro la UI di progresso
        if (progressPanel != null)
            progressPanel.SetActive(true);
        if (progressBar != null)
            progressBar.value = 0;
        if (progressText != null)
            progressText.text = "Generating colliders: 0%";

        int totalComponents = components.Count;
        CoACD coacd = GetComponent<CoACD>();
        if (coacd == null)
        {
            Debug.LogError("CoACD component not found on this GameObject.");
            return;
        }

        RuntimeColliderData runtimeData = new RuntimeColliderData();

        // Per ciascun componente decomposizione in background
        for (int i = 0; i < totalComponents; i++)
        {
            // Controlla se l'operazione è stata annullata
            if (token.IsCancellationRequested)
            {
                Debug.Log("Operazione annullata dall'utente.");
                break;
            }

            Transform comp = components[i];
            MeshFilter mf = comp.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                Debug.LogWarning($"Component {comp.name} does not have a valid MeshFilter or mesh.");
                continue;
            }

            Mesh mesh = mf.sharedMesh;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var vertexCount = mesh.vertexCount;


            List<CoACD.ComputedMeshData> computedData = await Task.Run(() =>
                coacd.RunACD_ComputeData(mesh, vertices, triangles, vertexCount, token), token);

            // Ricostruisco le Mesh in Unity
            List<Mesh> convexMeshes = coacd.CreateMeshesFromData(computedData);

            // Rimuovo eventuali collider esistenti
            foreach (Collider col in comp.GetComponents<Collider>())
            {
                Destroy(col);
            }

            // Assegno un nuovo MeshCollider per ciascuna mesh generata
            foreach (Mesh convexMesh in convexMeshes)
            {
                MeshCollider meshCollider = comp.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = convexMesh;
                meshCollider.convex = true;
            }

            // Salvo i dati per il salvataggio runtime
            MeshGroup group = new MeshGroup();
            group.baseMesh = MeshDataConverter.ConvertMeshToMeshData(mesh);
            foreach (Mesh convexMesh in convexMeshes)
            {
                group.computedMeshes.Add(MeshDataConverter.ConvertMeshToMeshData(convexMesh));
            }

            runtimeData.meshGroups.Add(group);

            // Aggiorno la UI di progresso
            float progress = (float)(i + 1) / totalComponents;
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"Generating colliders: {(int)(progress * 100)}%";
        }

        if (progressPanel != null)
            progressPanel.SetActive(false);


        if (!token.IsCancellationRequested)
        {
            SaveRuntimeColliderData(runtimeData, Manager.Instance.ModelName + ".json");
        }
    }

    public void CancelProcessing()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            Debug.Log("Richiesta di annullamento inviata.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// Saves the provided runtime collider data as JSON in a folder within Application.persistentDataPath.
    private void SaveRuntimeColliderData(RuntimeColliderData data, string fileName)
    {
        ColliderDataSaver.SaveRuntimeColliderData(data, fileName);
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
            ComponentObject component = child.GetComponent<ComponentObject>();
            if (component != null)
            {
                if((component.HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || component.IsAutomaticSnap)
                    continue;
            }
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
            if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                continue;
                
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
        if (rightmostChild != null && rightmostChild.position.x <
            tableBounds.min.x - rightmostChild.GetComponent<Renderer>().bounds.size.x / 2)
        {
            float startPosition = tableBounds.max.x;
            float currentX = startPosition;
            int childCount = components.Count;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = components[i];
                if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                    continue;
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    Vector3 pivotOffset = child.position - childBounds.center;

                    float width = childBounds.size.x;
                    float height = childBounds.size.y;
                    Vector3 newPosition = new Vector3(currentX + width / 2, tableBounds.max.y + height / 2 + 0.01f,
                        tableBounds.center.z);

                    newPosition += pivotOffset;
                    child.position = newPosition;
                    currentX += width + extraSpacing;
                }
            }
        }

        // Attiva o disattiva i figli in base alla loro posizione
        foreach (Transform child in components)
        {
            if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                continue;
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
            if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                continue;
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
        if (leftmostChild != null && leftmostChild.position.x >
            tableBounds.max.x + leftmostChild.GetComponent<Renderer>().bounds.size.x / 2)
        {
            float startPosition = tableBounds.min.x;
            float currentX = startPosition;
            int childCount = components.Count;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = components[i];
                if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                    continue;
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds childBounds = renderer.bounds;
                    Vector3 pivotOffset = child.position - childBounds.center;

                    float width = childBounds.size.x;
                    float height = childBounds.size.y;
                    Vector3 newPosition = new Vector3(currentX - width / 2, tableBounds.max.y + height / 2 + 0.01f,
                        tableBounds.center.z);

                    newPosition += pivotOffset;
                    child.position = newPosition;
                    currentX -= width + extraSpacing;
                }
            }
        }

        // Attiva o disattiva i figli in base alla loro posizione
        foreach (Transform child in components)
        {
            if((child.GetComponent<ComponentObject>().HasMoved && child.GetComponent<XRSimpleInteractable>() == null) || child.GetComponent<ComponentObject>().IsAutomaticSnap)
                continue;
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