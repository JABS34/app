using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Finds nearby drops and collects them through the contextual action button.</summary>
public sealed class PrototypePickupInteractor : MonoBehaviour
{
    [SerializeField] private float pickupRange = 2.5f;
    private PrototypeResourceInventory inventory;
    public PrototypeResourcePickup NearbyPickup { get; private set; }
    public PrototypeTreeNode NearbyTree { get; private set; }
    public bool HasInteraction => NearbyPickup != null || NearbyTree != null;
    public string ActionLabel => NearbyPickup != null ? "RECOGER" : NearbyTree != null ? "TALAR" : "ATACAR";

    private void Awake() => inventory = GetComponent<PrototypeResourceInventory>();

    private void Update()
    {
        NearbyPickup = null;
        NearbyTree = null;
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

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) InteractNearby();
    }

    public bool InteractNearby()
    {
        if (NearbyPickup != null && inventory != null)
        {
            inventory.Add(NearbyPickup.ResourceType, NearbyPickup.Amount);
            Destroy(NearbyPickup.gameObject);
            NearbyPickup = null;
            return true;
        }
        if (NearbyTree != null)
        {
            NearbyTree.Chop();
            return true;
        }
        return false;
    }
}
