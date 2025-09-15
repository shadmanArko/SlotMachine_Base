using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs.Data
{
    [Serializable]
    public class PaylineData
    {
        [SerializeField] private List<int> positions = new List<int>();
        [SerializeField] private Color lineColor = Color.yellow;
        [SerializeField] private bool isActive = true;

        public PaylineData(int[] positions, Color color, bool isActive = true)
        {
            this.positions = new List<int>(positions);
            this.lineColor = color;
            this.isActive = isActive;
        }
        
        public PaylineData() { }
        
        public IReadOnlyList<int> Positions => positions;
        public Color LineColor => lineColor;
        public bool IsActive => isActive;
    }
}