using System;
using Towers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [Serializable]
    public class TowerLottery
    {
        [SerializeField] private QualityWeights[] qualityLevels;

        public bool HasConfig => qualityLevels is { Length: > 0 };

        public GemQuality RollQuality(int playerLevel)
        {
            var row = GetRowForLevel(playerLevel);
            if (row == null) return GemQuality.Chipped;

            var total = row.TotalWeight;
            if (total <= 0) return GemQuality.Chipped;

            var roll = Random.Range(0, total);

            if (roll < row.chipped) return GemQuality.Chipped;
            roll -= row.chipped;

            if (roll < row.flawed) return GemQuality.Flawed;
            roll -= row.flawed;

            if (roll < row.normal) return GemQuality.Normal;
            roll -= row.normal;

            return roll < row.flawless ? GemQuality.Flawless : GemQuality.Perfect;
        }

        private QualityWeights GetRowForLevel(int level)
        {
            if (qualityLevels == null || qualityLevels.Length == 0)
                return null;

            QualityWeights best = null;
            var bestLevel = int.MinValue;

            foreach (var row in qualityLevels)
            {
                if (row == null) continue;
                if (row.playerLevel <= bestLevel) continue;
                if (row.playerLevel > level) continue;

                best = row;
                bestLevel = row.playerLevel;
            }

            return best ?? qualityLevels[^1];
        }

        [Serializable]
        public class QualityWeights
        {
            public int playerLevel;
            public int chipped;
            public int flawed;
            public int normal;
            public int flawless;
            public int perfect;

            public int TotalWeight => chipped + flawed + normal + flawless + perfect;
        }
    }
}