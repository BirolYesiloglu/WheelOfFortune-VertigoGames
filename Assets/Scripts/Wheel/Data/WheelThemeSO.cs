using UnityEngine;

namespace VertigoGames.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelTheme", menuName = "Vertigo/Wheel Theme")]
    public class WheelThemeSO : ScriptableObject
    {
        [SerializeField] private Sprite _wheelBase;
        [SerializeField] private Sprite _pointerSprite;

        public Sprite WheelBase => _wheelBase;
        public Sprite PointerSprite => _pointerSprite;
    }
}