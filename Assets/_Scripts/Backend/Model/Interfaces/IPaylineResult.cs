using System.Collections.Generic;

namespace _Scripts.Model
{
    public interface IPaylineResult
    {
        int PaylineIndex { get; }
        int MatchCount { get; }
        ISlotSymbol MatchedSymbol { get; }
        int Payout { get; }
        IReadOnlyList<int> MatchedPositions { get; }
    }
}