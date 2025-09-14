using _Scripts.Model;
using Cysharp.Threading.Tasks;

namespace Views
{
    public interface ISlotReelView
    {
        void SetReelIndex(int index);
        UniTask StartSpinAnimationAsync();
        UniTask StopSpinAnimationAsync(ISlotSymbol[] finalSymbols);
        void HighlightSymbolAt(int row);
        void ClearHighlights();
    }
}