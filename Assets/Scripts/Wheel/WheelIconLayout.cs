using UnityEngine;

[ExecuteAlways]
public class WheelIconLayout : MonoBehaviour
{
    [Header("Wheel Settings")]
    public RectTransform wheelRoot;
    public float radius = 140f;

    [Header("Slice Icons (8 items)")]
    public RectTransform[] iconSlots = new RectTransform[8];

    [ContextMenu("Recalculate Icon Positions")]
    public void Recalculate()
    {
        // Spin has 8 slots that's why length should be 8
        if (iconSlots == null || iconSlots.Length != 8)
        {
            Debug.LogError("IconSlots must contain exactly 8 elements!");
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            if (iconSlots[i] == null)
                continue;

            float angle = i * (360f / 8f); // 45 degrees per slice
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            iconSlots[i].anchoredPosition = new Vector2(x, y);
            iconSlots[i].localRotation = Quaternion.identity;
        }

        Debug.Log("Wheel slice icon positions recalculated.");
    }

    private void OnValidate()
    {
        //Recalculate();
    }
}