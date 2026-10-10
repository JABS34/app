using UnityEngine;

/// <summary>Three-hit tree resource node used during the preparation phase.</summary>
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PrototypeTreeNode : MonoBehaviour
{
    [SerializeField, Min(1)] private int hitsToFell = 3;
    [SerializeField, Min(1)] private int woodYield = 4;
    private int hitsRemaining;
    private Transform foliage;
    private Vector3 foliageStartPosition;

    public bool IsFelled { get; private set; }

    private void Awake()
    {
        if (foliage == null)
        {
            Transform visuals = transform.Find("Tree Visuals");
            if (visuals != null) foliage = visuals;
        }
        foliageStartPosition = foliage != null ? foliage.localPosition : Vector3.zero;
        hitsRemaining = hitsToFell;
    }

    public void Initialize(Transform foliageRoot)
    {
        foliage = foliageRoot;
        foliageStartPosition = foliage != null ? foliage.localPosition : Vector3.zero;
        hitsRemaining = hitsToFell;
    }

    public void Chop(int woodMultiplier = 1)
    {
        if (IsFelled) return;
        hitsRemaining--;
        if (hitsRemaining > 0)
        {
            if (foliage != null)
                foliage.localPosition = foliageStartPosition + new Vector3(Random.Range(-0.16f, 0.16f), 0f, Random.Range(-0.16f, 0.16f));
            return;
        }

        IsFelled = true;
        PrototypeResourcePickup.Spawn(transform.position, PrototypeResourceType.Wood, woodYield * Mathf.Max(1, woodMultiplier));
        Destroy(gameObject);
    }
}
