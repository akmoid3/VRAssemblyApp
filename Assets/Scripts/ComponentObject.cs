using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ComponentObject : MonoBehaviour
{
    private bool isPlaced = false;
    private bool isReleased = false;

    public bool IsReleased { get => isReleased; set => isReleased = value; }

    public bool GetIsPlaced()
    {
        return isPlaced;
    }

    public void SetIsPlaced(bool value)
    {
        isPlaced = value;
    }

}
