using System.Collections.Generic;

namespace _Scripts.Model
{
    public interface ISpinResult
    {
        ISlotSymbol[,] ReelResults { get; }
        IReadOnlyList<IPaylineResult> WinningPaylines { get; }
        int TotalPayout { get; }
        bool IsWin { get; }
    }
}