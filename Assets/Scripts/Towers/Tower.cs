using Enemies;
using UnityEngine;

namespace Towers
{
    public class Tower : MonoBehaviour
    {
        [SerializeField] private TowerConfig config;
        [SerializeField] private float cellSize = 1.5f;

        private float _cooldown;

        public TowerConfig Config => config;
        public string DisplayName => config != null ? config.DisplayName : name;
        public GemType Type => config != null ? config.Type : default;
        public GemQuality Quality => config != null ? config.Quality : default;
        public int Damage => config != null ? config.Damage : 0;
        public float RangeInCells => config != null ? config.Range : 0f;
        public float FireInterval => config != null ? config.FireInterval : 0f;
        public float CellSize => cellSize;

        private void Update()
        {
            if (config == null)
                return;

            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            var target = FindTarget();
            if (target == null)
                return;

            target.TakeDamage(config.Damage);
            _cooldown = config.FireInterval;
        }

        private Enemy FindTarget()
        {
            if (config == null)
                return null;

            var worldRange = config.Range * cellSize;

            var hits = Physics.OverlapSphere(transform.position, worldRange);
            Enemy best = null;
            var bestDistance = float.MaxValue;

            foreach (var t in hits)
            {
                var enemy = t.GetComponent<Enemy>();
                if (enemy == null)
                    continue;

                var distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }
    }
}