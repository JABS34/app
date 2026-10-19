using System;
using UnityEngine;

/// <summary>Local 12-slot inventory for the first survival prototype.</summary>
public sealed class PrototypeResourceInventory : MonoBehaviour
{
    public const int SlotCount = 12;
    public const int ResourceStackLimit = 30;

    [Serializable]
    private sealed class Slot
    {
        public PrototypeInventoryItem item;
        public int amount;
    }

    [SerializeField] private Slot[] slots = new Slot[SlotCount];
    public event Action Changed;

    private void Awake()
    {
        if (slots == null || slots.Length != SlotCount) slots = new Slot[SlotCount];
    }

    public int GetAmount(PrototypeResourceType type) => GetAmount(ToItem(type));

    public int GetAmount(PrototypeInventoryItem item)
    {
        int total = 0;
        if (slots == null) return total;
        foreach (Slot slot in slots)
            if (slot != null && slot.item == item) total += slot.amount;
        return total;
    }

    public void AddStarterKnife()
    {
        if (GetAmount(PrototypeInventoryItem.Knife) == 0) TryAdd(PrototypeInventoryItem.Knife, 1);
    }

    public string GetSlotLabel(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length) return string.Empty;
        Slot slot = slots[index];
        return slot == null || slot.amount <= 0 ? "VACÍO" : ItemLabel(slot.item) + "  x" + slot.amount;
    }

    public PrototypeInventoryItem GetSlotItem(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length || slots[index] == null)
            return PrototypeInventoryItem.Wood;
        return slots[index].item;
    }

    public int GetSlotAmount(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length || slots[index] == null) return 0;
        return slots[index].amount;
    }

    public bool CanAddResource(PrototypeResourceType type, int amount) => CanAdd(ToItem(type), amount);

    public bool TryAddResource(PrototypeResourceType type, int amount) => TryAdd(ToItem(type), amount);

    public bool TrySpendResource(PrototypeResourceType type, int amount)
    {
        if (amount <= 0 || GetAmount(type) < amount) return false;
        Remove(ToItem(type), amount);
        Changed?.Invoke();
        return true;
    }

    public bool TryCraftBat(out string message)
    {
        return TryCraft(PrototypeInventoryItem.Bat, PrototypeResourceType.Wood, 6,
            PrototypeResourceType.Fiber, 2, out message);
    }

    public bool TryCraftAxe(out string message)
    {
        if (GetAmount(PrototypeResourceType.Wood) < 4
            || GetAmount(PrototypeResourceType.Stone) < 3
            || GetAmount(PrototypeResourceType.Fiber) < 2)
        {
            message = "FALTAN MATERIALES PARA EL HACHA";
            return false;
        }
        Slot[] previousSlots = CopySlots();
        Remove(ToItem(PrototypeResourceType.Wood), 4);
        Remove(ToItem(PrototypeResourceType.Stone), 3);
        Remove(ToItem(PrototypeResourceType.Fiber), 2);
        if (!TryAdd(PrototypeInventoryItem.Axe, 1))
        {
            slots = previousSlots;
            message = "MOCHILA LLENA · LIBERA UN ESPACIO";
            return false;
        }
        message = "HACHA FABRICADA Y EQUIPADA";
        return true;
    }

    public bool TryCraft(PrototypeInventoryItem product, PrototypeResourceType first, int firstCost,
        PrototypeResourceType second, int secondCost, out string message)
    {
        if (GetAmount(first) < firstCost || GetAmount(second) < secondCost)
        {
            message = product == PrototypeInventoryItem.Bat
                ? "FALTAN MATERIALES PARA EL BATE"
                : "FALTAN MATERIALES";
            return false;
        }
        Slot[] previousSlots = CopySlots();
        Remove(ToItem(first), firstCost);
        Remove(ToItem(second), secondCost);
        if (!TryAdd(product, 1))
        {
            slots = previousSlots;
            message = "MOCHILA LLENA · LIBERA UN ESPACIO";
            return false;
        }
        message = product == PrototypeInventoryItem.Bat ? "BATE FABRICADO Y EQUIPADO" : "OBJETO FABRICADO";
        return true;
    }

    private bool TryAdd(PrototypeInventoryItem item, int amount)
    {
        if (amount <= 0 || !CanAdd(item, amount)) return false;
        int stackLimit = IsResource(item) ? ResourceStackLimit : 1;
        int remaining = amount;

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            Slot slot = slots[i];
            if (slot == null || slot.item != item || slot.amount >= stackLimit) continue;
            int added = Mathf.Min(remaining, stackLimit - slot.amount);
            slot.amount += added;
            remaining -= added;
        }
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i] != null && slots[i].amount > 0) continue;
            int added = Mathf.Min(remaining, stackLimit);
            slots[i] = new Slot { item = item, amount = added };
            remaining -= added;
        }
        Changed?.Invoke();
        return remaining == 0;
    }

    private bool CanAdd(PrototypeInventoryItem item, int amount)
    {
        if (amount <= 0) return false;
        int stackLimit = IsResource(item) ? ResourceStackLimit : 1;
        int available = 0;
        foreach (Slot slot in slots)
        {
            if (slot == null || slot.amount <= 0) available += stackLimit;
            else if (slot.item == item) available += stackLimit - slot.amount;
        }
        return available >= amount;
    }

    private void Remove(PrototypeInventoryItem item, int amount)
    {
        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            Slot slot = slots[i];
            if (slot == null || slot.item != item) continue;
            int removed = Mathf.Min(amount, slot.amount);
            slot.amount -= removed;
            amount -= removed;
            if (slot.amount <= 0) slots[i] = null;
        }
    }

    private Slot[] CopySlots()
    {
        Slot[] copy = new Slot[slots.Length];
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] != null) copy[i] = new Slot { item = slots[i].item, amount = slots[i].amount };
        return copy;
    }

    private static bool IsResource(PrototypeInventoryItem item)
    {
        return item == PrototypeInventoryItem.Wood
            || item == PrototypeInventoryItem.Stone
            || item == PrototypeInventoryItem.Fiber
            || item == PrototypeInventoryItem.Scrap
            || item == PrototypeInventoryItem.Food;
    }

    private static PrototypeInventoryItem ToItem(PrototypeResourceType type)
    {
        switch (type)
        {
            case PrototypeResourceType.Wood: return PrototypeInventoryItem.Wood;
            case PrototypeResourceType.Stone: return PrototypeInventoryItem.Stone;
            case PrototypeResourceType.Fiber: return PrototypeInventoryItem.Fiber;
            case PrototypeResourceType.Food: return PrototypeInventoryItem.Food;
            default: return PrototypeInventoryItem.Scrap;
        }
    }

    private static string ItemLabel(PrototypeInventoryItem item)
    {
        switch (item)
        {
            case PrototypeInventoryItem.Wood: return "MADERA";
            case PrototypeInventoryItem.Stone: return "PIEDRA";
            case PrototypeInventoryItem.Fiber: return "FIBRA";
            case PrototypeInventoryItem.Scrap: return "CHATARRA";
            case PrototypeInventoryItem.Food: return "COMIDA";
            case PrototypeInventoryItem.Knife: return "CUCHILLO";
            case PrototypeInventoryItem.Bat: return "BATE";
            case PrototypeInventoryItem.Axe: return "HACHA";
            default: return item.ToString().ToUpperInvariant();
        }
    }
}

public enum PrototypeResourceType
{
    Wood,
    Stone,
    Fiber,
    Scrap,
    Food
}

public enum PrototypeInventoryItem
{
    Wood,
    Stone,
    Fiber,
    Scrap,
    Food,
    Knife,
    Bat,
    Axe
}
