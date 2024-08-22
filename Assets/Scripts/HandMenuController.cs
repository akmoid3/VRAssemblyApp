using UnityEngine;
using UnityEngine.InputSystem;

public class HandMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject handMenu;

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
        if (StateManager.Instance.CurrentState == State.Record)
        {
            bool isActive = !handMenu.activeSelf;
            handMenu.SetActive(isActive);
        }
           
    }
}
