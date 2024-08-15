using UnityEngine;

public class Screw : Fastener
{
    private BaseScrewDriver screwdriverScript;
    [SerializeField] protected float pitch = 0.1f;
    private bool isScrewing = false;
    private bool isFirstError = true;
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
        if (screwdriverScript != null && socketTransform != null && screwdriverScript.gameObject.name == CorrectToolName)
        {
            Vector3 screwdriverDir = screwdriverScript.transform.forward;
            Vector3 screwDir = transform.forward;
            float dotProduct = Vector3.Dot(screwdriverDir.normalized, screwDir.normalized);

            if (dotProduct >= maxAllowedDotProduct)
            {
                float rotationSpeed = screwdriverScript.GetRotationSpeed();
                float linearMovement = (rotationSpeed * pitch / 360) * Time.deltaTime;

                if (!isScrewing && linearMovement > 0.0f)
                {
                    ScrewAudioManager.Instance.PlayScrewSound();
                    isScrewing = true;
                }
                else
                {
                    StopScrewSound();
                }

                // Update pitch based on linear movement
                float pitchAudio = Mathf.Clamp(linearMovement * 10.0f, 0.5f, 2.0f);
                ScrewAudioManager.Instance.SetPitch(pitchAudio);

                socketTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);
                socketTransform.Translate(Vector3.forward * linearMovement);

                float distanceTraveled = Mathf.Abs(Vector3.Distance(socketTransform.localPosition, initialSocketPosition));

                if (distanceTraveled >= distanceToTravel)
                {
                    isStopped = true;
                    fastenerRenderer.material.color = defaultColor;
                    StopScrewSound();
                }
            }
        }else if (screwdriverScript.gameObject.name != CorrectToolName && isFirstError)
        {
            Manager.Instance.IncrementCurrentError();
            isFirstError = false;
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

            if (!isScrewing && linearMovement > 0.0f)
            {
                ScrewAudioManager.Instance.PlayScrewSound();
                isScrewing = true;
            }
            else
            {
                StopScrewSound();
            }

            // Update pitch based on linear movement
            float pitchAudio = Mathf.Clamp(linearMovement * 10.0f, 0.5f, 2.0f);
            ScrewAudioManager.Instance.SetPitch(pitchAudio);

            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime * -1.0f);
            transform.Translate(Vector3.forward * linearMovement);

            float distanceTraveled = Mathf.Abs(Vector3.Distance(transform.localPosition, initialZPosition));

            if (distanceTraveled >= distanceToTravel)
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
        isFirstError = true;

    }

    private void StopScrewSound()
    {
        ScrewAudioManager.Instance.StopScrewSound();
        isScrewing = false;
    }
}
