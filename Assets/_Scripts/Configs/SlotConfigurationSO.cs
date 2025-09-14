using System.Collections.Generic;
using System.Linq;
using _Scripts.Model;
using Configs.Data;
using Configs.Interfaces;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "SlotConfiguration", menuName = "SlotGame/Slot Configuration")]
    public class SlotConfigurationSO : ScriptableObject, ISlotConfiguration
    {
        [Header("Reel Settings")]
        [SerializeField] private int reelCount = 5;
        [SerializeField] private int rowCount = 3;
        [SerializeField] private int minMatchCount = 3;

        [Header("Symbols")]
        [SerializeField] private List<SymbolData> symbolsData = new List<SymbolData>();

        [Header("Paylines")]
        [SerializeField] private List<PaylineData> paylinesData = new List<PaylineData>();

        [Header("Animation Settings")]
        [SerializeField] private float spinDuration = 2f;
        [SerializeField] private float reelStopDelay = 0.3f;
        
        private List<ISlotSymbol> _cachedSymbols;
        private List<IReadOnlyList<int>> _cachedPaylines;

        public int ReelCount => reelCount;
        public int RowCount => rowCount;
        public int MinMatchCount => minMatchCount;
        public float SpinDuration => spinDuration;
        public float ReelStopDelay => reelStopDelay;

        public IReadOnlyList<ISlotSymbol> Symbols
        {
            get
            {
                if (_cachedSymbols == null)
                {
                    _cachedSymbols = symbolsData.Select(s => new SlotSymbol(s.Id, s.SymbolName, s.Value) as ISlotSymbol).ToList();
                }
                return _cachedSymbols;
            }
        }

        public IReadOnlyList<IReadOnlyList<int>> Paylines
        {
            get
            {
                if (_cachedPaylines == null)
                {
                    _cachedPaylines = paylinesData
                        .Where(p => p.IsActive)
                        .Select(p => p.Positions)
                        .ToList<IReadOnlyList<int>>();
                }
                return _cachedPaylines;
            }
        }
        
        private void OnValidate()
        {
            _cachedSymbols = null;
            _cachedPaylines = null;
            
            reelCount = Mathf.Max(1, reelCount);
            rowCount = Mathf.Max(1, rowCount);
            minMatchCount = Mathf.Max(1, minMatchCount);
            spinDuration = Mathf.Max(0.1f, spinDuration);
            reelStopDelay = Mathf.Max(0f, reelStopDelay);
        }
        
        [ContextMenu("Create Default Configuration")]
        private void CreateDefaultConfiguration()
        {
            symbolsData.Clear();
            symbolsData.AddRange(new[]
            {
                new SymbolData(0, "Cherry", 10, Color.red),
                new SymbolData(1, "Lemon", 15, Color.yellow),
                new SymbolData(2, "Orange", 20, new Color(1f, 0.5f, 0f)),
                new SymbolData(3, "Plum", 25, Color.magenta),
                new SymbolData(4, "Bell", 30, Color.green),
                new SymbolData(5, "Bar", 50, Color.blue),
                new SymbolData(6, "Seven", 100, Color.white)
            });

            paylinesData.Clear();
            paylinesData.AddRange(new[]
            {
                CreatePaylineData(new[] {5, 6, 7, 8, 9}, Color.yellow),    // Middle row
                CreatePaylineData(new[] {0, 1, 2, 3, 4}, Color.red),       // Top row
                CreatePaylineData(new[] {10, 11, 12, 13, 14}, Color.blue), // Bottom row
                CreatePaylineData(new[] {0, 6, 12, 8, 4}, Color.green),    // Diagonal
                CreatePaylineData(new[] {10, 6, 2, 8, 14}, Color.cyan)     // Diagonal
            });
        }

        private PaylineData CreatePaylineData(int[] positions, Color color)
        {
            return new PaylineData(positions, color);
        }

        public SymbolData GetSymbolData(int symbolId)
        {
            return symbolsData.FirstOrDefault(s => s.Id == symbolId);
        }

        public PaylineData GetPaylineData(int paylineIndex)
        {
            return paylineIndex >= 0 && paylineIndex < paylinesData.Count 
                ? paylinesData[paylineIndex] 
                : null;
        }
    }
}