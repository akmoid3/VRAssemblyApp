using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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
    [SerializeField] private bool generateColliders;

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
    
    public bool GenerateColliders
    {
        get => generateColliders;
        set => generateColliders = value;
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

        if(generateColliders)
            AddCoACDColliders(Manager.Instance.Components,Manager.Instance.ModelName);
    }

    public void AddCoACDColliders(List<Transform> components, string name)
    {
        string fileName = name + ".json";
        string directoryPath = Path.Combine(Application.persistentDataPath, "ColliderData");
        string filePath = Path.Combine(directoryPath, fileName);

        if (File.Exists(filePath))
        {
            // Start loading process as a coroutine
            StartCoroutine(LoadCollidersFromFileAsync(components, filePath));
            return;
        }

        // Show progress UI
        if (progressPanel != null)
            progressPanel.SetActive(true);
        if (progressBar != null)
            progressBar.value = 0;
        if (progressText != null)
            progressText.text = "Preparing to generate colliders...";

        // Generate colliders with smooth progress
        StartCoroutine(GenerateCollidersAsync(components, name));
    }

    // Track current progress for smooth transitions
    private float currentProgressValue = 0f;
    private float targetProgressValue = 0f;

    private IEnumerator LoadCollidersFromFileAsync(List<Transform> components, string filePath)
    {
        // Reset progress tracking
        currentProgressValue = 0f;
        targetProgressValue = 0f;
        
        // Show loading progress UI
        if (progressPanel != null)
            progressPanel.SetActive(true);
        if (progressBar != null)
            progressBar.value = 0;
        if (progressText != null)
            progressText.text = "Loading collider data...";
        
        // Load file on a background thread
        RuntimeColliderData loadedData = null;
        bool loadingComplete = false;
        
        System.Threading.Tasks.Task.Run(() => {
            loadedData = ColliderDataLoader.LoadRuntimeColliderData(filePath);
            loadingComplete = true;
        });
        
        // Wait until file loading is complete, smoothly updating progress to 10%
        targetProgressValue = 0.1f;
        while (!loadingComplete)
        {
            SmoothProgressUpdate();
            yield return null;
        }
        
        if (loadedData != null)
        {
            Debug.Log("Using saved collider data...");
            
            // Update text while keeping progress at 10%
            if (progressText != null)
                progressText.text = "Processing saved collider data...";
                
            yield return new WaitForSeconds(0.1f); // Short pause for UI update
            
            int totalOperations = 0;
            // Count total operations for accurate progress
            for (int i = 0; i < components.Count && i < loadedData.meshGroups.Count; i++)
            {
                totalOperations += loadedData.meshGroups[i].computedMeshes.Count;
            }
            
            int completedOperations = 0;

            for (int i = 0; i < components.Count && i < loadedData.meshGroups.Count; i++)
            {
                Transform comp = components[i];
                MeshGroup group = loadedData.meshGroups[i];

                // Remove existing colliders
                foreach (Collider col in comp.GetComponents<Collider>())
                {
                    Destroy(col);
                }
                
                // Update target progress - removal phase (10-20%)
                targetProgressValue = 0.1f + 0.1f * (float)(i+1) / components.Count;
                SmoothProgressUpdate();
                
                if (progressText != null)
                    progressText.text = $"Applying colliders for {comp.name}...";
                
                yield return null;

                // Recreate and assign MeshCollider for each convex mesh
                for (int j = 0; j < group.computedMeshes.Count; j++)
                {
                    MeshData mData = group.computedMeshes[j];
                    Mesh convexMesh = MeshDataConverter.ConvertMeshDataToMesh(mData);
                    MeshCollider meshCollider = comp.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = convexMesh;
                    meshCollider.convex = true;
                    // Set the layer to OriginalColliders
                    meshCollider.gameObject.layer = LayerMask.NameToLayer("OriginalCollidersParent");
                    
                    completedOperations++;
                    
                    // Update progress from 20% to 95% during this phase
                    targetProgressValue = 0.2f + 0.75f * (float)completedOperations / totalOperations;
                    
                    // Process several meshes before yielding to reduce UI updates
                    if (j % 5 == 0) 
                    {
                        SmoothProgressUpdate();
                        yield return null;
                    }
                }
            }
            
            // Move to completion
            targetProgressValue = 1.0f;
            yield return StartCoroutine(CompleteProgressSmoothly("Colliders loaded successfully!"));
        }
        else
        {
            // Failed to load data
            if (progressText != null)
                progressText.text = "Failed to load collider data!";
                
            yield return new WaitForSeconds(1.0f);
            if (progressPanel != null)
                progressPanel.SetActive(false);
        }
    }

    private IEnumerator GenerateCollidersAsync(List<Transform> components, string name)
    {
        // Reset progress tracking
        currentProgressValue = 0f;
        targetProgressValue = 0f;
        
        CoACD coacd = GetComponent<CoACD>();
        if (coacd == null)
        {
            Debug.LogError("CoACD component not found on this GameObject.");
            if (progressText != null)
                progressText.text = "Error: CoACD component not found!";
            yield return new WaitForSeconds(1.0f);
            if (progressPanel != null)
                progressPanel.SetActive(false);
            yield break;
        }

        RuntimeColliderData runtimeData = new RuntimeColliderData();
        int totalComponents = components.Count;
        
        // Count total expected operations for accurate progress
        int totalOperations = totalComponents * 2; // Analysis and collider creation phases
        int completedOperations = 0;

        // Process each component
        for (int i = 0; i < totalComponents; i++)
        {
            Transform comp = components[i];
            
            if (progressText != null)
                progressText.text = $"Analyzing mesh for {comp.name}...";
                
            MeshFilter mf = comp.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                Debug.LogWarning($"Component {comp.name} does not have a valid MeshFilter or mesh.");
                completedOperations += 2; // Count as completed for progress calculation
                continue;
            }

            Mesh mesh = mf.sharedMesh;
            
            // Update target progress for analysis phase (0-40%)
            targetProgressValue = 0.4f * (float)completedOperations / totalOperations;
            SmoothProgressUpdate();
            
            yield return null;
            
            // This is potentially a long operation
            List<Mesh> convexMeshes = coacd.RunACD(mesh);
            completedOperations++;
            
            if (progressText != null)
                progressText.text = $"Creating colliders for {comp.name}...";

            // Remove existing colliders
            foreach (Collider col in comp.GetComponents<Collider>())
            {
                Destroy(col);
            }

            // Add new mesh colliders
            int meshCount = convexMeshes.Count;
            for (int j = 0; j < meshCount; j++)
            {
                Mesh convexMesh = convexMeshes[j];
                MeshCollider meshCollider = comp.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = convexMesh;
                meshCollider.convex = true;
                // Set the layer to OriginalColliders
                meshCollider.gameObject.layer = LayerMask.NameToLayer("OriginalCollidersParent");
                
                // Update target progress - creation phase (40-90%)
                float componentProgress = (float)j / meshCount;
                targetProgressValue = 0.4f + 0.5f * ((float)completedOperations / totalOperations + 
                                                    componentProgress / totalOperations);
                
                // Reduce UI updates to prevent stuttering
                if (j % 5 == 0 || j == meshCount-1)
                {
                    SmoothProgressUpdate();
                    yield return null;
                }
            }
            completedOperations++;

            // Save data for runtime
            MeshGroup group = new MeshGroup();
            group.baseMesh = MeshDataConverter.ConvertMeshToMeshData(mesh);
            foreach (Mesh convexMesh in convexMeshes)
            {
                group.computedMeshes.Add(MeshDataConverter.ConvertMeshToMeshData(convexMesh));
            }

            runtimeData.meshGroups.Add(group);
            yield return null;
        }

        // Save the collider data (90-95%)
        targetProgressValue = 0.95f;
        SmoothProgressUpdate();
        if (progressText != null)
            progressText.text = "Saving collider data...";
        
        yield return null;
        
        string fileName = name + ".json";
        SaveRuntimeColliderData(runtimeData, fileName);

        // Complete with smooth finish
        yield return StartCoroutine(CompleteProgressSmoothly("Collider generation complete!"));
    }

    // Update progress bar smoothly
    private void SmoothProgressUpdate()
    {
        if (progressBar != null)
        {
            // Clamp target value to valid range
            targetProgressValue = Mathf.Clamp01(targetProgressValue);
            
            // Smoothly update current value (don't go backwards)
            currentProgressValue = Mathf.Max(currentProgressValue, Mathf.Lerp(currentProgressValue, targetProgressValue, 0.3f));
            
            // Apply to progress bar
            progressBar.value = currentProgressValue;
        }
    }

    // Smooth completion and hide the progress panel
    private IEnumerator CompleteProgressSmoothly(string completionMessage)
    {
        // Final smooth progress to 100%
        targetProgressValue = 1.0f;
        
        if (progressText != null)
            progressText.text = completionMessage;
        
        // Smoothly reach 100%
        while (currentProgressValue < 0.99f)
        {
            currentProgressValue = Mathf.Lerp(currentProgressValue, 1.0f, 0.3f);
            if (progressBar != null)
                progressBar.value = currentProgressValue;
            yield return null;
        }
        
        // Ensure we hit exactly 100%
        if (progressBar != null)
            progressBar.value = 1.0f;
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide progress UI
        if (progressPanel != null)
            progressPanel.SetActive(false);
    }
    /// Processes each component, generates convex collider meshes using CoACD, assigns MeshColliders,
    /// and gathers all generated mesh data to save at runtime.
    public async void AddCoACDCollidersToComponentsAsync(List<Transform> components, string name)
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        string fileName = name + ".json";
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
                        // Set the layer to OriginalColliders
                        meshCollider.gameObject.layer = LayerMask.NameToLayer("OriginalCollidersParent");
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
                Debug.Log("Operation cancelled.");
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
                // Set the layer to OriginalColliders
               meshCollider.gameObject.layer = LayerMask.NameToLayer("OriginalCollidersParent");
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
            Debug.Log("Cancellation sent.");
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
            if (component != null && StateManager.Instance.CurrentState == State.PlayBack)
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
                
                GenerateSnapCollider(child);
            }

            // Chiamata ricorsiva per i figli
            if (child.childCount > 0)
            {
                CollectChildrenWithMesh(child, resultList);
            }
        }
    }

    private void GenerateSnapCollider(Transform component)
    {
        component.gameObject.layer = LayerMask.NameToLayer("OriginalCollidersParent");
        // Create a child GameObject for the collider
        GameObject colliderObj = new GameObject($"{component.name}_SnapCube");
        colliderObj.layer = LayerMask.NameToLayer("OriginalColliders");
    
        // Make it a child of the component
        colliderObj.transform.SetParent(component);
    
        // Position at the center of the component
        colliderObj.transform.localPosition = Vector3.zero;
        colliderObj.transform.localRotation = Quaternion.identity;
        colliderObj.transform.localScale = Vector3.one;
    
        // Create small cube collider (much smaller than the full component)
        BoxCollider boxCollider = colliderObj.AddComponent<BoxCollider>();
    
        float cubeSize = 0.01f; 
        boxCollider.size = new Vector3(cubeSize, cubeSize, cubeSize);
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
