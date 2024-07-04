using System.Collections;
using System.Collections.Generic;
using TMPro;
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


    [SerializeField] private TMP_Dropdown incrementDropdown;

    private MakeGrabbable makeGrabbable;

    [SerializeField] private GameObject newParent;

    [SerializeField] private bool isParentGrabbable = false;
    private XRInteractionManager interactionManager;

    private void Start()
    {
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
                }
                else
                {
                    modifyButton.gameObject.SetActive(false);
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

                    if(newParent == null)
                    {
                        newParent = new GameObject("NewParent");
                        newParent.AddComponent<Rigidbody>();
                        Rigidbody rb = newParent.GetComponent<Rigidbody>();
                        rb.isKinematic = true;
                        newParent.AddComponent<XRGrabInteractable>();
                        XRGrabInteractable grabInteractable = newParent.GetComponent<XRGrabInteractable>();
                        grabInteractable.enabled = false;
                        grabInteractable.selectMode = InteractableSelectMode.Multiple;
                        grabInteractable.useDynamicAttach = true;
                        
                    }
                    newParent.transform.position = Vector3.zero; 
                    newParent.transform.rotation = Quaternion.identity;

                    // Change the parent of the current selected component
                    currentSelectedComponent.transform.SetParent(newParent.transform);
                }
            }
        }
    }

    private void GroupSelection()
    {
        if (newParent != null)
        {
            
            foreach (Transform child in newParent.transform)
            {
                XRSimpleInteractable childGrabInteractable = child.GetComponent<XRSimpleInteractable>();

                if (childGrabInteractable != null)
                {
                    interactionManager.UnregisterInteractable(childGrabInteractable);
                    childGrabInteractable.enabled = false;
                }
            }

            newParent.AddComponent<Rigidbody>();
            Rigidbody rb = newParent.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            newParent.AddComponent<XRGrabInteractable>();

            XRGrabInteractable grabInteractable = newParent.GetComponent<XRGrabInteractable>();
            grabInteractable.enabled = true;
            grabInteractable.selectMode = InteractableSelectMode.Multiple;
            grabInteractable.useDynamicAttach = true;
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
