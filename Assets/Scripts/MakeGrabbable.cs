using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MakeGrabbable : MonoBehaviour
{
    private readonly List<Collider> originalColliders = new List<Collider>();
    private readonly List<IXRInteractable> originalInteractables = new List<IXRInteractable>();
    private XRInteractionManager interactionManager;
    private Manager manager;

    [Header("XRGrabInteractable Settings")]
    [SerializeField] private bool throwOnDetach = false;
    [SerializeField] private XRBaseInteractable.MovementType movementType = XRBaseInteractable.MovementType.VelocityTracking;
    [SerializeField] private bool useDynamicAttach = true;
    [SerializeField] private InteractableSelectMode selectMode = InteractableSelectMode.Multiple;

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
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if(GetComponent<XRSimpleInteractable>())
            Destroy(GetComponent<XRSimpleInteractable>());

        if (GetComponent<XRGrabInteractable>())
            Destroy(GetComponent<XRGrabInteractable>());

        if (GetComponent<MeshCollider>() == null)
        {
            MeshCollider collider = this.gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
        }
    }

    private void SaveOriginalCollidersAndInteractables() {

        originalColliders.Clear();
        originalInteractables.Clear();

        originalInteractables.AddRange(GetComponents<XRBaseInteractable>());
        originalColliders.AddRange(GetComponents<Collider>());
    }
    public void MakeObjectGrabbable()
    {
        if (gameObject.GetComponent<XRGrabInteractable>() != null)
            return;
        DestroyClonedColliders();
        SaveOriginalCollidersAndInteractables();

        DestroyInteractables();

        // Clone original colliders
        List<Collider> clonedColliders = GetClonedColliders();
        // Add grab interactable 
        XRGrabInteractable grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        grabInteractable.enabled = false;

        // Apply inspector-modifiable properties
        grabInteractable.throwOnDetach = throwOnDetach;
        grabInteractable.movementType = movementType;
        grabInteractable.useDynamicAttach = useDynamicAttach;
        grabInteractable.selectMode = selectMode;
        grabInteractable.selectEntered.AddListener(OnSelectEnter);
        grabInteractable.selectExited.AddListener(OnSelectExit);
        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);

        // Clear existing colliders and add cloned colliders
        grabInteractable.colliders.Clear();
        grabInteractable.colliders.AddRange(clonedColliders);


        grabInteractable.enabled = true;
    }

    private void DestroyClonedColliders()
    {

        var clonedColls = GetComponents<Collider>();
        foreach (var collider in clonedColls)
        {
            if (!originalColliders.Contains(collider))
            {
                Destroy(collider);
            }
        }
    }

    private List<Collider> GetClonedColliders()
    {
        List<Collider> clonedColliders = new List<Collider>();
        foreach (var collider in originalColliders)
        {
            Collider clonedCollider = gameObject.AddComponent(collider.GetType()) as Collider;
            collider.CopyPropertiesAndFields(clonedCollider);
            clonedColliders.Add(clonedCollider);
            collider.enabled = false;
        }
        return clonedColliders;
    }

    private void DestroyInteractables() {

        // Disable original interactables
        foreach (var interactable in originalInteractables)
        {
            interactionManager.UnregisterInteractable(interactable);
            Destroy(interactable as MonoBehaviour);
        }
    }
    public void MakeObjectNonGrabbable()
    {
        if(gameObject.GetComponent<XRSimpleInteractable>() != null)
            return;
        DestroyClonedColliders();
        SaveOriginalCollidersAndInteractables();

        DestroyInteractables();


        // Clone original colliders
        List<Collider> clonedColliders = GetClonedColliders();

        // Add grab interactable 
        XRSimpleInteractable simpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        simpleInteractable.enabled = false;

        // Apply inspector-modifiable 
        simpleInteractable.selectMode = selectMode;
        simpleInteractable.selectEntered.AddListener(OnSelectEnter);
        simpleInteractable.selectExited.AddListener(OnSelectExit);
        simpleInteractable.hoverEntered.AddListener(OnHoverEnter);
        simpleInteractable.hoverExited.AddListener(OnHoverExit);

        // Clear existing colliders and add cloned colliders
        simpleInteractable.colliders.Clear();
        simpleInteractable.colliders.AddRange(clonedColliders);


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
}
