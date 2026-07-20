using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SwapSystem
{
    private float _duration = 0.3f;

    public async UniTask<bool> Swap(Board board, BoardView boardView, Cell a, Cell b)
    {
        if (CellActivityService.IsBusy(a) || CellActivityService.IsBusy(b)) return false;

        CellActivityService.Add(a, CellActivity.Swapping);
        CellActivityService.Add(b, CellActivity.Swapping);

        SwapPiece(board, a, b);
        await SwapPieceView(boardView, a, b);

        CellActivityService.Remove(a, CellActivity.Swapping);
        CellActivityService.Remove(b, CellActivity.Swapping);

        return true;
    }

    public void SwapPiece(Board board, Cell a, Cell b)
    {
        Piece temp = a.Piece;

        board.SetPiece(a.X, a.Y, b.Piece);
        board.SetPiece(b.X, b.Y, temp);
    }

    private async UniTask SwapPieceView(BoardView boardView, Cell a, Cell b)
    {

        PieceView first = boardView.GetCellView(a).PieceView;
        PieceView second = boardView.GetCellView(b).PieceView;

        if (first == null || second == null)
        {
            Debug.LogError("ќдна из клеток не имеет PieceView");
            return;
        }

        boardView.GetCellView(a).SetPieceView(second);
        boardView.GetCellView(b).SetPieceView(first);

        Vector2 posA = boardView.GetCellPosition(a);
        Vector2 posB = boardView.GetCellPosition(b);

        Tween tween1 = first.MoveTo(posB, _duration);
        Tween tween2 = second.MoveTo(posA, _duration);

        await UniTask.WhenAll(
            tween1.AsyncWaitForCompletion().AsUniTask(),
            tween2.AsyncWaitForCompletion().AsUniTask());
    }
}
