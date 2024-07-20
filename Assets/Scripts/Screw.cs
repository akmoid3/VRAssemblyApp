using UnityEngine;

public class Screw : Fastener
{
    private BaseScrewDriver screwdriverScript;
    [SerializeField] protected float pitch = 0.1f;

    protected override void HandleInteraction()
    {
        if (screwdriverScript != null && isAligned)
        {
            Vector3 screwdriverDir = screwdriverScript.transform.forward;
            Vector3 screwDir = transform.forward;
            float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

            if (dotProduct >= maxAllowedDotProduct)
            {
                float rotationSpeed = screwdriverScript.GetRotationSpeed();
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);

                float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;
                transform.Translate(Vector3.forward * linearMovement);

                float distanceTraveled = Mathf.Abs(Vector3.Distance(transform.localPosition, initialZPosition));

                if (distanceTraveled >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                }
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
    }
}
