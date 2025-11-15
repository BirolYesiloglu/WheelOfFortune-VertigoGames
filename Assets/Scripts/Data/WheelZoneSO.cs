using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WheelZone", menuName = "Vertigo/Wheel Zone")]
public class WheelZoneSO : ScriptableObject
{
    [Header("Zone Info")]
    public int zoneIndex;

    [Header("Zone Flags")]
    public bool isSafeZone;   // 5. zone
    public bool isSuperZone;  // 30. zone

    [Header("Slices")]
    public List<WheelSliceSO> slices = new List<WheelSliceSO>();

    [TextArea]
    public string notes;
}