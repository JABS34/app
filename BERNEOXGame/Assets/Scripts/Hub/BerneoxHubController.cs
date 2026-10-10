using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Runtime-built BERNEOX launcher prototype. It deliberately uses no network,
/// account, currency, or reward data until those services are implemented.
/// </summary>
public sealed class BerneoxHubController : MonoBehaviour
{
    private const string DarkThemeKey = "berneox.theme.dark";
    private const string ReducedMotionKey = "berneox.motion.reduced";

    private static readonly Color DarkBackground = Hex("100A20");
    private static readonly Color DarkSurface = Hex("1A1430");
    private static readonly Color DarkRaised = Hex("241A3E");
    private static readonly Color DarkText = Hex("F5F1FA");
    private static readonly Color DarkSecondary = Hex("C7B4EB");
    private static readonly Color DarkAccent = Hex("6E35DD");
    private static readonly Color DarkMint = Hex("D9F2CC");
    private static readonly Color LightBackground = Hex("F5F1FC");
    private static readonly Color LightSurface = Hex("FFFFFF");
    private static readonly Color LightRaised = Hex("ECE5F8");
    private static readonly Color LightText = Hex("261A3D");
    private static readonly Color LightSecondary = Hex("514363");
    private static readonly Color LightAccent = Hex("5A2DC6");
    private static readonly Color LightMint = Hex("47633F");

    private readonly List<Action<bool>> themeBindings = new List<Action<bool>>();
    private bool isDark;
    private bool reduceMotion;
    private bool modalOpen;
    private CanvasGroup splashGroup;
    private CanvasGroup homeGroup;
    private Text[] splashLetters;
    private ScrollRect homeScroll;
    private Image pageBackground;
    private Text themeButtonLabel;
    private Text featuredStatus;
    private Text featuredTitle;
    private Text featuredDescription;
    private Text featuredButtonLabel;
    private HubWorldMotion worldMotion;
    private Font uiFont;
    private readonly Dictionary<string, Transform> showcaseModels = new Dictionary<string, Transform>();
    private string selectedGame = "Zombie Survival";

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        Application.targetFrameRate = 60;
        isDark = PlayerPrefs.GetInt(DarkThemeKey, 1) != 0;
        reduceMotion = PlayerPrefs.GetInt(ReducedMotionKey, 0) != 0;
        uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        BuildWorld();
        BuildInterface();
        ApplyTheme();
        if (worldMotion != null) worldMotion.SetReducedMotion(reduceMotion);
        StartCoroutine(PlayOpening());
    }

    private void BuildWorld()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("BERNEOX Hub Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = DarkBackground;
        camera.orthographic = true;
        camera.orthographicSize = 5.7f;
        camera.transform.position = new Vector3(0f, 1.1f, -13f);
        camera.transform.rotation = Quaternion.Euler(3f, 0f, 0f);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 80f;

        RenderSettings.ambientLight = new Color(0.31f, 0.27f, 0.40f);
        RenderSettings.reflectionIntensity = 0.1f;

        GameObject lightObject = new GameObject("Hub Soft Key Light");
        Light keyLight = lightObject.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.2f;
        keyLight.color = new Color(0.77f, 0.68f, 1f);
        keyLight.shadows = LightShadows.None;
        lightObject.transform.rotation = Quaternion.Euler(30f, -25f, 0f);

        Transform world = new GameObject("Night Beacon World").transform;
        Material groundMaterial = MakeMaterial("Hub Ground", Hex("211936"));
        Material treeMaterial = MakeMaterial("Night Fir", Hex("17162B"));
        Material trunkMaterial = MakeMaterial("Satin Bark", Hex("342844"));
        Material beaconMaterial = MakeMaterial("Signal Violet", Hex("BDA2FF"), Hex("8C54FF"));
        Material zombieMaterial = MakeMaterial("Friendly Zombie", Hex("566B58"));
        Material zombieHighlight = MakeMaterial("Zombie Highlight", Hex("81917A"));
        Material eyeMaterial = MakeMaterial("Signal Eyes", Hex("D9F2CC"), Hex("9DCB82"));

        CreatePrimitive(PrimitiveType.Cube, "Night Ground", world,
            new Vector3(0f, -2.15f, 5.4f), new Vector3(18f, 0.45f, 13f), groundMaterial);

        CreateTree(world, -4.2f, -0.3f, 4.9f, 2.6f, treeMaterial, trunkMaterial);
        CreateTree(world, -3.2f, -0.45f, 5.2f, 1.9f, treeMaterial, trunkMaterial);
        CreateTree(world, 3.9f, -0.32f, 5.4f, 2.7f, treeMaterial, trunkMaterial);
        CreateTree(world, 4.8f, -0.44f, 5.2f, 1.8f, treeMaterial, trunkMaterial);
        CreateTree(world, -2.35f, -0.55f, 6.8f, 1.45f, treeMaterial, trunkMaterial);
        CreateTree(world, 2.45f, -0.62f, 6.7f, 1.5f, treeMaterial, trunkMaterial);

        Transform beacon = CreateBeacon(world, new Vector3(-1.35f, -0.65f, 3.6f), beaconMaterial, groundMaterial);
        Transform zombie = CreateZombie(world, new Vector3(1.25f, -0.72f, 3.2f), zombieMaterial, zombieHighlight, eyeMaterial);
        showcaseModels.Add("Zombie Survival", zombie);
        showcaseModels.Add("Reflex", CreateReflexModel(world, new Vector3(1.25f, -0.72f, 3.2f)));
        showcaseModels.Add("Memory", CreateMemoryModel(world, new Vector3(1.25f, -0.72f, 3.2f)));
        showcaseModels.Add("Trivia", CreateTriviaModel(world, new Vector3(1.25f, -0.72f, 3.2f)));
        showcaseModels.Add("¿Qué prefieres?", CreateChoiceModel(world, new Vector3(1.25f, -0.72f, 3.2f)));
        foreach (KeyValuePair<string, Transform> entry in showcaseModels)
            entry.Value.gameObject.SetActive(entry.Key == selectedGame);

        // Small floating signal points move with the beacon, giving the opening one authored focal motion.
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            Transform point = CreatePrimitive(PrimitiveType.Sphere, "Beacon Signal Point", beacon,
                new Vector3(Mathf.Cos(angle) * 0.78f, 0.55f + i * 0.24f, Mathf.Sin(angle) * 0.15f),
                Vector3.one * 0.11f, beaconMaterial);
        }

        worldMotion = world.gameObject.AddComponent<HubWorldMotion>();
        worldMotion.SetTargets(beacon, zombie);
        worldMotion.SetShowcase(zombie);
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new GameObject("BERNEOX Hub UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.68f;
        scaler.referencePixelsPerUnit = 100f;

        EnsureEventSystem();
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        pageBackground = AddImage(canvasRect, "Theme Background", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, DarkBackground);
        pageBackground.raycastTarget = false;

        RectTransform safe = NewRect(canvasRect, "Safe Area");
        safe.anchorMin = Vector2.zero;
        safe.anchorMax = Vector2.one;
        safe.offsetMin = Vector2.zero;
        safe.offsetMax = Vector2.zero;
        safe.gameObject.AddComponent<HubSafeArea>();

        BuildHome(safe);
        BuildSplash(safe);
    }

    private void BuildHome(RectTransform parent)
    {
        GameObject home = new GameObject("Home", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform homeRect = home.GetComponent<RectTransform>();
        homeRect.SetParent(parent, false);
        Stretch(homeRect);
        homeGroup = home.GetComponent<CanvasGroup>();
        homeGroup.alpha = 0f;
        homeGroup.interactable = false;
        homeGroup.blocksRaycasts = false;

        RectTransform header = NewRect(homeRect, "Header");
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.sizeDelta = new Vector2(0f, 128f);
        header.anchoredPosition = new Vector2(0f, -12f);

        Text brand = CreateText(header, "Brand Name", "BERNEOX", 48, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(0.52f, 1f), new Color(1f, 1f, 1f, 1f));
        Text brandSub = CreateText(header, "Brand Descriptor", "PLATAFORMA DE JUEGOS", 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(0.52f, 0.38f), DarkSecondary);
        RectTransform brandRect = brand.rectTransform;
        brandRect.offsetMin = new Vector2(34f, 36f);
        brandRect.offsetMax = new Vector2(-10f, 0f);
        brandSub.rectTransform.offsetMin = new Vector2(37f, 5f);
        brandSub.rectTransform.offsetMax = new Vector2(-10f, 43f);
        BindText(brand, DarkText, LightText);
        BindText(brandSub, DarkSecondary, LightSecondary);

        Button themeButton = CreateButton(header, "Theme Button", "MODO OSCURO", 19, new Vector2(0.63f, 0.5f), new Vector2(224f, 78f), DarkRaised, DarkAccent);
        themeButtonLabel = themeButton.GetComponentInChildren<Text>();
        themeButton.onClick.AddListener(ToggleTheme);
        Button profileButton = CreateButton(header, "Profile Button", "PERFIL", 19, new Vector2(0.89f, 0.5f), new Vector2(164f, 78f), DarkRaised, DarkAccent);
        profileButton.onClick.AddListener(() => ShowInfo("Tu espacio", "El perfil compartido y las funciones en línea todavía no están conectados. Este prototipo no guarda datos personales.", "ENTENDIDO"));
        BindButtonSurface(themeButton, DarkRaised, LightRaised);
        BindButtonText(themeButton, DarkText, LightText);
        BindButtonSurface(profileButton, DarkRaised, LightRaised);
        BindButtonText(profileButton, DarkText, LightText);

        RectTransform viewport = NewRect(homeRect, "Home Scroll Viewport");
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(30f, 146f);
        viewport.offsetMax = new Vector2(-30f, -140f);
        viewport.gameObject.AddComponent<RectMask2D>();
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0f);
        viewportImage.raycastTarget = true;

        GameObject contentObject = new GameObject("Home Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform scrollContent = contentObject.GetComponent<RectTransform>();
        scrollContent.SetParent(viewport, false);
        scrollContent.anchorMin = new Vector2(0f, 1f);
        scrollContent.anchorMax = new Vector2(1f, 1f);
        scrollContent.pivot = new Vector2(0.5f, 1f);
        scrollContent.anchoredPosition = Vector2.zero;
        scrollContent.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 26, 36);
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        homeScroll = viewport.gameObject.AddComponent<ScrollRect>();
        homeScroll.content = scrollContent;
        homeScroll.viewport = viewport;
        homeScroll.horizontal = false;
        homeScroll.vertical = true;
        homeScroll.movementType = ScrollRect.MovementType.Clamped;
        homeScroll.inertia = true;
        homeScroll.scrollSensitivity = 42f;

        RectTransform intro = AddLayoutChild(scrollContent, "Intro", 190f);
        Text introTitle = CreateText(intro, "Home Title", "Un lugar,\nmuchos juegos.", 68, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), DarkText);
        introTitle.rectTransform.offsetMin = new Vector2(20f, 46f);
        introTitle.rectTransform.offsetMax = new Vector2(-20f, -2f);
        introTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
        introTitle.verticalOverflow = VerticalWrapMode.Overflow;
        BindText(introTitle, DarkText, LightText);

        RectTransform featured = AddLayoutChild(scrollContent, "Featured Zombie Survival", 600f);
        Image featuredSurface = featured.gameObject.AddComponent<Image>();
        featuredSurface.color = new Color(0.055f, 0.035f, 0.11f, 0.76f);
        featuredSurface.raycastTarget = false;
        BindGraphic(featuredSurface, new Color(0.055f, 0.035f, 0.11f, 0.76f), new Color(0.055f, 0.035f, 0.11f, 0.81f));

        RectTransform featuredContent = NewRect(featured, "Featured Content");
        Stretch(featuredContent);
        featuredStatus = CreateText(featuredContent, "Prototype Status", "PRIMER JUEGO  ·  EN DESARROLLO", 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), DarkMint);
        featuredStatus.rectTransform.pivot = new Vector2(0f, 1f);
        featuredStatus.rectTransform.sizeDelta = new Vector2(-58f, 38f);
        featuredStatus.rectTransform.anchoredPosition = new Vector2(30f, -24f);
        featuredTitle = CreateText(featuredContent, "Featured Title", "ZOMBIE\nSURVIVAL", 54, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(1f, 1f), DarkText);
        featuredTitle.rectTransform.offsetMin = new Vector2(30f, 148f);
        featuredTitle.rectTransform.offsetMax = new Vector2(-25f, -286f);
        featuredDescription = CreateText(featuredContent, "Featured Description", "Supervivencia 3D · La primera aventura de BERNEOX", 27, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0f), DarkSecondary);
        featuredDescription.rectTransform.pivot = new Vector2(0f, 0f);
        featuredDescription.rectTransform.sizeDelta = new Vector2(-60f, 56f);
        featuredDescription.rectTransform.anchoredPosition = new Vector2(30f, 90f);
        Button prototypeButton = CreateButton(featuredContent, "Prototype Details", "CONOCER EL PROTOTIPO", 23,
            new Vector2(0f, 0f), new Vector2(0.68f, 92f), DarkAccent, DarkText);
        prototypeButton.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
        prototypeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(30f, 20f);
        featuredButtonLabel = prototypeButton.GetComponentInChildren<Text>();
        prototypeButton.onClick.AddListener(ShowSelectedGameInfo);

        RectTransform catalogHeading = AddLayoutChild(scrollContent, "Catalog Heading", 94f);
        Text catalogTitle = CreateText(catalogHeading, "Catalog Title", "OTROS JUEGOS", 34, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(0.65f, 1f), DarkText);
        catalogTitle.rectTransform.offsetMin = new Vector2(18f, 0f);
        catalogTitle.rectTransform.offsetMax = new Vector2(-6f, 0f);
        BindText(catalogTitle, DarkText, LightText);
        Text catalogHint = CreateText(catalogHeading, "Catalog Hint", "Desliza para explorar", 23, FontStyle.Normal, TextAnchor.MiddleRight,
            new Vector2(0.55f, 0f), new Vector2(1f, 1f), DarkSecondary);
        catalogHint.rectTransform.offsetMin = Vector2.zero;
        catalogHint.rectTransform.offsetMax = new Vector2(-18f, 0f);
        BindText(catalogHint, DarkSecondary, LightSecondary);

        RectTransform gamesRow = AddLayoutChild(scrollContent, "Game Destinations", 330f);
        BuildGameCarousel(gamesRow);

        RectTransform footer = AddLayoutChild(scrollContent, "Prototype Footer", 72f);
        Text footerText = CreateText(footer, "Prototype Note", "Los juegos marcados como planificados todavía no se pueden abrir.", 21, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(1f, 1f), DarkSecondary);
        footerText.rectTransform.offsetMin = new Vector2(18f, 0f);
        footerText.rectTransform.offsetMax = new Vector2(-18f, 0f);
        BindText(footerText, DarkSecondary, LightSecondary);

        BuildBottomNavigation(homeRect);
    }

    private void BuildGameCarousel(RectTransform parent)
    {
        GameObject viewportObject = parent.gameObject;
        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0f);
        viewportImage.raycastTarget = true;
        viewportObject.AddComponent<RectMask2D>();
        RectTransform content = NewRect(viewportObject.transform, "Game Tiles");
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(1600f, 0f);

        HorizontalLayoutGroup layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 22, 10, 10);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        ScrollRect rowScroll = viewportObject.gameObject.AddComponent<ScrollRect>();
        rowScroll.content = content;
        rowScroll.viewport = parent;
        rowScroll.horizontal = true;
        rowScroll.vertical = false;
        rowScroll.movementType = ScrollRect.MovementType.Clamped;
        rowScroll.inertia = true;
        rowScroll.scrollSensitivity = 38f;

        CreateGameTile(content, "REFLEX", "Velocidad y precisión", "PLANIFICADO", Hex("A3C785"), 350f, "Reflex" );
        CreateGameTile(content, "MEMORY", "Memoria y patrones", "PLANIFICADO", Hex("92BDE8"), 350f, "Memory");
        CreateGameTile(content, "TRIVIA", "Preguntas por descubrir", "PLANIFICADO", Hex("E1C277"), 350f, "Trivia");
        CreateGameTile(content, "¿QUÉ PREFIERES?", "Decisiones para compartir", "PLANIFICADO", Hex("D58CB5"), 390f, "¿Qué prefieres?");
    }

    private void CreateGameTile(Transform parent, string title, string subtitle, string status, Color accent, float width, string infoTitle)
    {
        GameObject tile = new GameObject("Game Tile - " + title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        RectTransform rect = tile.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(width, 0f);
        LayoutElement element = tile.GetComponent<LayoutElement>();
        element.preferredWidth = width;
        element.minWidth = width;
        element.flexibleWidth = 0f;
        Image surface = tile.GetComponent<Image>();
        surface.color = isDark ? DarkSurface : LightSurface;
        Button button = tile.GetComponent<Button>();
        button.targetGraphic = surface;
        SetButtonColors(button, accent);
        button.onClick.AddListener(() => SelectGame(infoTitle));
        BindButtonSurface(button, DarkSurface, LightSurface);

        Image mark = AddImage(rect, "Game Signal", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(26f, -26f), new Vector2(136f, 84f), accent);
        mark.raycastTarget = false;
        Text statusText = CreateText(rect, "Game Status", status, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), accent);
        statusText.rectTransform.pivot = new Vector2(0f, 1f);
        statusText.rectTransform.sizeDelta = new Vector2(-44f, 34f);
        statusText.rectTransform.anchoredPosition = new Vector2(24f, -18f);
        Color statusLight = Color.Lerp(accent, Color.black, 0.42f);
        BindText(statusText, accent, statusLight);
        Text titleText = CreateText(rect, "Game Name", title, title.Length > 14 ? 27 : 32, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), isDark ? DarkText : LightText);
        titleText.rectTransform.pivot = new Vector2(0f, 0.5f);
        titleText.rectTransform.sizeDelta = new Vector2(-48f, 62f);
        titleText.rectTransform.anchoredPosition = new Vector2(24f, -30f);
        titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        Text subtitleText = CreateText(rect, "Game Description", subtitle, 21, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0f), isDark ? DarkSecondary : LightSecondary);
        subtitleText.rectTransform.pivot = new Vector2(0f, 0f);
        subtitleText.rectTransform.sizeDelta = new Vector2(-48f, 60f);
        subtitleText.rectTransform.anchoredPosition = new Vector2(24f, 18f);
        BindText(titleText, DarkText, LightText);
        BindText(subtitleText, DarkSecondary, LightSecondary);
    }

    private void BuildBottomNavigation(RectTransform parent)
    {
        RectTransform bar = NewRect(parent, "Bottom Navigation");
        bar.anchorMin = new Vector2(0f, 0f);
        bar.anchorMax = new Vector2(1f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.sizeDelta = new Vector2(0f, 128f);
        bar.anchoredPosition = Vector2.zero;
        Image barSurface = bar.gameObject.AddComponent<Image>();
        barSurface.color = isDark ? DarkBackground : LightSurface;
        BindGraphic(barSurface, DarkBackground, LightSurface);

        HorizontalLayoutGroup navLayout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
        navLayout.padding = new RectOffset(24, 24, 12, 12);
        navLayout.spacing = 12f;
        navLayout.childAlignment = TextAnchor.MiddleCenter;
        navLayout.childControlWidth = true;
        navLayout.childControlHeight = true;
        navLayout.childForceExpandWidth = true;
        navLayout.childForceExpandHeight = true;

        CreateNavButton(bar, "INICIO", 0, () =>
        {
            if (homeScroll != null) homeScroll.verticalNormalizedPosition = 1f;
        });
        CreateNavButton(bar, "JUEGOS", 1, () =>
        {
            if (homeScroll != null) homeScroll.verticalNormalizedPosition = 0.15f;
        });
        CreateNavButton(bar, "PERFIL", 2, () => ShowInfo("Tu espacio", "El perfil compartido se añadirá cuando se definan e implementen las cuentas. Este prototipo no utiliza información personal.", "ENTENDIDO"));

        Button settings = CreateButton(bar, "Settings Navigation", "AJUSTES", 19,
            new Vector2(1f, 0f), new Vector2(140f, 100f), DarkRaised, DarkAccent);
        settings.onClick.AddListener(ShowSettings);
        BindButtonSurface(settings, DarkRaised, LightRaised);
        BindButtonText(settings, DarkText, LightText);
    }

    private void CreateNavButton(Transform parent, string label, int index, Action action)
    {
        GameObject item = new GameObject("Navigation " + label, typeof(RectTransform));
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        LayoutElement le = item.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minWidth = 110f;

        Button button = item.AddComponent<Button>();
        Image target = item.AddComponent<Image>();
        target.color = new Color(1f, 1f, 1f, 0f);
        target.raycastTarget = true;
        button.targetGraphic = target;
        SetButtonColors(button, DarkAccent);
        button.onClick.AddListener(() => action?.Invoke());

        Text text = CreateText(rect, "Navigation Label", label, 18, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 1f), index == 0 ? DarkText : DarkSecondary);
        text.rectTransform.offsetMin = new Vector2(0f, 4f);
        text.rectTransform.offsetMax = Vector2.zero;
        BindText(text, index == 0 ? DarkText : DarkSecondary, index == 0 ? LightText : LightSecondary);
    }

    private void BuildSplash(RectTransform parent)
    {
        GameObject splash = new GameObject("BERNEOX Opening", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform splashRect = splash.GetComponent<RectTransform>();
        splashRect.SetParent(parent, false);
        Stretch(splashRect);
        splashGroup = splash.GetComponent<CanvasGroup>();
        splashGroup.alpha = 1f;
        splashGroup.interactable = true;
        splashGroup.blocksRaycasts = true;

        Image veil = splash.AddComponent<Image>();
        veil.color = new Color(0.055f, 0.035f, 0.12f, 0.42f);
        veil.raycastTarget = true;

        RectTransform logo = NewRect(splashRect, "Assembling Wordmark");
        logo.anchorMin = new Vector2(0.5f, 0.55f);
        logo.anchorMax = new Vector2(0.5f, 0.55f);
        logo.pivot = new Vector2(0.5f, 0.5f);
        logo.sizeDelta = new Vector2(760f, 150f);
        logo.anchoredPosition = Vector2.zero;
        string word = "BERNEOX";
        splashLetters = new Text[word.Length];
        for (int i = 0; i < word.Length; i++)
        {
            Text letter = CreateText(logo, "Letter " + word[i], word[i].ToString(), 92, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, DarkText);
            letter.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            letter.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            letter.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            letter.rectTransform.sizeDelta = new Vector2(94f, 128f);
            letter.rectTransform.anchoredPosition = new Vector2((i - 3) * 92f, 0f);
            letter.color = DarkText;
            splashLetters[i] = letter;
        }

        Text tagline = CreateText(splashRect, "Opening Tagline", "UN LUGAR, MUCHOS JUEGOS", 24, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.08f, 0.47f), new Vector2(0.92f, 0.47f), DarkSecondary);
        tagline.rectTransform.sizeDelta = new Vector2(0f, 48f);

        Button skip = CreateButton(splashRect, "Skip Opening", "OMITIR", 19, new Vector2(1f, 1f), new Vector2(162f, 76f), DarkRaised, DarkText);
        skip.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
        skip.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28f, -24f);
        skip.onClick.AddListener(SkipOpening);
    }

    private IEnumerator PlayOpening()
    {
        if (reduceMotion)
        {
            yield return new WaitForSecondsRealtime(0.35f);
            RevealHome(0.22f);
            yield break;
        }

        float canvasWidth = 1080f;
        for (int i = 0; i < splashLetters.Length; i++)
        {
            Text letter = splashLetters[i];
            float targetX = (i - 3) * 92f;
            float direction = i <= 2 ? -1f : 1f;
            letter.rectTransform.anchoredPosition = new Vector2(direction * canvasWidth, 0f);
            letter.transform.localScale = Vector3.one * 0.86f;
            StartCoroutine(AnimateLetter(letter.rectTransform, targetX, i * 0.075f));
        }

        yield return new WaitForSecondsRealtime(1.45f);
        if (splashGroup == null) yield break;
        yield return FadeGroup(splashGroup, 1f, 0f, 0.42f);
        RevealHome(0.5f);
    }

    private IEnumerator AnimateLetter(RectTransform letter, float targetX, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Vector2 from = letter.anchoredPosition;
        const float duration = 0.52f;
        float time = 0f;
        while (time < duration && letter != null)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float eased = 1f - Mathf.Pow(1f - t, 4f);
            letter.anchoredPosition = new Vector2(Mathf.LerpUnclamped(from.x, targetX, eased), 0f);
            letter.localScale = Vector3.one * Mathf.Lerp(0.86f, 1f, eased);
            yield return null;
        }
        if (letter != null)
        {
            letter.anchoredPosition = new Vector2(targetX, 0f);
            letter.localScale = Vector3.one;
        }
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        float time = 0f;
        group.alpha = from;
        while (time < duration && group != null)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            group.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }
        if (group != null) group.alpha = to;
    }

    private void RevealHome(float fadeSeconds)
    {
        if (homeGroup == null || splashGroup == null) return;
        StopAllCoroutines();
        homeGroup.interactable = true;
        homeGroup.blocksRaycasts = true;
        splashGroup.interactable = false;
        splashGroup.blocksRaycasts = false;
        if (reduceMotion || fadeSeconds <= 0f)
        {
            homeGroup.alpha = 1f;
            splashGroup.alpha = 0f;
        }
        else
        {
            StartCoroutine(FadeGroup(homeGroup, 0f, 1f, fadeSeconds));
            StartCoroutine(FadeGroup(splashGroup, splashGroup.alpha, 0f, fadeSeconds));
        }
    }

    private void SkipOpening()
    {
        if (splashGroup == null || splashGroup.alpha <= 0f) return;
        RevealHome(0.18f);
    }

    private void ToggleTheme()
    {
        isDark = !isDark;
        PlayerPrefs.SetInt(DarkThemeKey, isDark ? 1 : 0);
        PlayerPrefs.Save();
        ApplyTheme();
    }

    private void ToggleMotion()
    {
        reduceMotion = !reduceMotion;
        PlayerPrefs.SetInt(ReducedMotionKey, reduceMotion ? 1 : 0);
        PlayerPrefs.Save();
        if (worldMotion != null) worldMotion.SetReducedMotion(reduceMotion);
        CloseInfo();
        ShowSettings();
    }

    private void ApplyTheme()
    {
        Color bg = isDark ? DarkBackground : LightBackground;
        if (pageBackground != null) pageBackground.color = bg;
        Camera camera = Camera.main;
        if (camera != null) camera.backgroundColor = isDark ? DarkBackground : LightBackground;
        foreach (Action<bool> binding in themeBindings) binding(isDark);
        if (themeButtonLabel != null) themeButtonLabel.text = isDark ? "MODO OSCURO" : "MODO CLARO";
    }

    private void ShowSettings()
    {
        string motionState = reduceMotion ? "ACTIVADO" : "DESACTIVADO";
        ShowInfo("Ajustes", "Tema actual: " + (isDark ? "oscuro" : "claro") + ".\nMovimiento reducido: " + motionState + ".\n\nEl modo de color y esta preferencia se guardan solo en este dispositivo.",
            reduceMotion ? "DESACTIVAR MOVIMIENTO REDUCIDO" : "ACTIVAR MOVIMIENTO REDUCIDO", ToggleMotion, "CERRAR");
    }

    private void SelectGame(string game)
    {
        selectedGame = game;
        string title = game == "¿Qué prefieres?" ? "¿QUÉ\nPREFIERES?" : game.ToUpperInvariant();
        string status = game == "Zombie Survival" ? "PRIMER JUEGO  ·  EN DESARROLLO" : "JUEGO PLANIFICADO";
        string description = game == "Zombie Survival"
            ? "Supervivencia 3D · La primera aventura de BERNEOX"
            : game == "Reflex" ? "Velocidad y precisión"
            : game == "Memory" ? "Memoria y patrones"
            : game == "Trivia" ? "Preguntas por descubrir"
            : "Decisiones para compartir";
        if (featuredTitle != null) featuredTitle.text = title;
        if (featuredStatus != null) featuredStatus.text = status;
        if (featuredDescription != null) featuredDescription.text = description;
        if (featuredButtonLabel != null) featuredButtonLabel.text = game == "Zombie Survival" ? "CONOCER EL PROTOTIPO" : "VER ESTADO DEL JUEGO";
        foreach (KeyValuePair<string, Transform> entry in showcaseModels)
            if (entry.Value != null) entry.Value.gameObject.SetActive(entry.Key == game);
        if (worldMotion != null && showcaseModels.TryGetValue(game, out Transform model))
            worldMotion.SetShowcase(model);
    }

    private void ShowSelectedGameInfo()
    {
        if (selectedGame == "Zombie Survival")
            ShowInfo("Zombie Survival", "Este proyecto ya tiene una base de movimiento y joystick. La escena de juego todavía debe comprobarse y no está conectada a este catálogo.", "ENTENDIDO");
        else
            ShowInfo(selectedGame, "Este juego está planificado para BERNEOX, pero todavía no tiene una versión jugable.", "ENTENDIDO");
    }

    private void ShowInfo(string title, string body, string buttonLabel, Action action = null, string closeLabel = null)
    {
        if (modalOpen) return;
        modalOpen = true;
        Transform old = transform.Find("BERNEOX Hub UI/Info Overlay");
        if (old != null) Destroy(old.gameObject);

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        RectTransform overlay = NewRect(canvas.transform, "Info Overlay");
        Stretch(overlay);
        Image scrim = overlay.gameObject.AddComponent<Image>();
        scrim.color = new Color(0.025f, 0.015f, 0.055f, 0.78f);
        scrim.raycastTarget = true;
        Button dismiss = overlay.gameObject.AddComponent<Button>();
        dismiss.targetGraphic = scrim;
        dismiss.onClick.AddListener(CloseInfo);

        RectTransform sheet = NewRect(overlay, "Info Sheet");
        sheet.anchorMin = new Vector2(0.08f, 0.5f);
        sheet.anchorMax = new Vector2(0.92f, 0.5f);
        sheet.pivot = new Vector2(0.5f, 0.5f);
        sheet.sizeDelta = new Vector2(0f, 690f);
        sheet.anchoredPosition = Vector2.zero;
        Image surface = sheet.gameObject.AddComponent<Image>();
        surface.color = isDark ? DarkSurface : LightSurface;
        Button sheetBlocker = sheet.gameObject.AddComponent<Button>();
        sheetBlocker.targetGraphic = surface;
        sheetBlocker.onClick.AddListener(() => { });

        Text heading = CreateText(sheet, "Info Heading", title, 48, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), isDark ? DarkText : LightText);
        heading.rectTransform.pivot = new Vector2(0f, 1f);
        heading.rectTransform.sizeDelta = new Vector2(-72f, 110f);
        heading.rectTransform.anchoredPosition = new Vector2(36f, -36f);
        Text message = CreateText(sheet, "Info Body", body, 27, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(0f, 0f), new Vector2(1f, 1f), isDark ? DarkSecondary : LightSecondary);
        message.rectTransform.offsetMin = new Vector2(36f, 170f);
        message.rectTransform.offsetMax = new Vector2(-36f, -160f);
        message.verticalOverflow = VerticalWrapMode.Overflow;

        Button primary = CreateButton(sheet, "Info Primary Action", buttonLabel, 20,
            new Vector2(0f, 0f), new Vector2(0.64f, 96f), isDark ? DarkAccent : LightAccent, DarkText);
        primary.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
        primary.GetComponent<RectTransform>().anchoredPosition = new Vector2(36f, 32f);
        primary.onClick.AddListener(() =>
        {
            if (action != null) action();
            else CloseInfo();
        });

        if (!string.IsNullOrEmpty(closeLabel))
        {
            Button close = CreateButton(sheet, "Info Close", closeLabel, 19,
                new Vector2(1f, 0f), new Vector2(0.25f, 96f), isDark ? DarkRaised : LightRaised, isDark ? DarkText : LightText);
            close.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30f, 32f);
            close.onClick.AddListener(CloseInfo);
        }
    }

    private void CloseInfo()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform overlay = canvas.transform.Find("Info Overlay");
            if (overlay != null) Destroy(overlay.gameObject);
        }
        modalOpen = false;
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = go.GetComponent<EventSystem>();
        }
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        if (inputModule.actionsAsset == null) inputModule.AssignDefaultActions();
    }

    private Button CreateButton(Transform parent, string name, string label, int fontSize, Vector2 anchor, Vector2 size, Color fill, Color textColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = fill;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        SetButtonColors(button, fill);
        Text text = CreateText(rect, "Label", label, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 1f), textColor);
        text.rectTransform.offsetMin = new Vector2(10f, 4f);
        text.rectTransform.offsetMax = new Vector2(-10f, -4f);
        return button;
    }

    private void SetButtonColors(Button button, Color baseColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.09f);
        colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.12f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.45f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
    }

    private Text CreateText(Transform parent, string name, string value, int size, FontStyle style, TextAnchor alignment,
        Vector2 min, Vector2 max, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text text = go.GetComponent<Text>();
        text.font = uiFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        text.resizeTextForBestFit = false;
        text.lineSpacing = 1.05f;
        return text;
    }

    private static RectTransform AddLayoutChild(Transform parent, string name, float height)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        LayoutElement element = go.GetComponent<LayoutElement>();
        element.preferredHeight = height;
        element.minHeight = height;
        element.flexibleHeight = 0f;
        return rect;
    }

    private static RectTransform NewRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Image AddImage(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void BindText(Text target, Color dark, Color light)
    {
        themeBindings.Add(value => { if (target != null) target.color = value ? dark : light; });
    }

    private void BindGraphic(Graphic target, Color dark, Color light)
    {
        themeBindings.Add(value => { if (target != null) target.color = value ? dark : light; });
    }

    private void BindButtonSurface(Button button, Color dark, Color light)
    {
        themeBindings.Add(value =>
        {
            if (button == null) return;
            Image image = button.targetGraphic as Image;
            if (image != null) image.color = value ? dark : light;
            SetButtonColors(button, value ? dark : light);
        });
    }

    private void BindButtonText(Button button, Color dark, Color light)
    {
        Text label = button.GetComponentInChildren<Text>();
        if (label != null) BindText(label, dark, light);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Color Hex(string html)
    {
        ColorUtility.TryParseHtmlString("#" + html, out Color color);
        return color;
    }

    private static Material MakeMaterial(string name, Color color, Color? emission = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { name = name, color = color };
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        return material;
    }

    private static Transform CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return go.transform;
    }

    private static void CreateTree(Transform world, float x, float y, float z, float height, Material foliage, Material bark)
    {
        CreatePrimitive(PrimitiveType.Cylinder, "Fir Trunk", world, new Vector3(x, y - height * 0.34f, z),
            new Vector3(0.13f, height * 0.28f, 0.13f), bark);
        CreateCone(world, "Low-poly Fir", new Vector3(x, y + height * 0.05f, z), new Vector3(height * 0.68f, height, height * 0.68f), foliage);
    }

    private static Transform CreateBeacon(Transform world, Vector3 position, Material signal, Material baseMaterial)
    {
        Transform rig = new GameObject("Violet Beacon Rig").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        CreatePrimitive(PrimitiveType.Cylinder, "Beacon Foot", rig, new Vector3(0f, -0.3f, 0f), new Vector3(0.54f, 0.18f, 0.54f), baseMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, "Beacon Mast", rig, new Vector3(0f, 0.22f, 0f), new Vector3(0.15f, 0.44f, 0.15f), baseMaterial);
        CreatePrimitive(PrimitiveType.Sphere, "Beacon Core", rig, new Vector3(0f, 0.72f, 0f), new Vector3(0.42f, 0.5f, 0.42f), signal);
        Transform orbit = new GameObject("Beacon Orbit").transform;
        orbit.SetParent(rig, false);
        orbit.localPosition = new Vector3(0f, 0.72f, 0f);
        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI * 2f / 10f;
            CreatePrimitive(PrimitiveType.Sphere, "Signal Ring Node", orbit,
                new Vector3(Mathf.Cos(angle) * 0.73f, 0f, Mathf.Sin(angle) * 0.14f), Vector3.one * 0.09f, signal);
        }
        return rig;
    }

    private static Transform CreateZombie(Transform world, Vector3 position, Material body, Material detail, Material eyes)
    {
        Transform rig = new GameObject("Stylized Zombie Silhouette").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        CreatePrimitive(PrimitiveType.Capsule, "Zombie Torso", rig, new Vector3(0f, 0.05f, 0f), new Vector3(0.62f, 0.88f, 0.48f), body);
        CreatePrimitive(PrimitiveType.Sphere, "Zombie Head", rig, new Vector3(0f, 0.86f, 0f), new Vector3(0.48f, 0.45f, 0.42f), detail);
        CreatePrimitive(PrimitiveType.Capsule, "Zombie Left Arm", rig, new Vector3(-0.43f, 0.22f, 0f), new Vector3(0.17f, 0.48f, 0.17f), body).localRotation = Quaternion.Euler(0f, 0f, -34f);
        CreatePrimitive(PrimitiveType.Capsule, "Zombie Right Arm", rig, new Vector3(0.43f, 0.22f, 0f), new Vector3(0.17f, 0.5f, 0.17f), body).localRotation = Quaternion.Euler(0f, 0f, 37f);
        CreatePrimitive(PrimitiveType.Capsule, "Zombie Left Leg", rig, new Vector3(-0.19f, -0.63f, 0f), new Vector3(0.19f, 0.42f, 0.21f), detail);
        CreatePrimitive(PrimitiveType.Capsule, "Zombie Right Leg", rig, new Vector3(0.19f, -0.63f, 0f), new Vector3(0.19f, 0.42f, 0.21f), detail);
        CreatePrimitive(PrimitiveType.Sphere, "Zombie Eye Left", rig, new Vector3(-0.11f, 0.91f, -0.2f), Vector3.one * 0.055f, eyes);
        CreatePrimitive(PrimitiveType.Sphere, "Zombie Eye Right", rig, new Vector3(0.11f, 0.91f, -0.2f), Vector3.one * 0.055f, eyes);
        return rig;
    }

    private static Transform CreateReflexModel(Transform world, Vector3 position)
    {
        Transform rig = new GameObject("3D Game Preview - Reflex").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        Material dark = MakeMaterial("Reflex Target Frame", Hex("3B3150"));
        Material signal = MakeMaterial("Reflex Signal", Hex("D9F2CC"), Hex("9DCB82"));
        Transform outer = CreatePrimitive(PrimitiveType.Cylinder, "Reflex Outer Target", rig, new Vector3(0f, 0.2f, 0f), new Vector3(1.2f, 0.16f, 1.2f), dark);
        outer.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Transform inner = CreatePrimitive(PrimitiveType.Cylinder, "Reflex Inner Target", rig, new Vector3(0f, 0.2f, -0.1f), new Vector3(0.76f, 0.18f, 0.76f), signal);
        inner.localRotation = Quaternion.Euler(90f, 0f, 0f);
        CreatePrimitive(PrimitiveType.Sphere, "Reflex Center", rig, new Vector3(0f, 0.2f, -0.23f), Vector3.one * 0.25f, dark);
        CreatePrimitive(PrimitiveType.Cube, "Reflex Stand", rig, new Vector3(0f, -0.48f, 0.15f), new Vector3(0.16f, 0.9f, 0.16f), dark);
        return rig;
    }

    private static Transform CreateMemoryModel(Transform world, Vector3 position)
    {
        Transform rig = new GameObject("3D Game Preview - Memory").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        Material violet = MakeMaterial("Memory Tile Violet", Hex("9B7ADF"));
        Material mint = MakeMaterial("Memory Tile Mint", Hex("AFC89E"));
        Material rose = MakeMaterial("Memory Tile Rose", Hex("C688AE"));
        Transform a = CreatePrimitive(PrimitiveType.Cube, "Memory Tile A", rig, new Vector3(-0.55f, -0.1f, 0.1f), new Vector3(0.62f, 0.7f, 0.24f), violet);
        Transform b = CreatePrimitive(PrimitiveType.Cube, "Memory Tile B", rig, new Vector3(0.12f, 0.34f, 0.35f), new Vector3(0.62f, 0.7f, 0.24f), mint);
        Transform c = CreatePrimitive(PrimitiveType.Cube, "Memory Tile C", rig, new Vector3(0.68f, -0.1f, 0f), new Vector3(0.62f, 0.7f, 0.24f), rose);
        a.localRotation = Quaternion.Euler(0f, -13f, -9f);
        b.localRotation = Quaternion.Euler(0f, 5f, 8f);
        c.localRotation = Quaternion.Euler(0f, 14f, -7f);
        return rig;
    }

    private static Transform CreateTriviaModel(Transform world, Vector3 position)
    {
        Transform rig = new GameObject("3D Game Preview - Trivia").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        Material paper = MakeMaterial("Trivia Question", Hex("D9F2CC"));
        Material violet = MakeMaterial("Trivia Base", Hex("7454B3"));
        CreatePrimitive(PrimitiveType.Sphere, "Question Curve A", rig, new Vector3(-0.18f, 0.55f, 0f), Vector3.one * 0.42f, paper);
        CreatePrimitive(PrimitiveType.Sphere, "Question Curve B", rig, new Vector3(0.18f, 0.53f, 0f), Vector3.one * 0.42f, paper);
        CreatePrimitive(PrimitiveType.Capsule, "Question Stem", rig, new Vector3(0.19f, 0.18f, 0f), new Vector3(0.17f, 0.38f, 0.17f), paper);
        CreatePrimitive(PrimitiveType.Sphere, "Question Dot", rig, new Vector3(0.19f, -0.28f, 0f), Vector3.one * 0.19f, paper);
        CreatePrimitive(PrimitiveType.Cube, "Trivia Plinth", rig, new Vector3(0f, -0.62f, 0.12f), new Vector3(1.35f, 0.16f, 0.62f), violet);
        return rig;
    }

    private static Transform CreateChoiceModel(Transform world, Vector3 position)
    {
        Transform rig = new GameObject("3D Game Preview - Choices").transform;
        rig.SetParent(world, false);
        rig.localPosition = position;
        Material violet = MakeMaterial("Choice Door Violet", Hex("9A73E5"));
        Material mint = MakeMaterial("Choice Door Mint", Hex("B8D4A3"));
        Material dark = MakeMaterial("Choice Door Frame", Hex("392C50"));
        CreateDoor(rig, "Left Choice", new Vector3(-0.7f, -0.1f, 0f), violet, dark);
        CreateDoor(rig, "Right Choice", new Vector3(0.7f, -0.1f, 0.2f), mint, dark);
        return rig;
    }

    private static void CreateDoor(Transform parent, string name, Vector3 position, Material panel, Material frame)
    {
        Transform door = CreatePrimitive(PrimitiveType.Cube, name + " Panel", parent, position + new Vector3(0f, 0.35f, 0f), new Vector3(0.72f, 1.4f, 0.16f), panel);
        door.localRotation = Quaternion.Euler(0f, name.StartsWith("Left", StringComparison.Ordinal) ? -8f : 8f, 0f);
        CreatePrimitive(PrimitiveType.Cube, name + " Lintel", parent, position + new Vector3(0f, 1.12f, 0f), new Vector3(0.96f, 0.14f, 0.24f), frame);
        CreatePrimitive(PrimitiveType.Cube, name + " Base", parent, position + new Vector3(0f, -0.4f, 0f), new Vector3(1.02f, 0.14f, 0.26f), frame);
    }

    private static void CreateCone(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        const int sides = 9;
        Vector3[] vertices = new Vector3[sides + 1];
        int[] triangles = new int[sides * 3];
        vertices[0] = new Vector3(0f, 0.5f, 0f);
        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, -0.5f, Mathf.Sin(angle) * 0.5f);
            int triangle = i * 3;
            triangles[triangle] = 0;
            triangles[triangle + 1] = i + 1;
            triangles[triangle + 2] = (i + 1) % sides + 1;
        }
        Mesh mesh = new Mesh { name = name + " Mesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GameObject cone = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        cone.transform.SetParent(parent, false);
        cone.transform.localPosition = position;
        cone.transform.localScale = scale;
        cone.GetComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = cone.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }
}

internal sealed class HubSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake() => rectTransform = GetComponent<RectTransform>();
    private void Update()
    {
        Vector2Int size = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea != lastSafeArea || size != lastScreenSize)
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = size;
            rectTransform.anchorMin = lastSafeArea.position / size;
            rectTransform.anchorMax = (lastSafeArea.position + lastSafeArea.size) / size;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}

internal sealed class HubWorldMotion : MonoBehaviour
{
    private Transform beacon;
    private Transform zombie;
    private Transform orbit;
    private Transform showcase;
    private Vector3 beaconStart;
    private Vector3 zombieStart;
    private Vector3 showcaseStart;
    private Quaternion showcaseRotation;
    private bool reduced;
    private float time;

    public void SetTargets(Transform beaconTarget, Transform zombieTarget)
    {
        beacon = beaconTarget;
        zombie = zombieTarget;
        if (beacon != null)
        {
            beaconStart = beacon.localPosition;
            orbit = beacon.Find("Beacon Orbit");
        }
        if (zombie != null) zombieStart = zombie.localPosition;
    }

    public void SetReducedMotion(bool value) => reduced = value;

    public void SetShowcase(Transform target)
    {
        showcase = target;
        if (showcase == null) return;
        showcaseStart = showcase.localPosition;
        showcaseRotation = showcase.localRotation;
    }

    private void Update()
    {
        time += Time.unscaledDeltaTime;
        if (beacon != null)
        {
            beacon.localPosition = beaconStart + (reduced ? Vector3.zero : Vector3.up * Mathf.Sin(time * 1.1f) * 0.075f);
            if (orbit != null && !reduced) orbit.Rotate(0f, 0f, 18f * Time.unscaledDeltaTime, Space.Self);
        }
        if (zombie != null)
        {
            if (showcase == zombie)
            {
                zombie.localPosition = zombieStart + (reduced ? Vector3.zero : Vector3.up * Mathf.Sin(time * 1.6f + 1f) * 0.035f);
                zombie.localRotation = reduced ? Quaternion.identity : Quaternion.Euler(0f, Mathf.Sin(time * 0.55f) * 4f, Mathf.Sin(time * 1.3f) * 1.5f);
            }
        }
        if (showcase != null && showcase != zombie)
        {
            showcase.localPosition = showcaseStart + (reduced ? Vector3.zero : Vector3.up * Mathf.Sin(time * 1.2f) * 0.055f);
            showcase.localRotation = reduced ? showcaseRotation : showcaseRotation * Quaternion.Euler(0f, time * 10f, 0f);
        }
    }
}
