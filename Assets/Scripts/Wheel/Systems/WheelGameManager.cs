using UnityEngine;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Layout;
using VertigoGames.Wheel.UI;
using VertigoGames.Wheel.VFX;

namespace VertigoGames.Wheel.Systems
{
    /// <summary>
    /// Controls the entire wheel progression loop:
    /// - Tracks current zone
    /// - Applies themes
    /// - Handles bomb / safe logic
    /// - Subscribes to wheel spin results
    /// </summary>
    public class WheelGameManager : MonoBehaviour
    {
        /// <summary>
        /// Global access point. Lives across scenes.
        /// </summary>
        public static WheelGameManager Instance { get; private set; }

        [Header("Zone Settings")]
        [Tooltip("All wheel zones (from Zone 1 to Zone 30). Must match the progression order.")]
        [SerializeField] private WheelZoneSO[] _zones;

        [Header("Themes")]
        [SerializeField] private WheelThemeSO _bronzeTheme;
        [SerializeField] private WheelThemeSO _silverTheme;
        [SerializeField] private WheelThemeSO _goldTheme;

        [Header("References")]
        [SerializeField] private WheelController _wheelController;

        [SerializeField] private WheelIconLayout _iconLayout;
        public WheelIconLayout IconLayout => _iconLayout;
        public WheelController Wheel => _wheelController;

        private bool _isPaused = false;

        public void PauseZoneLoad() => _isPaused = true;
        public void ResumeZoneLoad() => _isPaused = false;


        /// <summary>
        /// Current active zone index.
        /// </summary>
        private int _currentZoneIndex = 0;

        // =======================================================================
        //  LIFECYCLE
        // =======================================================================
        private void Awake()
        {
            // Basic Singleton
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
                Debug.LogError("WheelGameManager: WheelController reference is missing!");
                return;
            }

            if (_zones == null || _zones.Length == 0)
            {
                Debug.LogError("WheelGameManager: No zones assigned!");
                return;
            }

            // FIRST ZONE LOAD
            LoadZone(_currentZoneIndex);

            // Subscribe to wheel spin results
            _wheelController.SubscribeToSpinResult(HandleSliceResult);
        }

        private void OnDestroy()
        {
            // Unsubscribe when destroyed
            if (Instance == this && _wheelController != null)
            {
                _wheelController.UnsubscribeFromSpinResult(HandleSliceResult);
            }
        }

        // =======================================================================
        //  PUBLIC HELPERS
        // =======================================================================

        /// <summary>
        /// Returns the index of a slice inside the current zone.
        /// Used by VFXController to know which slot to highlight.
        /// </summary>
        public int GetSliceIndex(WheelSliceSO slice)
        {
            WheelZoneSO currentZone = _zones[_currentZoneIndex];

            for (int i = 0; i < currentZone.Slices.Count; i++)
            {
                if (currentZone.Slices[i] == slice)
                    return i;
            }

            return -1;
        }


        /// <summary>
        /// Public getter for currently active zone.
        /// </summary>
        public WheelZoneSO CurrentZone => _zones[_currentZoneIndex];

        // =======================================================================
        //  ZONE / THEME HANDLING
        // =======================================================================

        /// <summary>
        /// Loads the zone data + theme for the given index.
        /// </summary>
        private void LoadZone(int index)
        {
            if (index < 0 || index >= _zones.Length)
                return;

            WheelZoneSO zone = _zones[index];

            // 1) Set zone (shuffle + animations inside)
            _wheelController.SetZone(zone);

            // 2) Apply theme (bronze/silver/gold)
            WheelThemeSO theme = GetThemeForZone(index);
            _wheelController.ApplyTheme(theme);

            _wheelController.ResetWheelRotation(0.25f);

            Debug.Log($"ZONE LOADED → {index + 1}/{_zones.Length}");
        }

        /// <summary>
        /// Determines which theme should be used based on the zone number.
        /// </summary>
        private WheelThemeSO GetThemeForZone(int index)
        {
            int zoneNumber = index + 1;

            if (zoneNumber >= 30)
                return _goldTheme;

            if (zoneNumber % 5 == 0)
                return _silverTheme;

            return _bronzeTheme;
        }

        // =======================================================================
        //  GAME LOOP LOGIC
        // =======================================================================

        /// <summary>
        /// Called every time the wheel finishes spinning.
        /// Handles bomb logic, safe zones and zone progression.
        /// </summary>
        private void HandleSliceResult(WheelSliceSO slice)
        {
            WheelZoneSO currentZone = _zones[_currentZoneIndex];

            if (_isPaused) return;

            // ---------------------------------------------------------
            // BOMB LOGIC
            // ---------------------------------------------------------
            if (slice.IsBomb)
            {
                if (currentZone.IsSafeZone)
                {
                    Debug.Log("SAFE ZONE → Bomb ignored!");
                }
                else
                {
                    Debug.Log("BOMB → Reset to Zone 1");

                    // ⭐ VFX stack reset
                    FindObjectOfType<WheelVFXController>().ResetRewardStack();

                    _currentZoneIndex = 0;
                    LoadZone(_currentZoneIndex);
                    return;
                }
            }
            else
            {
                // ---------------------------------------------------------
                // NORMAL PROGRESSION
                // ---------------------------------------------------------
                _currentZoneIndex++;

                if (_currentZoneIndex >= _zones.Length)
                    _currentZoneIndex = _zones.Length - 1;
            }

            LoadZone(_currentZoneIndex);
        }
    }
}