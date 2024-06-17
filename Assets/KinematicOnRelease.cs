using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KinematicOnRelease : MonoBehaviour
{
    public XRGrabInteractable interactable;
    public bool isSnapping = true;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectExited.AddListener(onSelectExit);
    }

    void onSelectExit(SelectExitEventArgs args) { 
        if(isSnapping)
            rb.isKinematic = true;
    }

}
