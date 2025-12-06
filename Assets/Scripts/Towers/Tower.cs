using Combat;
using UnityEngine;

namespace Towers
{
    [DisallowMultipleComponent]
    [SelectionBase]
    public class Tower : MonoBehaviour
    {
        [SerializeField] private TowerConfig config;

        [Min(0f)] [SerializeField] private float cellSize = 1.5f;

        [SerializeField] private TowerProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Min(0f)] [SerializeField] private float projectileSpeed = 10f;

        [Min(0f)] [SerializeField] private float projectileMaxLifetime = 5f;

        [SerializeField] private LayerMask targetLayerMask = ~0;

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
            if (target is not { IsAlive: true })
                return;

            FireAt(target);

            if (config.FireInterval > 0f)
                _cooldown = config.FireInterval;
        }

        private void FireAt(IDamageable target)
        {
            if (target is not { IsAlive: true })
                return;

            if (projectilePrefab == null)
            {
                if (Damage > 0)
                    target.TakeDamage(Damage);

                return;
            }

            var spawnPosition = firePoint != null ? firePoint.position : transform.position;
            var instance = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            instance.Init(target, Damage, projectileSpeed, projectileMaxLifetime);
        }

        private IDamageable FindTarget()
        {
            if (config == null)
                return null;

            var radius = WorldRange;
            if (radius <= 0f)
                return null;

            var hits = Physics.OverlapSphere(transform.position, radius, targetLayerMask);
            IDamageable best = null;
            var bestDistance = float.MaxValue;

            foreach (var с in hits)
            {
                var damageable = с.GetComponentInParent<IDamageable>();
                if (damageable is not { IsAlive: true })
                    continue;

                var distance = (damageable.Position - transform.position).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = damageable;
            }

            return best;
        }
    }
}