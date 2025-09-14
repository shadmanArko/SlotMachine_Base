namespace _Scripts.Model
{
    public interface IRandomProvider
    {
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
        float NextFloat();
    }
}