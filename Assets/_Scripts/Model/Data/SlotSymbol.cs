using System;

namespace _Scripts.Model
{
    [Serializable]
    public class SlotSymbol : ISlotSymbol
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Value { get; private set; }

        public SlotSymbol(int id, string name, int value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ISlotSymbol other)
                return Id == other.Id;
            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}