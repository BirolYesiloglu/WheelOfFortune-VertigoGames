using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoGames.Wheel.Data;

namespace VertigoGames.Wheel.Systems
{
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private int _score = 0;
        public int Score => _score;

        private List<WheelSliceSO> _earnedItems = new List<WheelSliceSO>();
        public IReadOnlyList<WheelSliceSO> EarnedItems => _earnedItems;
        public event System.Action<int> OnScoreChanged;
        public event Action OnRewardAdded;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Load saved total score
            _score = PlayerPrefs.GetInt("total_score", 0);
        }

        public void AddReward(WheelSliceSO slice)
        {
            _earnedItems.Add(slice);
            Debug.Log($"Item Added: {slice.SliceName}");
            OnRewardAdded?.Invoke();
        }

        public bool TrySpendScore(int amount)
        {
            if (_score < amount)
                return false;

            _score -= amount;

            PlayerPrefs.SetInt("total_score", _score);
            PlayerPrefs.Save();

            OnScoreChanged?.Invoke(_score);
            return true;
        }

        public void SpendScore(int amount)
        {
            _score -= amount;
            if (_score < 0) _score = 0;

            PlayerPrefs.SetInt("total_score", _score);
            PlayerPrefs.Save();

            OnScoreChanged?.Invoke(_score);
        }

        public int ConvertAllRewardsToScore()
        {
            int gained = 0;
            foreach (var slice in _earnedItems)
                gained += slice.RewardValue;

            _score += gained;

            PlayerPrefs.SetInt("total_score", _score);  // SAVE SCORE
            PlayerPrefs.Save();

            _earnedItems.Clear();
            OnScoreChanged?.Invoke(_score);

            return gained;
        }

        public void ResetRewards()
        {
            _earnedItems.Clear();
            OnScoreChanged?.Invoke(_score);
        }
    }
}