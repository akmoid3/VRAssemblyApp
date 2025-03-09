using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePhysics : MonoBehaviour
{
    private SnapToPosition snapToPosition;
    public Toggle myToggle;

    public void ToggleSnapPhysics()
    {
        if(snapToPosition == null)
            snapToPosition = FindObjectOfType<SnapToPosition>();
        
        snapToPosition.GetComponent<Rigidbody>().isKinematic = !myToggle.isOn;     
    }
}
