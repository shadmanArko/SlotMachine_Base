using System.Collections.Generic;
using _Scripts.Model;

namespace Configs.Interfaces
{
    public interface ISlotConfiguration
    {
        int ReelCount { get; }
        int RowCount { get; }
        IReadOnlyList<ISlotSymbol> Symbols { get; }
        IReadOnlyList<IReadOnlyList<int>> Paylines { get; }
        int MinMatchCount { get; }
        float SpinDuration { get; }
        float ReelStopDelay { get; }
    }
}