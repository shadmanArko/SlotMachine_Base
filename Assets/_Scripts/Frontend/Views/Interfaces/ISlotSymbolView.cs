using _Scripts.Model;

namespace Views
{
    public interface ISlotSymbolView
    {
        void SetSymbol(ISlotSymbol symbol);
        ISlotSymbol CurrentSymbol { get; }
        void PlayHighlightAnimation();
        void StopHighlightAnimation();
    }
}