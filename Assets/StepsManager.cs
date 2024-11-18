using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepsManager : MonoBehaviour
{


    public void WrongComponentError(List<ComponentData> assemblySequence, int currentStep, List<Transform> components)
    {
   
        int stepID = assemblySequence[currentStep].stepId;
        string componentToPlaceName = assemblySequence[currentStep].componentName;
        string componentToPlaceGroup = assemblySequence[currentStep].group;


        if (!Manager.Instance.CurrentAssembledSequence.TryGetValue(stepID, out var currentComponent))
        {
            foreach (var component in components)
            {
                ComponentObject componentObject = component.GetComponent<ComponentObject>();
                if ((component.name == componentToPlaceName && !componentObject.GetIsPlaced()) || (!componentObject.GetIsPlaced() && componentObject.GetGroup() != "None" && componentToPlaceGroup == componentObject.GetGroup()))
                {
                }
            }
        }
        else
        {

        }
    }
}
