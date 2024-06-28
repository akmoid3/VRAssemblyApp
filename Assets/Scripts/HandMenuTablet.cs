using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HandMenuTablet : MonoBehaviour
{
    [SerializeField]
    private GameObject handMenu;

    [SerializeField]
    private InputActionReference toggleReference;

    private void Awake()
    {
        toggleReference.action.started += Toggle;
    }

    private void OnDestroy()
    {
        toggleReference.action.started -= Toggle;
    }


    private void Update()
    {
        
    }

    private void Toggle(InputAction.CallbackContext context)
    {
      bool isActive = !handMenu.activeSelf;
      handMenu.SetActive(isActive);
    }
}
