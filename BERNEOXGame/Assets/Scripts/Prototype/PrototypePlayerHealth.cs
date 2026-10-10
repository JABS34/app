using System;
using UnityEngine;

/// <summary>Local test health for the first Zombie Survival encounter.</summary>
public sealed class PrototypePlayerHealth : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 100f;

    public float MaximumHealth => maximumHealth;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float, float> HealthChanged;
    public event Action Defeated;

    private void Awake()
    {
        CurrentHealth = maximumHealth;
        HealthChanged?.Invoke(CurrentHealth, maximumHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        HealthChanged?.Invoke(CurrentHealth, maximumHealth);
        if (!IsAlive)
        {
            PrototypePlayerController movement = GetComponent<PrototypePlayerController>();
            if (movement != null) movement.enabled = false;
            PrototypeMeleeAttack attack = GetComponent<PrototypeMeleeAttack>();
            if (attack != null) attack.enabled = false;
            Defeated?.Invoke();
        }
    }
}
