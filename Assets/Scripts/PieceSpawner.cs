using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner
{
    private DataResource[] _resources;

    public PieceSpawner(DataResource[] resources)
    {
        _resources = resources;
    }

    public Piece CreateRandomPiece()
    {
        var temp = _resources[Random.Range(0, _resources.Length)];
        return new Piece(temp);
    }

    public Piece CreateBooster(DataResource data)
    {
        return new Piece(data);
    }


    public Piece CreateValidPiece(Board board, int x, int y)
    {
        Piece piece;

        do { piece = CreateRandomPiece(); }

        while (WithoutMatch(board, x, y, piece));

        return piece;
    }

    private bool WithoutMatch(Board board, int x, int y, Piece piece)
    {
        if (x >= 2)
        {
            Cell a = board.GetCell(x - 1, y);
            Cell b = board.GetCell(x - 2, y);

            if (a.Piece.Data.ID == piece.Data.ID && b.Piece.Data.ID == piece.Data.ID) return true;
        }

        if (y >= 2)
        {
            Cell c = board.GetCell(x, y - 1);
            Cell d = board.GetCell(x, y - 2);

            if (c.Piece.Data.ID == piece.Data.ID && d.Piece.Data.ID == piece.Data.ID) return true;
        }
        return false;
    }
}