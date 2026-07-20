using System;
using UnityEngine;
[CreateAssetMenu(fileName = "Resource", menuName = "Data/Resource")] 
public class DataResource : ScriptableObject
{
    public int ID;
    public Sprite Icon;
    public TypePiece Types = TypePiece.Resource;
    public TypeActiveid Activeid = TypeActiveid.Now;
}
