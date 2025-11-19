using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VertigoGames.Wheel.Data;
using VertigoGames.Wheel.Systems;
using TMPro;

namespace VertigoGames.Wheel.VFX
{
    public class WheelVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WheelGameManager _gameManager;
        [SerializeField] private Canvas _mainCanvas;    
        [SerializeField] private Image _fxStarFlash;
        [SerializeField] private Image _starGlowAlpha;

        [Header("Reward Fly Settings")]
        [SerializeField] private RectTransform _rewardStackRoot;
        [SerializeField] private GameObject _rewardIconPrefab;
        [SerializeField] private float _iconWidth = 100f;
        [SerializeField] private float _iconSpacing = 10f;

        [SerializeField] private ScrollRect _rewardScrollRect;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_gameManager == null)
                _gameManager = FindObjectOfType<WheelGameManager>();

            if (_mainCanvas == null)
                _mainCanvas = FindObjectOfType<Canvas>();

            if (_rewardStackRoot == null)
                _rewardStackRoot = GetComponentInChildren<RectTransform>(true);

            if (_rewardScrollRect == null)
                _rewardScrollRect = GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);

            if (_fxStarFlash == null)
                _fxStarFlash = GameObject.Find("ui_star_flash")?.GetComponent<Image>();

            if (_starGlowAlpha == null)
                _starGlowAlpha = GameObject.Find("ui_star_glow_alpha")?.GetComponent<Image>();
        }
#endif
        public void PlayWheelGlowIdle()
        {
            if (_starGlowAlpha == null)
                return;

            _starGlowAlpha.DOKill();

            Color c = _starGlowAlpha.color;
            _starGlowAlpha.color = new Color(c.r, c.g, c.b, 0.25f);

            _starGlowAlpha
                .DOFade(0.45f, 1.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void PlaySilverTransition()
        {
            // Light flash
            _fxStarFlash.color = new Color(1, 1, 1, 0);
            _fxStarFlash.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(_fxStarFlash.DOFade(0.5f, 0.15f))
                .Append(_fxStarFlash.DOFade(0f, 0.25f))
                .OnComplete(() => _fxStarFlash.gameObject.SetActive(false));

            // Soft glow burst
            PlaySilverGlowBurst();
        }

        private void PlaySilverGlowBurst()
        {
            if (_starGlowAlpha == null) return;

            _starGlowAlpha.gameObject.SetActive(true);

            // Start state
            _starGlowAlpha.color = new Color(1, 1, 1, 0);
            _starGlowAlpha.transform.localScale = Vector3.one * 0.3f;

            Sequence glow = DOTween.Sequence();

            glow.Append(
                _starGlowAlpha.DOFade(0.45f, 0.25f)    
            );

            glow.Join(
                _starGlowAlpha.transform.DOScale(1.1f, 0.45f)
                    .SetEase(Ease.OutCubic)           
            );

            glow.Append(
                _starGlowAlpha.DOFade(0f, 0.4f)      
            );

            glow.OnComplete(() =>
            {
                _starGlowAlpha.gameObject.SetActive(false);
            });
        }

        public void PlayGoldTransition()
        {
            // FULL SCREEN FLASH
            _fxStarFlash.color = new Color(1, 1, 1, 0);
            _fxStarFlash.gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(_fxStarFlash.DOFade(1f, 0.1f))
                .Append(_fxStarFlash.DOFade(0f, 0.4f))
                .OnComplete(() => _fxStarFlash.gameObject.SetActive(false));

            // GLOW BURST
            PlayGoldGlowBurst();
        }

        private void PlayGoldGlowBurst()
        {
            if (_starGlowAlpha == null) return;

            _starGlowAlpha.gameObject.SetActive(true);

            // Reset state
            _starGlowAlpha.color = new Color(1, 1, 1, 0);
            _starGlowAlpha.transform.localScale = Vector3.one * 0.2f;

            Sequence glow = DOTween.Sequence();

            glow.Append(
                _starGlowAlpha.DOFade(0.85f, 0.25f)    
            );

            glow.Join(
                _starGlowAlpha.transform.DOScale(1.6f, 0.45f)
                    .SetEase(Ease.OutCubic)            
            );

            glow.Append(
                _starGlowAlpha.DOFade(0f, 0.45f)       
            );

            glow.OnComplete(() =>
            {
                _starGlowAlpha.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Called by GameManager AFTER reward logic & progression.
        /// </summary>
        public void PlayRewardEffect(WheelSliceSO slice)
        {
            if (slice == null) return;
            if (slice.IsBomb) return;

            FlyRewardToCorner(slice);
        }

        // --------------------------------------------------------------
        //  REWARD FLY ANIMATION
        // --------------------------------------------------------------
        private void FlyRewardToCorner(WheelSliceSO slice)
        {
            int index = _gameManager.GetSliceIndex(slice);
            if (index < 0)
            {
                Debug.LogWarning("VFX: Slice not found in current zone, skipping effect.");
                return;
            }

            RectTransform slot = _gameManager.IconLayout.GetSlot(index);
            if (slot == null)
            {
                Debug.LogWarning("VFX: Slot is null for index " + index);
                return;
            }

            if (slot.childCount == 0)
            {
                Debug.LogError("VFX: Slot has no child icon! " + slot.name);
                return;
            }

            RectTransform icon = slot.GetChild(0) as RectTransform;

            GameObject obj = Instantiate(_rewardIconPrefab, _mainCanvas.transform);
            RectTransform rt = obj.GetComponent<RectTransform>();
            Image img = obj.GetComponent<Image>();
            img.sprite = slice.Icon;

            // Start at wheel icon
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _mainCanvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, icon.position),
            null,
            out Vector3 worldPos);

            rt.position = worldPos;
            rt.localScale = Vector3.zero;

            // Target
            Vector3 targetPos = _rewardStackRoot.TransformPoint(Vector3.zero);

            // Animation
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(rt.DOMove(targetPos, 0.45f).SetEase(Ease.InOutCubic));
            seq.Append(rt.DOScale(0.65f, 0.15f));

            seq.OnComplete(() =>
            {
                rt.SetParent(_rewardStackRoot, false);

                RectTransform stackRT = _rewardStackRoot;

                LayoutElement layout = rt.GetComponent<LayoutElement>();
                float w = layout != null ? layout.preferredWidth : _iconWidth;
                float total = w + _iconSpacing;

                int idx = stackRT.childCount - 1;

                // Child pivot 0.5, root pivot 0.5 → centered
                float xPos = idx * total + (total * 0.5f);

                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition = new Vector2(xPos, 0);

                // Expand content width
                stackRT.sizeDelta = new Vector2(
                    (idx + 1) * total,
                    stackRT.sizeDelta.y
                );

                // Auto-scroll to the right
                DOVirtual.DelayedCall(0.05f, () =>
                {
                    _rewardScrollRect.horizontalNormalizedPosition = 1f;
                });
            });
        }

        // --------------------------------------------------------------
        //  REWARD STACK RESET
        // --------------------------------------------------------------
        public void ResetRewardStack()
        {
            foreach (Transform child in _rewardStackRoot)
            {
                Destroy(child.gameObject);
            }

            // StackRoot width reset
            _rewardStackRoot.sizeDelta = new Vector2(0, _rewardStackRoot.sizeDelta.y);
        }
    }
}