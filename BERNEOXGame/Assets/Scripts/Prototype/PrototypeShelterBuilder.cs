using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>Lets the player place snap-aligned wooden shelter pieces during preparation.</summary>
public sealed class PrototypeShelterBuilder : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PrototypeResourceInventory inventory;
    [SerializeField] private PrototypeHordeDirector hordeDirector;
    [SerializeField] private float gridSize = 1.5f;

    private PrototypeShelterPieceType selectedPiece;
    private bool isPlacing;
    private bool hasGroundTarget;
    private Vector3 placementPosition;
    private int rotationSteps;
    private GameObject preview;
    private Renderer previewRenderer;
    private Material previewMaterial;

    public bool IsPlacing => isPlacing;
    public bool CanPlace => isPlacing && hasGroundTarget && !HasPlacementBlocker() && inventory != null
        && inventory.GetAmount(PrototypeResourceType.Wood) >= WoodCost(selectedPiece);
    public string PieceLabel => selectedPiece == PrototypeShelterPieceType.Wall ? "PARED" : "BARRICADA";
    public string PlacementStatus => !isPlacing ? string.Empty
        : !hasGroundTarget ? "TOCA EL SUELO PARA ELEGIR DÓNDE CONSTRUIR"
        : HasPlacementBlocker() ? "UBICACIÓN OCUPADA · TOCA OTRO LUGAR"
        : inventory == null || inventory.GetAmount(PrototypeResourceType.Wood) < WoodCost(selectedPiece)
            ? "FALTA MADERA · " + inventory?.GetAmount(PrototypeResourceType.Wood) + "/" + WoodCost(selectedPiece)
            : PieceLabel + " · " + WoodCost(selectedPiece) + " MADERA · TOCA EL SUELO PARA MOVER";

    public void Initialize(Transform playerTransform, PrototypeResourceInventory playerInventory, PrototypeHordeDirector director)
    {
        player = playerTransform;
        inventory = playerInventory;
        hordeDirector = director;
    }

    private void Update()
    {
        if (!isPlacing || Time.timeScale <= 0f) return;
        if (hordeDirector != null && hordeDirector.HordeActive)
        {
            CancelPlacement();
            return;
        }

        if (!hasGroundTarget && player != null)
            placementPosition = Snap(player.position + player.forward * 4f);
        HandleWorldTap();
        UpdatePreview();
    }

    public bool SelectPiece(PrototypeShelterPieceType piece)
    {
        if (hordeDirector != null && hordeDirector.HordeActive) return false;
        selectedPiece = piece;
        isPlacing = true;
        hasGroundTarget = false;
        rotationSteps = 0;
        EnsurePreview();
        UpdatePreview();
        return true;
    }

    public void RotatePlacement()
    {
        if (isPlacing) rotationSteps = (rotationSteps + 1) % 4;
    }

    public bool PlacePiece()
    {
        if (!isPlacing || !hasGroundTarget || hordeDirector != null && hordeDirector.HordeActive) return false;
        if (inventory == null || HasPlacementBlocker()
            || !inventory.TrySpendResource(PrototypeResourceType.Wood, WoodCost(selectedPiece))) return false;

        CreatePiece(placementPosition, Quaternion.Euler(0f, rotationSteps * 90f, 0f), selectedPiece);
        hasGroundTarget = false;
        return true;
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        hasGroundTarget = false;
        if (preview != null) Destroy(preview);
        preview = null;
        previewRenderer = null;
    }

    private void HandleWorldTap()
    {
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;
                int pointerId = touch.touchId.ReadValue();
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId)) continue;
                SetGroundTarget(touch.position.ReadValue());
                return;
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            SetGroundTarget(Mouse.current.position.ReadValue());
        }
    }

    private void SetGroundTarget(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (!groundPlane.Raycast(ray, out float distance)) return;
        Vector3 point = ray.GetPoint(distance);
        point.x = Mathf.Clamp(point.x, -38f, 38f);
        point.z = Mathf.Clamp(point.z, -38f, 38f);
        placementPosition = Snap(point);
        hasGroundTarget = true;
    }

    private Vector3 Snap(Vector3 point)
    {
        return new Vector3(Mathf.Round(point.x / gridSize) * gridSize, 0f,
            Mathf.Round(point.z / gridSize) * gridSize);
    }

    private bool HasPlacementBlocker()
    {
        Vector3 size = PieceSize(selectedPiece);
        Vector3 center = placementPosition + Vector3.up * (size.y * 0.5f + 0.03f);
        return Physics.CheckBox(center, size * 0.48f, Quaternion.Euler(0f, rotationSteps * 90f, 0f),
            ~0, QueryTriggerInteraction.Ignore);
    }

    private void EnsurePreview()
    {
        if (preview != null) return;
        preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        preview.name = "Shelter Placement Preview";
        Collider collider = preview.GetComponent<Collider>();
        collider.isTrigger = true;
        previewRenderer = preview.GetComponent<Renderer>();
        previewMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        previewMaterial.SetFloat("_Surface", 1f);
        previewMaterial.SetFloat("_Blend", 0f);
        previewMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        previewMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        previewMaterial.SetInt("_ZWrite", 0);
        previewMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        previewMaterial.renderQueue = (int)RenderQueue.Transparent;
        previewRenderer.sharedMaterial = previewMaterial;
        previewRenderer.shadowCastingMode = ShadowCastingMode.Off;
        previewRenderer.receiveShadows = false;
    }

    private void UpdatePreview()
    {
        if (preview == null) return;
        Vector3 size = PieceSize(selectedPiece);
        preview.transform.position = placementPosition + Vector3.up * (size.y * 0.5f + 0.03f);
        preview.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
        preview.transform.localScale = size;
        Color color = CanPlace ? new Color(0.53f, 0.94f, 0.69f, 0.46f) : new Color(1f, 0.30f, 0.38f, 0.5f);
        previewMaterial.color = color;
    }

    private static GameObject CreatePiece(Vector3 position, Quaternion rotation, PrototypeShelterPieceType type)
    {
        Vector3 size = PieceSize(type);
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = type == PrototypeShelterPieceType.Wall ? "Shelter_Wood_Wall" : "Shelter_Wood_Barricade";
        piece.transform.position = position + Vector3.up * (size.y * 0.5f + 0.03f);
        piece.transform.rotation = rotation;
        piece.transform.localScale = size;
        piece.GetComponent<Renderer>().sharedMaterial = GetWoodMaterial();
        piece.AddComponent<PrototypeShelterPiece>().Initialize(type);
        return piece;
    }

    private static Material woodMaterial;

    private static Material GetWoodMaterial()
    {
        if (woodMaterial != null) return woodMaterial;
        woodMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        woodMaterial.color = new Color(0.40f, 0.24f, 0.15f, 1f);
        return woodMaterial;
    }

    private static int WoodCost(PrototypeShelterPieceType type) => type == PrototypeShelterPieceType.Wall ? 8 : 4;

    private static Vector3 PieceSize(PrototypeShelterPieceType type) => type == PrototypeShelterPieceType.Wall
        ? new Vector3(3f, 2.2f, 0.22f)
        : new Vector3(3f, 0.9f, 0.28f);
}

public enum PrototypeShelterPieceType
{
    Wall,
    Barricade
}
