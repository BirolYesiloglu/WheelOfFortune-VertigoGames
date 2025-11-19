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
                _scoreText = GameObject.Find("ui_score_value")?.GetComponent<TMP_Text>();
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
            _scoreText.transform.DOPop();
        }
    }
}