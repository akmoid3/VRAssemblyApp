using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TableComponentList", menuName = "ScriptableObjects/TableComponentList", order = 2)]
public class TableComponentListSO : ScriptableObject
{
    public List<TableComponentDataSO> components = new List<TableComponentDataSO>();
}