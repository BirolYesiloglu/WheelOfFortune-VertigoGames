using UnityEngine;
using System.Collections.Generic;

public class WheelSliceDatabase : MonoBehaviour
{
    [Header("Point Slices")]
    [SerializeField] private List<WheelSliceSO> _pointSlices;

    [Header("Chest Slices")]
    [SerializeField] private List<WheelSliceSO> _chestSlices;

    [Header("Bomb Slice")]
    [SerializeField] private WheelSliceSO _bombSlice;

    public IReadOnlyList<WheelSliceSO> PointSlices => _pointSlices;
    public IReadOnlyList<WheelSliceSO> ChestSlices => _chestSlices;
    public WheelSliceSO BombSlice => _bombSlice;

    private static WheelSliceDatabase _instance;
    public static WheelSliceDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<WheelSliceDatabase>();
            return _instance;
        }
    }
}