using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screw : MonoBehaviour
{
    private bool isCollidingWithScrewdriver = false;
    private ScrewDriver screwdriverScript;

    // Update is called once per frame
    void Update()
    {
        if (isCollidingWithScrewdriver && screwdriverScript != null)
        {
            transform.Rotate(Vector3.forward, screwdriverScript.GetRotationSpeed() * Time.deltaTime * -1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Screwdriver"))
        {
            isCollidingWithScrewdriver = true;
            screwdriverScript = other.GetComponentInParent<ScrewDriver>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Screwdriver"))
        {
            isCollidingWithScrewdriver = false;
            screwdriverScript = null;
        }
    }
}
