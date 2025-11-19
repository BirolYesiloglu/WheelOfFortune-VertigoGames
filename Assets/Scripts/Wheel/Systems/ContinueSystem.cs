using UnityEngine;

namespace VertigoGames.Wheel.Systems
{
    /// <summary>
    /// Stores continue-cycle logic (no Singleton, no static access).
    /// WheelGameManager controls when to increment/reset.
    /// </summary>
    public class ContinueSystem : MonoBehaviour
    {
        [Header("Continue Settings")]
        [SerializeField] private int _basePrice = 100;

        // How many continues have been used in the CURRENT run
        private int _continueCount = 0;

        /// <summary>
        /// Current continue price (e.g. 100 → 200 → 300...)
        /// </summary>
        public int CurrentPrice => _basePrice * (_continueCount + 1);

        /// <summary>
        /// Called when a bomb happens and player chooses to continue.
        /// </summary>
        public void IncrementContinueCount()
        {
            _continueCount++;
        }

        /// <summary>
        /// Called when the player restarts the entire run.
        /// </summary>
        public void ResetContinueCycle()
        {
            _continueCount = 0;
        }
    }
}