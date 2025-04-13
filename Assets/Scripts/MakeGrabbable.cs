using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MakeGrabbable : MonoBehaviour
{
    private readonly List<Collider> originalColliders = new List<Collider>();
    private readonly List<IXRInteractable> originalInteractables = new List<IXRInteractable>();
    private XRInteractionManager interactionManager;
    private Manager manager;
    private GameObject interactionColliderChild;

    [Header("XRGrabInteractable Settings")]
    [SerializeField] private bool throwOnDetach = false;
    [SerializeField] private XRBaseInteractable.MovementType movementType = XRBaseInteractable.MovementType.VelocityTracking;
    [SerializeField] private bool useDynamicAttach = true;
    [SerializeField] private InteractableSelectMode selectMode = InteractableSelectMode.Multiple;
    
    [Header("Interaction Collider Settings")]
    [SerializeField] private string interactionColliderChildName = "InteractionCollider";
    [SerializeField] [Range(0.1f, 1f)] private float colliderSimplificationScale = 0.9f;
    [SerializeField] private bool autoCreateInteractionCollider = true;
    private string interactionLayerName = "Interaction";

    public bool ThrowOnDetach { get => throwOnDetach; set => throwOnDetach = value; }
    public XRBaseInteractable.MovementType MovementType { get => movementType; set => movementType = value; }
    public bool UseDynamicAttach { get => useDynamicAttach; set => useDynamicAttach = value; }
    public InteractableSelectMode SelectMode { get => selectMode; set => selectMode = value; }

    private void Awake()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();
        ComponentInizialization();
    }

    private void Start()
    {
        manager = Manager.Instance;
    }

    private void ComponentInizialization()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = this.gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if(GetComponent<XRSimpleInteractable>())
            Destroy(GetComponent<XRSimpleInteractable>());

        if (GetComponent<XRGrabInteractable>())
            Destroy(GetComponent<XRGrabInteractable>());
            
        if (autoCreateInteractionCollider && interactionColliderChild == null)
        {
            CreateInteractionColliderChild();
        }
    }

    private void SaveOriginalCollidersAndInteractables() {
        originalColliders.Clear();
        originalInteractables.Clear();

        originalInteractables.AddRange(GetComponents<XRBaseInteractable>());
    }


    public GameObject CreateInteractionColliderChild()
    {
        Transform existingChild = transform.Find(interactionColliderChildName);
        if (existingChild != null)
        {
            interactionColliderChild = existingChild.gameObject;
            return interactionColliderChild;
        }

        int interactionLayer = LayerMask.NameToLayer(interactionLayerName);
        if (interactionLayer == -1)
        {
            Debug.LogWarning($"Layer '{interactionLayerName}' not found.");
            interactionLayer = 0;
        }

        interactionColliderChild = new GameObject(interactionColliderChildName);
        interactionColliderChild.transform.SetParent(transform);
        interactionColliderChild.transform.localPosition = Vector3.zero;
        interactionColliderChild.transform.localRotation = Quaternion.identity;
        interactionColliderChild.transform.localScale = Vector3.one * colliderSimplificationScale;
        
        interactionColliderChild.layer = interactionLayer;

        MeshCollider collider = interactionColliderChild.AddComponent<MeshCollider>();
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            collider.sharedMesh = meshFilter.sharedMesh;
        }
       
        
        collider.convex = true;
        collider.isTrigger = false;
        
        return interactionColliderChild;
    }

    public virtual void MakeObjectGrabbable()
    {
        if (gameObject.GetComponent<XRGrabInteractable>() != null)
            return;
            
        SaveOriginalCollidersAndInteractables();
        DestroyInteractables();
        
        // Make sure we have an interaction collider
        if (interactionColliderChild == null)
        {
            CreateInteractionColliderChild();
        }

        XRGrabInteractable grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        grabInteractable.enabled = false;
        
        grabInteractable.throwOnDetach = throwOnDetach;
        grabInteractable.movementType = movementType;
        grabInteractable.useDynamicAttach = useDynamicAttach;
        grabInteractable.selectMode = selectMode;
        grabInteractable.selectEntered.AddListener(OnSelectEnter);
        grabInteractable.selectExited.AddListener(OnSelectExit);
        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);

        grabInteractable.colliders.Clear();
        
        Collider interactionCollider = interactionColliderChild.GetComponent<Collider>();
        if (interactionCollider != null)
        {
            grabInteractable.colliders.Add(interactionCollider);
        }

        grabInteractable.enabled = true;
    }

    public void DestroyInteractables() {
        foreach (var interactable in originalInteractables)
        {
            interactionManager.UnregisterInteractable(interactable);
            Destroy(interactable as MonoBehaviour);
        }
        
        var currentInteractables = GetComponents<XRBaseInteractable>();
        foreach (var interactable in currentInteractables)
        {
            interactionManager.UnregisterInteractable(interactable);
            Destroy(interactable);
        }
    }

    public virtual void MakeObjectNonGrabbable()
    {
        if(gameObject.GetComponent<XRSimpleInteractable>() != null)
            return;
            
        SaveOriginalCollidersAndInteractables();
        DestroyInteractables();
        
        if (interactionColliderChild == null)
        {
            CreateInteractionColliderChild();
        }

        XRSimpleInteractable simpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        simpleInteractable.enabled = false;

        simpleInteractable.selectMode = selectMode;
        simpleInteractable.selectEntered.AddListener(OnSelectEnter);
        simpleInteractable.selectExited.AddListener(OnSelectExit);
        simpleInteractable.hoverEntered.AddListener(OnHoverEnter);
        simpleInteractable.hoverExited.AddListener(OnHoverExit);

        simpleInteractable.colliders.Clear();
        
        Collider interactionCollider = interactionColliderChild.GetComponent<Collider>();
        if (interactionCollider != null)
        {
            simpleInteractable.colliders.Add(interactionCollider);
        }

        simpleInteractable.enabled = true;
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        manager.OnSelectEnter(args);
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        manager.OnSelectExit(args);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        manager.OnHoverEnter(args);
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        manager.OnHoverExit(args);
    }

    private void OnDestroy()
    {
        if (interactionColliderChild != null && Application.isPlaying)
        {
            Destroy(interactionColliderChild);
        }
    }

}