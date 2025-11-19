using UnityEngine;
using TMPro;
using VertigoGames.Wheel.Systems;

namespace VertigoGames.Wheel.UI
{
    public class ZoneUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _zoneLabel;

        private void Start()
        {
            UpdateZoneLabel();

            WheelGameManager.Instance.OnZoneLoaded += UpdateZoneLabel;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_zoneLabel == null)
                _zoneLabel = GameObject.Find("ui_zone_value")?.GetComponent<TMP_Text>();
        }
#endif

        private void OnDestroy()
        {
            if (WheelGameManager.Instance != null)
                WheelGameManager.Instance.OnZoneLoaded -= UpdateZoneLabel;
        }

        private void UpdateZoneLabel()
        {
            int zone = WheelGameManager.Instance.CurrentLevelNumber;

            _zoneLabel.text = $"{zone}";

            // POP ANIMATION
            _zoneLabel.transform.DOPop();
        }
    }
}