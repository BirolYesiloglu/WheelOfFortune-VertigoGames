using UnityEngine;

namespace VertigoGames.Wheel.Layout
{
    /// <summary>
    /// Handles procedural layout of wheel slice icons.
    /// Ensures 8 slot objects exist and positions them in a perfect circle.
    /// Icons do NOT rotate; only the wheel rotates at runtime.
    /// </summary>
    [ExecuteAlways]
    public class WheelIconLayout : MonoBehaviour
    {
        private const int SliceCount = 8;

        [Header("Wheel Settings")]
        [Tooltip("Wheel root RectTransform (not rotated; wheelRoot is rotated by WheelController).")]
        [SerializeField] private RectTransform _wheelRoot;

        [Tooltip("Distance from wheel center to slice icons.")]
        [SerializeField] private float _radius = 140f;

        [Header("Prefabs / References")]
        [Tooltip("Prefab for each slice slot (contains child icon placeholder).")]
        [SerializeField] private RectTransform _slicePrefab;

        [Tooltip("Parent transform for generated slice slots.")]
        [SerializeField] private Transform _sliceRoot;

        [Header("Generated Slots (auto)")]
        [SerializeField] private RectTransform[] _iconSlots = new RectTransform[SliceCount];

        // --------------------------------------------------------------------
        /// <summary>
        /// Returns RectTransform of a slot by its index (0–7).
        /// Used by VFX to highlight the winning slice visually.
        /// </summary>
        public RectTransform GetSlot(int index)
        {
            if (index < 0 || index >= _iconSlots.Length)
                return null;

            return _iconSlots[index];
        }

        // --------------------------------------------------------------------

        [ContextMenu("Generate + Recalculate Icon Slots")]
        public void Recalculate()
        {
            if (!EnsureSlots())
                return;

            PositionSlots();
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Creates slot objects if missing.
        /// Ensures exactly 8 slots exist.
        /// </summary>
        private bool EnsureSlots()
        {
            if (_sliceRoot == null)
            {
                Debug.LogError("WheelIconLayout: sliceRoot is missing!");
                return false;
            }

            if (_slicePrefab == null)
            {
                Debug.LogError("WheelIconLayout: slicePrefab is missing!");
                return false;
            }

            for (int i = 0; i < SliceCount; i++)
            {
                if (_iconSlots[i] == null)
                {
                    RectTransform newSlot = Instantiate(_slicePrefab, _sliceRoot);
                    newSlot.name = $"slot_{i}";
                    newSlot.localScale = Vector3.one;
                    newSlot.localRotation = Quaternion.identity;

                    _iconSlots[i] = newSlot;
                }
            }

            return true;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Positions the 8 slice icons around the wheel in perfect circle layout.
        /// Icons stay upright (no rotation applied).
        /// </summary>
        private void PositionSlots()
        {
            float sliceAngle = 360f / SliceCount;

            for (int i = 0; i < SliceCount; i++)
            {
                RectTransform slot = _iconSlots[i];
                if (slot == null)
                    continue;

                float angle = i * sliceAngle;
                float rad = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(rad) * _radius;
                float y = Mathf.Sin(rad) * _radius;

                // Just position
                slot.anchoredPosition = new Vector2(x, y);
                slot.localRotation = Quaternion.identity;

                // Keep icon fully upright
                if (slot.childCount > 0)
                {
                    RectTransform icon = slot.GetChild(0) as RectTransform;

                    if (icon != null)
                    {
                        icon.localRotation = Quaternion.identity;
                        icon.localScale = Vector3.one;
                    }
                }
            }

#if UNITY_EDITOR
            Debug.Log("WheelIconLayout: Slots generated + positioned successfully.");
#endif
        }
    }
}