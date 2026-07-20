using System.Collections.Generic;
using UnityEngine;
public class Match
{
    public readonly List<Cell> Cells = new();
    public MatchType Type;
    public int ResourceID;
    public MatchDirection direction;

    public int Count => Cells.Count;
}