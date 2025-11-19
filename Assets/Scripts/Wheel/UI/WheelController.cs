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
        private Vector3 _pointerDefaultRotation = new Vector3(0,0,270);

        // --------------------------------------------------------------------

        private void Start()
        {
            if (!ValidateRefs()) return;

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

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveAllListeners();
                _exitButton.onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            }

            // Auto fill slice data at runtime
            _autoFill.FillZone(_currentZone);
            ApplyZoneData();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spinButton == null) _spinButton = GetComponentInChildren<Button>(true);
            if (_wheelRoot == null) _wheelRoot = GetComponentInChildren<RectTransform>(true);
            if (_pointer == null) _pointer = GetComponentInChildren<Image>(true);
            if (_autoFill == null) _autoFill = GetComponent<WheelZoneAutoFill>();
            if (_exitButton == null) _exitButton = GameObject.Find("ui_button_exit")?.GetComponent<Button>();
        }
#endif

        // --------------------------------------------------------------------
        // PUBLIC API
        // --------------------------------------------------------------------

        public void SubscribeToSpinResult(Action<WheelSliceSO> callback) => OnSliceEvaluated += callback;
        public void UnsubscribeFromSpinResult(Action<WheelSliceSO> callback) => OnSliceEvaluated -= callback;

        public void ApplyTheme(WheelThemeSO theme)
        {
            if (theme == null) return;

            if (_wheelBaseImage != null && theme.WheelBase != null)
                _wheelBaseImage.sprite = theme.WheelBase;

            if (_pointer != null && theme.PointerSprite != null)
                _pointer.sprite = theme.PointerSprite;

            // Camera background logic (Specific to scene, acceptable to keep here)
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.DOColor(theme.BackgroundColor, 0.4f).SetEase(Ease.OutQuad);
            }
        }

        public void ResetWheelRotation(float duration = 0.35f)
        {
            _wheelRoot.DORotate(Vector3.zero, duration).SetEase(Ease.OutCubic);
        }

        // --------------------------------------------------------------------
        // CORE LOGIC
        // --------------------------------------------------------------------

        private void Spin()
        {
            if (!_spinButton.interactable) return;

            // Lock UI & Feedback
            _spinButton.interactable = false;
            if (_exitButton != null) _exitButton.interactable = false;

            OnSpinStarted?.Invoke();

            // CLEAN EXTENSION CALL:
            _spinButton.transform.DOPress();

            // Calculate Math
            float sliceAngle = 360f / SliceCount;
            int targetSliceIndex = UnityEngine.Random.Range(0, SliceCount);
            float targetAngle = targetSliceIndex * sliceAngle;
            float extraTurns = UnityEngine.Random.Range(_extraTurnRange.x, _extraTurnRange.y);
            float totalAngle = extraTurns * 360f + targetAngle;

            // Spin Animation
            _wheelRoot
                .DORotate(new Vector3(0, 0, -totalAngle), _wheelSpinTime, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .OnUpdate(UpdatePointerKick)
                .OnComplete(OnSpinComplete);
        }

        private void OnSpinComplete()
        {
            OnSpinFinished?.Invoke();
            _lastSliceIndex = -1f;

            // Release UI Logic
            DOVirtual.DelayedCall(0.2f, () => { _spinButton.interactable = true; });
            if (_exitButton != null) _exitButton.interactable = true;

            // Reset Visuals
            _pointer.transform.DOKill();
            _pointer.transform.localEulerAngles = _pointerDefaultRotation;

            // CLEAN EXTENSION CALL:
            _spinButton.transform.DOResetScale();

            // Math: Determine Winning Slice
            float sliceAngle = 360f / SliceCount;
            float wheelZ = _wheelRoot.localEulerAngles.z;
            float normalized = Mathf.Repeat(-wheelZ + _pointerAngleOffset, 360f);
            int index = Mathf.FloorToInt((normalized + sliceAngle * 0.5f) / sliceAngle) % SliceCount;

            WheelSliceSO slice = _currentZone.Slices[index];

            Debug.Log($"SPIN RESULT → {slice.SliceName} (index: {index})");
            OnSliceEvaluated?.Invoke(slice);
        }

        public void SetZone(WheelZoneSO zone)
        {
            if (zone == null) return;

            _currentZone = zone;

            // Data setup
            _autoFill.FillZone(_currentZone);
            _currentZone.ShuffleSlices();

            // Instant Reset
            _wheelRoot.localEulerAngles = Vector3.zero;

            // CLEAN EXTENSION CALLS:
            _wheelRoot.transform.DOPop(); // Wheel pops in

            _pointer.transform.DOKill(complete: true);

            _pointer.transform.localEulerAngles = _pointerDefaultRotation;

            _pointer.transform
                .DOPunchRotation(new Vector3(0, 0, 15f), 0.25f, 20, 1)
                .OnComplete(() =>
                {
                    _pointer.transform.localEulerAngles = _pointerDefaultRotation;
                });

            ApplyZoneData();
        }

        // --------------------------------------------------------------------
        // PRIVATE HELPERS
        // --------------------------------------------------------------------

        private void PlayPointerKick()
        {
            if (_pointer == null) return;

            // Physics-based kick logic (Domain specific, keep it here)
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

        private void ApplyZoneData()
        {
            var layout = WheelGameManager.Instance.IconLayout;
            var zone = _currentZone;

            for (int i = 0; i < zone.Slices.Count; i++)
            {
                var slot = layout.GetSlot(i);
                if (slot.childCount == 0) continue;

                var icon = slot.GetChild(0).GetComponent<Image>();
                icon.sprite = zone.Slices[i].Icon;

                // Scale Logic
                if (zone.Slices[i].IsBomb)
                    icon.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1);
                else
                    icon.rectTransform.localScale = Vector3.one;
            }
        }

        private bool ValidateRefs()
        {
            if (_spinButton == null || _wheelRoot == null || _pointer == null)
            {
                Debug.LogError("WheelController: Missing critical references!");
                return false;
            }
            return true;
        }
    }
}