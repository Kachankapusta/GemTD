using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(menuName = "GemTD/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int bounty = 1;
        [SerializeField] private float moveSpeed = 3.5f;

        public string Id => id;
        public GameObject Prefab => prefab;
        [Min(1)] public int MaxHealth => maxHealth;
        [Min(0)] public int Bounty => bounty;
        [Min(0f)] public float MoveSpeed => moveSpeed;
    }
}