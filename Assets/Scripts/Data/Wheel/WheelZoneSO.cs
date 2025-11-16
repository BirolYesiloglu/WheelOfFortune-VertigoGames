using UnityEngine;
using System.Collections.Generic;

namespace VertigoGames.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelZone", menuName = "Vertigo/Wheel Zone")]
    public class WheelZoneSO : ScriptableObject
    {
        [Header("Zone Info")]
        [SerializeField] private int _zoneIndex;
        public int ZoneIndex => _zoneIndex;

        [Header("Zone Flags")]
        [SerializeField] private bool _isSafeZone;   // 5. zone
        public bool IsSafeZone => _isSafeZone;

        [SerializeField] private bool _isSuperZone;  // 30. zone
        public bool IsSuperZone => _isSuperZone;

        [Header("Slices (Auto-Filled)")]
        [SerializeField] private List<WheelSliceSO> _slices = new List<WheelSliceSO>();
        public IReadOnlyList<WheelSliceSO> Slices => _slices;

        [Header("Notes")]
        [TextArea, SerializeField] private string _notes;
        public string Notes => _notes;


        // ---------------------------------------------------------
        // Slice Management API (for AutoFill system)
        // ---------------------------------------------------------
        public void ClearSlices()
        {
            _slices.Clear();
        }

        public void AddSlice(WheelSliceSO slice)
        {
            if (slice != null)
                _slices.Add(slice);
        }

        public void AddSlices(IEnumerable<WheelSliceSO> slices)
        {
            _slices.AddRange(slices);
        }
    }
}