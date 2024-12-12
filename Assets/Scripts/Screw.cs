using System;
using UnityEngine;

public class Screw : Fastener
{
    private BaseScrewDriver screwdriverScript;

    [SerializeField] protected float pitch = 0.1f;
    private bool isScrewing = false;

    protected override void HandleInteraction()
    {
        if (isStopped) return;
        if (StateManager.Instance.CurrentState == State.PlayBack)
        {
            if (screwdriverScript != null && socketTransform != null && screwdriverScript.ToolName == CorrectToolName)
            {
                DynamometerScrewDriver dynamometerScrewDriver = screwdriverScript as DynamometerScrewDriver;
                if (dynamometerScrewDriver && dynamometerScrewDriver.Force != CorrectToolForce)
                    return;
                Interaction();

            }
        }
        else if (screwdriverScript != null && isAligned)
        {
            Interaction();
        }
    }

    private void Interaction()
    {
        Vector3 screwdriverDir = screwdriverScript.transform.forward;
        Vector3 screwDir = MapSelectedAxisToTransformDirection(selectedAxisDirRaw);
        float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

        if (dotProduct >= maxAllowedDotProduct)
        {
            float rotationSpeed = screwdriverScript.GetRotationSpeed();
            float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;

            if (!isScrewing && linearMovement > 0.0f)
            {
                AudioManager.Instance.PlaySound(audioSource, "screw", true, 1f);
                isScrewing = true;
            }
            else
            {
                StopScrewSound();
            }

            // Update pitch based on linear movement
            float pitchAudio = Mathf.Clamp(linearMovement * 10.0f, 0.5f, 2.0f);
            AudioManager.Instance.SetPitch(audioSource, pitchAudio);

            transform.Rotate(selectedAxisDirRaw, rotationSpeed * Time.deltaTime * -1.0f);
            transform.Translate(selectedAxisDirRaw * linearMovement);

            float distanceTraveled = Vector3.Distance(transform.localPosition, InitialPosition);

            if ((distanceTraveled >= distanceToTravel && !socketTransform) || (socketTransform && Vector3.Distance(transform.position, socketTransform.position) <= 0.01f))
            {
                isStopped = true;
                fastenerRenderer.material.color = defaultColor;
                StopScrewSound();
            }
        }
    }

    protected override void OnToolCollisionEnter(Collider other)
    {
        screwdriverScript = other.GetComponentInParent<BaseScrewDriver>();
    }

    protected override void OnToolCollisionExit(Collider other)
    {
        screwdriverScript = null;
        StopScrewSound(); // Stop sound when screwdriver leaves
    }

    private void StopScrewSound()
    {
        AudioManager.Instance.StopSound(audioSource);
        isScrewing = false;
    }
}
