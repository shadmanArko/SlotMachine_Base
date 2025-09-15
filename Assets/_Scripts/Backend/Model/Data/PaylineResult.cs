using System.Collections.Generic;

namespace _Scripts.Model
{
    public class PaylineResult : IPaylineResult
    {
        public int PaylineIndex { get; }
        public int MatchCount { get; }
        public ISlotSymbol MatchedSymbol { get; }
        public int Payout { get; }
        public IReadOnlyList<int> MatchedPositions { get; }

        public PaylineResult(int paylineIndex, int matchCount, ISlotSymbol matchedSymbol, 
            int payout, List<int> matchedPositions)
        {
            PaylineIndex = paylineIndex;
            MatchCount = matchCount;
            MatchedSymbol = matchedSymbol;
            Payout = payout;
            MatchedPositions = new List<int>(matchedPositions);
        }
    }
}