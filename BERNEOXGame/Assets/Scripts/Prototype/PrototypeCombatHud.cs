using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Touch-friendly health and melee controls for the first combat scene.</summary>
public sealed class PrototypeCombatHud : MonoBehaviour
{
    private static Sprite circleSprite;
    private PrototypePlayerHealth playerHealth;
    private PrototypeMeleeAttack meleeAttack;
    private PrototypePickupInteractor pickupInteractor;
    private PrototypeResourceInventory inventory;
    private PrototypeHordeDirector hordeDirector;
    private PrototypeShelterBuilder shelterBuilder;
    private Image healthFill;
    private Text healthValue;
    private Text zombieCount;
    private Text resourceCount;
    private Text attackLabel;
    private Text[] inventorySlotLabels = new Text[PrototypeResourceInventory.SlotCount];
    private Text craftingMessage;
    private Text toastLabel;
    private CanvasGroup toastGroup;
    private float toastVisibleUntil;
    private Button attackButton;
    private RectTransform attackButtonRect;
    private CanvasGroup defeatOverlay;
    private GameObject inventoryOverlay;
    private GameObject craftingOverlay;
    private GameObject buildingOverlay;
    private GameObject buildingControls;
    private Text buildingStatus;
    private float displayedHealth = 1f;
    private Coroutine attackPulse;

    private static readonly Color Ink = new Color(0.055f, 0.035f, 0.11f, 0.88f);
    private static readonly Color Violet = new Color(0.55f, 0.32f, 0.92f, 1f);
    private static readonly Color Mint = new Color(0.77f, 0.91f, 0.69f, 1f);
    private static readonly Color Coral = new Color(0.98f, 0.34f, 0.43f, 1f);

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PrototypePlayerHealth>();
        meleeAttack = FindAnyObjectByType<PrototypeMeleeAttack>();
        pickupInteractor = FindAnyObjectByType<PrototypePickupInteractor>();
        inventory = FindAnyObjectByType<PrototypeResourceInventory>();
        hordeDirector = FindAnyObjectByType<PrototypeHordeDirector>();
        shelterBuilder = FindAnyObjectByType<PrototypeShelterBuilder>();
        if (playerHealth == null || meleeAttack == null)
        {
            Debug.LogError("Combat HUD needs a player health component and melee attack component.");
            enabled = false;
            return;
        }

        BuildInterface();
        playerHealth.HealthChanged += OnHealthChanged;
        playerHealth.Defeated += OnPlayerDefeated;
        meleeAttack.Attacked += OnAttackExecuted;
        if (inventory != null) inventory.Changed += RefreshInventory;
        if (pickupInteractor != null)
        {
            pickupInteractor.CraftingRequested += OpenCrafting;
            pickupInteractor.MessageShown += ShowCraftingMessage;
        }
        Time.timeScale = 1f;
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaximumHealth);
        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.Defeated -= OnPlayerDefeated;
        }
        if (meleeAttack != null) meleeAttack.Attacked -= OnAttackExecuted;
        if (inventory != null) inventory.Changed -= RefreshInventory;
        if (pickupInteractor != null)
        {
            pickupInteractor.CraftingRequested -= OpenCrafting;
            pickupInteractor.MessageShown -= ShowCraftingMessage;
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (healthFill == null) return;
        float target = playerHealth.CurrentHealth / playerHealth.MaximumHealth;
        displayedHealth = Mathf.MoveTowards(displayedHealth, target, Time.unscaledDeltaTime * 1.7f);
        healthFill.fillAmount = displayedHealth;
        healthFill.color = Color.Lerp(Coral, Mint, Mathf.InverseLerp(0.12f, 0.9f, displayedHealth));
        if (attackLabel != null) attackLabel.text = pickupInteractor != null ? pickupInteractor.ActionLabel : "ATACAR";
        if (resourceCount != null && inventory != null)
            resourceCount.text = "MADERA " + inventory.GetAmount(PrototypeResourceType.Wood)
                + "   PIEDRA " + inventory.GetAmount(PrototypeResourceType.Stone)
                + "   FIBRA " + inventory.GetAmount(PrototypeResourceType.Fiber)
                + "   CHATARRA " + inventory.GetAmount(PrototypeResourceType.Scrap)
                + "   COMIDA " + inventory.GetAmount(PrototypeResourceType.Food);
        if (zombieCount != null && hordeDirector != null) zombieCount.text = hordeDirector.StatusText;
        if (buildingStatus != null && shelterBuilder != null && shelterBuilder.IsPlacing)
            buildingStatus.text = shelterBuilder.PlacementStatus;
        if (buildingControls != null && shelterBuilder != null && !shelterBuilder.IsPlacing)
            buildingControls.SetActive(false);
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame) ToggleInventory();
        if (Keyboard.current != null && shelterBuilder != null && shelterBuilder.IsPlacing)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) shelterBuilder.RotatePlacement();
            if (Keyboard.current.enterKey.wasPressedThisFrame) TryPlaceShelterPiece();
            if (Keyboard.current.escapeKey.wasPressedThisFrame) CancelShelterPlacement();
        }
        if (toastGroup != null && toastGroup.alpha > 0f && Time.unscaledTime > toastVisibleUntil)
            toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 0f, Time.unscaledDeltaTime * 2.5f);
    }

    private void BuildInterface()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("Combat HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = canvasObject.GetComponent<RectTransform>();
        BuildHealthPanel(root);
        BuildZombieCounter(root);
        BuildResourceCounter(root);
        BuildAttackButton(root);
        BuildInventoryButton(root);
        BuildConstructionButton(root);
        BuildInventoryOverlay(root);
        BuildCraftingOverlay(root);
        BuildConstructionOverlay(root);
        BuildToast(root);
    }

    private void BuildResourceCounter(RectTransform root)
    {
        RectTransform panel = NewRect(root, "Resource Counter Panel");
        Anchor(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -150f), new Vector2(640f, 62f));
        Image background = panel.gameObject.AddComponent<Image>();
        background.color = Ink;
        background.raycastTarget = false;
        resourceCount = MakeText(panel, "Resource Counter", "MADERA 0   PIEDRA 0   FIBRA 0   CHATARRA 0   COMIDA 0", 15, FontStyle.Bold, Color.white);
        resourceCount.alignment = TextAnchor.MiddleCenter;
        Anchor(resourceCount.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-20f, -8f));
    }

    private void BuildInventoryButton(RectTransform root)
    {
        GameObject buttonObject = new GameObject("Inventory Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(root, false);
        Anchor(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(200f, 62f));
        Image image = buttonObject.GetComponent<Image>();
        image.color = Violet;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ToggleInventory);
        Text label = MakeText(buttonObject.transform, "Inventory Button Label", "MOCHILA  ·  I", 18, FontStyle.Bold, Color.white);
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        label.alignment = TextAnchor.MiddleCenter;
    }

    private void BuildConstructionButton(RectTransform root)
    {
        GameObject buttonObject = new GameObject("Construction Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(root, false);
        Anchor(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -124f), new Vector2(200f, 58f));
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.32f, 0.62f, 0.44f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OpenConstruction);
        Text label = MakeText(buttonObject.transform, "Construction Button Label", "CONSTRUIR", 17, FontStyle.Bold, Color.white);
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        label.alignment = TextAnchor.MiddleCenter;
    }

    private void BuildConstructionOverlay(RectTransform root)
    {
        RectTransform overlay = NewRect(root, "Construction Overlay");
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.SetAsLastSibling();
        buildingOverlay = overlay.gameObject;
        buildingOverlay.AddComponent<Image>().color = new Color(0.025f, 0.015f, 0.055f, 0.84f);
        RectTransform card = NewRect(overlay, "Construction Card");
        Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 500f));
        card.gameObject.AddComponent<Image>().color = Ink;
        Text title = MakeText(card, "Construction Title", "CONSTRUYE TU REFUGIO", 30, FontStyle.Bold, Color.white);
        Anchor(title.rectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f), Vector2.zero, Vector2.zero);
        title.alignment = TextAnchor.MiddleCenter;
        Button close = CreateOverlayButton(card, "Close Construction", "VOLVER", new Vector2(0f, -188f), new Vector2(180f, 58f), Violet);
        close.onClick.AddListener(() => buildingOverlay.SetActive(false));
        Button wall = CreateOverlayButton(card, "Build Wall", "PARED DE MADERA\n8 madera\nBloquea el paso", new Vector2(-190f, 24f), new Vector2(330f, 210f), new Color(0.35f, 0.48f, 0.36f, 1f));
        wall.onClick.AddListener(() => BeginShelterPlacement(PrototypeShelterPieceType.Wall));
        Button barricade = CreateOverlayButton(card, "Build Barricade", "BARRICADA\n4 madera\nDefensa baja", new Vector2(190f, 24f), new Vector2(330f, 210f), new Color(0.43f, 0.34f, 0.26f, 1f));
        barricade.onClick.AddListener(() => BeginShelterPlacement(PrototypeShelterPieceType.Barricade));
        buildingOverlay.SetActive(false);

        RectTransform controls = NewRect(root, "Building Placement Controls");
        Anchor(controls, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(780f, 180f));
        controls.gameObject.AddComponent<Image>().color = Ink;
        buildingControls = controls.gameObject;
        buildingStatus = MakeText(controls, "Building Status", "TOCA EL SUELO PARA ELEGIR EL LUGAR", 18, FontStyle.Bold, Mint);
        Anchor(buildingStatus.rectTransform, new Vector2(0.04f, 0.57f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);
        buildingStatus.alignment = TextAnchor.MiddleCenter;
        Button rotate = CreateOverlayButton(controls, "Rotate Piece", "ROTAR", new Vector2(-230f, -48f), new Vector2(190f, 62f), Violet);
        rotate.onClick.AddListener(() => shelterBuilder?.RotatePlacement());
        Button place = CreateOverlayButton(controls, "Place Piece", "COLOCAR", new Vector2(0f, -48f), new Vector2(210f, 62f), new Color(0.25f, 0.62f, 0.40f, 1f));
        place.onClick.AddListener(TryPlaceShelterPiece);
        Button cancel = CreateOverlayButton(controls, "Cancel Piece", "CANCELAR", new Vector2(230f, -48f), new Vector2(190f, 62f), Coral);
        cancel.onClick.AddListener(CancelShelterPlacement);
        buildingControls.SetActive(false);
    }

    private void OpenConstruction()
    {
        if (shelterBuilder == null) { ShowToast("CONSTRUCCIÓN NO DISPONIBLE"); return; }
        if (hordeDirector != null && hordeDirector.HordeActive) { ShowToast("CONSTRUYE ENTRE HORDAS"); return; }
        if (inventoryOverlay != null) inventoryOverlay.SetActive(false);
        if (craftingOverlay != null) craftingOverlay.SetActive(false);
        Time.timeScale = 0f;
        buildingOverlay.SetActive(true);
    }

    private void BeginShelterPlacement(PrototypeShelterPieceType type)
    {
        Time.timeScale = 1f;
        buildingOverlay.SetActive(false);
        if (shelterBuilder == null || !shelterBuilder.SelectPiece(type))
        {
            ShowToast("CONSTRUYE ENTRE HORDAS");
            return;
        }
        buildingControls.SetActive(true);
    }

    private void TryPlaceShelterPiece()
    {
        if (shelterBuilder == null) return;
        if (shelterBuilder.PlacePiece()) ShowToast(shelterBuilder.PieceLabel + " COLOCADA");
        else ShowToast(shelterBuilder.PlacementStatus.Contains("MADERA") ? "NO HAY MADERA SUFICIENTE" : "UBICACIÓN NO VÁLIDA");
    }

    private void CancelShelterPlacement()
    {
        shelterBuilder?.CancelPlacement();
        if (buildingControls != null) buildingControls.SetActive(false);
        Time.timeScale = 1f;
    }

    private void BuildInventoryOverlay(RectTransform root)
    {
        RectTransform overlay = NewRect(root, "Inventory Overlay");
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.SetAsLastSibling();
        inventoryOverlay = overlay.gameObject;
        Image scrim = inventoryOverlay.AddComponent<Image>();
        scrim.color = new Color(0.025f, 0.015f, 0.055f, 0.84f);

        RectTransform card = NewRect(overlay, "Inventory Card");
        Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 590f));
        card.gameObject.AddComponent<Image>().color = Ink;
        Text title = MakeText(card, "Inventory Title", "MOCHILA  ·  12 ESPACIOS", 30, FontStyle.Bold, Color.white);
        Anchor(title.rectTransform, new Vector2(0.08f, 0.84f), new Vector2(0.78f, 0.97f), Vector2.zero, Vector2.zero);
        title.alignment = TextAnchor.MiddleLeft;
        Button close = CreateOverlayButton(card, "Close Inventory", "CERRAR", new Vector2(0f, 236f), new Vector2(150f, 56f), Violet);
        close.onClick.AddListener(ClosePanels);

        for (int i = 0; i < PrototypeResourceInventory.SlotCount; i++)
        {
            int column = i % 4;
            int row = i / 4;
            RectTransform slot = NewRect(card, "Inventory Slot " + (i + 1));
            Anchor(slot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-330f + column * 220f, 112f - row * 122f), new Vector2(204f, 100f));
            slot.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.12f, 0.23f, 1f);
            inventorySlotLabels[i] = MakeText(slot, "Slot Contents", "VACÍO", 18, FontStyle.Bold, new Color(0.86f, 0.82f, 0.93f, 1f));
            Anchor(inventorySlotLabels[i].rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12f, -10f));
            inventorySlotLabels[i].alignment = TextAnchor.MiddleCenter;
        }
        inventoryOverlay.SetActive(false);
    }

    private void BuildCraftingOverlay(RectTransform root)
    {
        RectTransform overlay = NewRect(root, "Crafting Overlay");
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.SetAsLastSibling();
        craftingOverlay = overlay.gameObject;
        craftingOverlay.AddComponent<Image>().color = new Color(0.025f, 0.015f, 0.055f, 0.84f);

        RectTransform card = NewRect(overlay, "Crafting Card");
        Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 520f));
        card.gameObject.AddComponent<Image>().color = Ink;
        Text title = MakeText(card, "Crafting Title", "MESA DE TRABAJO", 30, FontStyle.Bold, Color.white);
        Anchor(title.rectTransform, new Vector2(0.08f, 0.82f), new Vector2(0.72f, 0.96f), Vector2.zero, Vector2.zero);
        title.alignment = TextAnchor.MiddleLeft;
        Button close = CreateOverlayButton(card, "Close Crafting", "CERRAR", new Vector2(0f, 208f), new Vector2(150f, 56f), Violet);
        close.onClick.AddListener(ClosePanels);

        Button bat = CreateOverlayButton(card, "Craft Bat", "BATE\n6 madera + 2 fibra\nDaño 18  ·  alcance 1,4", new Vector2(-190f, 28f), new Vector2(330f, 220f), new Color(0.43f, 0.25f, 0.68f, 1f));
        bat.onClick.AddListener(CraftBat);
        Button axe = CreateOverlayButton(card, "Craft Axe", "HACHA\n4 madera + 3 piedra + 2 fibra\nDaño 28  ·  madera ×2", new Vector2(190f, 28f), new Vector2(330f, 220f), new Color(0.43f, 0.25f, 0.68f, 1f));
        axe.onClick.AddListener(CraftAxe);

        craftingMessage = MakeText(card, "Crafting Message", "FABRICA AQUÍ DURANTE LA PREPARACIÓN", 18, FontStyle.Bold, Mint);
        Anchor(craftingMessage.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.26f), Vector2.zero, Vector2.zero);
        craftingMessage.alignment = TextAnchor.MiddleCenter;
        craftingOverlay.SetActive(false);
    }

    private void BuildToast(RectTransform root)
    {
        RectTransform panel = NewRect(root, "Interaction Toast");
        Anchor(panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 250f), new Vector2(720f, 66f));
        panel.gameObject.AddComponent<Image>().color = Ink;
        toastGroup = panel.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastLabel = MakeText(panel, "Interaction Toast Label", string.Empty, 18, FontStyle.Bold, Color.white);
        Anchor(toastLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, -8f));
        toastLabel.alignment = TextAnchor.MiddleCenter;
    }

    private Button CreateOverlayButton(Transform parent, string name, string value, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        Image image = go.GetComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        Text label = MakeText(go.transform, name + " Label", value, 20, FontStyle.Bold, Color.white);
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-18f, -16f));
        label.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private void ToggleInventory()
    {
        if (inventoryOverlay == null) return;
        if (!inventoryOverlay.activeSelf && hordeDirector != null && hordeDirector.HordeActive)
        {
            ShowToast("LA MOCHILA SE ABRE ENTRE HORDAS");
            return;
        }
        bool open = !inventoryOverlay.activeSelf;
        if (craftingOverlay != null) craftingOverlay.SetActive(false);
        inventoryOverlay.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
        if (open) RefreshInventory();
    }

    private void OpenCrafting()
    {
        if (craftingOverlay == null) return;
        if (inventoryOverlay != null) inventoryOverlay.SetActive(false);
        craftingOverlay.SetActive(true);
        Time.timeScale = 0f;
        ShowCraftingMessage("FABRICA AQUÍ DURANTE LA PREPARACIÓN");
    }

    private void ClosePanels()
    {
        if (inventoryOverlay != null) inventoryOverlay.SetActive(false);
        if (craftingOverlay != null) craftingOverlay.SetActive(false);
        Time.timeScale = 1f;
    }

    private void RefreshInventory()
    {
        if (inventory == null || inventorySlotLabels == null) return;
        for (int i = 0; i < inventorySlotLabels.Length; i++)
            if (inventorySlotLabels[i] != null) inventorySlotLabels[i].text = inventory.GetSlotLabel(i);
    }

    private void CraftBat()
    {
        if (hordeDirector != null && hordeDirector.HordeActive)
        {
            ShowCraftingMessage("ESPERA A QUE TERMINE LA HORDA");
            return;
        }
        string message;
        if (inventory == null) message = "INVENTARIO NO DISPONIBLE";
        else if (inventory.TryCraftBat(out message)) meleeAttack.Equip(PrototypeWeapon.Bat);
        ShowCraftingMessage(message);
    }

    private void CraftAxe()
    {
        if (hordeDirector != null && hordeDirector.HordeActive)
        {
            ShowCraftingMessage("ESPERA A QUE TERMINE LA HORDA");
            return;
        }
        string message;
        if (inventory == null) message = "INVENTARIO NO DISPONIBLE";
        else if (inventory.TryCraftAxe(out message)) meleeAttack.Equip(PrototypeWeapon.Axe);
        ShowCraftingMessage(message);
    }

    private void ShowCraftingMessage(string message)
    {
        if (craftingMessage != null) craftingMessage.text = message;
        if (craftingOverlay == null || !craftingOverlay.activeSelf) ShowToast(message);
    }

    private void ShowToast(string message)
    {
        if (toastLabel == null || toastGroup == null) return;
        toastLabel.text = message;
        toastGroup.alpha = 1f;
        toastVisibleUntil = Time.unscaledTime + 2.2f;
    }

    private void BuildHealthPanel(RectTransform root)
    {
        RectTransform panel = NewRect(root, "Health Panel");
        Anchor(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -42f), new Vector2(390f, 94f));
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = Ink;
        panelImage.raycastTarget = false;

        Text label = MakeText(panel, "Health Label", "VIDA", 23, FontStyle.Bold, Mint);
        Anchor(label.rectTransform, Vector2.zero, Vector2.zero, new Vector2(20f, 50f), new Vector2(115f, 30f));

        healthValue = MakeText(panel, "Health Value", "100 / 100", 19, FontStyle.Bold, Color.white);
        healthValue.alignment = TextAnchor.MiddleRight;
        Anchor(healthValue.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 50f), new Vector2(145f, 30f));

        RectTransform barRect = NewRect(panel, "Health Bar Track");
        Anchor(barRect, Vector2.zero, Vector2.zero, new Vector2(20f, 19f), new Vector2(-40f, 15f));
        barRect.anchorMax = new Vector2(1f, 0f);
        Image track = barRect.gameObject.AddComponent<Image>();
        track.color = new Color(1f, 1f, 1f, 0.16f);
        track.raycastTarget = false;

        RectTransform fillRect = NewRect(barRect, "Health Fill");
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        healthFill = fillRect.gameObject.AddComponent<Image>();
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFill.color = Mint;
        healthFill.raycastTarget = false;
    }

    private void BuildZombieCounter(RectTransform root)
    {
        RectTransform panel = NewRect(root, "Zombie Counter Panel");
        Anchor(panel, Vector2.one, Vector2.one, new Vector2(-42f, -42f), new Vector2(315f, 76f));
        Image background = panel.gameObject.AddComponent<Image>();
        background.color = Ink;
        background.raycastTarget = false;

        zombieCount = MakeText(panel, "Zombie Counter", "PREPÁRATE", 20, FontStyle.Bold, Color.white);
        zombieCount.alignment = TextAnchor.MiddleCenter;
        Anchor(zombieCount.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -16f));
    }

    private void BuildAttackButton(RectTransform root)
    {
        RectTransform ring = NewRect(root, "Attack Button Halo");
        Anchor(ring, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-153f, 157f), new Vector2(232f, 232f));
        ring.pivot = new Vector2(0.5f, 0.5f);
        Image halo = ring.gameObject.AddComponent<Image>();
        halo.sprite = GetCircleSprite();
        halo.color = new Color(Violet.r, Violet.g, Violet.b, 0.38f);
        halo.raycastTarget = false;

        GameObject buttonObject = new GameObject("Attack Button", typeof(RectTransform), typeof(Image), typeof(Button));
        attackButtonRect = buttonObject.GetComponent<RectTransform>();
        attackButtonRect.SetParent(ring, false);
        attackButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        attackButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        attackButtonRect.pivot = new Vector2(0.5f, 0.5f);
        attackButtonRect.sizeDelta = new Vector2(192f, 192f);
        Image face = buttonObject.GetComponent<Image>();
        face.sprite = GetCircleSprite();
        face.color = Coral;
        attackButton = buttonObject.GetComponent<Button>();
        attackButton.targetGraphic = face;
        ColorBlock colors = attackButton.colors;
        colors.normalColor = Coral;
        colors.highlightedColor = new Color(1f, 0.48f, 0.55f, 1f);
        colors.pressedColor = new Color(0.77f, 0.22f, 0.34f, 1f);
        colors.disabledColor = new Color(Coral.r, Coral.g, Coral.b, 0.32f);
        colors.fadeDuration = 0.08f;
        attackButton.colors = colors;
        attackButton.onClick.AddListener(OnAttackPressed);

        attackLabel = MakeText(buttonObject.transform, "Attack Label", "ATACAR", 25, FontStyle.Bold, Color.white);
        attackLabel.alignment = TextAnchor.MiddleCenter;
        Anchor(attackLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-16f, -16f));
    }

    private void OnAttackPressed()
    {
        if (pickupInteractor != null && pickupInteractor.InteractNearby()) return;
        if (meleeAttack != null && meleeAttack.enabled) meleeAttack.Attack();
    }

    private void OnAttackExecuted()
    {
        if (attackPulse != null) StopCoroutine(attackPulse);
        attackPulse = StartCoroutine(PulseAttackButton());
    }

    private IEnumerator PulseAttackButton()
    {
        if (attackButtonRect == null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.12f)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Sin(Mathf.Clamp01(elapsed / 0.12f) * Mathf.PI);
            attackButtonRect.localScale = Vector3.one * (1f + amount * 0.09f);
            yield return null;
        }
        attackButtonRect.localScale = Vector3.one;
        attackPulse = null;
    }

    private void OnHealthChanged(float current, float maximum)
    {
        if (healthValue != null) healthValue.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(maximum);
        if (healthFill != null) healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
    }

    private void OnPlayerDefeated()
    {
        if (attackButton != null) attackButton.interactable = false;
        if (defeatOverlay != null) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        RectTransform overlay = NewRect(canvas.transform, "Defeat Overlay");
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        Image scrim = overlay.gameObject.AddComponent<Image>();
        scrim.color = new Color(0.025f, 0.012f, 0.06f, 0.86f);
        defeatOverlay = overlay.gameObject.AddComponent<CanvasGroup>();
        defeatOverlay.alpha = 0f;

        RectTransform card = NewRect(overlay, "Defeat Card");
        Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 390f));
        Image cardSurface = card.gameObject.AddComponent<Image>();
        cardSurface.color = Ink;

        Text title = MakeText(card, "Defeat Title", "TE HAN ALCANZADO", 38, FontStyle.Bold, Color.white);
        Anchor(title.rectTransform, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);
        title.alignment = TextAnchor.MiddleCenter;
        Text detail = MakeText(card, "Defeat Detail", "Vuelve a intentarlo y despeja la zona.", 22, FontStyle.Normal, new Color(0.82f, 0.77f, 0.9f, 1f));
        Anchor(detail.rectTransform, new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.58f), Vector2.zero, Vector2.zero);
        detail.alignment = TextAnchor.MiddleCenter;

        Button retry = CreateRetryButton(card);
        retry.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        StartCoroutine(FadeDefeatOverlay());
    }

    private IEnumerator FadeDefeatOverlay()
    {
        float elapsed = 0f;
        while (elapsed < 0.22f && defeatOverlay != null)
        {
            elapsed += Time.unscaledDeltaTime;
            defeatOverlay.alpha = Mathf.Clamp01(elapsed / 0.22f);
            yield return null;
        }
        if (defeatOverlay != null) defeatOverlay.alpha = 1f;
    }

    private Button CreateRetryButton(Transform parent)
    {
        GameObject go = new GameObject("Retry Button", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Anchor(rect, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(370f, 78f));
        Image image = go.GetComponent<Image>();
        image.color = Violet;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        Text label = MakeText(rect, "Retry Label", "INTENTAR DE NUEVO", 22, FontStyle.Bold, Color.white);
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        label.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
            eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        if (inputModule.actionsAsset == null) inputModule.AssignDefaultActions();
    }

    private static Text MakeText(Transform parent, string name, string value, int size, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static RectTransform NewRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = min == max ? min : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Combat HUD Circle";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float edge = 0.48f - Mathf.Sqrt(dx * dx + dy * dy);
            byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(edge * size * 0.8f) * 255f);
            texture.SetPixel(x, y, new Color32(255, 255, 255, alpha));
        }
        texture.Apply(false, true);
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        circleSprite.name = "Combat HUD Circle Sprite";
        return circleSprite;
    }
}
