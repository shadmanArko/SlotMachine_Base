using System;
using UnityEngine;

namespace Configs.Data
{
    [Serializable]
    public class SymbolData
    {
        [SerializeField] private int id;
        [SerializeField] private string symbolName;
        [SerializeField] private int value;
        [SerializeField] private Color symbolColor = Color.white;
        [SerializeField] private Sprite symbolSprite;

        public int Id => id;
        public string SymbolName => symbolName;
        public int Value => value;
        public Color SymbolColor => symbolColor;
        public Sprite SymbolSprite => symbolSprite;

        public SymbolData(int id, string name, int value, Color color)
        {
            this.id = id;
            this.symbolName = name;
            this.value = value;
            this.symbolColor = color;
        }
    }
}