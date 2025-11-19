using UnityEngine;
using UnityEngine.UI;
using VertigoGames.Wheel.Systems;
using VertigoGames.Wheel.VFX;

namespace VertigoGames.Wheel.UI
{
    /// <summary>
    /// Controls the Cash Out button logic:
    /// - Enabled ONLY in safe/super zones
    /// - Disabled while spinning
    /// - Disabled while processing
    /// - Disabled if no earned rewards exist
    /// </summary>
    public class CashOutUIController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button _cashOutButton;

        [Header("References")]
        [SerializeField] private RewardManager _rewardManager;
        [SerializeField] private WheelVFXController _vfx;

        private bool _isSpinning = false;
        private bool _isProcessing = false;

        // --------------------------------------------------------------------

        private void Awake()
        {
            if (_cashOutButton != null)
            {
                _cashOutButton.onClick.RemoveAllListeners();
                _cashOutButton.onClick.AddListener(OnCashOutPressed);
            }
        }

        private void Start()
        {
            // Subscribe to wheel spin events
            var wheel = FindObjectOfType<WheelController>();
            if (wheel != null)
            {
                wheel.OnSpinStarted += HandleSpinStarted;
                wheel.OnSpinFinished += HandleSpinFinished;
            }

            // Subscribe to zone change event
            var gm = WheelGameManager.Instance;
            if (gm != null)
            {
                gm.OnZoneLoaded += RefreshInteractable;
            }

            // Subscribe to reward-added event
            if (_rewardManager != null)
                _rewardManager.OnRewardAdded += RefreshInteractable;

            RefreshInteractable();
        }

        private void OnDestroy()
        {
            // Safely unsubscribe from all events
            var wheel = FindObjectOfType<WheelController>();
            if (wheel != null)
            {
                wheel.OnSpinStarted -= HandleSpinStarted;
                wheel.OnSpinFinished -= HandleSpinFinished;
            }

            var gm = WheelGameManager.Instance;
            if (gm != null)
            {
                gm.OnZoneLoaded -= RefreshInteractable;
            }

            if (_rewardManager != null)
                _rewardManager.OnRewardAdded -= RefreshInteractable;
        }

        // --------------------------------------------------------------------
        // Editor Auto-Reference
        // --------------------------------------------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_cashOutButton == null)
                _cashOutButton = GetComponent<Button>();

            if (_rewardManager == null)
                _rewardManager = FindObjectOfType<RewardManager>();

            if (_vfx == null)
                _vfx = FindObjectOfType<WheelVFXController>();
        }
#endif

        // --------------------------------------------------------------------
        // Spin Events
        // --------------------------------------------------------------------

        private void HandleSpinStarted()
        {
            _isSpinning = true;
            RefreshInteractable();
        }

        private void HandleSpinFinished()
        {
            _isSpinning = false;
            RefreshInteractable();
        }

        // --------------------------------------------------------------------
        // Main Logic - Controls Button Interactability
        // --------------------------------------------------------------------

        /// <summary>
        /// Updates whether the Cash Out button should be interactable.
        /// </summary>
        private void RefreshInteractable()
        {
            var gm = WheelGameManager.Instance;

            if (gm == null || _cashOutButton == null)
                return;

            bool zoneAllows =
                gm.CurrentZone.IsSafeZone ||
                gm.CurrentZone.IsSuperZone;

            bool hasRewards =
                _rewardManager != null &&
                _rewardManager.EarnedItems.Count > 0;

            // Final condition
            bool canClick =
                zoneAllows &&
                hasRewards &&
                !_isSpinning &&
                !_isProcessing;

            _cashOutButton.interactable = canClick;
        }

        // --------------------------------------------------------------------
        // Cash Out Action
        // --------------------------------------------------------------------

        /// <summary>
        /// Called when the Cash Out button is pressed.
        /// Converts all earned rewards to score, resets the VFX stack,
        /// and returns the game to Zone 1.
        /// </summary>
        private void OnCashOutPressed()
        {
            // Prevent double-clicks or spam
            if (_isProcessing)
                return;

            _isProcessing = true;
            _cashOutButton.interactable = false;

            int gained = _rewardManager.ConvertAllRewardsToScore();
            _vfx.ResetRewardStack();

            Debug.Log("Cash Out! +" + gained + " points");

            // Return to Zone 1
            WheelGameManager.Instance.GoToZone1();

            _isProcessing = false;
            RefreshInteractable();
        }
    }
}