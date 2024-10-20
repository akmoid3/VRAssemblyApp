using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMenuManager : MonoBehaviour
{
    private Manager manager;
    [SerializeField] private float increment = 0.1f;

    [SerializeField] public GameObject handMenuPanel;

    [SerializeField] public TMP_Text componentNameText;
    [SerializeField] public TMP_Text positionXText;
    [SerializeField] public TMP_Text positionYText;
    [SerializeField] public TMP_Text positionZText;
    [SerializeField] public TMP_Text rotationXText;
    [SerializeField] public TMP_Text rotationYText;
    [SerializeField] public TMP_Text rotationZText;

    [SerializeField] public Button addPosXButton;
    [SerializeField] public Button addPosYButton;
    [SerializeField] public Button addPosZButton;
    [SerializeField] public Button addRotXButton;
    [SerializeField] public Button addRotYButton;
    [SerializeField] public Button addRotZButton;

    [SerializeField] public Button reducePosXButton;
    [SerializeField] public Button reducePosYButton;
    [SerializeField] public Button reducePosZButton;
    [SerializeField] public Button reduceRotXButton;
    [SerializeField] public Button reduceRotYButton;
    [SerializeField] public Button reduceRotZButton;

    [SerializeField] public Button saveComponentButton;
    [SerializeField] public Button modifyButton;
    [SerializeField] public Button addStepButton;
    [SerializeField] public Button groupSelectionButton;
    [SerializeField] public Button removeButton;

    [SerializeField] public Button groupSelectionButton2;

    [SerializeField] public TMP_Dropdown incrementDropdown;
    [SerializeField] private Transform buildingPosition;

    private MakeGrabbable makeGrabbable;

    [SerializeField] private GameObject group;

    [SerializeField] private bool isGrabInteractableEnabled = false;
    [SerializeField] private bool modifying = false;
    [SerializeField] private bool newStep = false;

    [SerializeField] public GameObject groupPanel;

    private XRInteractionManager interactionManager;


    private List<Button> allButtonsToDeactivate;

    private List<Collider> clonedColliders = new List<Collider>();

    public void Start()
    {
        groupPanel.SetActive(false);
        handMenuPanel.SetActive(true);
        manager = Manager.Instance;
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
        addPosXButton.onClick.AddListener(() => AddToPosition(Vector3.right * increment,manager.CurrentSelectedComponent));
        addPosYButton.onClick.AddListener(() => AddToPosition(Vector3.up * increment, manager.CurrentSelectedComponent));
        addPosZButton.onClick.AddListener(() => AddToPosition(Vector3.forward * increment, manager.CurrentSelectedComponent));
        addRotXButton.onClick.AddListener(() => AddToRotation(Vector3.right * increment, manager.CurrentSelectedComponent));
        addRotYButton.onClick.AddListener(() => AddToRotation(Vector3.up * increment, manager.CurrentSelectedComponent));
        addRotZButton.onClick.AddListener(() => AddToRotation(Vector3.forward * increment, manager.CurrentSelectedComponent));

        reducePosXButton.onClick.AddListener(() => AddToPosition(Vector3.left * increment, manager.CurrentSelectedComponent));
        reducePosYButton.onClick.AddListener(() => AddToPosition(Vector3.down * increment, manager.CurrentSelectedComponent));
        reducePosZButton.onClick.AddListener(() => AddToPosition(Vector3.back * increment, manager.CurrentSelectedComponent));
        reduceRotXButton.onClick.AddListener(() => AddToRotation(Vector3.left * increment, manager.CurrentSelectedComponent));
        reduceRotYButton.onClick.AddListener(() => AddToRotation(Vector3.down * increment, manager.CurrentSelectedComponent));
        reduceRotZButton.onClick.AddListener(() => AddToRotation(Vector3.back * increment, manager.CurrentSelectedComponent));

        modifyButton.onClick.AddListener(() => ModifyComponent(Manager.CurrentSelectedComponent));
        addStepButton.onClick.AddListener(() => ModifyComponent(Manager.CurrentSelectedComponent));
        removeButton.onClick.AddListener(() => RemoveComponent(Manager.CurrentSelectedComponent));
        modifyButton.onClick.AddListener(() => SetModifyingTrue());
        addStepButton.onClick.AddListener(() => SetNewStepTrue());

        groupSelectionButton.onClick.AddListener(() => GroupSelection());

        groupSelectionButton2.onClick.AddListener(() => GroupSelection());

        saveComponentButton.onClick.AddListener(() => SaveComponent(Manager.CurrentSelectedComponent));

        // Set up dropdown listener
        incrementDropdown.onValueChanged.AddListener(UpdateIncrement);
        UpdateIncrement(incrementDropdown.value); // Set initial increment based on dropdown value

        interactionManager = FindObjectOfType<XRInteractionManager>();
    }

    public void Update()
    {
        GameObject currentSelectedComponent = manager.CurrentSelectedComponent;

        if (currentSelectedComponent != null)
        {
            UpdateComponentUI(currentSelectedComponent);
            HandleComponentObject(currentSelectedComponent);
        }
        else
        {
            ResetUIForNoSelection();
        }

        UpdateGroupSelectionButton();
    }

    public void UpdateComponentUI(GameObject component)
    {
        Vector3 position = component.transform.position;
        Vector3 rotation = component.transform.eulerAngles;

        componentNameText.text = component.name;
        positionXText.text = $"{position.x:F2}";
        positionYText.text = $"{position.y:F2}";
        positionZText.text = $"{position.z:F2}";
        rotationXText.text = $"{rotation.x:F2}";
        rotationYText.text = $"{rotation.y:F2}";
        rotationZText.text = $"{rotation.z:F2}";
    }

    public void HandleComponentObject(GameObject component)
    {
        if (component.name != "Group")
        {
            ComponentObject componentObject = component.GetComponent<ComponentObject>();

            if (componentObject)
            {
                if (componentObject.GetIsPlaced())
                {
                    SetComponentModificationUI(true);
                    SetTransformsButtonsActive(false, allButtonsToDeactivate);
                }
                else
                {
                    SetComponentModificationUI(false);
                    SetTransformsButtonsActive(true, allButtonsToDeactivate);
                }
            }
        }
    }

    public void SetComponentModificationUI(bool isPlaced)
    {
        modifyButton.gameObject.SetActive(isPlaced);
        addStepButton.gameObject.SetActive(isPlaced);
        removeButton.gameObject.SetActive(isPlaced);
    }

    public void ResetUIForNoSelection()
    {
        positionXText.text = "N/A";
        positionYText.text = "N/A";
        positionZText.text = "N/A";
        rotationXText.text = "N/A";
        rotationYText.text = "N/A";
        rotationZText.text = "N/A";

        SetComponentModificationUI(false);
        SetTransformsButtonsActive(false, allButtonsToDeactivate);
    }

    public void UpdateGroupSelectionButton()
    {
        if (group && group.transform.childCount > 0)
        {
            groupSelectionButton.gameObject.SetActive(true);
        }
        else
        {
            groupSelectionButton.gameObject.SetActive(false);
        }
    }


    public void SetModifyingTrue()
    {
        modifying = true;
    }

    public void SetNewStepTrue()
    {
        newStep = true;
    }

    public void SetTransformsButtonsActive(bool isActive, List<Button> allButtonsToDeactivate)
    {
        foreach (var button in allButtonsToDeactivate)
        {
            button.gameObject.SetActive(isActive);
        }
    }


    public void AddToPosition(Vector3 increment, GameObject currentSelectedComponent)
    {
        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.position += increment;
        }
    }

    public void AddToRotation(Vector3 increment, GameObject currentSelectedComponent)
    {
        if (currentSelectedComponent != null)
        {
            currentSelectedComponent.transform.eulerAngles += increment;
        }
    }

    public void ModifyComponent(GameObject currentSelectedComponent)
    {

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

    public void RemoveComponent(GameObject currentSelectedComponent)
    {
        ModifyComponent(currentSelectedComponent);
        manager.RemoveComponentFromSequence();
    }

    public void SaveComponent(GameObject currentSelectedComponent)
    {

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

                    if (group == null)
                    {
                        group = new GameObject("Group");
                        group.transform.position = buildingPosition.position;
                        group.transform.rotation = buildingPosition.rotation;
                    }

                    // Change the parent of the current selected component
                    currentSelectedComponent.transform.SetParent(group.transform);
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

    public void GroupSelection()
    {
        if (group != null)
        {
            isGrabInteractableEnabled = !isGrabInteractableEnabled;

            groupPanel.SetActive(isGrabInteractableEnabled);
            handMenuPanel.SetActive(!isGrabInteractableEnabled);

            if (isGrabInteractableEnabled)
            {
               
                // Disable all child XRSimpleInteractables
                foreach (Transform child in group.transform)
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


                // Add Rigidbody if not present
                Rigidbody rb = group.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = group.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;

                // Add XRGrabInteractable
                XRGrabInteractable grabInteractable = group.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = group.AddComponent<XRGrabInteractable>();
                }
                grabInteractable.enabled = false;
                grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
                grabInteractable.selectMode = InteractableSelectMode.Multiple;
                grabInteractable.useDynamicAttach = true;
                grabInteractable.throwOnDetach = false;

                if(manager != null) {
                    grabInteractable.selectEntered.AddListener(manager.OnSelectEnter);
                    grabInteractable.selectExited.AddListener(manager.OnSelectExit);
                }
               
             

                grabInteractable.colliders.Clear();
                grabInteractable.colliders.AddRange(clonedColliders);


                grabInteractable.enabled = true;

            }
            else
            {

                XRBaseInteractable grabInteractable = group.GetComponent<XRBaseInteractable>();
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

                // Enable child interactable and restore original colliders
                foreach (Transform child in group.transform)
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

    public float Increment { get => increment; set => increment = value; }
    public GameObject Group { get => group; set => group = value; }
    public XRInteractionManager InteractionManager { get => interactionManager; set => interactionManager = value; }
    public bool IsGrabInteractableEnabled { get => isGrabInteractableEnabled; set => isGrabInteractableEnabled = value; }
    public Manager Manager { get => manager; set => manager = value; }
    public bool NewStep { get => newStep; set => newStep = value; }
    public bool Modifying { get => modifying; set => modifying = value; }
    public List<Button> AllButtonsToDeactivate { get => allButtonsToDeactivate; set => allButtonsToDeactivate = value; }

    public void UpdateIncrement(int index)
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
