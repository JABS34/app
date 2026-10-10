using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Finds nearby drops and collects them through the contextual action button.</summary>
public sealed class PrototypePickupInteractor : MonoBehaviour
{
    [SerializeField] private float pickupRange = 2.5f;
    private PrototypeResourceInventory inventory;
    public PrototypeResourcePickup NearbyPickup { get; private set; }
    public bool CanCollect => NearbyPickup != null;

    private void Awake() => inventory = GetComponent<PrototypeResourceInventory>();

    private void Update()
    {
        NearbyPickup = null;
        float nearest = pickupRange * pickupRange;
        foreach (PrototypeResourcePickup pickup in FindObjectsByType<PrototypeResourcePickup>())
        {
            if (pickup == null) continue;
            float distance = (pickup.transform.position - transform.position).sqrMagnitude;
            if (distance > nearest) continue;
            nearest = distance;
            NearbyPickup = pickup;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) CollectNearby();
    }

    public bool CollectNearby()
    {
        if (NearbyPickup == null || inventory == null) return false;
        inventory.Add(NearbyPickup.ResourceType, NearbyPickup.Amount);
        Destroy(NearbyPickup.gameObject);
        NearbyPickup = null;
        return true;
    }
}
