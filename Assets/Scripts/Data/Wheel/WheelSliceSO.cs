using UnityEngine;

[CreateAssetMenu(fileName = "WheelSlice", menuName = "Vertigo/Wheel Slice")]
public class WheelSliceSO : ScriptableObject
{
    [SerializeField] private string _sliceName;
    public string SliceName => _sliceName;

    [Header("Visual")]
    [SerializeField] private Sprite _icon;
    public Sprite Icon => _icon;

    [Header("Reward Settings")]
    [SerializeField] private RewardType _rewardType;
    public RewardType RewardType => _rewardType;

    [SerializeField] private int _rewardValue;
    public int RewardValue => _rewardValue;

    [Header("Flags")]
    [SerializeField] private bool _isBomb;
    public bool IsBomb => _isBomb;

    [SerializeField] private bool _isSpecial;
    public bool IsSpecial => _isSpecial;

    [TextArea, SerializeField] private string _description;
    public string Description => _description;
}

public enum RewardType
{
    None,
    Points,
    Chest,
    Item,
    Currency
}