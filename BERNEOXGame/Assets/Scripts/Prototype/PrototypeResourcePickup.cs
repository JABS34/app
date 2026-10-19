using UnityEngine;
using System.Collections.Generic;

/// <summary>A simple glowing resource drop for the offline prototype.</summary>
public sealed class PrototypeResourcePickup : MonoBehaviour
{
    private static readonly Dictionary<Color, Material> ResourceMaterials = new Dictionary<Color, Material>();
    [SerializeField] private PrototypeResourceType resourceType;
    [SerializeField, Min(1)] private int amount = 1;

    public PrototypeResourceType ResourceType => resourceType;
    public int Amount => amount;

    public static PrototypeResourcePickup Spawn(Vector3 position, PrototypeResourceType type, int count)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "Recurso_" + type;
        pickup.transform.position = new Vector3(position.x, 0.30f, position.z);
        pickup.transform.localScale = Vector3.one;
        Collider collider = pickup.GetComponent<Collider>();
        collider.isTrigger = true;
        pickup.GetComponent<SphereCollider>().radius = 0.75f;
        PrototypeResourcePickup component = pickup.AddComponent<PrototypeResourcePickup>();
        component.resourceType = type;
        component.amount = Mathf.Max(1, count);
        component.RefreshVisual();
        return component;
    }

    public void RefreshVisual()
    {
        transform.localScale = Vector3.one;
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null) sphere.radius = 0.75f;
        Renderer rootRenderer = GetComponent<Renderer>();
        if (rootRenderer != null) rootRenderer.enabled = false;
        if (transform.Find("Resource_Visual") != null) return;

        Transform visual = new GameObject("Resource_Visual").transform;
        visual.SetParent(transform, false);
        Material main = GetMaterial(resourceType, GetResourceColor(resourceType));
        Material dark = GetMaterial(resourceType, Color.Lerp(GetResourceColor(resourceType), Color.black, 0.38f));
        Material accent = GetMaterial(resourceType, Color.Lerp(GetResourceColor(resourceType), Color.white, 0.42f));

        switch (resourceType)
        {
            case PrototypeResourceType.Wood:
                ResourcePart(PrimitiveType.Cylinder, "Cut Log", visual, new Vector3(-0.08f, -0.10f, 0f), new Vector3(0.22f, 0.48f, 0.22f), main, Quaternion.Euler(90f, 0f, 68f));
                ResourcePart(PrimitiveType.Cylinder, "Split Log", visual, new Vector3(0.10f, -0.02f, 0.08f), new Vector3(0.18f, 0.38f, 0.18f), dark, Quaternion.Euler(90f, 0f, -28f));
                ResourcePart(PrimitiveType.Cylinder, "Short Branch", visual, new Vector3(0f, 0.08f, -0.1f), new Vector3(0.11f, 0.35f, 0.11f), accent, Quaternion.Euler(90f, 0f, 14f));
                break;
            case PrototypeResourceType.Stone:
                ResourcePart(PrimitiveType.Sphere, "Stone Chunk A", visual, new Vector3(-0.18f, -0.10f, 0f), new Vector3(0.42f, 0.32f, 0.36f), main, Quaternion.Euler(18f, 24f, 7f));
                ResourcePart(PrimitiveType.Sphere, "Stone Chunk B", visual, new Vector3(0.15f, -0.14f, 0.06f), new Vector3(0.36f, 0.28f, 0.34f), dark, Quaternion.Euler(0f, 51f, 16f));
                ResourcePart(PrimitiveType.Sphere, "Stone Chip", visual, new Vector3(0.02f, 0.10f, -0.13f), new Vector3(0.26f, 0.22f, 0.22f), accent, Quaternion.Euler(30f, 6f, 2f));
                break;
            case PrototypeResourceType.Fiber:
                ResourcePart(PrimitiveType.Capsule, "Fiber Bundle A", visual, new Vector3(-0.18f, 0f, 0f), new Vector3(0.12f, 0.42f, 0.12f), main, Quaternion.Euler(4f, 0f, 19f));
                ResourcePart(PrimitiveType.Capsule, "Fiber Bundle B", visual, new Vector3(0f, 0f, 0.04f), new Vector3(0.12f, 0.45f, 0.12f), accent, Quaternion.Euler(-8f, 0f, -12f));
                ResourcePart(PrimitiveType.Capsule, "Fiber Bundle C", visual, new Vector3(0.18f, 0f, -0.01f), new Vector3(0.12f, 0.38f, 0.12f), dark, Quaternion.Euler(0f, 0f, -24f));
                ResourcePart(PrimitiveType.Cylinder, "Fiber Tie", visual, new Vector3(0f, -0.05f, 0f), new Vector3(0.2f, 0.08f, 0.2f), dark, Quaternion.identity);
                break;
            case PrototypeResourceType.Scrap:
                ResourcePart(PrimitiveType.Cube, "Bent Metal Plate", visual, new Vector3(-0.12f, -0.04f, 0f), new Vector3(0.54f, 0.13f, 0.36f), main, Quaternion.Euler(12f, 0f, -13f));
                ResourcePart(PrimitiveType.Cube, "Rusty Metal Fragment", visual, new Vector3(0.16f, 0.08f, 0.02f), new Vector3(0.35f, 0.14f, 0.28f), dark, Quaternion.Euler(-8f, 17f, 19f));
                ResourcePart(PrimitiveType.Cylinder, "Exposed Bolt", visual, new Vector3(0f, 0.17f, 0.13f), new Vector3(0.08f, 0.10f, 0.08f), accent, Quaternion.identity);
                break;
            case PrototypeResourceType.Food:
                ResourcePart(PrimitiveType.Cube, "Ration Pouch", visual, new Vector3(0f, -0.02f, 0f), new Vector3(0.44f, 0.50f, 0.30f), main, Quaternion.Euler(0f, 0f, -8f));
                ResourcePart(PrimitiveType.Cube, "Ration Seal", visual, new Vector3(0f, 0.24f, 0f), new Vector3(0.46f, 0.08f, 0.31f), dark, Quaternion.Euler(0f, 0f, -8f));
                ResourcePart(PrimitiveType.Cube, "Medical Cross Horizontal", visual, new Vector3(0f, 0f, 0.157f), new Vector3(0.24f, 0.07f, 0.025f), ColorMaterial(Color.white), Quaternion.identity);
                ResourcePart(PrimitiveType.Cube, "Medical Cross Vertical", visual, new Vector3(0f, 0f, 0.17f), new Vector3(0.07f, 0.24f, 0.025f), ColorMaterial(Color.white), Quaternion.identity);
                break;
        }
    }

    private static Transform ResourcePart(PrimitiveType type, string name, Transform parent, Vector3 position,
        Vector3 scale, Material material, Quaternion rotation)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.transform.localRotation = rotation;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying) Destroy(collider);
            else DestroyImmediate(collider);
        }
        return part.transform;
    }

    private static Material GetMaterial(PrototypeResourceType type, Color color)
    {
        if (ResourceMaterials.TryGetValue(color, out Material existing) && existing != null) return existing;
        Material created = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        created.color = color;
        if (created.HasProperty("_EmissionColor"))
        {
            created.EnableKeyword("_EMISSION");
            created.SetColor("_EmissionColor", color * 0.22f);
        }
        ResourceMaterials[color] = created;
        return created;
    }

    private static Material ColorMaterial(Color color)
    {
        return GetMaterial(PrototypeResourceType.Food, color);
    }

    public static Color GetResourceColor(PrototypeResourceType type)
    {
        switch (type)
        {
            case PrototypeResourceType.Wood: return new Color(0.76f, 0.44f, 0.2f);
            case PrototypeResourceType.Stone: return new Color(0.62f, 0.66f, 0.72f);
            case PrototypeResourceType.Fiber: return new Color(0.53f, 0.78f, 0.42f);
            case PrototypeResourceType.Food: return new Color(0.93f, 0.3f, 0.34f);
            default: return new Color(0.58f, 0.68f, 0.76f);
        }
    }

    private void Awake()
    {
        // The prototype uses a level ground plane at y=0. Keep pickups resting on it;
        // they should not hover or drift while the player is trying to collect them.
        transform.position = new Vector3(transform.position.x, 0.30f, transform.position.z);
    }
}
