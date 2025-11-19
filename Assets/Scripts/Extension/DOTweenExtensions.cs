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

    public static void DOPress(this Transform target, float pressScale = 0.9f, float duration = 0.1f)
    {
        target.DOKill(true);
        target.DOScale(pressScale, duration).SetEase(Ease.OutQuad);
    }
    public static void DOResetScale(this Transform target, float duration = 0.1f)
    {
        target.DOKill(true);
        target.DOScale(1f, duration).SetEase(Ease.OutBack);
    }

    public static void DOShakeImpulse(this Transform target, float strength = 10f)
    {
        target.DOKill(complete: false);

        target.DOPunchRotation(new Vector3(0, 0, strength), 0.25f, 20, 1);
    }
}