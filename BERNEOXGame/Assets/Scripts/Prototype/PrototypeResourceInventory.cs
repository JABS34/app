using System.Collections.Generic;
using UnityEngine;

/// <summary>Temporary local-only resource totals for the combat prototype.</summary>
public sealed class PrototypeResourceInventory : MonoBehaviour
{
    private readonly Dictionary<PrototypeResourceType, int> amounts = new Dictionary<PrototypeResourceType, int>();

    public int GetAmount(PrototypeResourceType type) => amounts.TryGetValue(type, out int value) ? value : 0;

    public void Add(PrototypeResourceType type, int amount)
    {
        if (amount <= 0) return;
        amounts[type] = GetAmount(type) + amount;
    }
}

public enum PrototypeResourceType
{
    Wood,
    Scrap
}
