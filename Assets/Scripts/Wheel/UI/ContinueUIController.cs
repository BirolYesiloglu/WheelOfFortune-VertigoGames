using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using VertigoGames.Wheel.Systems;

namespace VertigoGames.Wheel.UI
{
    public class ContinueUIController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _goldIcon;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_panel == null)
                _panel = gameObject;

            if (_yesButton == null)
                _yesButton = transform.Find("yes_button")?.GetComponent<Button>();

            if (_noButton == null)
                _noButton = transform.Find("no_button")?.GetComponent<Button>();

            if (_priceText == null)
                _priceText = GetComponentInChildren<TMP_Text>(true);

            if (_goldIcon == null)
                _goldIcon = transform.Find("ui_gold_icon")?.GetComponent<Image>();
        }
#endif
        /// <summary>
        /// Opens the continue UI panel.
        /// </summary>
        public void Show(int price, Action onYes, Action onNo)
        {
            _panel.SetActive(true);
            _priceText.text = price.ToString();

            bool canAfford = WheelGameManager.Instance.Rewards.Score >= price;
            _yesButton.interactable = canAfford;

            // Reset listeners
            _yesButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            // Gold Icon alpha
            if (_goldIcon != null)
            {
                Color c = _goldIcon.color;
                c.a = canAfford ? 1f : 0.5f;
                _goldIcon.color = c;
            }

            if (canAfford)
            {
                _yesButton.onClick.AddListener(() =>
                {
                    Hide();
                    onYes?.Invoke();
                });
            }

            _noButton.onClick.AddListener(() =>
            {
                Hide();
                onNo?.Invoke();
            });
        }

        /// <summary>
        /// Closes the continue UI panel.
        /// </summary>
        private void Hide()
        {
            _panel.SetActive(false);
        }
    }
}