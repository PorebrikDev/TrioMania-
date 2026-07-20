using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
    private RectTransform _rt;
    private Image _image;


    private void Awake()
    {
        _image = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        PieceViewAnimations.Reset(_rt);
    }

    private void OnDisable()
    {
        PieceViewAnimations.Reset(_rt);
    }

    private void OnDestroy()
    {
        PieceViewAnimations.Reset(_rt);
    }

    public void SetPiece(Piece piece) => _image.sprite = piece.Data.Icon;

    public Tween MoveTo(Vector2 target, float duration) =>  _rt.DOAnchorPos(target, duration);

    public UniTask PlayDestroy(TypePiece type) =>  PieceViewAnimations.Destroy(_rt, type);

    public UniTask PlayBooster(Cell cell) => PieceViewAnimations.PlayBooster(_rt, cell);

    //public UniTask PlayShake(Cell cell) => PieceViewAnimations.Shake(_rt);
    private bool _isShaking;

    public async UniTask PlayShake(Cell cell)
    {
        await PieceViewAnimations.Shake(_rt);
    }
}