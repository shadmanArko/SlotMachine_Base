using System;
using System.Collections.Generic;
using Configs.Interfaces;
using Cysharp.Threading.Tasks;

namespace _Scripts.Model
{
    public class SlotGameModel : ISlotGameModel
    {
        public event Action<ISpinResult> OnSpinCompleted;
        public event Action OnSpinStarted;

        private readonly ISlotConfiguration _config;
        private readonly IRandomProvider _randomProvider;
        private bool _isSpinning;

        public bool CanSpin => !_isSpinning;
        
        public SlotGameModel(ISlotConfiguration config, IRandomProvider randomProvider)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        public void Initialize()
        {
            _isSpinning = false;
        }

        public async UniTask<ISpinResult> PerformSpinAsync()
        {
            ValidateCanSpin();
            
            _isSpinning = true;
            OnSpinStarted?.Invoke();

            try
            {
                var result = GenerateSpinResult();
                
                OnSpinCompleted?.Invoke(result);
                return result;
            }
            finally
            {
                _isSpinning = false;
            }
        }

        private void ValidateCanSpin()
        {
            if (!CanSpin)
                throw new InvalidOperationException("Cannot spin while another spin is in progress");
        }

        private ISpinResult GenerateSpinResult()
        {
            var reelResults = GenerateReelResults();
            var winningPaylines = CalculateWinningPaylines(reelResults);
            return new SpinResult(reelResults, winningPaylines);
        }

        private ISlotSymbol[,] GenerateReelResults()
        {
            var results = new ISlotSymbol[_config.ReelCount, _config.RowCount];
            var symbols = _config.Symbols;
            
            for (var reel = 0; reel < _config.ReelCount; reel++)
            {
                for (var row = 0; row < _config.RowCount; row++)
                {
                    var symbolIndex = _randomProvider.Next(symbols.Count);
                    results[reel, row] = symbols[symbolIndex];
                }
            }
            
            return results;
        }

        private List<IPaylineResult> CalculateWinningPaylines(ISlotSymbol[,] reelResults)
        {
            var winningPaylines = new List<IPaylineResult>();

            for (var paylineIndex = 0; paylineIndex < _config.Paylines.Count; paylineIndex++)
            {
                var paylineResult = EvaluatePayline(paylineIndex, _config.Paylines[paylineIndex], reelResults);
                if (paylineResult != null)
                    winningPaylines.Add(paylineResult);
            }

            return winningPaylines;
        }

        private IPaylineResult EvaluatePayline(int paylineIndex, IReadOnlyList<int> paylinePositions, ISlotSymbol[,] reelResults)
        {
            if (paylinePositions.Count == 0) 
                return null;

            var symbolsOnPayline = ExtractSymbolsFromPayline(paylinePositions, reelResults);
            if (symbolsOnPayline.symbols.Count == 0) 
                return null;

            var matchResult = AnalyzeConsecutiveMatches(symbolsOnPayline.symbols, symbolsOnPayline.positions);
            
            return IsValidWin(matchResult.matchCount) 
                ? CreatePaylineResult(paylineIndex, symbolsOnPayline.symbols[0], matchResult.matchCount, matchResult.matchedPositions)
                : null;
        }

        private (List<ISlotSymbol> symbols, List<int> positions) ExtractSymbolsFromPayline(
            IReadOnlyList<int> paylinePositions, 
            ISlotSymbol[,] reelResults)
        {
            var symbols = new List<ISlotSymbol>();
            var positions = new List<int>();

            foreach (var position in paylinePositions)
            {
                var coordinates = ConvertPositionToCoordinates(position);
                
                if (IsValidPosition(coordinates))
                {
                    symbols.Add(reelResults[coordinates.reel, coordinates.row]);
                    positions.Add(position);
                }
            }

            return (symbols, positions);
        }

        private (int reel, int row) ConvertPositionToCoordinates(int position) => 
            (position % _config.ReelCount, position / _config.ReelCount);

        private bool IsValidPosition((int reel, int row) coordinates) => 
            coordinates.reel >= 0 && coordinates.reel < _config.ReelCount && 
            coordinates.row >= 0 && coordinates.row < _config.RowCount;

        private bool IsValidWin(int matchCount) => matchCount >= _config.MinMatchCount;

        private (int matchCount, List<int> matchedPositions) AnalyzeConsecutiveMatches(
            List<ISlotSymbol> symbols, 
            List<int> positions)
        {
            if (symbols.Count == 0)
                return (0, new List<int>());

            var firstSymbol = symbols[0];
            var matchCount = 1;
            var matchedPositions = new List<int> { positions[0] };

            for (var i = 1; i < symbols.Count; i++)
            {
                if (!symbols[i].Equals(firstSymbol))
                    break;
                    
                matchCount++;
                matchedPositions.Add(positions[i]);
            }

            return (matchCount, matchedPositions);
        }

        private IPaylineResult CreatePaylineResult(
            int paylineIndex, 
            ISlotSymbol symbol, 
            int matchCount, 
            List<int> matchedPositions)
        {
            var basePayout = CalculateBasePayout(symbol, matchCount);
            var totalPayout = ApplyMultipliers(basePayout, symbol, matchCount);
            
            return new PaylineResult(paylineIndex, matchCount, symbol, totalPayout, matchedPositions);
        }

        private int CalculateBasePayout(ISlotSymbol symbol, int matchCount)
        {
            return symbol.Value * matchCount;
        }

        private int ApplyMultipliers(int basePayout, ISlotSymbol symbol, int matchCount)
        {
            return basePayout;
        }
    }
}