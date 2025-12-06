using Combat;
using UnityEngine;

namespace Towers
{
    [DisallowMultipleComponent]
    public class TowerProjectile : MonoBehaviour
    {
        [Min(0f)] [SerializeField] private float speed = 10f;

        [Min(0f)] [SerializeField] private float maxLifetime = 5f;

        private int _damage;
        private bool _initialized;
        private float _lifetime;

        private IDamageable _target;

        private void Update()
        {
            if (!_initialized)
                return;

            _lifetime += Time.deltaTime;
            if (_lifetime >= maxLifetime || _target is not { IsAlive: true })
            {
                Destroy(gameObject);
                return;
            }

            var targetPos = _target.Position;
            var currentPos = transform.position;
            var toTarget = targetPos - currentPos;

            var distanceThisFrame = speed * Time.deltaTime;
            var sqrDistanceThisFrame = distanceThisFrame * distanceThisFrame;

            if (toTarget.sqrMagnitude <= sqrDistanceThisFrame)
            {
                HitTarget();
                return;
            }

            var direction = toTarget.normalized;
            transform.position = currentPos + direction * distanceThisFrame;
        }

        public void Init(IDamageable target, int damage, float overrideSpeed, float overrideLifetime)
        {
            _target = target;
            _damage = Mathf.Max(0, damage);

            if (overrideSpeed > 0f)
                speed = overrideSpeed;

            if (overrideLifetime > 0f)
                maxLifetime = overrideLifetime;

            _initialized = true;
        }

        private void HitTarget()
        {
            if (_target is { IsAlive: true } && _damage > 0)
                _target.TakeDamage(_damage);

            Destroy(gameObject);
        }
    }
}