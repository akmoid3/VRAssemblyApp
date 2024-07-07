using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField] private Manager manager;
    [SerializeField] private float increment = 0.1f;

    [SerializeField] private TMP_Text componentNameText;
    [SerializeField] private TMP_Text positionXText;
    [SerializeField] private TMP_Text positionYText;
    [SerializeField] private TMP_Text positionZText;
    [SerializeField] private TMP_Text rotationXText;
    [SerializeField] private TMP_Text rotationYText;
    [SerializeField] private TMP_Text rotationZText;

    [SerializeField] private Button addPosXButton;
    [SerializeField] private Button addPosYButton;
    [SerializeField] private Button addPosZButton;
    [SerializeField] private Button addRotXButton;
    [SerializeField] private Button addRotYButton;
    [SerializeField] private Button addRotZButton;

    [SerializeField] private Button reducePosXButton;
    [SerializeField] private Button reducePosYButton;
    [SerializeField] private Button reducePosZButton;
    [SerializeField] private Button reduceRotXButton;
    [SerializeField] private Button reduceRotYButton;
    [SerializeField] private Button reduceRotZButton;

    [SerializeField] private Button saveComponentButton;
    [SerializeField] private Button modifyButton;
    [SerializeField] private Button groupSelectionButton;
    [SerializeField] private Toggle toggle;


    [SerializeField] private TMP_Dropdown incrementDropdown;

    private MakeGrabbable makeGrabbable;

    [SerializeField] private GameObject Group;

    [SerializeField] private bool isGrabInteractableEnabled = false;
    private XRInteractionManager interactionManager;


    private List<Button> allButtonsToDeactivate;
    private void Start()
    {
        toggle.isOn = isGrabInteractableEnabled;
        // Initialize the button list
        allButtonsToDeactivate = new List<Button>
        {
            addPosXButton, addPosYButton, addPosZButton,
            addRotXButton, addRotYButton, addRotZButton,
            reducePosXButton, reducePosYButton, reducePosZButton,
            reduceRotXButton, reduceRotYButton, reduceRotZButton,
            saveComponentButton
        };

        // Set up button listeners
        addPosXButton.onClick.AddListener(() => AddToPosition(Vector3.right * increment));
        addPosYButton.onClick.AddListener(() => AddToPosition(Vector3.up * increment));
        addPosZButton.onClick.AddListener(() => AddToPosition(Vector3.forward * increment));
        addRotXButton.onClick.AddListener(() => AddToRotation(Vector3.right * increment));
        addRotYButton.onClick.AddListener(() => AddToRotation(Vector3.up * increment));
        addRotZButton.onClick.AddListener(() => AddToRotation(Vector3.forward * increment));

        reducePosXButton.onClick.AddListener(() => AddToPosition(Vector3.left * increment));
        reducePosYButton.onClick.AddListener(() => AddToPosition(Vector3.down * increment));
        reducePosZButton.onClick.AddListener(() => AddToPosition(Vector3.back * increment));
        reduceRotXButton.onClick.AddListener(() => AddToRotation(Vector3.left * increment));
        reduceRotYButton.onClick.AddListener(() => AddToRotation(Vector3.down * increment));
        reduceRotZButton.onClick.AddListener(() => AddToRotation(Vector3.back * increment));

        modifyButton.onClick.AddListener(() => Modify());
        groupSelectionButton.onClick.AddListener(() => GroupSelection());
        saveComponentButton.onClick.AddListener(() => SaveComponent());

        // Set up dropdown listener
        incrementDropdown.onValueChanged.AddListener(UpdateIncrement);
        UpdateIncrement(incrementDropdown.value); // Set initial increment based on dropdown value

        interactionManager = FindObjectOfType<XRInteractionManager>();
    }

    void Update()
    {
        GameObject currentSelectedComponent = manager.GetCurrentSelectedComponent();
        if (currentSelectedComponent != null)
        {
            ComponentObject componentObject = currentSelectedComponent.GetComponent<ComponentObject>();
            Vector3 position = currentSelectedComponent.transform.position;
            Vector3 rotation = currentSelectedComponent.transform.eulerAngles;

            componentNameText.text = currentSelectedComponent.name;
            positionXText.text = $"{position.x:F2}";
            positionYText.text = $"{position.y:F2}";
            positionZText.text = $"{position.z:F2}";
            rotationXText.text = $"{rotation.x:F2}";
            rotationYText.text = $"{rotation.y:F2}";
            rotationZText.text = $"{rotation.z:F2}";

            if (componentObject)
            {
                if (componentObject.GetIsPlaced())
                {
                    modifyButton.gameObject.SetActive(true);
                    SetButtonsActive(false);

                }
                else
                {
                    modifyButton.gameObject.SetActive(false);
                    SetButtonsActive(true);
                }
            }
        }
        else
        {
            positionXText.text = "N/A";
            positionYText.text = "N/A";
            positionZText.text = "N/A";
            rotationXText.text = "N/A";
            rotationYText.text = "N/A";
            rotationZText.text = "N/A";

            modifyButton.gameObject.SetActive(false);
        }
    }

    private void SetButtonsActive(bool isActive)
    {
        foreach (var button in allButtonsToDeactivate)
        {
            button.gameObject.SetActive(isActive);
        }
    }

    private void AddToPosition(Vector3 increment)
    {
        if (manager.GetCurrentSelectedComponent() != null)
        {
            manager.GetCurrentSelectedComponent().transform.position += increment;
        }
    }

    private void AddToRotation(Vector3 increment)
    {
        if (manager.GetCurrentSelectedComponent() != null)
        {
            manager.GetCurrentSelectedComponent().transform.eulerAngles += increment;
        }
    }

    private void Modify()
    {
        GameObject currentSelectedComponent = manager.GetCurrentSelectedComponent();
        
        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.SetParent(null);
            ComponentObject componentObject = currentSelectedComponent.GetComponent<ComponentObject>();
            makeGrabbable = currentSelectedComponent.GetComponent<MakeGrabbable>();

            if (componentObject != null)
            {
                if (componentObject.GetIsPlaced()) {

                    if (makeGrabbable != null)
                    {
                        StartCoroutine(makeGrabbable.MakeObjectGrabbable());
                    }

                    componentObject.SetIsPlaced(false);

                }
                
            }
            

            
        }
    }

    private void SaveComponent()
    {
        GameObject currentSelectedComponent = manager.GetCurrentSelectedComponent();

        if (currentSelectedComponent != null)
        {
            ComponentObject componentObject = currentSelectedComponent.GetComponent<ComponentObject>();
            makeGrabbable = currentSelectedComponent.GetComponent<MakeGrabbable>();

            if (componentObject != null)
            {
                if (!componentObject.GetIsPlaced())
                {

                    if (makeGrabbable != null)
                    {
                        makeGrabbable.MakeObjectNonGrabbable();
                    }

                    componentObject.SetIsPlaced(true);

                    if(Group == null)
                    {
                        Group = new GameObject("Group");
                        Group.transform.position = Vector3.zero;
                        Group.transform.rotation = Quaternion.identity;
                    }
                   

                    // Change the parent of the current selected component
                    currentSelectedComponent.transform.SetParent(Group.transform);
                }
            }
        }
    }

    private void GroupSelection()
    {
        StartCoroutine(GroupSelection2());
    }

    private IEnumerator GroupSelection2()
    {
        if (Group != null)
        {
            isGrabInteractableEnabled = !isGrabInteractableEnabled;

            toggle.isOn = isGrabInteractableEnabled;
            List<Collider> clonedColliders = new List<Collider>();

            if (isGrabInteractableEnabled)
            {
                // Disable all child XRSimpleInteractables
                foreach (Transform child in Group.transform)
                {
                    XRBaseInteractable childGrabInteractable = child.GetComponent<XRBaseInteractable>();

                    if (childGrabInteractable != null)
                    {
                        interactionManager.UnregisterInteractable(childGrabInteractable as IXRInteractable);
                        childGrabInteractable.enabled = false;
                    }


                    // Clone child colliders
                    Collider[] childColliders = child.GetComponents<Collider>();
                    foreach (Collider col in childColliders)
                    {
                        Collider clonedCollider = child.gameObject.AddComponent(col.GetType()) as Collider;
                        CopyPropertiesAndFields(col, clonedCollider);
                        clonedColliders.Add(clonedCollider);
                        col.enabled = false; // Disable the original collider
                    }

                }
                    yield return 0;

                ComponentObject componentObject = Group.GetComponent<ComponentObject>();
                if (componentObject == null)
                {
                    componentObject = Group.AddComponent<ComponentObject>();
                }


                // Add Rigidbody if not present
                Rigidbody rb = Group.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = Group.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;

                // Add XRGrabInteractable
                XRGrabInteractable grabInteractable = Group.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = Group.AddComponent<XRGrabInteractable>();
                }
                grabInteractable.enabled = false;
                grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
                grabInteractable.selectMode = InteractableSelectMode.Multiple;
                grabInteractable.useDynamicAttach = true;
                grabInteractable.throwOnDetach = false;
                grabInteractable.selectEntered.AddListener(manager.OnSelectEnter);
                grabInteractable.selectExited.AddListener(manager.OnSelectExit);


                grabInteractable.colliders.Clear();
                grabInteractable.colliders.AddRange(clonedColliders);

                yield return 0;

                grabInteractable.enabled = true;

            }
            else
            {
                XRBaseInteractable grabInteractable = Group.GetComponent<XRBaseInteractable>();
                if (grabInteractable != null)
                {
                    interactionManager.UnregisterInteractable(grabInteractable as IXRInteractable);
                    Destroy(grabInteractable);
                }

                foreach (Collider col in clonedColliders)
                {
                    Destroy(col);
                }
                clonedColliders.Clear();

                // Enable child XRSimpleInteractables and restore original colliders
                foreach (Transform child in Group.transform)
                {
                    XRBaseInteractable childGrabInteractable = child.GetComponent<XRBaseInteractable>();
                    if (childGrabInteractable != null)
                    {
                        interactionManager.RegisterInteractable(childGrabInteractable as IXRInteractable);
                        childGrabInteractable.enabled = true;
                    }

                    Collider[] childColliders = child.GetComponents<Collider>();
                    foreach (Collider col in childColliders)
                    {
                        col.enabled = true; // Enable the original collider
                    }
                }
            }
        }
    }

    private void CopyPropertiesAndFields(object source, object destination)
    {
        // copy all properties
        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                try
                {
                    property.SetValue(destination, property.GetValue(source));
                }
                catch
                {
                    // Ignora le proprietà che non possono essere copiate
                }
            }
        }

        // Copy all fields
        var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                field.SetValue(destination, field.GetValue(source));
            }
            catch
            {
                // Ignora i campi che non possono essere copiati
            }
        }
    }

    private void UpdateIncrement(int index)
    {
        switch (index)
        {
            case 0:
                increment = 0.1f;
                break;
            case 1:
                increment = 0.05f;
                break;
            case 2:
                increment = 0.01f;
                break;
            case 3:
                increment = 1f;
                break;
            default:
                increment = 0.1f;
                break;
        }
    }
}
