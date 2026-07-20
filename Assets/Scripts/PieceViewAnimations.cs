using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;

public static class PieceViewAnimations
{
    private static readonly Vector3 rotate360Vec = new Vector3(0f, 0f, 360);
    private static readonly Vector2 arrowpos = new Vector3(1f, 1f);
    private static readonly float timeDestroy = 0.4f;
    private static readonly float timeArrow1 = 0.5f;
    private static readonly float timeArrow2 = 0.3f;
    const float distance = 1400f;




    public static async UniTask PlayBooster(RectTransform rt, Cell cell)
    {
        switch (cell.Piece.Data.Types)
        {
            case TypePiece.Arrow:
                await BoosterArrow(rt, cell);
                break;

            case TypePiece.Bomb:
                await BoosterBomb(rt, cell);
                break;

            case TypePiece.Lightning:
                await BoosterLightning(rt, cell);
                break;
        }
    }

    private static async UniTask BoosterArrow(RectTransform rt, Cell cell)
    {
        int oldIndex = rt.GetSiblingIndex();
        rt.SetAsLastSibling();

        var direction = (ArrowDirection)Random.Range(0, 4);

        Vector2 startPos = rt.anchoredPosition;
        Vector2 teleportPos;
        Vector2 secondTarget;
        Quaternion rotation;
        bool isHorisontal;

        switch (direction)
        {
            case ArrowDirection.Up:
                teleportPos = startPos + Vector2.up * distance;
                secondTarget = startPos + Vector2.down * distance;
                rotation = Quaternion.Euler(0, 0, -135);
                isHorisontal = false;
                break;

            case ArrowDirection.Down:
                teleportPos = startPos + Vector2.down * distance;
                secondTarget = startPos + Vector2.up * distance;
                rotation = Quaternion.Euler(0, 0, 45);
                isHorisontal = false;
                break;

            case ArrowDirection.Left:
                teleportPos = startPos + Vector2.left * distance;
                secondTarget = startPos + Vector2.right * distance;
                rotation = Quaternion.Euler(0, 0, -45);
                isHorisontal = true;
                break;

            default:
                teleportPos = startPos + Vector2.right * distance;
                secondTarget = startPos + Vector2.left * distance;
                rotation = Quaternion.Euler(0, 0, 135);
                isHorisontal = true;
                break;
        }

        Sequence seq = DOTween.Sequence().SetTarget(rt).SetLink(rt.gameObject);

        seq.Append(rt.DOAnchorPos(startPos + arrowpos * distance, timeArrow1).SetEase(Ease.Linear));

        seq.AppendCallback(() =>
        {
            rt.anchoredPosition = teleportPos;
            rt.localRotation = rotation;
            rt.localScale = Vector3.one * 3f;
        });

        seq.Append(rt.DOAnchorPos(secondTarget, timeArrow2).SetEase(Ease.Linear));
        seq.AppendCallback(() =>
        {
            Controller.Instance.FindCellDestroy(cell.X, cell.Y, isHorisontal);
        });

        await seq.AsyncWaitForCompletion();

        if (rt == null || !rt.gameObject.activeInHierarchy) return;

        rt.SetSiblingIndex(oldIndex);

    }

    private static async UniTask BoosterBomb(RectTransform rt, Cell cell)
    {
        int oldIndex = rt.GetSiblingIndex();
        rt.SetAsLastSibling();

        if (rt == null)
            return;

        Tween tween = rt
            .DOScale(1.5f, 0.8f)
            .SetEase(Ease.OutBack)
            .SetTarget(rt)
            .SetLink(rt.gameObject);

        Controller.Instance.FindBombDestroy(cell.X, cell.Y);

        await tween.AsyncWaitForCompletion();

        if (rt == null || !rt.gameObject.activeInHierarchy)
            return;


        rt.localScale = Vector3.one;
        rt.SetSiblingIndex(oldIndex);

    }

    private static async UniTask BoosterLightning(RectTransform rt, Cell cell)
    {
        int oldIndex = rt.GetSiblingIndex();
        rt.SetAsLastSibling();

        Sequence seq = DOTween.Sequence().SetTarget(rt).SetLink(rt.gameObject);

        seq.Append(rt.DOScale(1.5f, 0.2f));

        seq.Append(rt.DORotate(rotate360Vec, 0.3f, RotateMode.FastBeyond360));

        await seq.AsyncWaitForCompletion();

        if (rt == null || !rt.gameObject.activeInHierarchy) return;

        await Controller.Instance.FindLightningDestroy(Controller.Instance._spawner.CreateRandomPiece());

        rt.SetSiblingIndex(oldIndex);
    }

    public static UniTask Destroy(RectTransform rt, TypePiece type)
    {
        switch (type)
        {
            case TypePiece.Resource: return DestroyResourece(rt);

            case TypePiece.Arrow: return UniTask.CompletedTask;

            case TypePiece.Bomb: return UniTask.CompletedTask;

            case TypePiece.Lightning: return UniTask.CompletedTask;
        }

        if (type == TypePiece.Resource) return DestroyResourece(rt);

        return UniTask.CompletedTask;
    }
    public static async UniTask DestroyResourece(RectTransform rt)
    {
        if (rt == null)
            return;

        Tween tween = DOTween.To(
            () => 0f,
            x =>
            {
                rt.localRotation = Quaternion.Euler(0, 0, 360 * x);
                rt.localScale = Vector3.one * (1 - x);
            },
            1f,
            timeDestroy
        )
        .SetEase(Ease.InBack)
        .SetTarget(rt)
        .SetLink(rt.gameObject);

        await tween.AsyncWaitForCompletion();

        if (rt == null)
            return;

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    public static void Reset(RectTransform rt)
    {
        rt.DOKill();

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    public static async UniTask Shake(RectTransform rt)
    {
        int oldIndex = rt.GetSiblingIndex();
        rt.SetAsLastSibling();

        Tween tween = rt
            .DORotate(new Vector3(0, 0, 20), 0.08f)
            .SetLoops(6, LoopType.Yoyo)
            .SetTarget(rt)
            .SetLink(rt.gameObject);

        await tween.AsyncWaitForCompletion();

        if (rt == null || !rt.gameObject.activeInHierarchy)
            return;

        rt.localRotation = Quaternion.identity;
        rt.SetSiblingIndex(oldIndex);
    }
}