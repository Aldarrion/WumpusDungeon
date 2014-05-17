using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WumpusDungeon
{
    struct Inventory
    {
        public int SpearCount { get; set; }
        public int Score { get; set; }
        public int TorchCount { get; set; }

        public Inventory(int score, int spearCount, int torchCount)
            :this()
        {
            this.SpearCount = spearCount;
            this.Score = score;
            this.TorchCount = torchCount;
        }
    }
}
