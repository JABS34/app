using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ZombiePrototypeVisualUpgrade
{
    private const string ScenePath = "Assets/Scenes/ZombiePrototype.unity";
    private const string MarkerName = "APOCALYPTIC_SCENE_DRESSING";
    private static Mesh coneMesh;
    private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();

    static ZombiePrototypeVisualUpgrade()
    {
        EditorApplication.delayCall += UpgradeOpenScene;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += UpgradeOpenScene;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path == ScenePath) EditorApplication.delayCall += UpgradeOpenScene;
    }

    private static void UpgradeOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || scene.isDirty || HasUpgradeMarker(scene)) return;
        UpgradeScene(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Zombie Survival: escenario nocturno apocalíptico aplicado al prototipo.");
    }

    [MenuItem("BERNEOX/Actualizar escenario apocalíptico")]
    private static void UpgradeFromMenu()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Zombie Survival", "Abre primero Assets/Scenes/ZombiePrototype.unity.", "Aceptar");
            return;
        }
        if (HasUpgradeMarker(scene))
        {
            EditorUtility.DisplayDialog("Zombie Survival", "La mejora visual ya está aplicada en esta escena.", "Aceptar");
            return;
        }
        UpgradeScene(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    public static void UpgradeScene(Scene scene)
    {
        if (!scene.IsValid() || HasUpgradeMarker(scene)) return;

        GameObject dressing = new GameObject(MarkerName);
        SceneManager.MoveGameObjectToScene(dressing, scene);

        ConfigureNightLighting();
        GameObject ground = GameObject.Find("Ground_Prototype");
        if (ground != null) ground.GetComponent<Renderer>().sharedMaterial = Material(new Color(0.095f, 0.12f, 0.11f));

        GameObject player = GameObject.Find("Player_Prototype");
        PrototypePlayerHealth playerHealth = player != null ? player.GetComponent<PrototypePlayerHealth>() : null;
        if (player != null) BuildPlayerRig(player);

        PrototypeHordeDirector horde = Object.FindAnyObjectByType<PrototypeHordeDirector>();
        PrototypeZombieEnemy templateEnemy = FindZombieTemplate();
        GameObject zombieTemplate = templateEnemy != null ? templateEnemy.gameObject : null;
        if (templateEnemy != null) BuildZombieRig(templateEnemy);

        AddForest(dressing.transform);
        AddAbandonedOutpost(dressing.transform);
        AddWreckedCar(dressing.transform);
        AddCampfire(dressing.transform);
        AddRubbleAndBrush(dressing.transform);
        AddMoon(dressing.transform);
        UpgradeResourceVisuals();
        MarkEnvironmentStatic(dressing.transform);

        if (horde != null && playerHealth != null && zombieTemplate != null)
        {
            List<PrototypeZombieEnemy> dormant = new List<PrototypeZombieEnemy>();
            Vector3[] positions = { new Vector3(9f, 1f, 10f), new Vector3(-12f, 1f, 8f), new Vector3(11f, 1f, -10f) };
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject zombie = Object.Instantiate(zombieTemplate, positions[i], Quaternion.Euler(0f, 180f, 0f));
                zombie.name = "Zombie_Visible_Preparacion_" + (i + 1).ToString("00");
                zombie.transform.SetParent(dressing.transform, true);
                zombie.SetActive(true);
                PrototypeZombieEnemy enemy = zombie.GetComponent<PrototypeZombieEnemy>();
                enemy.enabled = false;
                enemy.Initialize(playerHealth, zombie.transform.childCount > 0 ? zombie.transform.GetChild(0) : null);
                dormant.Add(enemy);
            }
            horde.SetDormantZombies(dormant.ToArray());
        }

        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static bool HasUpgradeMarker(Scene scene)
    {
        if (!scene.IsValid()) return false;
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == MarkerName) return true;
        return false;
    }

    private static PrototypeZombieEnemy FindZombieTemplate()
    {
        foreach (PrototypeZombieEnemy enemy in Object.FindObjectsByType<PrototypeZombieEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (enemy != null && enemy.name == "Zombie_Wave_Template") return enemy;
        return null;
    }

    private static void ConfigureNightLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.11f, 0.14f, 0.20f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.035f, 0.055f, 0.09f);
        RenderSettings.fogStartDistance = 22f;
        RenderSettings.fogEndDistance = 82f;

        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject != null)
        {
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.075f);
            camera.farClipPlane = 95f;
            camera.fieldOfView = 66f;
        }

        GameObject moonLight = GameObject.Find("Directional Light");
        if (moonLight != null)
        {
            Light light = moonLight.GetComponent<Light>();
            light.color = new Color(0.54f, 0.65f, 0.9f);
            light.intensity = 0.48f;
            light.shadows = LightShadows.Soft;
            moonLight.transform.rotation = Quaternion.Euler(34f, -28f, 0f);
        }
        QualitySettings.shadowDistance = 48f;
    }

    private static void BuildPlayerRig(GameObject player)
    {
        if (player.transform.Find("Player_Visual_Rig") != null) return;
        Renderer capsule = player.GetComponent<Renderer>();
        if (capsule != null) capsule.enabled = false;

        Transform rig = ChildRoot(player.transform, "Player_Visual_Rig");
        Material jacket = Material(new Color(0.13f, 0.18f, 0.25f));
        Material vest = Material(new Color(0.31f, 0.27f, 0.20f));
        Material skin = Material(new Color(0.56f, 0.39f, 0.30f));
        Material cloth = Material(new Color(0.37f, 0.34f, 0.30f));
        Material metal = Material(new Color(0.16f, 0.20f, 0.23f));
        Material boot = Material(new Color(0.09f, 0.085f, 0.08f));

        Part(PrimitiveType.Capsule, "Jacket Torso", rig, new Vector3(0f, 0.04f, 0f), new Vector3(0.62f, 0.60f, 0.39f), jacket);
        Part(PrimitiveType.Cube, "Survival Vest", rig, new Vector3(0f, 0.08f, 0.202f), new Vector3(0.48f, 0.43f, 0.11f), vest);
        Part(PrimitiveType.Cube, "Vest Left Pocket", rig, new Vector3(-0.13f, -0.06f, 0.268f), new Vector3(0.13f, 0.15f, 0.055f), cloth);
        Part(PrimitiveType.Cube, "Vest Right Pocket", rig, new Vector3(0.13f, -0.06f, 0.268f), new Vector3(0.13f, 0.15f, 0.055f), cloth);
        Part(PrimitiveType.Cube, "Backpack", rig, new Vector3(0f, 0.13f, -0.28f), new Vector3(0.43f, 0.55f, 0.22f), cloth);
        Part(PrimitiveType.Cube, "Backpack Roll", rig, new Vector3(0f, 0.43f, -0.30f), new Vector3(0.40f, 0.12f, 0.25f), vest);
        Part(PrimitiveType.Capsule, "Neck", rig, new Vector3(0f, 0.45f, 0.02f), new Vector3(0.19f, 0.17f, 0.18f), skin);

        Transform head = NewPivot("Head Pivot", rig, new Vector3(0f, 0.64f, 0f));
        Part(PrimitiveType.Sphere, "Head", head, Vector3.zero, new Vector3(0.31f, 0.37f, 0.30f), skin);
        Part(PrimitiveType.Sphere, "Hood", head, new Vector3(0f, 0.12f, -0.035f), new Vector3(0.35f, 0.28f, 0.32f), jacket);
        Part(PrimitiveType.Cube, "Face Wrap", head, new Vector3(0f, -0.09f, 0.137f), new Vector3(0.26f, 0.10f, 0.055f), cloth);
        Part(PrimitiveType.Sphere, "Left Eye", head, new Vector3(-0.07f, 0.02f, 0.135f), Vector3.one * 0.035f, metal);
        Part(PrimitiveType.Sphere, "Right Eye", head, new Vector3(0.07f, 0.02f, 0.135f), Vector3.one * 0.035f, metal);

        Transform leftArm = NewPivot("Left Arm Pivot", rig, new Vector3(-0.36f, 0.27f, 0f));
        Part(PrimitiveType.Capsule, "Left Sleeve", leftArm, new Vector3(0f, -0.24f, 0f), new Vector3(0.22f, 0.34f, 0.22f), jacket);
        Part(PrimitiveType.Capsule, "Left Glove", leftArm, new Vector3(0f, -0.51f, 0.015f), new Vector3(0.18f, 0.16f, 0.18f), boot);
        Transform rightArm = NewPivot("Right Arm Pivot", rig, new Vector3(0.36f, 0.27f, 0f));
        Part(PrimitiveType.Capsule, "Right Sleeve", rightArm, new Vector3(0f, -0.24f, 0f), new Vector3(0.22f, 0.34f, 0.22f), jacket);
        Part(PrimitiveType.Capsule, "Right Glove", rightArm, new Vector3(0f, -0.51f, 0.015f), new Vector3(0.18f, 0.16f, 0.18f), boot);

        Transform leftLeg = NewPivot("Left Leg Pivot", rig, new Vector3(-0.16f, -0.30f, 0f));
        Part(PrimitiveType.Capsule, "Left Trouser", leftLeg, new Vector3(0f, -0.27f, 0f), new Vector3(0.23f, 0.36f, 0.24f), cloth);
        Part(PrimitiveType.Cube, "Left Boot", leftLeg, new Vector3(0f, -0.53f, 0.06f), new Vector3(0.25f, 0.14f, 0.36f), boot);
        Transform rightLeg = NewPivot("Right Leg Pivot", rig, new Vector3(0.16f, -0.30f, 0f));
        Part(PrimitiveType.Capsule, "Right Trouser", rightLeg, new Vector3(0f, -0.27f, 0f), new Vector3(0.23f, 0.36f, 0.24f), cloth);
        Part(PrimitiveType.Cube, "Right Boot", rightLeg, new Vector3(0f, -0.53f, 0.06f), new Vector3(0.25f, 0.14f, 0.36f), boot);

        PrototypeCharacterAnimation animation = player.GetComponent<PrototypeCharacterAnimation>();
        if (animation == null) animation = player.AddComponent<PrototypeCharacterAnimation>();
        animation.Configure(leftArm, rightArm, leftLeg, rightLeg, head);
    }

    private static void BuildZombieRig(PrototypeZombieEnemy enemy)
    {
        Transform zombie = enemy.transform;
        Transform oldRig = zombie.Find("Zombie_Visual_Rig");
        if (oldRig != null) return;
        for (int i = zombie.childCount - 1; i >= 0; i--) Object.DestroyImmediate(zombie.GetChild(i).gameObject);

        Transform rig = ChildRoot(zombie, "Zombie_Visual_Rig");
        Material flesh = Material(new Color(0.28f, 0.37f, 0.31f));
        Material rotten = Material(new Color(0.19f, 0.26f, 0.23f));
        Material tornCloth = Material(new Color(0.19f, 0.16f, 0.19f));
        Material bone = Material(new Color(0.67f, 0.63f, 0.48f));
        Material eye = EmissiveMaterial(new Color(0.95f, 0.23f, 0.12f));
        Part(PrimitiveType.Capsule, "Torn Coat", rig, new Vector3(0f, 0.05f, 0f), new Vector3(0.70f, 0.62f, 0.40f), tornCloth);
        Part(PrimitiveType.Cube, "Ragged Shoulder", rig, new Vector3(-0.25f, 0.29f, 0f), new Vector3(0.32f, 0.20f, 0.45f), rotten);
        Part(PrimitiveType.Cube, "Exposed Ribs", rig, new Vector3(0f, 0.02f, 0.208f), new Vector3(0.28f, 0.27f, 0.045f), bone);
        Part(PrimitiveType.Sphere, "Zombie Head", rig, new Vector3(0f, 0.60f, 0.02f), new Vector3(0.36f, 0.38f, 0.34f), flesh);
        Part(PrimitiveType.Cube, "Broken Jaw", rig, new Vector3(0f, 0.43f, 0.16f), new Vector3(0.23f, 0.10f, 0.18f), rotten);
        Part(PrimitiveType.Sphere, "Left Glowing Eye", rig, new Vector3(-0.085f, 0.64f, 0.17f), Vector3.one * 0.05f, eye);
        Part(PrimitiveType.Sphere, "Right Glowing Eye", rig, new Vector3(0.085f, 0.64f, 0.17f), Vector3.one * 0.05f, eye);
        Part(PrimitiveType.Cube, "Hair Tuft", rig, new Vector3(0f, 0.81f, -0.01f), new Vector3(0.3f, 0.10f, 0.24f), rotten);

        Transform leftArm = NewPivot("Left Arm Pivot", rig, new Vector3(-0.41f, 0.20f, 0f));
        leftArm.localRotation = Quaternion.Euler(-20f, 0f, -12f);
        Part(PrimitiveType.Capsule, "Left Decayed Arm", leftArm, new Vector3(0f, -0.27f, 0f), new Vector3(0.21f, 0.39f, 0.21f), flesh);
        Part(PrimitiveType.Capsule, "Left Hand", leftArm, new Vector3(0f, -0.53f, 0.04f), new Vector3(0.17f, 0.17f, 0.17f), rotten);
        Transform rightArm = NewPivot("Right Arm Pivot", rig, new Vector3(0.41f, 0.20f, 0f));
        rightArm.localRotation = Quaternion.Euler(-24f, 0f, 14f);
        Part(PrimitiveType.Capsule, "Right Decayed Arm", rightArm, new Vector3(0f, -0.27f, 0f), new Vector3(0.21f, 0.39f, 0.21f), flesh);
        Part(PrimitiveType.Capsule, "Right Hand", rightArm, new Vector3(0f, -0.53f, 0.04f), new Vector3(0.17f, 0.17f, 0.17f), rotten);
        Transform leftLeg = NewPivot("Left Leg Pivot", rig, new Vector3(-0.17f, -0.28f, 0f));
        Part(PrimitiveType.Capsule, "Left Torn Trouser", leftLeg, new Vector3(0f, -0.30f, 0f), new Vector3(0.22f, 0.40f, 0.23f), tornCloth);
        Part(PrimitiveType.Cube, "Left Exposed Shin", leftLeg, new Vector3(0f, -0.49f, 0.07f), new Vector3(0.12f, 0.18f, 0.13f), bone);
        Transform rightLeg = NewPivot("Right Leg Pivot", rig, new Vector3(0.17f, -0.28f, 0f));
        Part(PrimitiveType.Capsule, "Right Torn Trouser", rightLeg, new Vector3(0f, -0.30f, 0f), new Vector3(0.22f, 0.40f, 0.23f), rotten);
        Part(PrimitiveType.Cube, "Right Boot", rightLeg, new Vector3(0f, -0.54f, 0.06f), new Vector3(0.24f, 0.12f, 0.32f), tornCloth);

        PrototypeCharacterAnimation animation = zombie.GetComponent<PrototypeCharacterAnimation>();
        if (animation == null) animation = zombie.gameObject.AddComponent<PrototypeCharacterAnimation>();
        animation.Configure(leftArm, rightArm, leftLeg, rightLeg, rig.Find("Zombie Head"));
        enemy.Initialize(GameObject.Find("Player_Prototype")?.GetComponent<PrototypePlayerHealth>(), rig);
    }

    private static void AddForest(Transform parent)
    {
        Material bark = Material(new Color(0.18f, 0.12f, 0.11f));
        Material[] foliage =
        {
            Material(new Color(0.055f, 0.16f, 0.13f)),
            Material(new Color(0.075f, 0.20f, 0.16f)),
            Material(new Color(0.10f, 0.22f, 0.17f)),
            Material(new Color(0.09f, 0.16f, 0.19f))
        };
        Material dead = Material(new Color(0.15f, 0.13f, 0.12f));
        Random.State previousRandom = Random.state;
        Random.InitState(3609);

        foreach (PrototypeTreeNode existing in Object.FindObjectsByType<PrototypeTreeNode>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform visuals = existing.transform.Find("Tree Visuals");
            if (visuals == null || visuals.Find("Pine Canopy Upgrade") != null) continue;
            Transform pine = ChildRoot(visuals, "Pine Canopy Upgrade");
            foreach (Transform child in visuals)
                if (child.name.StartsWith("Canopy")) child.gameObject.SetActive(false);
            CreateCone(pine, "Lower Pine Boughs", new Vector3(0f, 1.45f, 0f), 2.5f, 1.9f, foliage[Random.Range(0, foliage.Length)]);
            CreateCone(pine, "Middle Pine Boughs", new Vector3(0f, 2.35f, 0f), 2.2f, 1.5f, foliage[Random.Range(0, foliage.Length)]);
            CreateCone(pine, "Upper Pine Boughs", new Vector3(0f, 3.15f, 0f), 1.8f, 1.1f, foliage[Random.Range(0, foliage.Length)]);
            CreateCone(pine, "Pine Crown", new Vector3(0f, 3.85f, 0f), 1.35f, 0.76f, foliage[Random.Range(0, foliage.Length)]);
        }

        int made = 0;
        int attempts = 0;
        while (made < 62 && attempts++ < 240)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float radius = Random.Range(14f, 39f);
            Vector3 point = new Vector3(direction.x * radius, 0f, direction.y * radius);
            if (NearZone(point, new Vector3(-14f, 0f, 14f), 9f)
                || NearZone(point, new Vector3(14f, 0f, -13f), 7f)
                || NearZone(point, new Vector3(5f, 0f, -4f), 5.5f)) continue;
            if (Random.value < 0.17f) CreateDeadTree(parent, point, Random.Range(0.8f, 1.35f), dead);
            else CreatePineTree(parent, point, Random.Range(0.8f, 1.38f), bark, foliage[Random.Range(0, foliage.Length)]);
            made++;
        }
        Random.state = previousRandom;

        Transform path = ChildRoot(parent, "Mud Trail");
        Material mud = Material(new Color(0.12f, 0.105f, 0.095f));
        FlatPatch(path, "Trail Center", new Vector3(0f, 0.012f, 12f), new Vector3(5f, 0.025f, 34f), mud, -12f);
        FlatPatch(path, "Trail Branch", new Vector3(-9f, 0.015f, 16f), new Vector3(3.2f, 0.025f, 14f), mud, 38f);
    }

    private static void CreatePineTree(Transform parent, Vector3 position, float scale, Material bark, Material needles)
    {
        GameObject tree = new GameObject("PineTree_Apocalyptic");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;
        CapsuleCollider collider = tree.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 2f, 0f);
        collider.height = 4f;
        collider.radius = 0.34f;
        Part(PrimitiveType.Cylinder, "Pine Trunk", tree.transform, new Vector3(0f, 2f, 0f), new Vector3(0.36f, 2f, 0.36f), bark);
        CreateCone(tree.transform, "Needle Tier One", new Vector3(0f, 1.55f, 0f), 2.8f, 2.1f, needles);
        CreateCone(tree.transform, "Needle Tier Two", new Vector3(0f, 2.55f, 0f), 2.5f, 1.7f, needles);
        CreateCone(tree.transform, "Needle Tier Three", new Vector3(0f, 3.55f, 0f), 2.15f, 1.35f, needles);
        CreateCone(tree.transform, "Needle Crown", new Vector3(0f, 4.45f, 0f), 1.6f, 0.96f, needles);
    }

    private static void CreateDeadTree(Transform parent, Vector3 position, float scale, Material bark)
    {
        GameObject tree = new GameObject("DeadTree_Silhouette");
        tree.transform.SetParent(parent, false);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;
        CapsuleCollider collider = tree.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 2f, 0f);
        collider.height = 4f;
        collider.radius = 0.27f;
        Transform trunk = Part(PrimitiveType.Cylinder, "Dead Trunk", tree.transform, new Vector3(0f, 2f, 0f), new Vector3(0.34f, 2f, 0.34f), bark);
        trunk.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-8f, 8f));
        Branch(tree.transform, new Vector3(-0.55f, 2.8f, 0f), new Vector3(-0.85f, 3.45f, 0.05f), 0.17f, bark);
        Branch(tree.transform, new Vector3(0.48f, 2.55f, 0f), new Vector3(1.18f, 3.0f, -0.08f), 0.15f, bark);
        Branch(tree.transform, new Vector3(-0.2f, 3.65f, 0.05f), new Vector3(-0.65f, 4.2f, 0f), 0.11f, bark);
    }

    private static void AddAbandonedOutpost(Transform parent)
    {
        Transform camp = ChildRoot(parent, "Abandoned Survivor Outpost");
        camp.position = new Vector3(-14f, 0f, 14f);
        Material floor = Material(new Color(0.20f, 0.14f, 0.10f));
        Material plank = Material(new Color(0.29f, 0.19f, 0.13f));
        Material rotten = Material(new Color(0.15f, 0.12f, 0.10f));
        Material tarp = Material(new Color(0.12f, 0.15f, 0.17f));
        Block(camp, "Raised timber floor", new Vector3(0f, 0.12f, 0f), new Vector3(7f, 0.24f, 5.8f), floor);
        for (int i = -3; i <= 3; i++) Block(camp, "Worn floor plank", new Vector3(i * 0.9f, 0.26f, 0f), new Vector3(0.78f, 0.05f, 5.6f), plank);
        Block(camp, "Back shelter wall", new Vector3(0f, 1.5f, 2.7f), new Vector3(6.8f, 2.7f, 0.2f), plank);
        Block(camp, "Left broken wall", new Vector3(-3.3f, 1.0f, 0.4f), new Vector3(0.22f, 1.8f, 4.6f), plank);
        Block(camp, "Left top brace", new Vector3(-3.3f, 2.25f, 0.4f), new Vector3(0.26f, 0.22f, 4.6f), rotten, new Vector3(0f, 0f, 9f));
        Block(camp, "Right remaining wall", new Vector3(3.2f, 0.9f, 1.8f), new Vector3(0.2f, 1.7f, 1.8f), plank, new Vector3(0f, 0f, -5f));
        Block(camp, "Collapsed roof beam", new Vector3(-0.2f, 2.85f, 0.6f), new Vector3(7.6f, 0.2f, 0.23f), rotten, new Vector3(0f, 0f, -8f));
        Block(camp, "Torn tarp roof", new Vector3(1.35f, 2.75f, 1.25f), new Vector3(3.4f, 0.07f, 2.5f), tarp, new Vector3(0f, 0f, 8f));
        Block(camp, "Door brace", new Vector3(-0.2f, 1.0f, -2.6f), new Vector3(0.17f, 2f, 0.18f), rotten, new Vector3(0f, 0f, -12f));
        CreateCrate(camp, new Vector3(-2.0f, 0.35f, -1.0f), plank);
        CreateCrate(camp, new Vector3(-2.65f, 0.35f, -1.25f), floor);
        Block(camp, "Warning sign", new Vector3(4f, 1.45f, -1.8f), new Vector3(0.13f, 2.7f, 0.13f), rotten);
        Block(camp, "Warning board", new Vector3(4f, 2.5f, -1.8f), new Vector3(1.35f, 0.75f, 0.12f), floor);
        AddPointLight("Outpost Lantern", camp.position + new Vector3(1f, 2.4f, 2.1f), new Color(1f, 0.43f, 0.16f), 2.1f, 8f, camp);
    }

    private static void AddWreckedCar(Transform parent)
    {
        Transform wreck = ChildRoot(parent, "Burned Vehicle Wreck");
        wreck.position = new Vector3(14f, 0f, -13f);
        wreck.rotation = Quaternion.Euler(0f, 28f, 0f);
        Material rust = Material(new Color(0.22f, 0.105f, 0.07f));
        Material rustLight = Material(new Color(0.36f, 0.18f, 0.10f));
        Material glass = Material(new Color(0.075f, 0.11f, 0.13f));
        Material rubber = Material(new Color(0.055f, 0.06f, 0.065f));
        Block(wreck, "Collapsed chassis", new Vector3(0f, 0.72f, 0f), new Vector3(5.3f, 0.78f, 2.25f), rust, new Vector3(0f, 0f, -4f));
        Block(wreck, "Crushed hood", new Vector3(1.65f, 1.0f, 0f), new Vector3(1.55f, 0.2f, 2.2f), rustLight, new Vector3(0f, 0f, 12f));
        Block(wreck, "Cabin shell", new Vector3(-0.6f, 1.48f, 0f), new Vector3(2.55f, 1.1f, 1.95f), rust);
        Block(wreck, "Windshield shadow", new Vector3(0.55f, 1.68f, 0.02f), new Vector3(0.08f, 0.62f, 1.62f), glass, new Vector3(0f, 0f, -20f));
        Block(wreck, "Missing side door", new Vector3(-0.85f, 1.26f, 1.0f), new Vector3(1.3f, 0.72f, 0.08f), rustLight, new Vector3(0f, 0f, 12f));
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            Transform wheel = Part(PrimitiveType.Cylinder, "Flat tire", wreck, new Vector3(x * 1.55f, 0.48f, z * 1.08f), new Vector3(0.55f, 0.18f, 0.55f), rubber);
            wheel.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        Block(wreck, "Bent bumper", new Vector3(2.7f, 0.43f, 0f), new Vector3(0.23f, 0.23f, 2.5f), rustLight, new Vector3(12f, 0f, 18f));
        Block(wreck, "Exhaust plume debris", new Vector3(-2.4f, 0.22f, -0.45f), new Vector3(1.2f, 0.15f, 0.35f), rubber, new Vector3(0f, 0f, 18f));
    }

    private static void AddCampfire(Transform parent)
    {
        Transform fire = ChildRoot(parent, "Cold Ember Camp");
        fire.position = new Vector3(5f, 0f, -4f);
        Material charcoal = Material(new Color(0.11f, 0.095f, 0.085f));
        Material log = Material(new Color(0.24f, 0.14f, 0.095f));
        Material flame = EmissiveMaterial(new Color(1f, 0.24f, 0.035f));
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 1.2f, 0.2f, Mathf.Sin(angle) * 1.0f);
            Transform rock = Part(PrimitiveType.Sphere, "Fire ring stone", fire, pos, new Vector3(0.47f, 0.3f, 0.4f), charcoal);
            rock.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 14f);
        }
        Transform logOne = Part(PrimitiveType.Cylinder, "Burnt log", fire, new Vector3(0f, 0.28f, 0f), new Vector3(0.22f, 0.75f, 0.22f), log);
        logOne.localRotation = Quaternion.Euler(90f, 0f, 34f);
        Transform logTwo = Part(PrimitiveType.Cylinder, "Burnt log", fire, new Vector3(0f, 0.31f, 0f), new Vector3(0.2f, 0.66f, 0.2f), log);
        logTwo.localRotation = Quaternion.Euler(90f, 0f, -42f);
        CreateCone(fire, "Last ember", new Vector3(0f, 0.27f, 0f), 0.72f, 0.18f, flame);
        AddPointLight("Low campfire glow", fire.position + new Vector3(0f, 0.65f, 0f), new Color(1f, 0.27f, 0.07f), 1.7f, 9f, fire);
        FlatPatch(fire, "Ash patch", new Vector3(0f, 0.015f, 0f), new Vector3(3.5f, 0.02f, 2.8f), Material(new Color(0.09f, 0.085f, 0.08f)), 0f);
    }

    private static void AddRubbleAndBrush(Transform parent)
    {
        Material stone = Material(new Color(0.19f, 0.22f, 0.23f));
        Material stoneLight = Material(new Color(0.26f, 0.28f, 0.27f));
        Material deadGrass = Material(new Color(0.21f, 0.20f, 0.13f));
        Random.State previousRandom = Random.state;
        Random.InitState(9107);
        for (int i = 0; i < 55; i++)
        {
            Vector2 d = Random.insideUnitCircle.normalized;
            float r = Random.Range(7.5f, 37f);
            Vector3 pos = new Vector3(d.x * r, 0.15f, d.y * r);
            if (NearZone(pos, new Vector3(-14f, 0f, 14f), 7f) || NearZone(pos, new Vector3(14f, 0f, -13f), 5.5f)) continue;
            Transform rock = Part(PrimitiveType.Sphere, "Forest rubble", parent, pos,
                new Vector3(Random.Range(0.3f, 1.2f), Random.Range(0.22f, 0.75f), Random.Range(0.3f, 1.0f)),
                Random.value > 0.5f ? stone : stoneLight);
            rock.rotation = Random.rotation;
        }
        for (int i = 0; i < 34; i++)
        {
            Vector2 d = Random.insideUnitCircle.normalized;
            float r = Random.Range(8f, 35f);
            CreateBrush(parent, new Vector3(d.x * r, 0f, d.y * r), deadGrass);
        }
        Random.state = previousRandom;
    }

    private static void CreateBrush(Transform parent, Vector3 position, Material material)
    {
        Transform brush = ChildRoot(parent, "Dead Undergrowth");
        brush.position = position;
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f;
            Transform stalk = CreateCone(brush, "Dry thorn", Vector3.zero, Random.Range(0.55f, 0.95f), 0.08f, material);
            stalk.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), angle, Random.Range(-22f, 22f));
        }
    }

    private static void AddMoon(Transform parent)
    {
        Material moon = EmissiveMaterial(new Color(0.35f, 0.46f, 0.72f));
        Transform orb = Part(PrimitiveType.Sphere, "Moon", parent, new Vector3(-24f, 30f, 52f), Vector3.one * 6.5f, moon);
        orb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void UpgradeResourceVisuals()
    {
        foreach (PrototypeResourcePickup pickup in Object.FindObjectsByType<PrototypeResourcePickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (pickup != null) pickup.RefreshVisual();
    }

    private static void MarkEnvironmentStatic(Transform root)
    {
        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in descendants)
            GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);
    }

    private static void CreateCrate(Transform parent, Vector3 position, Material wood)
    {
        Transform crate = ChildRoot(parent, "Salvage Crate");
        crate.localPosition = position;
        Material dark = Material(new Color(0.13f, 0.095f, 0.07f));
        Block(crate, "Crate body", Vector3.zero, new Vector3(0.9f, 0.8f, 0.9f), wood);
        Block(crate, "Crate front brace", new Vector3(0f, 0f, 0.47f), new Vector3(0.88f, 0.12f, 0.06f), dark, new Vector3(0f, 0f, 35f));
        Block(crate, "Crate lid brace", new Vector3(0f, 0.41f, 0f), new Vector3(0.9f, 0.1f, 0.1f), dark);
    }

    private static void AddPointLight(string objectName, Vector3 position, Color color, float intensity, float range, Transform parent)
    {
        GameObject lightObject = new GameObject(objectName);
        lightObject.transform.SetParent(parent, true);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    private static void FlatPatch(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material, float rotation)
    {
        Transform patch = Part(PrimitiveType.Cube, objectName, parent, position, scale, material);
        patch.localRotation = Quaternion.Euler(0f, rotation, 0f);
    }

    private static void Branch(Transform parent, Vector3 start, Vector3 end, float thickness, Material material)
    {
        Vector3 direction = end - start;
        Transform branch = Part(PrimitiveType.Cylinder, "Snapped dead branch", parent, (start + end) * 0.5f,
            new Vector3(thickness, direction.magnitude * 0.5f, thickness), material);
        branch.up = direction.normalized;
    }

    private static Transform CreateCone(Transform parent, string objectName, Vector3 position, float height, float diameter, Material material)
    {
        if (coneMesh == null) coneMesh = BuildConeMesh(7);
        GameObject cone = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
        cone.transform.SetParent(parent, false);
        cone.transform.localPosition = position;
        cone.transform.localScale = new Vector3(diameter, height, diameter);
        cone.GetComponent<MeshFilter>().sharedMesh = coneMesh;
        cone.GetComponent<MeshRenderer>().sharedMaterial = material;
        return cone.transform;
    }

    private static Mesh BuildConeMesh(int sides)
    {
        Mesh mesh = new Mesh { name = "Low Poly Fir Cone" };
        Vector3[] vertices = new Vector3[sides + 2];
        int[] triangles = new int[sides * 6];
        vertices[0] = new Vector3(0f, 1f, 0f);
        vertices[sides + 1] = Vector3.zero;
        for (int i = 0; i < sides; i++)
        {
            float angle = 2f * Mathf.PI * i / sides;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, 0f, Mathf.Sin(angle) * 0.5f);
            int side = i * 3;
            int next = ((i + 1) % sides) + 1;
            triangles[side] = 0;
            triangles[side + 1] = i + 1;
            triangles[side + 2] = next;
            int baseFace = sides * 3 + side;
            triangles[baseFace] = sides + 1;
            triangles[baseFace + 1] = next;
            triangles[baseFace + 2] = i + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Transform NewPivot(string name, Transform parent, Vector3 position)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        return pivot;
    }

    private static Transform ChildRoot(Transform parent, string name)
    {
        Transform child = new GameObject(name).transform;
        child.SetParent(parent, false);
        return child;
    }

    private static Transform Part(PrimitiveType primitive, string name, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return part.transform;
    }

    private static void Block(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation = default)
    {
        Transform part = Part(PrimitiveType.Cube, name, parent, position, scale, material);
        part.localRotation = Quaternion.Euler(rotation);
        if (scale.x > 0.5f && scale.y > 0.5f && scale.z > 0.5f)
        {
            BoxCollider collider = part.gameObject.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
        }
    }

    private static Material Material(Color color)
    {
        if (Materials.TryGetValue(color, out Material existing) && existing != null) return existing;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material created = new Material(shader) { color = color };
        Materials[color] = created;
        return created;
    }

    private static Material EmissiveMaterial(Color color)
    {
        Material material = Material(color);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.8f);
        }
        return material;
    }

    private static bool NearZone(Vector3 point, Vector3 center, float radius) =>
        new Vector2(point.x - center.x, point.z - center.z).sqrMagnitude < radius * radius;
}
