using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Systems;
using VertigoGames.Wheel.UI;

namespace VertigoGames.Wheel.VFX
{
    public class WheelVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WheelController _wheelController;
        [SerializeField] private WheelGameManager _gameManager;
        [SerializeField] private Canvas _mainCanvas;

        [Header("Reward Fly Settings")]
        [SerializeField] private Transform _rewardStackRoot;  
        [SerializeField] private GameObject _rewardIconPrefab;

        [SerializeField] private ScrollRect _rewardScrollRect;

        private void Start()
        {
            _wheelController.SubscribeToSpinResult(OnSliceWon);
        }

        private void OnSliceWon(WheelSliceSO slice)
        {
            if (slice == null) return;
            if (slice.IsBomb) return;

            FlyRewardToCorner(slice);
        }

        private void FlyRewardToCorner(WheelSliceSO slice)
        {
            int index = _gameManager.GetSliceIndex(slice);
            RectTransform slot = _gameManager.IconLayout.GetSlot(index);
            RectTransform icon = slot.GetChild(0) as RectTransform;

            // ⭐ UÇUŞ CANVAS'INA EKLE (viewport'a değil)
            GameObject obj = Instantiate(_rewardIconPrefab, _mainCanvas.transform);
            RectTransform rt = obj.GetComponent<RectTransform>();
            Image img = obj.GetComponent<Image>();

            img.sprite = slice.Icon;

            // Başlangıç pozisyonu
            rt.position = icon.position;
            rt.localScale = Vector3.zero;

            // Hedef → StackRoot world pozisyonu
            Vector3 targetPos = _rewardStackRoot.TransformPoint(Vector3.zero);

            Sequence seq = DOTween.Sequence();

            seq.Append(rt.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(rt.DOMove(targetPos, 0.45f).SetEase(Ease.InOutCubic));
            seq.Append(rt.DOScale(0.65f, 0.15f));

            seq.OnComplete(() =>
            {
                // ⭐ UÇUŞ BİTTİ → STACK'e alın
                rt.SetParent(_rewardStackRoot, false);
            });
        }

        public void ResetRewardStack()
        {
            foreach (Transform child in _rewardStackRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}