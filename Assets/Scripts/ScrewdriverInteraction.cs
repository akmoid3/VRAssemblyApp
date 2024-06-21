using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ScrewdriverInteraction : MonoBehaviour
{
    public float maxScrewDistance = 0.05f;
    private bool isScrewing = false;
    private Transform currentScrew;

    void Start()
    {
    }

    void Update()
    {
        if (isScrewing && currentScrew != null)
        {
            float rotationAngle = transform.rotation.eulerAngles.z;
            currentScrew.rotation = Quaternion.Euler(0f, 0f, -rotationAngle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Screw"))
        {
            isScrewing = true;
            currentScrew = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Screw"))
        {
            isScrewing = false;
            currentScrew = null;
        }
    }
}
