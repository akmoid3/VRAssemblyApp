using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screw : MonoBehaviour
{
    private ScrewDriver screwdriverScript;

    [SerializeField] private float pitch = 0.1f; // Distance the screw moves per rotation
    private float maxAllowedDotProduct = -0.9f;

    [SerializeField] private bool isCollidingWithScrewdriver = false;
    [SerializeField] private bool canStop = false;
    [SerializeField] private bool isStopped = false; // Flag if the screw should stop moving

    void Update()
    {
        if (isCollidingWithScrewdriver && screwdriverScript != null && !isStopped)
        {
            // Check the alignment using dot product
            Vector3 screwdriverDir = screwdriverScript.transform.forward;
            Vector3 screwDir = transform.forward;
            float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

            if (dotProduct <= maxAllowedDotProduct)
            {
                float rotationSpeed = screwdriverScript.GetRotationSpeed();
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1);

                // Calculate the movement along the screw's axis based on the rotation speed and pitch
                float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;
                transform.Translate(Vector3.forward * linearMovement);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Screwdriver"))
        {
            isCollidingWithScrewdriver = true;
            screwdriverScript = other.GetComponentInParent<ScrewDriver>();
            canStop = true;
            Debug.Log("entered");
        }
        else if(other.CompareTag("Component") && canStop)
        {
            // Check if the screw has collided with a component
            if (!isStopped)
            {
                isStopped = true;
                Debug.Log("Screw has reached maximum depth");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Screwdriver"))
        {
            isCollidingWithScrewdriver = false;
            screwdriverScript = null;
        }
        else if (other.CompareTag("Component") && isStopped)
        {
                canStop = false;
                isStopped = false;
        }
    }
}
