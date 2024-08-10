using UnityEngine;

public class Screw : Fastener
{
    private BaseScrewDriver screwdriverScript;
    [SerializeField] protected float pitch = 0.1f;

    protected override void HandleInteraction()
    {
        if (Manager.Instance.State == State.PlayBack)
        {
            HandlePlayBackInteraction();
        }
        else if (screwdriverScript != null && isAligned)
        {
            HandleNormalInteraction();
        }
    }

    private void HandlePlayBackInteraction()
    {
        if (screwdriverScript != null && socketTransform != null)
        {
            Vector3 screwdriverDir = screwdriverScript.transform.forward;
            Vector3 screwDir = transform.forward;
            float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

            if (dotProduct >= maxAllowedDotProduct)
            {
                float rotationSpeed = screwdriverScript.GetRotationSpeed();
                float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;

                socketTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);
                socketTransform.Translate(Vector3.forward * linearMovement);

                float distanceTraveled = Mathf.Abs(Vector3.Distance(socketTransform.position, initialSocketPosition));

                if (distanceTraveled >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                }
            }
        }
    }

    private void HandleNormalInteraction()
    {
        Vector3 screwdriverDir = screwdriverScript.transform.forward;
        Vector3 screwDir = transform.forward;
        float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

        if (dotProduct >= maxAllowedDotProduct)
        {
            float rotationSpeed = screwdriverScript.GetRotationSpeed();
            float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;

            if (socketTransform != null)
            {
                socketTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);
                socketTransform.Translate(Vector3.forward * linearMovement);

                float distanceTraveled = Mathf.Abs(Vector3.Distance(socketTransform.localPosition, initialSocketPosition));

                if (distanceTraveled >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                }
            }
            else
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);
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
