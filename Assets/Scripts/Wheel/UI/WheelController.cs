using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Systems;

namespace VertigoGames.Wheel.UI
{
    /// <summary>
    /// Handles wheel spinning logic, determines winning slice,
    /// and fires spin result event to external systems (GameManager / VFX).
    /// </summary>
    public class WheelController : MonoBehaviour
    {
        // Fired when a spin ends and we know the winning slice.
        public event Action<WheelSliceSO> OnSliceEvaluated;

        [Header("Wheel Data")]
        [SerializeField] private WheelZoneSO _currentZone;

        [Header("UI References")]
        [SerializeField] private RectTransform _wheelRoot;
        [SerializeField] private Image[] _sliceIcons;
        [SerializeField] private Image _pointer;
        [SerializeField] private Image _wheelBaseImage;
        [SerializeField] private Button _spinButton;

        [Header("Spin Settings")]
        [SerializeField] private float _wheelSpinTime = 3f;
        [SerializeField] private Vector2 _extraTurnRange = new Vector2(3, 6);
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        [Header("Pointer Settings")]
        [Tooltip("Pointer direction in degrees. Defines slice index 0 relative to wheel.")]
        [SerializeField] private float _pointerAngleOffset = 0f;

        private const int SliceCount = 8;

        // --------------------------------------------------------------------

        private void Start()
        {
            if (!ValidateRefs())
                return;

            DOTween.Init(recycleAllByDefault: true);

            // Assign spin button event
            _spinButton.onClick.RemoveAllListeners();
            _spinButton.onClick.AddListener(Spin);

            // Auto fill slice data at runtime
            WheelZoneAutoFill.FillZone(_currentZone);
            ApplyZoneData();
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// External systems (GameManager, VFX) can subscribe to spin result here.
        /// </summary>
        public void SubscribeToSpinResult(Action<WheelSliceSO> callback)
        {
            OnSliceEvaluated += callback;
        }

        /// <summary>
        /// Unsubscribe from spin result event.
        /// </summary>
        public void UnsubscribeFromSpinResult(Action<WheelSliceSO> callback)
        {
            OnSliceEvaluated -= callback;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Applies wheel base + pointer visuals based on active theme.
        /// </summary>
        public void ApplyTheme(WheelThemeSO theme)
        {
            if (theme == null)
                return;

            // Apply wheel base appearance
            if (_wheelBaseImage != null && theme.WheelBase != null)
                _wheelBaseImage.sprite = theme.WheelBase;

            // Apply pointer appearance
            if (_pointer != null && theme.PointerSprite != null)
                _pointer.sprite = theme.PointerSprite;
        }

        /// <summary>
        /// Smooth reset of wheel rotation. Called after spin or zone change.
        /// </summary>
        public void ResetWheelRotation(float duration = 0.35f)
        {
            _wheelRoot
                .DORotate(Vector3.zero, duration)
                .SetEase(Ease.OutCubic);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Ensures required UI elements are assigned.
        /// </summary>
        private bool ValidateRefs()
        {
            if (_spinButton == null)
            {
                Debug.LogError("WheelController: Spin button missing!");
                return false;
            }

            if (_wheelRoot == null)
            {
                Debug.LogError("WheelController: Wheel root missing!");
                return false;
            }

            if (_pointer == null)
            {
                Debug.LogError("WheelController: Pointer missing!");
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Updates UI slice icons based on the active zone's slice data.
        /// </summary>
        private void ApplyZoneData()
        {
            if (_currentZone == null)
            {
                Debug.LogError("WheelController: No zone assigned!");
                return;
            }

            if (_sliceIcons.Length != SliceCount)
            {
                Debug.LogError($"WheelController: SliceIcons must be {SliceCount}!");
                return;
            }

            if (_currentZone.Slices.Count != SliceCount)
            {
                Debug.LogError($"WheelController: Zone slice count invalid! Expected {SliceCount}.");
                return;
            }

            for (int i = 0; i < SliceCount; i++)
            {
                WheelSliceSO slice = _currentZone.Slices[i];
                _sliceIcons[i].sprite = slice.Icon;
                _sliceIcons[i].preserveAspect = true;
            }
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Called when the user presses the Spin button.
        /// Calculates final rotation and animates the wheel.
        /// </summary>
        private void Spin()
        {
            if (!_spinButton.interactable)
                return;

            // Lock button + press animation
            _spinButton.interactable = false;
            _spinButton.transform.DOScale(0.9f, 0.15f);

            float sliceAngle = 360f / SliceCount;

            // Select random slice
            int targetSliceIndex = UnityEngine.Random.Range(0, SliceCount);
            float targetAngle = targetSliceIndex * sliceAngle;

            // Add extra full rotations
            float extraTurns = UnityEngine.Random.Range(_extraTurnRange.x, _extraTurnRange.y);
            float totalAngle = extraTurns * 360f + targetAngle;

            _wheelRoot
                .DORotate(new Vector3(0, 0, -totalAngle), _wheelSpinTime, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .OnComplete(OnSpinComplete);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Called automatically when wheel rotation animation ends.
        /// Determines the winning slice and triggers VFX + GameManager events.
        /// </summary>
        private void OnSpinComplete()
        {
            // Delay enable for better UX feeling
            DOVirtual.DelayedCall(0.2f, () =>
            {
                _spinButton.interactable = true;
            });

            // Release button scale
            _spinButton.transform.DOScale(1f, 0.15f);


            float sliceAngle = 360f / SliceCount;
            float wheelZ = _wheelRoot.localEulerAngles.z;

            // Normalize wheel rotation relative to pointer orientation
            float normalized = Mathf.Repeat(-wheelZ + _pointerAngleOffset, 360f);

            // Find nearest slice
            int index = Mathf.FloorToInt((normalized + sliceAngle * 0.5f) / sliceAngle) % SliceCount;

            WheelSliceSO slice = _currentZone.Slices[index];

            Debug.Log($"SPIN RESULT → {slice.SliceName} (index: {index})");

            OnSliceEvaluated?.Invoke(slice);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Changes active zone, shuffles slices, resets wheel,
        /// and plays enter animations.
        /// </summary>
        public void SetZone(WheelZoneSO zone)
        {
            if (zone == null)
                return;

            _currentZone = zone;

            // Runtime fill + shuffle
            WheelZoneAutoFill.FillZone(_currentZone);
            _currentZone.ShuffleSlices();

            // Reset wheel instantly
            _wheelRoot.localEulerAngles = Vector3.zero;

            // Wheel pop animation
            _wheelRoot.localScale = Vector3.one * 0.9f;
            _wheelRoot
                .DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack);

            // Pointer shake
            _pointer.transform
                .DOShakeRotation(
                    duration: 0.25f,
                    strength: 10f,
                    vibrato: 18,
                    randomness: 90f,
                    fadeOut: true
                )
                .SetEase(Ease.OutQuad);

            // Update icon sprites
            ApplyZoneData();
        }
    }
}