using UnityEngine;
using DG.Tweening;

public static class DOTweenExtensions
{
    public static void DOPop(this Transform target, float scaleAmount = 1.25f, float duration = 0.15f)
    {
        target.DOKill(true); 

        DOTween.Sequence()
            .Append(target.DOScale(scaleAmount, duration).SetEase(Ease.OutQuad))
            .Append(target.DOScale(1f, duration).SetEase(Ease.OutBack));
    }
}