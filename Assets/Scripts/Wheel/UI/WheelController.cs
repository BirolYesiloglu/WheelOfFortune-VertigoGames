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

        [Header("References")]
        [SerializeField] private WheelZoneAutoFill _autoFill;

        [Header("UI References")]
        [SerializeField] private RectTransform _wheelRoot;
        [SerializeField] private Image _pointer;
        [SerializeField] private Image _wheelBaseImage;
        [SerializeField] private Button _spinButton;
        [SerializeField] private Button _exitButton;

        [Header("Spin Settings")]
        [SerializeField] private float _wheelSpinTime = 3f;
        [SerializeField] private Vector2 _extraTurnRange = new Vector2(3, 6);
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        [Header("Pointer Settings")]
        [Tooltip("Pointer direction in degrees. Defines slice index 0 relative to wheel.")]
        [SerializeField] private float _pointerAngleOffset = 0f;

        public event Action OnSpinStarted;
        public event Action OnSpinFinished;

        private const int SliceCount = 8;
        private float _lastSliceIndex = -1f;
        private float _sliceAngleSize = 360f / SliceCount;
        private Vector3 _pointerDefaultRotation;

        // --------------------------------------------------------------------

        private void Start()
        {
            if (!ValidateRefs())
                return;

            if (_autoFill == null)
            {
                Debug.LogError("WheelController: AutoFill reference missing!");
                return;
            }

            _pointerDefaultRotation = _pointer.transform.localEulerAngles;

            DOTween.Init(recycleAllByDefault: true);

            // Assign spin button event
            _spinButton.onClick.RemoveAllListeners();
            _spinButton.onClick.AddListener(Spin);

            // Auto fill slice data at runtime
            _autoFill.FillZone(_currentZone);
            ApplyZoneData();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spinButton == null)
                _spinButton = GetComponentInChildren<Button>(true);

            if (_wheelRoot == null)
                _wheelRoot = GetComponentInChildren<RectTransform>(true);

            if (_pointer == null)
                _pointer = GetComponentInChildren<Image>(true);

            if (_autoFill == null)
                _autoFill = GetComponent<WheelZoneAutoFill>();

            if (_exitButton == null)
                _exitButton = GameObject.Find("ui_button_exit")?.GetComponent<Button>();
        }
#endif

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

            if (_wheelBaseImage != null && theme.WheelBase != null)
                _wheelBaseImage.sprite = theme.WheelBase;

            if (_pointer != null && theme.PointerSprite != null)
                _pointer.sprite = theme.PointerSprite;

            // Camera background color transition
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.DOColor(theme.BackgroundColor, 0.4f)
                   .SetEase(Ease.OutQuad);
            }
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
            var layout = WheelGameManager.Instance.IconLayout;
            var zone = _currentZone;

            for (int i = 0; i < zone.Slices.Count; i++)
            {
                var slot = layout.GetSlot(i);
                if (slot.childCount == 0)
                {
                    Debug.LogError("Slot missing child icon at index " + i);
                    continue;
                }

                var icon = slot.GetChild(0).GetComponent<Image>();
                icon.sprite = zone.Slices[i].Icon;

                // 🔥 SCALE LOGIC
                if (zone.Slices[i].IsBomb)
                    icon.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1);
                else
                    icon.rectTransform.localScale = Vector3.one; // normal slice
            }
        }

        // --------------------------------------------------------------
        // MINIMAL POINTER KICK (A-LEVEL)
        // --------------------------------------------------------------
        private void PlayPointerKick()
        {
            if (_pointer == null)
                return;

            float kick = 4f;

            float targetZ = _pointerDefaultRotation.z + UnityEngine.Random.Range(-kick, kick);

            _pointer.transform.DOKill();

            _pointer.transform
                .DOLocalRotate(new Vector3(0, 0, targetZ), 0.08f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {

            _pointer.transform
                        .DOLocalRotate(_pointerDefaultRotation, 0.1f)
                        .SetEase(Ease.OutQuad);
                });
        }

        private void UpdatePointerKick()
        {
            float z = Mathf.Repeat(_wheelRoot.localEulerAngles.z, 360f);

            int sliceIndex = Mathf.FloorToInt(z / _sliceAngleSize);

            if (sliceIndex != _lastSliceIndex)
            {
                PlayPointerKick();
                _lastSliceIndex = sliceIndex;
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
            OnSpinStarted?.Invoke();
            _spinButton.transform.DOScale(0.9f, 0.15f);

            if (_exitButton != null)
                _exitButton.interactable = false;

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
                .OnUpdate(UpdatePointerKick)
                .OnComplete(OnSpinComplete);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Called automatically when wheel rotation animation ends.
        /// Determines the winning slice and triggers VFX + GameManager events.
        /// </summary>
        private void OnSpinComplete()
        {
            OnSpinFinished?.Invoke();
            _lastSliceIndex = -1f;
            // Delay enable for better UX feeling
            DOVirtual.DelayedCall(0.2f, () =>
            {
                _spinButton.interactable = true;
            });

            if (_exitButton != null)
                _exitButton.interactable = true;

            _pointer.transform.DOKill();
            _pointer.transform.localEulerAngles = _pointerDefaultRotation;

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
            _autoFill.FillZone(_currentZone);
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