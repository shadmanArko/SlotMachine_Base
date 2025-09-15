using System;
using System.Collections.Generic;
using _Scripts.Model;
using Configs;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class SlotReelView : MonoBehaviour, ISlotReelView
    {
        [Header("Required References")]
        [SerializeField] private RectTransform symbolContainer;
        [SerializeField] private GameObject symbolPrefab;
        [SerializeField] private Mask maskComponent;

        [Header("Reel Settings")]
        [SerializeField] private int visibleSymbolCount = 3;
        [Tooltip("More symbols for smooth scrolling")]
        [SerializeField] private int totalSymbolsInReel = 15;
        [SerializeField] private float symbolHeight = 80f;

        [Header("Animation")]
        [SerializeField] private float spinSpeed = 2000f; // pixels/second
        [SerializeField] private float stopDuration = 1f;
        [SerializeField] private AnimationCurve stopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private List<SlotSymbolView> _symbolViews = new List<SlotSymbolView>();
        private DiContainer _container;
        private SlotConfigurationSO _config;
        private bool _isSpinning = false;
        private Sequence _spinSequence;
        private int _reelIndex = 0;

        [Inject]
        public void Construct(SlotConfigurationSO config, DiContainer container)
        {
            _config = config;
            _container = container;
        }

        private void Start()
        {
            CreateInitialSymbols();
        }

        public void SetReelIndex(int index)
        {
            _reelIndex = index;
        }

        public async UniTask StartSpinAnimationAsync()
        {
            if (_isSpinning) return;
            _isSpinning = true;

            CreateSpinAnimation();
        }

        public async UniTask StopSpinAnimationAsync(ISlotSymbol[] finalSymbols)
        {
            if (!_isSpinning) return;

            _spinSequence?.Kill();
            SetFinalSymbols(finalSymbols);
            await AnimateToFinalPositions();
            _isSpinning = false;
        }

        public void HighlightSymbolAt(int row)
        {
            if (row >= 0 && row < _symbolViews.Count)
            {
                _symbolViews[row]?.PlayHighlightAnimation();
            }
        }

        public void ClearHighlights()
        {
            foreach (var symbolView in _symbolViews)
            {
                symbolView?.StopHighlightAnimation();
            }
        }

        private void CreateInitialSymbols()
        {
            ClearExistingSymbols();
            
            for (int i = 0; i < totalSymbolsInReel; i++)
            {
                CreateSymbolAt(i);
            }
        }

        private void ClearExistingSymbols()
        {
            foreach (var symbol in _symbolViews)
            {
                if (symbol != null) 
                    DestroyImmediate(symbol.gameObject);
            }
            _symbolViews.Clear();
        }

        private void CreateSymbolAt(int index)
        {
            GameObject symbolObj = _container.InstantiatePrefab(symbolPrefab, symbolContainer);
            SlotSymbolView symbolView = symbolObj.GetComponent<SlotSymbolView>();

            var randomSymbol = GetRandomSymbol();
            symbolView.SetSymbol(randomSymbol);
            _symbolViews.Add(symbolView);

            PositionSymbol(symbolObj, index);
        }

        private ISlotSymbol GetRandomSymbol()
        {
            var symbols = _config.Symbols;
            return symbols[UnityEngine.Random.Range(0, symbols.Count)];
        }

        private void PositionSymbol(GameObject symbolObj, int index)
        {
            RectTransform rectTransform = symbolObj.GetComponent<RectTransform>();
            float yPosition = -index * symbolHeight;
            rectTransform.anchoredPosition = new Vector2(0, yPosition);
        }

        private void CreateSpinAnimation()
        {
            _spinSequence = DOTween.Sequence();

            foreach (var symbolView in _symbolViews)
            {
                var rectTransform = symbolView.GetComponent<RectTransform>();
        
                var tween = rectTransform.DOAnchorPosY(-totalSymbolsInReel * symbolHeight, 
                        totalSymbolsInReel * symbolHeight / spinSpeed)
                    .SetEase(Ease.Linear)
                    .OnStepComplete(() => {
                        ResetSymbolPosition(rectTransform);
                        RandomizeSymbol(symbolView);
                    });

                _spinSequence.Join(tween);
            }
    
            // Move the infinite loop to the Sequence level
            _spinSequence.SetLoops(-1, LoopType.Restart);
        }

        private void ResetSymbolPosition(RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = new Vector2(0, (totalSymbolsInReel - visibleSymbolCount) * symbolHeight);
        }

        private void RandomizeSymbol(SlotSymbolView symbolView)
        {
            var randomSymbol = GetRandomSymbol();
            symbolView.SetSymbol(randomSymbol);
        }

        private void SetFinalSymbols(ISlotSymbol[] finalSymbols)
        {
            for (int i = 0; i < Math.Min(finalSymbols.Length, visibleSymbolCount) && i < _symbolViews.Count; i++)
            {
                _symbolViews[i].SetSymbol(finalSymbols[i]);
            }
        }

        private async UniTask AnimateToFinalPositions()
        {
            var tasks = new List<UniTask>();
            
            for (int i = 0; i < _symbolViews.Count; i++)
            {
                var symbolView = _symbolViews[i];
                var rectTransform = symbolView.GetComponent<RectTransform>();
                Vector2 finalPosition = new Vector2(0, -i * symbolHeight);
                
                var tween = rectTransform.DOAnchorPos(finalPosition, stopDuration)
                    .SetEase(stopCurve);
                    
                tasks.Add(tween.ToUniTask());
            }

            await UniTask.WhenAll(tasks);
        }

        private void OnDestroy()
        {
            _spinSequence?.Kill();
        }
    }
}