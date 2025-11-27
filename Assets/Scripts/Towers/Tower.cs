using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private int damage = 10;

    private float cooldown;

    private void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        var target = FindTarget();
        if (target == null)
            return;

        target.TakeDamage(damage);
        cooldown = fireInterval;
    }

    private Enemy FindTarget()
    {
        var hits = Physics.OverlapSphere(transform.position, range);
        Enemy best = null;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponent<Enemy>();
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