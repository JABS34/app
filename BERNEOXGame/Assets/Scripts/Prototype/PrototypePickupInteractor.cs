using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Finds nearby drops and collects them through the contextual action button.</summary>
public sealed class PrototypePickupInteractor : MonoBehaviour
{
    [SerializeField] private float pickupRange = 2.5f;
    private PrototypeResourceInventory inventory;
    private PrototypeMeleeAttack meleeAttack;
    private PrototypeHordeDirector hordeDirector;
    public PrototypeResourcePickup NearbyPickup { get; private set; }
    public PrototypeTreeNode NearbyTree { get; private set; }
    public PrototypeCraftingStation NearbyStation { get; private set; }
    public event Action CraftingRequested;
    public event Action<string> MessageShown;
    public string ActionLabel => NearbyPickup != null ? "RECOGER" : NearbyTree != null ? "TALAR" : NearbyStation != null ? "FABRICAR" : "ATACAR";

    private void Awake()
    {
        inventory = GetComponent<PrototypeResourceInventory>();
        meleeAttack = GetComponent<PrototypeMeleeAttack>();
        hordeDirector = FindAnyObjectByType<PrototypeHordeDirector>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        NearbyPickup = null;
        NearbyTree = null;
        NearbyStation = null;
        float nearest = pickupRange * pickupRange;
        foreach (PrototypeResourcePickup pickup in FindObjectsByType<PrototypeResourcePickup>())
        {
            if (pickup == null) continue;
            float distance = (pickup.transform.position - transform.position).sqrMagnitude;
            if (distance > nearest) continue;
            nearest = distance;
            NearbyPickup = pickup;
        }

        if (NearbyPickup == null)
        {
            foreach (PrototypeTreeNode tree in FindObjectsByType<PrototypeTreeNode>())
            {
                if (tree == null || tree.IsFelled) continue;
                float distance = (tree.transform.position - transform.position).sqrMagnitude;
                if (distance > nearest) continue;
                nearest = distance;
                NearbyTree = tree;
            }
        }

        if (NearbyPickup == null && NearbyTree == null && (hordeDirector == null || !hordeDirector.HordeActive))
        {
            foreach (PrototypeCraftingStation station in FindObjectsByType<PrototypeCraftingStation>())
            {
                float distance = (station.transform.position - transform.position).sqrMagnitude;
                if (distance > nearest) continue;
                nearest = distance;
                NearbyStation = station;
            }
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) InteractNearby();
    }

    public bool InteractNearby()
    {
        if (NearbyPickup != null && inventory != null)
        {
            if (!inventory.TryAddResource(NearbyPickup.ResourceType, NearbyPickup.Amount))
            {
                MessageShown?.Invoke("MOCHILA LLENA · EL MATERIAL SIGUE EN EL SUELO");
                return true;
            }
            Destroy(NearbyPickup.gameObject);
            NearbyPickup = null;
            return true;
        }
        if (NearbyTree != null)
        {
            int woodMultiplier = meleeAttack != null && meleeAttack.EquippedWeapon == PrototypeWeapon.Axe ? 2 : 1;
            NearbyTree.Chop(woodMultiplier);
            return true;
        }
        if (NearbyStation != null)
        {
            CraftingRequested?.Invoke();
            return true;
        }
        return false;
    }
}
