using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]

public class BoardView : MonoBehaviour
{
    [SerializeField] private CellView _cellPrefab;
    [SerializeField] private PieceView _piecePrefab;

    [SerializeField] private RectTransform _boardRect;

    [SerializeField] private Transform _cellRoot;
    [SerializeField] private Transform _pieceRoot;

    private CellView[,] _cellView;
    private Stack<PieceView> _pieceViewsPool = new();

    public CellView GetCellView(Cell cell)
    {
        return _cellView[cell.X, cell.Y];
    }

    public Vector2 GetCellPosition(Cell cell)
    {
        return ((RectTransform)_cellView[cell.X, cell.Y].transform).anchoredPosition;
    }

    public void Build(Board board, DataLevel lvl)
    {
        _cellView = new CellView[board.Width, board.Height];
        CreatePoolPieceView(lvl.CellCount);

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                Cell cell = board.GetCell(x, y);

                CreateCellAndView(cell, lvl);

                if (cell.Piece != null)
                {
                    CreatePieceAndView(cell, lvl);
                }
            }
        }
    }

    public Cell GetCellByScreenPosition(Board board, Vector2 screenPos, DataLevel lvl)
    {
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_boardRect, screenPos, null, out localPos);

        int x = Mathf.FloorToInt(localPos.x / lvl.CellSize);
        int y = Mathf.FloorToInt(-localPos.y / lvl.CellSize);

        if (x < 0 || x >= board.Width) return null;
        if (y < 0 || y >= board.Height) return null;

        return board.GetCell(x, y);
    }

    private void CreateCellAndView(Cell cell, DataLevel lvl)
    {
        CellView view = Instantiate(_cellPrefab, _cellRoot);

        view.SetCell(cell);
        _cellView[cell.X, cell.Y] = view;

        RectTransform rect = view.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(cell.X * lvl.CellSize + lvl.CellSize * 0.5f, -cell.Y * lvl.CellSize - lvl.CellSize * 0.5f);
    }

    public PieceView CreatePieceAndView(Cell cell, DataLevel lvl)
    {
        PieceView view = GetFromPool();

        view.SetPiece(cell.Piece);
        _cellView[cell.X, cell.Y].SetPieceView(view);

        RectTransform rect = view.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(cell.X * lvl.CellSize + lvl.CellSize * 0.5f, -cell.Y * lvl.CellSize - lvl.CellSize * 0.5f);

        return view;
    }

    public void CreatePoolPieceView(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PieceView view = Instantiate(_piecePrefab, _pieceRoot);

            view.gameObject.SetActive(false);

            _pieceViewsPool.Push(view);
        }
    }

    public PieceView GetFromPool()
    {
        if (_pieceViewsPool.Count == 0)
        {
            Debug.Log("Пулл Piece пуст, создал еще 10");
            CreatePoolPieceView(10);
        }

        PieceView view = _pieceViewsPool.Pop();
        view.gameObject.SetActive(true);
        return view;
    }

    public void BackToPool(PieceView view)
    {
        view.gameObject.SetActive(false);
        _pieceViewsPool.Push(view);
    }
}