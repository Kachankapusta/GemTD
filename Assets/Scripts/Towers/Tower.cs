using Combat;
using UnityEngine;

namespace Towers
{
    public class Tower : MonoBehaviour
    {
        [SerializeField] private TowerConfig config;
        [SerializeField] private float cellSize = 1.5f;
        [SerializeField] private TowerProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float projectileMaxLifetime = 5f;

        private float _cooldown;

        public TowerConfig Config => config;
        public string DisplayName => config != null ? config.DisplayName : name;
        public GemType Type => config != null ? config.Type : default;
        public GemQuality Quality => config != null ? config.Quality : default;
        public int Damage => config != null ? config.Damage : 0;
        public float RangeInCells => config != null ? config.Range : 0f;
        public float FireInterval => config != null ? config.FireInterval : 0f;
        public float CellSize => cellSize;
        public float WorldRange => config != null ? (config.Range + 0.5f) * cellSize : 0f;

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
            if (target == null || !target.IsAlive)
                return;

            FireAt(target);
            _cooldown = config.FireInterval;
        }

        private void FireAt(IDamageable target)
        {
            if (target == null || !target.IsAlive)
                return;

            if (projectilePrefab == null)
            {
                target.TakeDamage(config.Damage);
                return;
            }

            var spawnPosition = firePoint != null ? firePoint.position : transform.position;
            var instance = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            instance.Init(target, config.Damage, projectileSpeed, projectileMaxLifetime);
        }

        private IDamageable FindTarget()
        {
            if (config == null)
                return null;

            var hits = Physics.OverlapSphere(transform.position, WorldRange);
            IDamageable best = null;
            var bestDistance = float.MaxValue;

            foreach (var t in hits)
            {
                var damageable = t.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive)
                    continue;

                var distance = (damageable.Position - transform.position).sqrMagnitude;
                if (!(distance < bestDistance))
                    continue;

                bestDistance = distance;
                best = damageable;
            }

            return best;
        }
    }
}