using System.Collections.Generic;
using System.Linq;

namespace _Scripts.Model
{
    public class SpinResult : ISpinResult
    {
        public ISlotSymbol[,] ReelResults { get; }
        public IReadOnlyList<IPaylineResult> WinningPaylines { get; }
        public int TotalPayout { get; }
        public bool IsWin => TotalPayout > 0;

        public SpinResult(ISlotSymbol[,] reelResults, List<IPaylineResult> winningPaylines)
        {
            ReelResults = (ISlotSymbol[,])reelResults.Clone();
            WinningPaylines = winningPaylines.ToList();
            TotalPayout = winningPaylines.Sum(p => p.Payout);
        }
    }
}