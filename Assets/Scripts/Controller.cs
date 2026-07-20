using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Controller : MonoBehaviour
{
    public static Controller Instance { get; private set; }

    public event Action RefreshSwapCount;

    [SerializeField] private GameInput _input;
    [SerializeField] private DataLevel _lvl;
    [SerializeField] private DataResource[] _resource;
    [SerializeField] private DataScore _dataScore;
    [SerializeField] private DataResource _bomb;
    [SerializeField] private DataResource _blisk;
    [SerializeField] private DataResource _arrow;
    [SerializeField] private BoardView _boardView;

    private Board _board;
    public PieceSpawner _spawner;
    private MatchFinder _finder;
    private SwapSystem _swap;
    private DestroySystem _destroy;
    public ScoreSystem _scoreSystem;

    private List<Cell> _otherDestroy = new();
    private List<Match> list = new();
    private Cell selectedCell;
    private float _time = 0.3f;

    private bool _islifeCycleRunning = false;
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        Instance = this;

        _finder = new MatchFinder();
        _swap = new SwapSystem();
        _destroy = new DestroySystem();
        _scoreSystem = new ScoreSystem(_dataScore);
    }

    private void Start()
    {
        _input.OnTouchStart += TouchStartMetod;
        _input.OnToutchEnd += TouchEndMetod;

        _board = new Board(_lvl.Width, _lvl.Height);
        _spawner = new PieceSpawner(_resource);

        GenerateValidPieces();
        _boardView.Build(_board, _lvl);

        list = _finder.FindMatches(_board);

        _destroy.OnDestroyed += _scoreSystem.AddScore;

        Debug.Log("Controller Start");
        Debug.Log(_lvl.SwapAwailable);

        IsPaused = false;
    }

    public bool Pause(bool temp) => IsPaused = temp;

    public async UniTask StartFalling()
    {
        List<UniTask> moves = new();

        for (int x = 0; x < _lvl.Width; x++)
        {
            int writeY = _lvl.Height - 1;

            for (int readY = _lvl.Height - 1; readY >= 0; readY--)
            {
                Cell from = _board.GetCell(x, readY);

                if (from.Piece == null) continue;

                if (readY != writeY)
                {
                    Cell to = _board.GetCell(x, writeY);
                    CellActivityService.Add(from, CellActivity.Falling);
                    CellActivityService.Add(to, CellActivity.Falling);

                    to.Piece = from.Piece;

                    from.Piece = null;

                    PieceView view = _boardView.GetCellView(from).PieceView;

                    _boardView.GetCellView(to).SetPieceView(view);
                    _boardView.GetCellView(from).SetPieceView(null);

                    Tween tween = view.MoveTo(_boardView.GetCellPosition(to), _time).SetEase(Ease.OutBounce);

                    moves.Add(UnlockAfterFall(tween, from, to));
                }
                writeY--;
            }
        }
        await UniTask.WhenAll(moves);
    }

    private async UniTask UnlockAfterFall(Tween tween, Cell from, Cell to)
    {
        await tween.AsyncWaitForCompletion();

        CellActivityService.Remove(from, CellActivity.Falling);
        CellActivityService.Remove(to, CellActivity.Falling);
    }

    private bool ValidSwap(Board board, Cell a, Cell b)
    {
        int first = _finder.FastCheckMatch(board);
        _swap.SwapPiece(board, a, b);

        int second = _finder.FastCheckMatch(board);
        _swap.SwapPiece(board, a, b);

        return first != second;
    }

    private void TouchStartMetod(Vector2 ScreenPos)
    {
        selectedCell = _boardView.GetCellByScreenPosition(_board, ScreenPos, _lvl);

        if (selectedCell == null)
            return;

        if (CellActivityService.IsBusy(selectedCell))
        {
            selectedCell = null;
            return;
        }
        Debug.Log($"Выбрана клетка {selectedCell.X} : {selectedCell.Y}");
    }

    private async void TouchEndMetod(Vector2 dir)
    {
        if (selectedCell == null)
            return;

        Cell start = selectedCell;
        selectedCell = null;

        int targetX = start.X;
        int targetY = start.Y;

        if (dir == Vector2.right) targetX++;

        else if (dir == Vector2.left) targetX--;

        else if (dir == Vector2.up) targetY--;

        else if (dir == Vector2.down) targetY++;

        if (targetX < 0 || targetX >= _lvl.Width)
            return;

        if (targetY < 0 || targetY >= _lvl.Height)
            return;

        Cell target = _board.GetCell(targetX, targetY);

        bool swapped = await _swap.Swap(_board, _boardView, start, target);
        if (!swapped)
            return;

        bool isValid = ValidSwap(_board, start, target);

        if (!isValid)
        {
            await _swap.Swap(_board, _boardView, target, start);
            return;
        }
        RefreshSwapCount?.Invoke();
        await UniTask.WaitUntil(() => !_islifeCycleRunning);

        await LifeCycle();
    }

    private async UniTask LifeCycle()
    {
        _islifeCycleRunning = true;
        do
        {
            list = _finder.FindMatches(_board);

            if (list.Count == 0) break;

            await _destroy.Destroy(_board, _boardView, list);

            await GenerateBoosters();

            await StartFalling();

            await GenerateRandomPieces();

            await PlayBoost(_board, _boardView);

            await _destroy.Destroy(_board, _boardView, _otherDestroy);

            await StartFalling();

            await GenerateRandomPieces();
        }

        while (list.Count != 0);

        _islifeCycleRunning = false;
    }

    private void GenerateValidPieces()
    {
        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                Piece piece = _spawner.CreateValidPiece(_board, x, y);
                _board.SetPiece(x, y, piece);
            }
        }
    }

    private async UniTask GenerateBoosters()
    {

        List<UniTask> tasks = new();

        foreach (Cell cell in _finder.Line4)

            CreateBooster(cell, _arrow);

        foreach (Cell cell in _finder.Line5)
            CreateBooster(cell, _blisk);

        foreach (Cell cell in _finder.TLineSpec)
            CreateBooster(cell, _bomb);

        await UniTask.WhenAll(tasks);
    }


    private void CreateBooster(Cell cell, DataResource data)
    {
        PieceView oldView = _boardView.GetCellView(cell).PieceView;

        if (oldView != null)
        {
            _boardView.BackToPool(oldView);
            _boardView.GetCellView(cell).SetPieceView(null);
        }

        cell.Piece = _spawner.CreateBooster(data);

        PieceView view = _boardView.CreatePieceAndView(cell, _lvl);
    }

    private async UniTask GenerateRandomPieces()
    {
        List<UniTask> moves = new();

        for (int x = 0; x < _lvl.Width; x++)
        {
            for (int y = 0; y < _lvl.Height; y++)
            {
                Cell cell = _board.GetCell(x, y);

                if (cell.Piece != null) continue;

                cell.Piece = _spawner.CreateRandomPiece();
                CellActivityService.Add(cell, CellActivity.Spawning);

                PieceView view = _boardView.CreatePieceAndView(cell, _lvl);

                RectTransform rect = view.GetComponent<RectTransform>();

                Vector2 target = rect.anchoredPosition;
                rect.anchoredPosition += Vector2.up * 500;

                Tween tween = rect.DOAnchorPos(target, _time);
                moves.Add(UnlockAfterSpawn(tween, cell));
            }
        }
        await UniTask.WhenAll(moves);
    }

    private async UniTask UnlockAfterSpawn(Tween tween, Cell cell)
    {
        await tween.AsyncWaitForCompletion();
        CellActivityService.Remove(cell, CellActivity.Spawning);
    }

    public void FindCellDestroy(int xCell, int yCell, bool horizontal)
    {
        if (horizontal)
        {
            for (int x = 0; x < _board.Width; x++)
                _otherDestroy.Add(_board.GetCell(x, yCell));
        }

        else
        {
            for (int y = 0; y < _board.Height; y++)
                _otherDestroy.Add(_board.GetCell(xCell, y));
        }
    }

    private void OnDestroy()
    {
        _destroy.OnDestroyed -= _scoreSystem.AddScore;
    }

    private async UniTask PlayBoost(Board board, BoardView boardView)
    {
        _otherDestroy.Clear();

        List<UniTask> animations = new();

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                Cell cell = board.GetCell(x, y);

                if (cell.Piece.Data.Types == TypePiece.Resource || cell.Piece.Data.Activeid == TypeActiveid.AfterSwap) continue;

                CellActivityService.Add(cell, CellActivity.Falling);
                _otherDestroy.Add(cell);

                PieceView view = boardView.GetCellView(cell).PieceView;
                animations.Add(view.PlayBooster(cell));
            }
        }
        await UniTask.WhenAll(animations);

        foreach (Cell cell in _otherDestroy)
        {
            CellActivityService.Remove(cell, CellActivity.Falling);
        }
    }

    public void FindBombDestroy(int xCell, int yCell)
    {
        for (int y = yCell - 1; y <= yCell + 1; y++)
        {
            for (int x = xCell - 1; x <= xCell + 1; x++)
            {
                if (x < 0 || x >= _board.Width) continue;

                if (y < 0 || y >= _board.Height) continue;


                _otherDestroy.Add(_board.GetCell(x, y));
            }
        }
    }

    public async UniTask FindLightningDestroy(Piece piece)
    {
        List<UniTask> tasks = new();

        int type = piece.Data.ID;

        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                Cell cell = _board.GetCell(x, y);

                if (cell.Piece.Data.ID == type)
                {
                    if (!_otherDestroy.Contains(cell))
                    {
                        _otherDestroy.Add(cell);
                        PieceView view = _boardView.GetCellView(cell).PieceView;
                        tasks.Add(view.PlayShake(cell));
                    }

                }
            }
        }
        await UniTask.WhenAll(tasks);
    }
    public int GetStartSwapCount() =>  _lvl.SwapAwailable;
}