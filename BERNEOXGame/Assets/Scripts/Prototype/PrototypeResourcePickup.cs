using UnityEngine;

/// <summary>A simple glowing resource drop for the offline prototype.</summary>
public sealed class PrototypeResourcePickup : MonoBehaviour
{
    [SerializeField] private PrototypeResourceType resourceType;
    [SerializeField, Min(1)] private int amount = 1;
    private Vector3 basePosition;

    public PrototypeResourceType ResourceType => resourceType;
    public int Amount => amount;

    public static PrototypeResourcePickup Spawn(Vector3 position, PrototypeResourceType type, int count)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = type == PrototypeResourceType.Wood ? "Recurso_Madera" : "Recurso_Chatarra";
        pickup.transform.position = position + Vector3.up * 0.45f;
        pickup.transform.localScale = Vector3.one * 0.48f;
        Collider collider = pickup.GetComponent<Collider>();
        collider.isTrigger = true;
        Renderer renderer = pickup.GetComponent<Renderer>();
        Color color = type == PrototypeResourceType.Wood
            ? new Color(0.76f, 0.44f, 0.2f)
            : new Color(0.58f, 0.68f, 0.76f);
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.color = color;
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.65f);
        }
        renderer.sharedMaterial = material;
        PrototypeResourcePickup component = pickup.AddComponent<PrototypeResourcePickup>();
        component.resourceType = type;
        component.amount = Mathf.Max(1, count);
        return component;
    }

    private void Awake() => basePosition = transform.position;

    private void Update()
    {
        transform.position = basePosition + Vector3.up * (0.12f + Mathf.Sin(Time.time * 2.5f) * 0.1f);
        transform.Rotate(Vector3.up, 70f * Time.deltaTime, Space.World);
    }
}
