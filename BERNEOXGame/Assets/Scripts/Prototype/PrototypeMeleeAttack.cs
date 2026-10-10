using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Short-range attack action used by the prototype HUD and keyboard.</summary>
public sealed class PrototypeMeleeAttack : MonoBehaviour
{
    [SerializeField] private float damage = 40f;
    [SerializeField] private float range = 2.3f;
    [SerializeField] private float cooldown = 0.55f;
    [SerializeField, Range(-1f, 1f)] private float facingThreshold = 0.3f;

    private float nextAttackTime;
    public event Action Attacked;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Attack();
    }

    public void Attack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;
        Attacked?.Invoke();

        Vector3 origin = transform.position;
        origin.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        PrototypeZombieEnemy[] zombies = FindObjectsByType<PrototypeZombieEnemy>();
        foreach (PrototypeZombieEnemy zombie in zombies)
        {
            if (zombie == null || zombie.IsDefeated) continue;
            Vector3 toZombie = zombie.transform.position - transform.position;
            toZombie.y = 0f;
            float distance = toZombie.magnitude;
            if (distance > range || distance < 0.001f) continue;
            if (Vector3.Dot(forward, toZombie / distance) < facingThreshold) continue;
            zombie.TakeDamage(damage);
        }
    }
}
