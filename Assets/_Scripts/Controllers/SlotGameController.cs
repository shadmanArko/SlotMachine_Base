using System;
using _Scripts.Model;
using Controllers.Interfaces;
using Cysharp.Threading.Tasks;
using Events;
using UniRx;
using Views;
using Zenject;

namespace Controllers
{
    public class SlotGameController : ISlotGameController, IInitializable, IDisposable
    {
        private readonly ISlotGameModel _model;
        private readonly ISlotMachineView _view;
        private readonly IEventBus _eventBus;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private readonly Subject<Unit> _gameInitializedSubject = new Subject<Unit>();
        
        public IObservable<Unit> OnGameInitialized => _gameInitializedSubject.AsObservable();
        
        public SlotGameController(
            ISlotGameModel model, 
            ISlotMachineView view, 
            IEventBus eventBus)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Initialize()
        {
            SetupModelSubscriptions();
            SetupViewSubscriptions();
            SetupEventBusSubscriptions();
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            try
            {
                _model.Initialize();
                await UniTask.Delay(TimeSpan.FromMilliseconds(100)); // Initialization delay
                
                _view.ShowMessage("Ready to Spin!");
                _view.SetSpinButtonEnabled(true);
                _view.UpdatePayoutDisplay(0);
                
                _gameInitializedSubject.OnNext(Unit.Default);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to initialize: {ex.Message}");
            }
        }

        private void SetupModelSubscriptions()
        {
            _model.OnSpinStarted += HandleModelSpinStarted;
            _model.OnSpinCompleted += HandleModelSpinCompleted;
        }

        private void SetupViewSubscriptions()
        {
            _view.OnSpinRequested
                .Where(_ => _model.CanSpin)
                .Subscribe(_ => RequestSpin())
                .AddTo(_disposables);
                
            _view.OnSpinRequested
                .Where(_ => !_model.CanSpin)
                .Subscribe(_ => _view.ShowMessage("Cannot spin while another spin is in progress"))
                .AddTo(_disposables);
        }

        private void SetupEventBusSubscriptions()
        {
            _eventBus.OnEvent<OnSpinButtonPressed>()
                .Where(_ => _model.CanSpin)
                .Subscribe(_ => RequestSpin())
                .AddTo(_disposables);
                
            _eventBus.OnEvent<OnSpinButtonPressed>()
                .Where(_ => !_model.CanSpin)
                .Subscribe(_ => _view.ShowMessage("Cannot spin while another spin is in progress"))
                .AddTo(_disposables);
        }

        private void RequestSpin()
        {
            ProcessSpinAsync().Forget();
        }

        private async UniTaskVoid ProcessSpinAsync()
        {
            try
            {
                var result = await _model.PerformSpinAsync();
                await HandleSpinResult(result);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Spin failed: {ex.Message}");
                _view.SetSpinButtonEnabled(true);
            }
        }

        private void HandleModelSpinStarted()
        {
            _view.SetSpinButtonEnabled(false);
            _view.ShowMessage("Spinning...");
            _view.HideWinEffects();
            _view.StartReelAnimations();
        }

        private void HandleModelSpinCompleted(ISpinResult result)
        {
            _view.StopReelAnimations(result.ReelResults);
        }

        private async UniTask HandleSpinResult(ISpinResult result)
        {
            // Wait for reel animations to complete
            await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
            
            _view.SetSpinButtonEnabled(true);
            
            if (result.IsWin)
            {
                await HandleWinResult(result);
            }
            else
            {
                HandleLoseResult();
            }
        }

        private async UniTask HandleWinResult(ISpinResult result)
        {
            _view.ShowMessage($"WIN! Payout: {result.TotalPayout}");
            _view.UpdatePayoutDisplay(result.TotalPayout);
            _view.ShowWinEffects();
            
            // Highlight winning positions
            var winningPositions = ExtractWinningPositions(result);
            _view.HighlightWinningSymbols(winningPositions);
            
            // Show win effects for duration
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            _view.HideWinEffects();
            _view.ClearSymbolHighlights();
        }

        private void HandleLoseResult()
        {
            _view.ShowMessage("Try Again!");
            _view.UpdatePayoutDisplay(0);
        }

        private int[] ExtractWinningPositions(ISpinResult result)
        {
            var positions = new System.Collections.Generic.List<int>();
            
            foreach (var paylineResult in result.WinningPaylines)
            {
                positions.AddRange(paylineResult.MatchedPositions);
            }
            
            return positions.ToArray();
        }

        public void Dispose()
        {
            UnsubscribeFromModelEvents();
            _disposables?.Dispose();
            _gameInitializedSubject?.Dispose();
        }

        private void UnsubscribeFromModelEvents()
        {
            if (_model == null) return;
            
            _model.OnSpinStarted -= HandleModelSpinStarted;
            _model.OnSpinCompleted -= HandleModelSpinCompleted;
        }
    }
}