using UnityEngine;

namespace Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        Vector3 Position { get; }
        void TakeDamage(int amount);
    }
}