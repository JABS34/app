using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Short-range attack action used by the prototype HUD and keyboard.</summary>
public sealed class PrototypeMeleeAttack : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 1.0f;
    [SerializeField] private float cooldown = 0.4f;
    [SerializeField, Range(-1f, 1f)] private float facingThreshold = 0.3f;

    private float nextAttackTime;
    public PrototypeWeapon EquippedWeapon { get; private set; } = PrototypeWeapon.Knife;
    public event Action Attacked;
    public event Action<int> HitConfirmed;

    public void Equip(PrototypeWeapon weapon)
    {
        EquippedWeapon = weapon;
        switch (weapon)
        {
            case PrototypeWeapon.Bat:
                damage = 18f;
                range = 1.4f;
                cooldown = 0.7f;
                break;
            case PrototypeWeapon.Axe:
                damage = 28f;
                range = 1.3f;
                cooldown = 1.0f;
                break;
            default:
                damage = 10f;
                range = 1.0f;
                cooldown = 0.4f;
                break;
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0f && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Attack();
    }

    public void Attack()
    {
        if (Time.timeScale <= 0f || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;
        Attacked?.Invoke();

        Vector3 origin = transform.position;
        origin.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        PrototypeZombieEnemy[] zombies = FindObjectsByType<PrototypeZombieEnemy>();
        int hitCount = 0;
        foreach (PrototypeZombieEnemy zombie in zombies)
        {
            if (zombie == null || zombie.IsDefeated) continue;
            Vector3 toZombie = zombie.transform.position - transform.position;
            toZombie.y = 0f;
            float distance = toZombie.magnitude;
            if (distance > range || distance < 0.001f) continue;
            if (Vector3.Dot(forward, toZombie / distance) < facingThreshold) continue;
            zombie.TakeDamage(damage, transform.position);
            hitCount++;
        }
        if (hitCount > 0) HitConfirmed?.Invoke(hitCount);
    }
}

public enum PrototypeWeapon
{
    Knife,
    Bat,
    Axe
}
