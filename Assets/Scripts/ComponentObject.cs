using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentObject : MonoBehaviour
{
    private bool isPlaced = false;

    public bool GetIsPlaced()
    {
        return isPlaced;
    }

    public void SetIsPlaced(bool value)
    {
        isPlaced = value;
    }

}
