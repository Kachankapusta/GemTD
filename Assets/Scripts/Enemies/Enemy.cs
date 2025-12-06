using System;
using Combat;
using Core;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private NavMeshAgent agent;

        private int _currentHealth;
        public EnemyConfig Config => config;

        public float HealthPercent => config != null && config.MaxHealth > 0
            ? (float)_currentHealth / config.MaxHealth
            : 0f;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (config != null)
                ApplyConfig();
        }

        public bool IsAlive => _currentHealth > 0;
        public Vector3 Position => transform.position;

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
                return;

            _currentHealth -= amount;
            if (_currentHealth < 0)
                _currentHealth = 0;

            RaiseHealthChanged();

            if (_currentHealth <= 0)
                Die(true);
        }

        public event Action<float> HealthChanged;

        public void Init(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
            ApplyConfig();
        }

        public void OnReachedEnd()
        {
            Die(false);
        }

        private void ApplyConfig()
        {
            if (config == null)
                return;

            _currentHealth = config.MaxHealth;

            if (agent != null)
                agent.speed = config.MoveSpeed;

            RaiseHealthChanged();
        }

        private void RaiseHealthChanged()
        {
            HealthChanged?.Invoke(HealthPercent);
        }

        private void Die(bool giveReward)
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                if (giveReward && config != null)
                    gm.Resources.AddGold(config.Bounty);

                gm.NotifyEnemyRemoved();
            }

            Destroy(gameObject);
        }
    }
}