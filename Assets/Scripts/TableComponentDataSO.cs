using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TableComponentData", menuName = "ScriptableObjects/TableComponentData", order = 1)]
public class TableComponentDataSO : ScriptableObject
{
    public string id;
    public string type;
    public List<AttributeData> attributes = new List<AttributeData>();
    public GameObject prefab;
}