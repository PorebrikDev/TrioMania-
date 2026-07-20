using UnityEngine;
[CreateAssetMenu(menuName = "Data/Level")]
public class DataLevel : ScriptableObject
{
    public int LevelId = 1;
    public int SwapAwailable = 25;
    [Header("")]
    public int Width = 9;
    public int Height = 13;
    [Header("")]
    public int CellSize = 115;
    public int CellCount => Width * Height;
}
