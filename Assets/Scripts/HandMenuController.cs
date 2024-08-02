using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HandMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject handMenu;
    private readonly Manager manager = Manager.Instance;

    [SerializeField]
    private InputActionReference toggleReference;

    private void Awake()
    {
        toggleReference.action.started += Toggle;
        handMenu.SetActive(false);
    }
    private void OnDestroy()
    {
        toggleReference.action.started -= Toggle;
    }

    private void Toggle(InputAction.CallbackContext context)
    {
        if (manager.GetRecording())
        {
            bool isActive = !handMenu.activeSelf;
            handMenu.SetActive(isActive);
        }
           
    }
}
