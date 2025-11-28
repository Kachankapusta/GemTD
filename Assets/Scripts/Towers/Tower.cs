using Enemies;
using UnityEngine;

namespace Towers
{
    public class Tower : MonoBehaviour
    {
        [SerializeField] private float range = 5f;
        [SerializeField] private float fireInterval = 0.5f;
        [SerializeField] private int damage = 10;
        [SerializeField] private int goldCost;
        [SerializeField] private int lumberCost = 1;

        public int GoldCost => goldCost < 0 ? 0 : goldCost;
        public int LumberCost => lumberCost < 0 ? 0 : lumberCost;

        private float _cooldown;

        private void Update()
        {
            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            var target = FindTarget();
            if (target == null)
                return;

            target.TakeDamage(damage);
            _cooldown = fireInterval;
        }

        private Enemy FindTarget()
        {
            var hits = Physics.OverlapSphere(transform.position, range);
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