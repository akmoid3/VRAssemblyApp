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
    [SerializeField] private Button addStepButton;
    [SerializeField] private Button groupSelectionButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Toggle toggle;


    [SerializeField] private TMP_Dropdown incrementDropdown;

    private MakeGrabbable makeGrabbable;

    [SerializeField] private GameObject Group;

    [SerializeField] private bool isGrabInteractableEnabled = false;
    [SerializeField] private bool modifying = false;
    [SerializeField] private bool newStep = false;

    private XRInteractionManager interactionManager;


    private List<Button> allButtonsToDeactivate;
    private void Start()
    {
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

        modifyButton.onClick.AddListener(() => ModifyComponent());
        addStepButton.onClick.AddListener(() => ModifyComponent());
        removeButton.onClick.AddListener(() => RemoveComponent());
        modifyButton.onClick.AddListener(() => Modifying());
        addStepButton.onClick.AddListener(() => NewStep());

        groupSelectionButton.onClick.AddListener(() => GroupSelection());
        toggle.isOn = isGrabInteractableEnabled;

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

            Vector3 position = currentSelectedComponent.transform.position;
            Vector3 rotation = currentSelectedComponent.transform.eulerAngles;

            componentNameText.text = currentSelectedComponent.name;
            positionXText.text = $"{position.x:F2}";
            positionYText.text = $"{position.y:F2}";
            positionZText.text = $"{position.z:F2}";
            rotationXText.text = $"{rotation.x:F2}";
            rotationYText.text = $"{rotation.y:F2}";
            rotationZText.text = $"{rotation.z:F2}";

            if (currentSelectedComponent.name != "Group")
            {
                ComponentObject componentObject = currentSelectedComponent.GetComponent<ComponentObject>();

                if (componentObject)
                {
                    if (componentObject.GetIsPlaced())
                    {
                        modifyButton.gameObject.SetActive(true);
                        addStepButton.gameObject.SetActive(true);
                        removeButton.gameObject.SetActive(true);

                        SetTransformsButtonsActive(false);

                    }
                    else
                    {
                        modifyButton.gameObject.SetActive(false);
                        addStepButton.gameObject.SetActive(false);
                        removeButton.gameObject.SetActive(false);

                        SetTransformsButtonsActive(true);
                    }
                }
            }
            else
            {
                modifyButton.gameObject.SetActive(false);
                addStepButton.gameObject.SetActive(false);
                removeButton.gameObject.SetActive(false);

                SetTransformsButtonsActive(true);
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
            addStepButton.gameObject.SetActive(false);
            removeButton.gameObject.SetActive(false);
            SetTransformsButtonsActive(false);


        }

        if (Group && Group.transform.childCount > 0)
        {
            groupSelectionButton.gameObject.SetActive(true);
            toggle.gameObject.SetActive(true);
        }
        else
        {
            groupSelectionButton.gameObject.SetActive(false);
            toggle.gameObject.SetActive(false);
        }




    }

    private void Modifying()
    {
        modifying = true;
    }

    private void NewStep()
    {
        newStep = true;
    }

    private void SetTransformsButtonsActive(bool isActive)
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

    private void ModifyComponent()
    {
        GameObject currentSelectedComponent = manager.GetCurrentSelectedComponent();

        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.SetParent(null);
            ComponentObject componentObject = currentSelectedComponent.GetComponent<ComponentObject>();
            makeGrabbable = currentSelectedComponent.GetComponent<MakeGrabbable>();

            if (componentObject != null)
            {
                if (componentObject.GetIsPlaced())
                {

                    if (makeGrabbable != null)
                    {
                        makeGrabbable.MakeObjectGrabbable();
                    }

                    componentObject.SetIsPlaced(false);
                }
            }
        }
    }

    private void RemoveComponent()
    {
        ModifyComponent();
        manager.RemoveComponentFromSequence();

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

                    if (Group == null)
                    {
                        Group = new GameObject("Group");
                        Group.transform.position = Vector3.zero;
                        Group.transform.rotation = Quaternion.identity;
                    }

                    // Change the parent of the current selected component
                    currentSelectedComponent.transform.SetParent(Group.transform);
                }
            }

            if (modifying)
            {
                manager.ModifyBuildingSequence();
                modifying = false;
            }
            else if (newStep)
            {
                manager.SaveBuildingSequence();
                newStep = false;
            }
            else
            {
                manager.SaveBuildingSequence();

            }
        }
    }

    private void GroupSelection()
    {
        StartCoroutine(GroupSelectionMethod());
    }

    private IEnumerator GroupSelectionMethod()
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

                    ComponentObject componentObj = child.GetComponent<ComponentObject>();
                    if (componentObj != null)
                        componentObj.enabled = false;

                    // Clone child colliders
                    Collider[] childColliders = child.GetComponents<Collider>();
                    foreach (Collider col in childColliders)
                    {
                        Collider clonedCollider = child.gameObject.AddComponent(col.GetType()) as Collider;
                        col.CopyPropertiesAndFields(clonedCollider);
                        clonedColliders.Add(clonedCollider);
                        col.enabled = false; // Disable the original collider
                    }

                }
                yield return 0;


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

                    ComponentObject componentObj = child.GetComponent<ComponentObject>();
                    if (componentObj != null)
                        componentObj.enabled = true;

                    Collider[] childColliders = child.GetComponents<Collider>();
                    foreach (Collider col in childColliders)
                    {
                        col.enabled = true; // Enable the original collider
                    }
                }
            }
        }
    }

    private static readonly float[] Increments = { 0.1f, 0.05f, 0.01f, 1f };

    private void UpdateIncrement(int index)
    {
        if (index >= 0 && index < Increments.Length)
        {
            increment = Increments[index];
        }
        else
        {
            increment = Increments[0];
        }
    }
}
