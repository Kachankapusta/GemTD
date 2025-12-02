using System;
using Core;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private NavMeshAgent agent;

        private int _currentHealth;

        public EnemyConfig Config => config;

        public float HealthPercent => config != null && config.MaxHealth > 0
            ? (float)_currentHealth / config.MaxHealth
            : 0f;

        public event Action<float> HealthChanged;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            ApplyConfig();
        }

        public void Init(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
            ApplyConfig();
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0)
                return;

            _currentHealth -= amount;

            if (_currentHealth < 0)
                _currentHealth = 0;

            RaiseHealthChanged();

            if (_currentHealth <= 0)
                Die();
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
            var percent = HealthPercent;
            HealthChanged?.Invoke(percent);
        }

        private void Die()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                if (config != null)
                    gameManager.AddGold(config.Bounty);

                gameManager.NotifyEnemyRemoved();
            }

            Destroy(gameObject);
        }
    }
}