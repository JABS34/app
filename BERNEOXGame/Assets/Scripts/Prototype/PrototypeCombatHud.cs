using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private Image healthFill;
    private Text healthValue;
    private Text zombieCount;
    private Text resourceCount;
    private Text attackLabel;
    private Button attackButton;
    private RectTransform attackButtonRect;
    private CanvasGroup defeatOverlay;
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
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaximumHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.Defeated -= OnPlayerDefeated;
        }
        if (meleeAttack != null) meleeAttack.Attacked -= OnAttackExecuted;
    }

    private void Update()
    {
        if (healthFill == null) return;
        float target = playerHealth.CurrentHealth / playerHealth.MaximumHealth;
        displayedHealth = Mathf.MoveTowards(displayedHealth, target, Time.unscaledDeltaTime * 1.7f);
        healthFill.fillAmount = displayedHealth;
        healthFill.color = Color.Lerp(Coral, Mint, Mathf.InverseLerp(0.12f, 0.9f, displayedHealth));
        if (attackLabel != null) attackLabel.text = pickupInteractor != null && pickupInteractor.CanCollect ? "RECOGER" : "ATACAR";
        if (resourceCount != null && inventory != null)
            resourceCount.text = "MADERA  " + inventory.GetAmount(PrototypeResourceType.Wood) + "     CHATARRA  " + inventory.GetAmount(PrototypeResourceType.Scrap);
        if (zombieCount != null && hordeDirector != null) zombieCount.text = hordeDirector.StatusText;
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
    }

    private void BuildResourceCounter(RectTransform root)
    {
        RectTransform panel = NewRect(root, "Resource Counter Panel");
        Anchor(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -150f), new Vector2(430f, 62f));
        Image background = panel.gameObject.AddComponent<Image>();
        background.color = Ink;
        background.raycastTarget = false;
        resourceCount = MakeText(panel, "Resource Counter", "MADERA  0     CHATARRA  0", 18, FontStyle.Bold, Color.white);
        resourceCount.alignment = TextAnchor.MiddleCenter;
        Anchor(resourceCount.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-20f, -8f));
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
        if (pickupInteractor != null && pickupInteractor.CollectNearby()) return;
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
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
