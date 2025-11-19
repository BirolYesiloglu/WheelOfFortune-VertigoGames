using TMPro;
using UnityEngine;
using DG.Tweening;
using VertigoGames.Wheel.Systems;

namespace VertigoGames.Wheel.UI
{
    public class ScoreUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;

        private void Start()
        {
            var rewards = WheelGameManager.Instance.Rewards;

            _scoreText.text = rewards.Score.ToString();

            rewards.OnScoreChanged += HandleScoreChanged;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_scoreText == null)
                _scoreText = GetComponentInChildren<TMPro.TMP_Text>(true);
        }
#endif

        private void OnDestroy()
        {
            var rewards = WheelGameManager.Instance.Rewards;
            rewards.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            _scoreText.text = newScore.ToString();

            // POP ANIMATION
            _scoreText.transform.DOKill();
            _scoreText.transform.localScale = Vector3.one;

            _scoreText.transform
                .DOScale(1.25f, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _scoreText.transform
                        .DOScale(1f, 0.15f)
                        .SetEase(Ease.OutBack);
                });
        }
    }
}