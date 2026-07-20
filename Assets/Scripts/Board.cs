using UnityEngine;

public class Board
{
    private Cell[,] _cells;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Board(int width, int height)
    {
        Width = width;
        Height = height;

        _cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new Cell(x,y);
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        return _cells[x, y];
    }

    public void SetPiece(int x, int y, Piece piece)
    {
        _cells[x, y].Piece = piece;
    }

}