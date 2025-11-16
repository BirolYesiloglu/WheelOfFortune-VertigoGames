using UnityEngine;

namespace VertigoGames.Wheel.Layout
{
    [ExecuteAlways]
    public class WheelIconLayout : MonoBehaviour
    {
        [Header("Wheel Settings")]
        [SerializeField] private RectTransform _wheelRoot;
        [SerializeField] private float _radius = 140f;

        [Header("Slice Icons (8 items)")]
        [SerializeField] private RectTransform[] _iconSlots = new RectTransform[8];

        [ContextMenu("Recalculate Icon Positions")]
        public void Recalculate()
        {
            if (_iconSlots == null || _iconSlots.Length != 8)
            {
                Debug.LogError("IconSlots must contain exactly 8 elements!");
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                if (_iconSlots[i] == null)
                    continue;

                float angle = i * (360f / 8f);
                float rad = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(rad) * _radius;
                float y = Mathf.Sin(rad) * _radius;

                _iconSlots[i].anchoredPosition = new Vector2(x, y);
                _iconSlots[i].localRotation = Quaternion.identity;
            }

            Debug.Log("Wheel slice icon positions recalculated.");
        }
    }
}