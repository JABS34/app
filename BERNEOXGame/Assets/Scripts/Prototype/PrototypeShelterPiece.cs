using UnityEngine;

/// <summary>Marks a player-built defensive segment for the shelter prototype.</summary>
public sealed class PrototypeShelterPiece : MonoBehaviour
{
    public PrototypeShelterPieceType PieceType { get; private set; }
    public float CurrentHealth { get; private set; } = 100f;
    public float MaximumHealth { get; private set; } = 100f;
    public bool IsDamaged => CurrentHealth > 0f && CurrentHealth < MaximumHealth;
    private Renderer pieceRenderer;
    private Color originalColor;

    public void Initialize(PrototypeShelterPieceType type)
    {
        PieceType = type;
        MaximumHealth = type == PrototypeShelterPieceType.Wall ? 160f : 90f;
        CurrentHealth = MaximumHealth;
        pieceRenderer = GetComponent<Renderer>();
        if (pieceRenderer != null) originalColor = pieceRenderer.sharedMaterial.color;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        UpdateDamageAppearance();
        if (CurrentHealth <= 0f) Destroy(gameObject);
    }

    public bool Repair(float amount)
    {
        if (amount <= 0f || !IsDamaged) return false;
        CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + amount);
        UpdateDamageAppearance();
        return true;
    }

    private void UpdateDamageAppearance()
    {
        if (pieceRenderer == null) return;
        float damage = 1f - CurrentHealth / MaximumHealth;
        pieceRenderer.material.color = Color.Lerp(originalColor, new Color(0.72f, 0.22f, 0.18f), damage * 0.7f);
    }
}
