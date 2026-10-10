using UnityEngine;

/// <summary>Marks a player-built defensive segment for the shelter prototype.</summary>
public sealed class PrototypeShelterPiece : MonoBehaviour
{
    public PrototypeShelterPieceType PieceType { get; private set; }
    public float CurrentHealth { get; private set; } = 100f;

    public void Initialize(PrototypeShelterPieceType type)
    {
        PieceType = type;
        CurrentHealth = type == PrototypeShelterPieceType.Wall ? 160f : 90f;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        if (CurrentHealth <= 0f) Destroy(gameObject);
    }
}
