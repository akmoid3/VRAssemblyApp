using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private float hintCooldown = 1.0f;
    [SerializeField] private int hintCount;
    [SerializeField] private Material highlightMaterial;
    private Material lineMaterial;

    private Dictionary<int, bool> hintShownForStep = new Dictionary<int, bool>();
    private bool isWaiting = false;
    public static event Action<int> OnHintCountChanged;

    private LineRenderer currentLineRenderer;
    private Transform currentComponentTransform;
    private Transform currentSnapPointTransform;

    private bool canChangeLine = false;

    private void Start()
    {
        CreateLineMaterial();
    }

    private void CreateLineMaterial()
    {
        Material material = Resources.Load<Material>("Sprites/tratteggio");
        lineMaterial = material;

        currentLineRenderer = gameObject.AddComponent<LineRenderer>();
        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startWidth = 0.009f;
        currentLineRenderer.endWidth = 0.009f;
        currentLineRenderer.startColor = Color.green;
        currentLineRenderer.endColor = Color.red;

        currentLineRenderer.textureMode = LineTextureMode.Tile;
        currentLineRenderer.textureScale = new Vector2(12, 1);

        currentLineRenderer.enabled = false;
    }


    public virtual void ShowHint(int currentStep, Transform component, SnapToPosition interactor)
    {

        if (hintShownForStep.ContainsKey(currentStep) && hintShownForStep[currentStep])
        {
            return;
        }

        hintCount++;
        hintShownForStep[currentStep] = true;
        OnHintCountChanged?.Invoke(hintCount);

        if (interactor != null)
        {
            ShowSnapPoint(interactor, currentStep);
            CurvedHint(currentStep, component, interactor);
        }
    }


    private void Update()
    {
        if (StateManager.Instance.CurrentState == State.PlayBack)
        {
            GameObject currentSelectedComponent = Manager.Instance.CurrentSelectedComponent;
            if (currentSelectedComponent && Manager.Instance.ComponentsThatCanSnap.Contains(currentSelectedComponent.transform))
                currentComponentTransform = currentSelectedComponent.transform;


            if (currentLineRenderer != null && currentComponentTransform != null && currentSnapPointTransform != null)
            {
                DrawCurvedLine(currentComponentTransform.position, currentSnapPointTransform.position);

                currentLineRenderer.material.mainTextureOffset -= new Vector2(Time.deltaTime * 2f, 0);

            }
        }

    }

    private void DrawCurvedLine(Vector3 startPoint, Vector3 endPoint)
    {
        float distance = Vector3.Distance(startPoint, endPoint);
        Vector3 direction = (endPoint - startPoint).normalized;

        // Calculate the angle between the direction vector and the up vector
        float angle = Vector3.Angle(direction, Vector3.up);

        // Adjust curve intensity based on the angle
        float curveIntensity = Mathf.Clamp((distance * 0.5f) * Mathf.Cos(angle * Mathf.Deg2Rad), 0.1f, 1.5f);

        // Calculate control point based on direction
        Vector3 controlPoint = Vector3.Lerp(startPoint, endPoint, 0.5f) + (Vector3.Cross(direction, Vector3.up) * curveIntensity);

        List<Vector3> curvePoints = CalculateBezierCurvePoints(startPoint, controlPoint, endPoint, 30);

        currentLineRenderer.positionCount = curvePoints.Count;
        currentLineRenderer.SetPositions(curvePoints.ToArray());
    }


    private List<Vector3> CalculateBezierCurvePoints(Vector3 start, Vector3 control, Vector3 end, int numPoints)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            Vector3 pointOnCurve = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end;
            points.Add(pointOnCurve);
        }
        return points;
    }

    public void ShowSnapPoint(SnapToPosition interactor, int currentStep)
    {
        Transform correctSnappoint = interactor.transform.GetChild(currentStep);

        if (correctSnappoint != null)
        {
            correctSnappoint.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public virtual void HighlightComponentToPlace(List<Transform> components)
    {
        if (components == null)
            return;
        if (isWaiting)
            return;

        foreach (var component in components)
        {
            StartCoroutine(HandleHintCooldown(component.gameObject));
        }

    }

    private void CurvedHint(int currentStep, Transform component, SnapToPosition interactor)
    {
        currentComponentTransform = component;
        currentSnapPointTransform = interactor.transform.GetChild(currentStep);

        if (currentComponentTransform != null && currentSnapPointTransform != null)
        {
            currentLineRenderer.enabled = true;
        }
    }

    public IEnumerator HandleHintCooldown(GameObject component)
    {
        isWaiting = true;
        yield return StartCoroutine(ChangeColorTemporarily(component));
        yield return new WaitForSeconds(hintCooldown);
        isWaiting = false;
    }

    public IEnumerator ChangeColorTemporarily(GameObject component)
    {
        MeshRenderer meshRenderer = component.GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            Material[] originalMaterials = meshRenderer.materials;
            Material[] tempMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < tempMaterials.Length; i++)
            {
                tempMaterials[i] = highlightMaterial;
            }

            meshRenderer.materials = tempMaterials;

            yield return new WaitForSeconds(hintCooldown);

            meshRenderer.materials = originalMaterials;
        }
    }

    private void ClearHintLines()
    {
        if(currentLineRenderer != null)
        currentLineRenderer.enabled = false;
    }

    private void HideSnappoints(SnapToPosition interactor)
    {
        if(interactor == null)
            return;
        // Hide snap points if visible
        foreach (Transform snapPoint in interactor.transform)
        {
            MeshRenderer snapRenderer = snapPoint.GetComponent<MeshRenderer>();
            if (snapRenderer != null)
            {
                snapRenderer.enabled = false;
            }
        }
    }

    public virtual void HideHints(SnapToPosition interactor)
    {
        ClearHintLines();
        HideSnappoints(interactor);
    }

    public int HintCount { get => hintCount; set => hintCount = value; }
}