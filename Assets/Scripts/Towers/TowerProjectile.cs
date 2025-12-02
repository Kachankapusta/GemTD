using Enemies;
using UnityEngine;

namespace Towers
{
    public class TowerProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float maxLifetime = 5f;

        private Enemy _target;
        private Vector3 _lastKnownTargetPosition;
        private bool _hasTarget;

        public void Init(Enemy target, int damageAmount, float projectileSpeed, float lifetime)
        {
            _target = target;
            damage = damageAmount;
            speed = projectileSpeed;
            maxLifetime = lifetime > 0f ? lifetime : maxLifetime;

            if (_target != null)
            {
                _hasTarget = true;
                _lastKnownTargetPosition = _target.transform.position;
            }
        }

        private void Update()
        {
            if (maxLifetime > 0f)
            {
                maxLifetime -= Time.deltaTime;
                if (maxLifetime <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (_target == null)
            {
                if (_hasTarget)
                {
                    MoveTowards(_lastKnownTargetPosition);
                    if (Vector3.Distance(transform.position, _lastKnownTargetPosition) <= 0.05f) Destroy(gameObject);

                    return;
                }

                Destroy(gameObject);
                return;
            }

            _lastKnownTargetPosition = _target.transform.position;
            MoveTowards(_lastKnownTargetPosition);

            if (Vector3.Distance(transform.position, _lastKnownTargetPosition) <= 0.1f)
            {
                _target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        private void MoveTowards(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            if (direction.sqrMagnitude <= 0f)
                return;

            transform.position += direction * (speed * Time.deltaTime);
        }
    }
}