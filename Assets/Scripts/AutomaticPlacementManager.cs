using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class AutomaticPlacementManager : MonoBehaviour
{
    [SerializeField] private Transform showSolutionPosition;
    private GameObject interactorClone;
    private bool isPlacingComponent = false;

    public void PlaceCurrentStepComponent(int stepIndex, Transform componentToPlace, SnapToPosition interactor,
        float timeMovement)
    {
        if (isPlacingComponent) return; // Prevent spamming if placement is ongoing

        isPlacingComponent = true;
        componentToPlace.GetComponent<ComponentObject>().IsReleased = true;

        Transform correctSnappoint = interactor.transform.GetChild(stepIndex);
        if (correctSnappoint == null)
        {
            Debug.LogWarning($"Snap point for step index {stepIndex} not found in the interactor.");
            isPlacingComponent = false; // Reset flag if snap point is not found
            return;
        }

        StartCoroutine(SmoothMoveAndResetFlag(componentToPlace, correctSnappoint, timeMovement));
        // Start coroutine to move the component and reset the flag afterward
    }

    public void PlaceStepComponent(int stepIndex, Transform componentToPlace, SnapToPosition interactor)
    {
        Transform correctSnappoint = interactor.transform.GetChild(stepIndex);
        componentToPlace.GetComponent<ComponentObject>().IsReleased = true;

        if (correctSnappoint == null)
        {
            Debug.LogWarning($"Snap point for step index {stepIndex} not found in the interactor.");
            isPlacingComponent = false; // Reset flag if snap point is not found
            return;
        }

        MoveComponent(componentToPlace, correctSnappoint);
    }

    private IEnumerator SmoothMoveAndResetFlag(Transform component, Transform correctSnappoint, float duration)
    {
        yield return StartCoroutine(SmoothMoveComponent(component, correctSnappoint, duration));
        isPlacingComponent = false; // Reset flag after placement is complete
    }

    public IEnumerator SmoothMoveComponent(Transform component, Transform correctSnappoint, float duration)
    {
        Vector3 initialPosition = component.position;
        Quaternion initialRotation = component.rotation;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Continuously update target position and rotation to follow snap point
            Vector3 targetPosition = correctSnappoint.position;
            Quaternion targetRotation = correctSnappoint.rotation;

            float t = elapsedTime / duration;
            component.position = Vector3.Lerp(initialPosition, targetPosition, t);
            component.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final alignment with the snap point after the movement completes
        component.position = correctSnappoint.position;
        component.rotation = correctSnappoint.rotation;
    }

    public void MoveComponent(Transform component, Transform correctSnappoint)
    {
        // Final alignment with the snap point after the movement completes
        component.transform.SetPositionAndRotation(correctSnappoint.transform.position, correctSnappoint.rotation);
        //component.rotation = correctSnappoint.rotation;
    }
}