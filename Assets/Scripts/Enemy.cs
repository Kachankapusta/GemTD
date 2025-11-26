using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int bounty = 1;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(bounty);

        Destroy(gameObject);
    }
}