using UnityEngine;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Layout;
using VertigoGames.Wheel.UI;
using VertigoGames.Wheel.VFX;

namespace VertigoGames.Wheel.Systems
{
    public class WheelGameManager : MonoBehaviour
    {
        public static WheelGameManager Instance { get; private set; }

        [Header("Zone Settings")]
        [SerializeField] private WheelZoneSO[] _zones;

        [Header("Themes")]
        [SerializeField] private WheelThemeSO _bronzeTheme;
        [SerializeField] private WheelThemeSO _silverTheme;
        [SerializeField] private WheelThemeSO _goldTheme;

        [Header("References")]
        [SerializeField] private WheelController _wheelController;
        [SerializeField] private WheelIconLayout _iconLayout;
        [SerializeField] private ContinueSystem _continueSystem;
        [SerializeField] private ContinueUIController _continueUI;
        [SerializeField] private RewardManager _rewardManager;
        [SerializeField] private WheelVFXController _vfx;

        public WheelIconLayout IconLayout => _iconLayout;
        public RewardManager Rewards => _rewardManager;

        private bool _isPaused = false;
        public void PauseZoneLoad() => _isPaused = true;
        public void ResumeZoneLoad() => _isPaused = false;

        private int _currentZoneIndex = 0;
        public int GetCurrentZoneIndex => _currentZoneIndex;
        private int _currentLevelNumber = 1;
        public int CurrentLevelNumber => _currentLevelNumber; // To hide loop

        public event System.Action OnZoneLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_wheelController == null)
            {
                Debug.LogError("WheelGameManager: Missing WheelController reference!");
                return;
            }

            if (_zones == null || _zones.Length == 0)
            {
                Debug.LogError("WheelGameManager: No zones assigned!");
                return;
            }

            _vfx.PlayWheelGlowIdle();

            LoadZone(_currentZoneIndex);
            _wheelController.SubscribeToSpinResult(HandleSliceResult);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_wheelController == null)
                _wheelController = FindObjectOfType<WheelController>();

            if (_iconLayout == null)
                _iconLayout = FindObjectOfType<WheelIconLayout>();

            if (_rewardManager == null)
                _rewardManager = FindObjectOfType<RewardManager>();

            if (_vfx == null)
                _vfx = FindObjectOfType<WheelVFXController>();

            if (_continueSystem == null)
                _continueSystem = FindObjectOfType<ContinueSystem>();

            if (_continueUI == null)
                _continueUI = FindObjectOfType<ContinueUIController>();
        }
#endif

        private void OnDestroy()
        {
            if (Instance == this && _wheelController != null)
            {
                _wheelController.UnsubscribeFromSpinResult(HandleSliceResult);
            }
        }

        public WheelZoneSO CurrentZone => _zones[_currentZoneIndex];

        public int GetSliceIndex(WheelSliceSO slice)
        {
            var zone = CurrentZone;

            for (int i = 0; i < zone.Slices.Count; i++)
            {
                if (zone.Slices[i] == slice)
                    return i;
            }

            return -1;
        }

        private void LoadZone(int index)
        {
            if (index < 0 || index >= _zones.Length)
                return;

            WheelZoneSO zone = _zones[index];

            // Set zone data
            _wheelController.SetZone(zone);

            // Calculate theme once
            WheelThemeSO theme = GetThemeForZone(index);

            _wheelController.ApplyTheme(theme);

            // Zone VFX
            if (theme == _goldTheme)
                _vfx.PlayGoldTransition();
            else if (theme == _silverTheme)
                _vfx.PlaySilverTransition();

            // Reset wheel rotation visually
            _wheelController.ResetWheelRotation(0.25f);

            Debug.Log($"ZONE LOADED → {index + 1}/{_zones.Length}");

            OnZoneLoaded?.Invoke();
        }

        private WheelThemeSO GetThemeForZone(int index)
        {
            int zoneNumber = index + 1;

            if (zoneNumber >= 30)
                return _goldTheme;

            if (zoneNumber % 5 == 0)
                return _silverTheme;

            return _bronzeTheme;
        }

        public void GoToZone1()
        {
            _currentZoneIndex = 0;
            _currentLevelNumber = 1;
            LoadZone(0);
        }

        private void HandleSliceResult(WheelSliceSO slice)
        {
            if (_isPaused) return;

            WheelZoneSO currentZone = CurrentZone;

            // -----------------------------
            // BOMB CASE
            // -----------------------------
            if (slice.IsBomb)
            {
                if (currentZone.IsSafeZone)
                {
                    Debug.Log("SAFE ZONE → Bomb ignored");
                }
                else
                {
                    Debug.Log("BOMB → Continue screen");

                    PauseZoneLoad();

                    _continueUI.Show(
                        _continueSystem.CurrentPrice,

                        // YES (Continue)
                        () =>
                        {
                            Debug.Log("CONTINUE accepted");

                            int price = _continueSystem.CurrentPrice;

                            if (!_rewardManager.TrySpendScore(price))
                            {
                                Debug.Log("NOT ENOUGH SCORE → Continue cannot be used!");
                                return;
                            }
                            _continueSystem.IncrementContinueCount();

                            // 3) REFRESH current wheel (shuffle + rotation reset)
                            // Shuffle active zone
                            CurrentZone.ShuffleSlices();

                            // Reset wheel rotation
                            _wheelController.ResetWheelRotation(0f);

                            // Reapply wheel theme (optional)
                            _wheelController.ApplyTheme(GetThemeForZone(_currentZoneIndex));

                            // Reapply zone data to wheel using layout (UI icon update)
                            _wheelController.SetZone(CurrentZone);

                            // 4) Resume game
                            ResumeZoneLoad();
                        },

                        // NO (Exit – full reset)
                        () =>
                        {
                            Debug.Log("CONTINUE declined → Full reset");

                            _rewardManager.ResetRewards();
                            _vfx.ResetRewardStack();

                            _continueSystem.ResetContinueCycle();

                            _currentZoneIndex = 0;
                            ResumeZoneLoad();
                            LoadZone(0);
                        }
                    );

                    return;
                }
            }
            else
            {
                _rewardManager.AddReward(slice);
                _vfx.PlayRewardEffect(slice);

                // Progress zone index
                _currentZoneIndex++;
                _currentLevelNumber++;

                if (_currentZoneIndex >= _zones.Length)
                {
                    // Loop back to Zone 1
                    _currentZoneIndex = 0;
                }
            }
            LoadZone(_currentZoneIndex);
        }
    }
}