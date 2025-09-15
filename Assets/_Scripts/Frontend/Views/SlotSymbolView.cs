using _Scripts.Model;
using Configs;
using Configs.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class SlotSymbolView : MonoBehaviour, ISlotSymbolView
    {
        [Header("Visual Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image symbolImage;
        [SerializeField] private TextMeshProUGUI symbolText;

        private ISlotSymbol _currentSymbol;
        private SlotConfigurationSO _config;
        private Sequence _highlightSequence;

        public ISlotSymbol CurrentSymbol => _currentSymbol;

        [Inject]
        public void Construct(SlotConfigurationSO config)
        {
            _config = config;
        }

        private void Awake()
        {
            ValidateComponents();
        }

        public void SetSymbol(ISlotSymbol symbol)
        {
            if (symbol == null) return;

            _currentSymbol = symbol;
            UpdateVisualComponents();
        }

        public void PlayHighlightAnimation()
        {
            StopHighlightAnimation();

            _highlightSequence = DOTween.Sequence()
                .Append(transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(0.9f, 0.2f))
                .Append(transform.DOScale(1.05f, 0.2f))
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopHighlightAnimation()
        {
            _highlightSequence?.Kill();
            transform.localScale = Vector3.one;
        }

        private void ValidateComponents()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (symbolText == null)
                symbolText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void UpdateVisualComponents()
        {
            if (_currentSymbol == null || _config == null) return;

            var symbolData = _config.GetSymbolData(_currentSymbol.Id);
            if (symbolData == null) return;

            UpdateBackgroundColor(symbolData);
            UpdateSymbolSprite(symbolData);
            UpdateSymbolText(symbolData);
        }

        private void UpdateBackgroundColor(SymbolData symbolData)
        {
            if (backgroundImage != null)
                backgroundImage.color = symbolData.SymbolColor;
        }

        private void UpdateSymbolSprite(SymbolData symbolData)
        {
            if (symbolImage != null)
            {
                if (symbolData.SymbolSprite != null)
                {
                    symbolImage.sprite = symbolData.SymbolSprite;
                    symbolImage.color = Color.white;
                }
                else
                {
                    symbolImage.sprite = null;
                    symbolImage.color = Color.clear;
                }
            }
        }

        private void UpdateSymbolText(SymbolData symbolData)
        {
            if (symbolText != null)
            {
                symbolText.text = _currentSymbol.Name;
                symbolText.color = CalculateContrastColor(symbolData.SymbolColor);
            }
        }

        private Color CalculateContrastColor(Color backgroundColor)
        {
            float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
            return luminance > 0.5f ? Color.black : Color.white;
        }

        private void OnDestroy()
        {
            StopHighlightAnimation();
        }
    }
}