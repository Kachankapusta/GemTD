using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class ResourceManager
    {
        [Tooltip("Starting Gold")] [SerializeField]
        private int gold;

        [Tooltip("Starting Lumber")] [SerializeField]
        private int lumber;

        [Tooltip("Starting Lives")] [SerializeField]
        private int lives = 50;

        public int Gold => gold;
        public int Lumber => lumber;
        public int Lives => lives;

        public event Action<int> GoldChanged;
        public event Action<int> LumberChanged;
        public event Action<int> LivesChanged;

        public void Init(int startLives)
        {
            lives = Mathf.Max(0, startLives);
            NotifyAll();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            gold += amount;
            GoldChanged?.Invoke(gold);
        }

        public void AddLumber(int amount)
        {
            if (amount <= 0) return;

            lumber += amount;
            LumberChanged?.Invoke(lumber);
        }

        public bool CanAfford(int goldCost, int lumberCost)
        {
            return gold >= goldCost && lumber >= lumberCost;
        }

        public bool TrySpend(int goldCost, int lumberCost)
        {
            if (!CanAfford(goldCost, lumberCost))
                return false;

            if (goldCost > 0)
            {
                gold -= goldCost;
                if (gold < 0) gold = 0;
                GoldChanged?.Invoke(gold);
            }

            if (lumberCost <= 0) return true;
            lumber -= lumberCost;
            if (lumber < 0) lumber = 0;
            LumberChanged?.Invoke(lumber);

            return true;
        }

        public void ChangeLives(int delta, Action onZeroLives = null)
        {
            if (delta == 0) return;

            lives += delta;
            if (lives < 0) lives = 0;

            LivesChanged?.Invoke(lives);

            if (lives <= 0)
                onZeroLives?.Invoke();
        }

        public void NotifyAll()
        {
            GoldChanged?.Invoke(gold);
            LumberChanged?.Invoke(lumber);
            LivesChanged?.Invoke(lives);
        }
    }
}