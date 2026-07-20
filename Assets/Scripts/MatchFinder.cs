using System.Collections.Generic;
using UnityEngine;

public class MatchFinder
{
    private List<Match> _matches = new();

    public List<Cell> Line4 = new();
    public List<Cell> Line5 = new();
    public List<Cell> TLineSpec = new();
    public List<Match> FindMatches(Board board)
    {
        Line4.Clear();
        Line5.Clear();
        TLineSpec.Clear();
        _matches.Clear();

        FindHorizontal(board);
        FindVertical(board);
        MergeMatches(board);
        return _matches;
    }

    private void FindHorizontal(Board board)
    {
        for (int y = 0; y < board.Height; y++)
        {
            int x = 0;

            while (x < board.Width)
            {
                Cell start = board.GetCell(x, y);

                if (start.Piece == null)
                {
                    x++;
                    continue;
                }

                int count = 1;

                while (x + count < board.Width)
                {
                    Cell next = board.GetCell(x + count, y);

                    if (next.Piece == null)
                        break;

                    if (next.Piece.Data.ID != start.Piece.Data.ID)
                        break;

                    count++;
                }

                if (count >= 3)
                {
                    Match match = new Match();

                    match.ResourceID = start.Piece.Data.ID;

                    match.Type = count switch
                    {
                        3 => MatchType.Line3,
                        4 => MatchType.Line4,
                        _ => MatchType.Line5
                    };

                    match.direction = MatchDirection.Horizontal;

                    for (int i = 0; i < count; i++)
                    {
                        Cell cell = board.GetCell(x + i, y);
                        match.Cells.Add(cell);

                        if (count == 4 && i == 3)
                            Line4.Add(cell);

                        if (count == 5 && i == 2)
                            Line5.Add(cell);
                    }

                    _matches.Add(match);    
                }

                x += count;
            }
        }
    }

    private void FindVertical(Board board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            int y = 0;

            while (y < board.Height)
            {
                Cell start = board.GetCell(x, y);

                if (start.Piece == null)
                {
                    y++;
                    continue;
                }

                int count = 1;

                while (y + count < board.Height)
                {
                    Cell next = board.GetCell(x, y + count);

                    if (next.Piece == null)
                        break;

                    if (next.Piece.Data.ID != start.Piece.Data.ID)
                        break;

                    count++;
                }

                if (count >= 3)
                {
                    Match match = new Match();

                    match.ResourceID = start.Piece.Data.ID;

                    match.Type = count switch
                    {
                        3 => MatchType.Line3,
                        4 => MatchType.Line4,
                        _ => MatchType.Line5
                    };

                    match.direction = MatchDirection.Vertical;

                    for (int i = 0; i < count; i++)
                    {
                        Cell cell = board.GetCell(x, y+i);
                        match.Cells.Add(cell);

                        if (count == 4 && i == 3)
                            Line4.Add(cell);

                        if (count == 5 && i == 2)
                            Line5.Add(cell);
                    }
                    _matches.Add(match);
                }

                y += count;
            }
        }
    }

    private void MergeMatches(Board board)
    {
        for (int i = 0; i < _matches.Count; i++)
        {
            for (int j = i + 1; j < _matches.Count; j++)
            {
                Match first = _matches[i];
                Match second = _matches[j];

                if (!HasCommonCell(first, second)) continue;
                Merge(board, first, second);

                _matches.RemoveAt(j);
                j--;
            }
        }
    }

    private bool HasCommonCell(Match first, Match second)
    {
        foreach (Cell cell in first.Cells)
        {
            if (second.Cells.Contains(cell)) return true;
        }
        return false;
    }

    private void Merge(Board board, Match first, Match second)
    {
        foreach (Cell cell in second.Cells)
        {
            if (!first.Cells.Contains(cell))
                first.Cells.Add(cell);
        }

        if (first.direction != second.direction)
            first.direction = MatchDirection.Mixed;

        first.Type = GetShape(board, first);
    }


    private MatchType GetShape(Board board, Match match)
    {
        foreach (Cell cell in match.Cells)
        {
            bool left = Contains(match, cell.X - 1, cell.Y);
            bool right = Contains(match, cell.X + 1, cell.Y);
            bool up = Contains(match, cell.X, cell.Y - 1);
            bool down = Contains(match, cell.X, cell.Y + 1);

            int neighbours = 0;

            if (left) neighbours++;
            if (right) neighbours++;
            if (up) neighbours++;
            if (down) neighbours++;

            // ÷ентр буквы T
            if (neighbours == 3)

            {
                if(!TLineSpec.Contains(cell))
                TLineSpec.Add(cell);

                return MatchType.TShape;
            }

            // ”гол буквы L
            if ((left && up) ||
                (left && down) ||
                (right && up) ||
                (right && down))
                return MatchType.LShape;
        }

        if (match.Cells.Count == 5)

            return MatchType.Line5;

        if (match.Cells.Count == 4)
            return MatchType.Line4;

        return MatchType.Line3;
    }

    private bool Contains(Match match, int x, int y)
    {
        foreach (Cell cell in match.Cells)
        {
            if (cell.X == x && cell.Y == y)
                return true;
        }

        return false;
    }


    public int FastCheckMatch(Board board)
    {
        int amount = 0;

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width - 2; x++)
            {
                Piece a = board.GetCell(x, y).Piece;
                Piece b = board.GetCell(x + 1, y).Piece;
                Piece c = board.GetCell(x + 2, y).Piece;

                if (a == null || b == null || c == null) continue;

                if (a.Data.ID == b.Data.ID && b.Data.ID == c.Data.ID) amount++;
            }
        }

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height - 2; y++)
            {
                Piece a = board.GetCell(x, y).Piece;
                Piece b = board.GetCell(x, y + 1).Piece;
                Piece c = board.GetCell(x, y + 2).Piece;

                if (a == null || b == null || c == null)
                    continue;

                if (a.Data.ID == b.Data.ID && b.Data.ID == c.Data.ID)
                    amount++;
            }
        }
        return amount;
    }
}