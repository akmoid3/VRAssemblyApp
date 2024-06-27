using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class HandMenuTablet : MonoBehaviour
{
    public GameObject targetObject; // The GameObject you want to enable/disable

    private InputDevice leftController;

    void Start()
    {
        TryInitializeLeftController();
    }

    void Update()
    {
        if (!leftController.isValid)
        {
            TryInitializeLeftController();
        }
        else
        {
            bool xButtonPressed;
            if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out xButtonPressed) && xButtonPressed)
            {
                // Toggle the active state of the target object
                targetObject.SetActive(!targetObject.activeSelf);
            }
        }
    }

    private void TryInitializeLeftController()
    {
        var leftHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandedControllers);

        if (leftHandedControllers.Count > 0)
        {
            leftController = leftHandedControllers[0];
            Debug.Log("Left controller found.");
        }
        else
        {
            Debug.LogWarning("Left controller not found.");
        }
    }
}
