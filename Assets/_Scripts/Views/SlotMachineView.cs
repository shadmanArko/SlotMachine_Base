using System;
using System.Collections.Generic;
using _Scripts.Model;
using Configs;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class SlotMachineView : MonoBehaviour, ISlotMachineView, IInitializable
    {
        [Header("UI Components")]
        [SerializeField] private Button spinButton;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI payoutText;
        [SerializeField] private GameObject winEffectPanel;
        [SerializeField] private ParticleSystem winParticles;

        [Header("Reel Views")]
        [SerializeField] private List<SlotReelView> reelViews = new List<SlotReelView>();

        private readonly Subject<Unit> _spinRequestedSubject = new Subject<Unit>();
        private CompositeDisposable _disposables = new CompositeDisposable();
        private Sequence _winEffectSequence;
        
        private SlotConfigurationSO _config;
        private GameSettingsConfigSO _gameSettings;

        public IObservable<Unit> OnSpinRequested => _spinRequestedSubject.AsObservable();
        

         [Inject]
        public void Construct(SlotConfigurationSO config, GameSettingsConfigSO gameSettings)
        {
            _config = config;
            _gameSettings = gameSettings;
        }

        private void Awake()
        {
            ValidateComponents();
        }

        public void Initialize()
        {
            SetupUI();
            InitializeReelViews();
        }

        private void ValidateComponents()
        {
            if (spinButton == null)
                Debug.LogError($"{nameof(spinButton)} is not assigned!");
            
            if (reelViews.Count == 0)
                Debug.LogError("No reel views assigned!");
        }

        private void SetupUI()
        {
            if (spinButton != null)
            {
                spinButton.OnClickAsObservable()
                    .Subscribe(_ => _spinRequestedSubject.OnNext(Unit.Default))
                    .AddTo(_disposables);
            }

            if (winEffectPanel != null)
                winEffectPanel.SetActive(false);
        }

        private void InitializeReelViews()
        {
            for (int i = 0; i < reelViews.Count; i++)
            {
                if (reelViews[i] != null)
                {
                    reelViews[i].SetReelIndex(i);
                }
            }
        }

        public void SetSpinButtonEnabled(bool enabled)
        {
            if (spinButton != null)
            {
                spinButton.interactable = enabled;
            }
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                AnimateTextPunch(messageText.transform);
            }
        }

        public void UpdatePayoutDisplay(int payout)
        {
            if (payoutText != null)
            {
                payoutText.text = payout > 0 ? $"Payout: {payout}" : "";
                
                if (payout > 0)
                {
                    AnimatePayoutUpdate();
                }
            }
        }

        public void StartReelAnimations()
        {
            foreach (var reelView in reelViews)
            {
                reelView?.StartSpinAnimationAsync().Forget();
            }
        }

        public void StopReelAnimations(ISlotSymbol[,] results)
        {
            StopReelAnimationsSequentially(results).Forget();
        }

        public void ShowWinEffects()
        {
            if (!_gameSettings.EnableWinAnimations) return;

            ShowWinPanel();
            PlayWinParticles();
        }

        public void HideWinEffects()
        {
            HideWinPanel();
            StopWinParticles();
        }

        public void HighlightWinningSymbols(int[] winningPositions)
        {
            // Convert positions to reel/row coordinates and highlight
            foreach (var position in winningPositions)
            {
                var (reel, row) = ConvertPositionToCoordinates(position);
                HighlightSymbolAt(reel, row);
            }
        }

        public void ClearSymbolHighlights()
        {
            foreach (var reelView in reelViews)
            {
                reelView?.ClearHighlights();
            }
        }

        private async UniTaskVoid StopReelAnimationsSequentially(ISlotSymbol[,] results)
        {
            for (int reelIndex = 0; reelIndex < reelViews.Count && reelIndex < results.GetLength(0); reelIndex++)
            {
                if (reelViews[reelIndex] != null)
                {
                    var columnSymbols = ExtractColumnSymbols(results, reelIndex);
                    reelViews[reelIndex].StopSpinAnimationAsync(columnSymbols).Forget();
                    
                    if (reelIndex < reelViews.Count - 1)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.ReelStopDelay));
                    }
                }
            }
        }

        private ISlotSymbol[] ExtractColumnSymbols(ISlotSymbol[,] results, int reelIndex)
        {
            var columnSymbols = new ISlotSymbol[results.GetLength(1)];
            for (int row = 0; row < columnSymbols.Length; row++)
            {
                columnSymbols[row] = results[reelIndex, row];
            }
            return columnSymbols;
        }

        private void ShowWinPanel()
        {
            if (winEffectPanel != null)
            {
                winEffectPanel.SetActive(true);
                
                _winEffectSequence = DOTween.Sequence()
                    .Append(winEffectPanel.transform.DOScale(0.8f, 0.1f))
                    .Append(winEffectPanel.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
                    .Append(winEffectPanel.transform.DOScale(1f, 0.2f));
            }
        }

        private void HideWinPanel()
        {
            if (winEffectPanel != null)
            {
                _winEffectSequence?.Kill();
                winEffectPanel.transform.localScale = Vector3.one;
                winEffectPanel.SetActive(false);
            }
        }

        private void PlayWinParticles()
        {
            if (winParticles != null)
            {
                winParticles.Play();
            }
        }

        private void StopWinParticles()
        {
            if (winParticles != null)
            {
                winParticles.Stop();
            }
        }

        private void AnimateTextPunch(Transform textTransform)
        {
            textTransform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 2, 0.2f);
        }

        private void AnimatePayoutUpdate()
        {
            payoutText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 3, 0.3f);
        }

        private (int reel, int row) ConvertPositionToCoordinates(int position)
        {
            return (position % _config.ReelCount, position / _config.ReelCount);
        }

        private void HighlightSymbolAt(int reel, int row)
        {
            if (reel >= 0 && reel < reelViews.Count && reelViews[reel] != null)
            {
                reelViews[reel].HighlightSymbolAt(row);
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _winEffectSequence?.Kill();
            _spinRequestedSubject?.Dispose();
        }
    }
}