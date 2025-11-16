using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Systems;

namespace VertigoGames.Wheel
{
    public class WheelController : MonoBehaviour
    {
        [Header("Wheel Data")]
        [SerializeField] private WheelZoneSO _currentZone;

        [Header("UI References")]
        [SerializeField] private RectTransform _wheelRoot;
        [SerializeField] private Image[] _sliceIcons;
        [SerializeField] private Image _pointer;
        [SerializeField] private Button _spinButton;

        [Header("Spin Settings")]
        [SerializeField] private float _wheelSpinTime = 3f;
        [SerializeField] private Vector2 _extraTurnRange = new Vector2(3, 6);
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        [Header("Pointer Settings")]
        [Tooltip("Pointer rotation offset in degrees. Determines which slice is considered index 0. " +
                 "For example: 0° = Right (3 o'clock), 90° = Up (12 o'clock), 180° = Left, 270° = Down.")]
        [SerializeField] private float _pointerAngleOffset = 0f;

        private const int SliceCount = 8;

        // -----------------------------------------------------------------------------------
        // INITIALIZATION
        // -----------------------------------------------------------------------------------
        private void Start()
        {
            Debug.Log($"[WheelController] Start - spinButton: {_spinButton}, zone: {_currentZone}");

            if (_spinButton == null)
            {
                Debug.LogError("Spin Button reference is missing in the Inspector!");
                return;
            }

            if (_wheelRoot == null)
            {
                Debug.LogError("Wheel root is missing!");
                return;
            }

            if (_pointer == null)
            {
                Debug.LogError("Pointer is missing!");
                return;
            }

            DOTween.Init(recycleAllByDefault: true);

            _spinButton.onClick.RemoveAllListeners();
            _spinButton.onClick.AddListener(Spin);

            WheelZoneAutoFill.FillZone(_currentZone);

            ApplyZoneData();
        }

        // -----------------------------------------------------------------------------------
        // DATA → UI
        // -----------------------------------------------------------------------------------
        private void ApplyZoneData()
        {
            if (_currentZone == null)
            {
                Debug.LogError("No zone assigned to WheelController!");
                return;
            }

            if (_sliceIcons.Length != SliceCount)
            {
                Debug.LogError($"WheelController: SliceIcons array must be {SliceCount}, but is {_sliceIcons.Length}");
                return;
            }

            if (_currentZone.Slices.Count != SliceCount)
            {
                Debug.LogError($"Zone slice count must be {SliceCount}, but got {_currentZone.Slices.Count}!");
                return;
            }

            for (int i = 0; i < SliceCount; i++)
            {
                var slice = _currentZone.Slices[i];
                _sliceIcons[i].sprite = slice.Icon;
                _sliceIcons[i].preserveAspect = true;
            }
        }

        // -----------------------------------------------------------------------------------
        // SPIN LOGIC
        // -----------------------------------------------------------------------------------
        private void Spin()
        {
            if (!_spinButton.interactable)
                return;

            _spinButton.interactable = false;
            _spinButton.transform.DOScale(0.9f, 0.15f);

            float sliceAngle = 360f / SliceCount;
            int targetSliceIndex = Random.Range(0, SliceCount);
            float targetAngle = targetSliceIndex * sliceAngle;

            float extraTurns = Random.Range(_extraTurnRange.x, _extraTurnRange.y);
            float totalAngle = extraTurns * 360f + targetAngle;

            _wheelRoot
                .DORotate(
                    new Vector3(0, 0, -totalAngle),
                    _wheelSpinTime,
                    RotateMode.FastBeyond360
                )
                .SetEase(_spinEase)
                .OnComplete(OnSpinComplete);
        }

        // -----------------------------------------------------------------------------------
        // SPIN COMPLETE → DETERMINE RESULT
        // -----------------------------------------------------------------------------------
        private void OnSpinComplete()
        {
            _spinButton.interactable = true;
            _spinButton.transform.DOScale(1f, 0.15f);

            float sliceAngle = 360f / SliceCount;

            float z = _wheelRoot.localEulerAngles.z;

            float normalized = Mathf.Repeat(-z + _pointerAngleOffset, 360f);

            int index = Mathf.FloorToInt((normalized + sliceAngle * 0.5f) / sliceAngle) % SliceCount;

            var slice = _currentZone.Slices[index];
            Debug.Log($"SPIN RESULT → {slice.SliceName} (index: {index}, angle: {normalized})");

            EvaluateSlice(slice);
        }

        // -----------------------------------------------------------------------------------
        // RESULT LOGIC
        // -----------------------------------------------------------------------------------
        private void EvaluateSlice(WheelSliceSO slice)
        {
            if (slice.IsBomb)
            {
                Debug.Log("BOMB → All rewards lost.");
                return;
            }

            switch (slice.RewardType)
            {
                case RewardType.Points:
                    Debug.Log($"POINTS → +{slice.RewardValue}");
                    break;

                case RewardType.Chest:
                    Debug.Log($"CHEST → Tier {slice.RewardValue}");
                    break;
            }

            if (_currentZone.IsSafeZone)
                Debug.Log("SAFE ZONE → Progress protected.");

            if (_currentZone.IsSuperZone)
                Debug.Log("SUPER ZONE → Massive rewards!");
        }
    }
}