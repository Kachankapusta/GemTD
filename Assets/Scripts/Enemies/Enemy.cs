using Core;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;

        private int _currentHealth;

        private void Awake()
        {
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

            if (_currentHealth <= 0)
                Die();
        }

        private void ApplyConfig()
        {
            if (config == null)
                return;

            _currentHealth = config.MaxHealth;

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.speed = config.MoveSpeed;
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