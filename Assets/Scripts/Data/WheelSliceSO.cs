using UnityEngine;

[CreateAssetMenu(fileName = "WheelSlice", menuName = "Vertigo/Wheel Slice")]
public class WheelSliceSO : ScriptableObject
{
    [Header("Visual")]
    public Sprite icon;

    [Header("Reward Settings")]
    public RewardType rewardType;
    public int rewardValue;

    [Header("Flags")]
    public bool isBomb;
    public bool isSpecial;

    [TextArea]
    public string description;
}

public enum RewardType
{
    None,
    Points,
    Chest,
    Item,
    Currency
}