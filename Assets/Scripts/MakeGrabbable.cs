using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MakeGrabbable : MonoBehaviour
{
    private List<Collider> originalColliders = new List<Collider>();
    private List<IXRInteractable> originalInteractables = new List<IXRInteractable>();
    private XRGrabInteractable grabInteractable;
    private XRInteractionManager interactionManager;
    private Manager manager;
    
    private void Start()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();
        manager = FindObjectOfType<Manager>();

        if (manager == null)
        {
            Debug.LogError("Manager not found in the scene.");
            return;
        }
        StartCoroutine(MakeObjectGrabbable());
    }

    public IEnumerator MakeObjectGrabbable()
    {
        // Save original interactables and colliders
        originalInteractables.AddRange(GetComponents<XRBaseInteractable>());
        originalColliders.AddRange(GetComponents<Collider>());

        // Disable original interactables
        foreach (var interactable in originalInteractables)
        {
            interactionManager.UnregisterInteractable(interactable);
            (interactable as MonoBehaviour).enabled = false;
        }

        // Clone original colliders
        List<Collider> clonedColliders = new List<Collider>();
        foreach (var collider in originalColliders)
        {
            Collider clonedCollider = gameObject.AddComponent(collider.GetType()) as Collider;
            collider.CopyPropertiesAndFields(clonedCollider);
            clonedColliders.Add(clonedCollider);
            collider.enabled = false;
        }
        // Add grab interactable 
        grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        grabInteractable.enabled = false;
        grabInteractable.throwOnDetach = false;
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.useDynamicAttach = true;
        grabInteractable.selectMode = InteractableSelectMode.Multiple;
        grabInteractable.selectEntered.AddListener(OnSelectEnter);
        grabInteractable.selectExited.AddListener(OnSelectExit);

        yield return 0; // Wait a frame for the component to initialize

        


        // Clear existing colliders and add cloned colliders
        grabInteractable.colliders.Clear();
        grabInteractable.colliders.AddRange(clonedColliders);

        yield return 0; // Wait a frame for the component to initialize

        grabInteractable.enabled = true;
    }

    public void MakeObjectNonGrabbable()
    {
        foreach (var collider in originalColliders)
        {
            collider.enabled = true;
        }

        if (grabInteractable != null)
        {
            interactionManager.UnregisterInteractable(grabInteractable as IXRInteractable);
            Destroy(grabInteractable);
        }

       

        // Remove cloned colliders
        var clonedColliders = GetComponents<Collider>();
        foreach (var collider in clonedColliders)
        {
            if (!originalColliders.Contains(collider))
            {
                Destroy(collider);
            }
        }

        // enable original interactables
        foreach (var interactable in originalInteractables)
        {
            interactionManager.RegisterInteractable(interactable);
            (interactable as MonoBehaviour).enabled = true;
            (interactable as XRBaseInteractable).selectEntered.AddListener(OnSelectEnter);
            (interactable as XRBaseInteractable).selectExited.AddListener(OnSelectExit);
        }

        originalInteractables.Clear();
        originalColliders.Clear();
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        manager.OnSelectEnter(args);
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        manager.OnSelectExit(args);
    }

}
