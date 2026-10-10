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
    private PrototypePlayerHealth player;
    private Transform healthBar;
    private Transform healthBarFill;
    private Renderer healthBarFillRenderer;
    private Camera mainCamera;
    private float pieceHeight;

    public void Initialize(PrototypeShelterPieceType type)
    {
        PieceType = type;
        MaximumHealth = type == PrototypeShelterPieceType.Wall ? 160f : 90f;
        CurrentHealth = MaximumHealth;
        pieceHeight = type == PrototypeShelterPieceType.Wall ? 2.2f : 0.9f;
        player = FindAnyObjectByType<PrototypePlayerHealth>();
        pieceRenderer = GetComponent<Renderer>();
        if (pieceRenderer != null) originalColor = pieceRenderer.sharedMaterial.color;
        CreateHealthBar();
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
        float damage = 1f - CurrentHealth / MaximumHealth;
        if (pieceRenderer != null)
            pieceRenderer.material.color = Color.Lerp(originalColor, new Color(0.72f, 0.22f, 0.18f), damage * 0.45f);
        if (healthBarFill != null)
        {
            float ratio = CurrentHealth / MaximumHealth;
            healthBarFill.localScale = new Vector3(0.96f * ratio, 0.07f, 0.035f);
            healthBarFill.localPosition = new Vector3(-0.48f * (1f - ratio), 0f, 0.035f);
            healthBarFillRenderer.material.color = Color.Lerp(new Color(0.95f, 0.28f, 0.3f), new Color(0.46f, 0.88f, 0.62f), ratio);
        }
    }

    private void LateUpdate()
    {
        if (healthBar == null) return;
        if (mainCamera == null) mainCamera = Camera.main;
        bool visible = IsDamaged && player != null
            && (player.transform.position - transform.position).sqrMagnitude <= 144f;
        healthBar.gameObject.SetActive(visible);
        if (!visible) return;

        healthBar.position = transform.position + Vector3.up * (pieceHeight * 0.5f + 0.23f);
        if (mainCamera != null)
        {
            Vector3 toCamera = mainCamera.transform.position - healthBar.position;
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude > 0.001f) healthBar.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
        }
    }

    private void CreateHealthBar()
    {
        GameObject root = new GameObject("Shelter Health Bar");
        healthBar = root.transform;
        CreateBarPart("Health Bar Background", healthBar, new Vector3(1.08f, 0.11f, 0.045f),
            new Color(0.055f, 0.035f, 0.11f, 1f));
        GameObject fill = CreateBarPart("Health Bar Fill", healthBar, new Vector3(0.96f, 0.07f, 0.035f),
            new Color(0.46f, 0.88f, 0.62f, 1f));
        healthBarFill = fill.transform;
        healthBarFillRenderer = fill.GetComponent<Renderer>();
        healthBar.gameObject.SetActive(false);
    }

    private static GameObject CreateBarPart(string objectName, Transform parent, Vector3 size, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localScale = size;
        part.GetComponent<Renderer>().material.color = color;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part;
    }

    private void OnDestroy()
    {
        if (healthBar != null) Destroy(healthBar.gameObject);
    }
}
