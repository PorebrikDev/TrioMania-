using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class DestroySystem
{
    public event Action<List<Match>> OnDestroyed;

    public async UniTask Destroy(Board board, BoardView boardView, List<Match> matches)
    {
        List<UniTask> task = new();

        foreach (Match match in matches)
        {
           task.Add(  DestroyMatch(board, boardView, match));
        }

        await UniTask.WhenAll(task);

        OnDestroyed?.Invoke(matches);
    }

    private async UniTask DestroyMatch(Board board, BoardView boardView, Match match)
    {
        List<UniTask> animations = new();

        foreach (Cell cell in match.Cells)
        {
            CellActivityService.Add(cell, CellActivity.Destroying);
            PieceView view = boardView.GetCellView(cell).PieceView;

            if (view != null)
            {
                animations.Add(view.PlayDestroy(cell.Piece.Data.Types));
            }
        }

        await UniTask.WhenAll(animations);

        foreach (Cell cell in match.Cells)
        {
            DestroyCell(board, boardView, cell);
            CellActivityService.Remove(cell, CellActivity.Destroying);
        }
    }

    private void DestroyCell(Board board, BoardView boardView, Cell cell)
    {
        PieceView view = boardView.GetCellView(cell).PieceView;
   
        if (view != null)
        {
            boardView.BackToPool(view);
        }

        boardView.GetCellView(cell).SetPieceView(null);

        cell.Piece = null;
    }


    public async UniTask Destroy(Board board, BoardView boardView, List<Cell> cells)
    {
        List<UniTask> animations = new();

        foreach (Cell cell in cells)
        {
            CellActivityService.Add(cell, CellActivity.Destroying);

            PieceView view = boardView.GetCellView(cell).PieceView;

            if (view != null)
                animations.Add(view.PlayDestroy(cell.Piece.Data.Types));
        }

        await UniTask.WhenAll(animations);

        foreach (Cell cell in cells)
        {
            CellActivityService.Remove(cell, CellActivity.Destroying);

            DestroyCell(board, boardView, cell);
        }
    }
}