using System.Collections.Generic;
using System.IO;
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
    private const string VillageMarkerName = "APOCALYPTIC_VILLAGE_DETAILS_V2";
    private const string DistrictMarkerName = "APOCALYPTIC_DISTRICT_AND_CHARACTER_V3";
    private const string NaturalForestMarkerName = "NATURAL_FOREST_REFINEMENT_V4";
    private const string FieldPolishMarkerName = "FIELD_LAYOUT_AND_WATER_V5";
    private const string FieldLifeMarkerName = "FIELD_PHYSICS_AND_VARIETY_V6";
    private const float BuildZoneRadius = 7.5f;
    private static readonly Vector3 BuildZoneCenter = new Vector3(0f, 0f, -27f);
    private static Mesh coneMesh;
    private static Mesh grassMesh;
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
        if (scene.path != ScenePath || scene.isDirty) return;
        if (!HasUpgradeMarker(scene))
        {
            UpgradeScene(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: escenario nocturno apocalíptico aplicado al prototipo.");
        }
        else if (!HasVillagePass(scene))
        {
            AddVillageDetails(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: poblado abandonado, vegetación y ambiente animado añadidos.");
        }
        else if (!HasDistrictPass(scene))
        {
            AddDistrictAndCharacterPass(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: distrito ampliado, siluetas orgánicas y superviviente detallado aplicados.");
        }
        else if (!HasNaturalForestPass(scene))
        {
            RefineNaturalForest(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: copas forestales naturales y materiales mate aplicados.");
        }
        else if (!HasFieldPolishPass(scene))
        {
            AddFieldPolishPass(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: campo ampliado, zona de construcción, lago animado y objetos apoyados en el suelo.");
        }
        else if (!HasFieldLifePass(scene))
        {
            AddFieldLifePass(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Zombie Survival: relieve de terreno, estructuras rurales y especies de árboles añadidos.");
        }
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
        if (HasFieldLifePass(scene))
        {
            EditorUtility.DisplayDialog("Zombie Survival", "La mejora visual ya está aplicada en esta escena.", "Aceptar");
            return;
        }
        if (!HasUpgradeMarker(scene)) UpgradeScene(scene);
        if (!HasVillagePass(scene)) AddVillageDetails(scene);
        if (!HasDistrictPass(scene)) AddDistrictAndCharacterPass(scene);
        if (!HasNaturalForestPass(scene)) RefineNaturalForest(scene);
        if (!HasFieldPolishPass(scene)) AddFieldPolishPass(scene);
        if (!HasFieldLifePass(scene)) AddFieldLifePass(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    public static void ApplyAllUpgradesForBatch()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!HasUpgradeMarker(scene)) UpgradeScene(scene);
        if (!HasVillagePass(scene)) AddVillageDetails(scene);
        if (!HasDistrictPass(scene)) AddDistrictAndCharacterPass(scene);
        if (!HasNaturalForestPass(scene)) RefineNaturalForest(scene);
        if (!HasFieldPolishPass(scene)) AddFieldPolishPass(scene);
        if (!HasFieldLifePass(scene)) AddFieldLifePass(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Zombie Survival: mejoras de escenario aplicadas y guardadas desde la herramienta de edición.");
    }

    public static void UpgradeScene(Scene scene)
    {
        if (!scene.IsValid() || HasUpgradeMarker(scene)) return;

        GameObject dressing = new GameObject(MarkerName);
        SceneManager.MoveGameObjectToScene(dressing, scene);

        ConfigureNightLighting();
        GameObject ground = GameObject.Find("Ground_Prototype");
        if (ground != null) ground.GetComponent<Renderer>().sharedMaterial = Material(new Color(0.15f, 0.17f, 0.16f));

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
        foreach (PrototypeZombieEnemy enemy in Object.FindObjectsByType<PrototypeZombieEnemy>(FindObjectsInactive.Include))
            if (enemy != null && enemy.name == "Zombie_Wave_Template") return enemy;
        return null;
    }

    private static void ConfigureNightLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.19f, 0.22f, 0.29f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.075f, 0.095f, 0.13f);
        RenderSettings.fogStartDistance = 34f;
        RenderSettings.fogEndDistance = 102f;

        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject != null)
        {
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.11f);
            camera.farClipPlane = 115f;
            camera.fieldOfView = 66f;
        }

        GameObject moonLight = GameObject.Find("Directional Light");
        if (moonLight != null)
        {
            Light light = moonLight.GetComponent<Light>();
            light.color = new Color(0.54f, 0.65f, 0.9f);
            light.intensity = 0.68f;
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

        foreach (PrototypeTreeNode existing in Object.FindObjectsByType<PrototypeTreeNode>(FindObjectsInactive.Include))
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
        foreach (PrototypeResourcePickup pickup in Object.FindObjectsByType<PrototypeResourcePickup>(FindObjectsInactive.Include))
            if (pickup != null) pickup.RefreshVisual();
    }

    private static void MarkEnvironmentStatic(Transform root)
    {
        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        PrototypeApocalypseMotion motion = root.GetComponentInParent<PrototypeApocalypseMotion>();
        HashSet<Transform> animated = new HashSet<Transform>();
        if (motion != null && motion.AnimatedTransforms != null)
            foreach (Transform target in motion.AnimatedTransforms) if (target != null) animated.Add(target);
        foreach (Transform child in descendants)
            if (!animated.Contains(child) && child.GetComponent<ParticleSystem>() == null)
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

    [MenuItem("BERNEOX/Ampliar distrito y mejorar superviviente")]
    private static void AddVillageDetailsFromMenu()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            EditorUtility.DisplayDialog("Zombie Survival", "Abre Assets/Scenes/ZombiePrototype.unity.", "Aceptar");
            return;
        }
        if (!HasUpgradeMarker(scene)) UpgradeScene(scene);
        if (!HasVillagePass(scene)) AddVillageDetails(scene);
        if (!HasDistrictPass(scene)) AddDistrictAndCharacterPass(scene);
        if (!HasNaturalForestPass(scene)) RefineNaturalForest(scene);
        if (!HasFieldPolishPass(scene)) AddFieldPolishPass(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static bool HasVillagePass(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == VillageMarkerName) return true;
        return false;
    }

    private static bool HasDistrictPass(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == DistrictMarkerName) return true;
        return false;
    }

    private static bool HasNaturalForestPass(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == NaturalForestMarkerName) return true;
        return false;
    }

    private static bool HasFieldPolishPass(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == FieldPolishMarkerName) return true;
        return false;
    }

    private static bool HasFieldLifePass(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == FieldLifeMarkerName) return true;
        return false;
    }

    private static void AddFieldLifePass(Scene scene)
    {
        GameObject marker = new GameObject(FieldLifeMarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);
        Material mud = SurfaceMaterial("CompactedBuildSoil", new Color(0.34f, 0.28f, 0.21f), SurfaceKind.Mud);
        GameObject ground = GameObject.Find("Ground_Prototype");
        if (ground != null)
        {
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mud;
            if (ground.GetComponent<MeshCollider>() == null)
            {
                MeshFilter filter = ground.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    MeshCollider collider = ground.AddComponent<MeshCollider>();
                    collider.sharedMesh = filter.sharedMesh;
                }
            }
        }
        Transform root = marker.transform;
        Material stone = SurfaceMaterial("FieldTexturedStone", new Color(0.38f, 0.40f, 0.38f), SurfaceKind.Stone);
        Material timber = SurfaceMaterial("FieldAgedWood", new Color(0.37f, 0.25f, 0.16f), SurfaceKind.Wood);
        Material dark = SurfaceMaterial("FieldCharredTimber", new Color(0.20f, 0.16f, 0.12f), SurfaceKind.Wood);
        Material bark = SurfaceMaterial("FieldBarkGrey", new Color(0.31f, 0.29f, 0.24f), SurfaceKind.Wood);
        Material leaf = SurfaceMaterial("FieldCanopyMoss", new Color(0.20f, 0.31f, 0.18f), SurfaceKind.Foliage);
        Material fruit = SurfaceMaterial("OrchardFruit", new Color(0.66f, 0.25f, 0.12f), SurfaceKind.Foliage);
        BuildFieldWell(root, new Vector3(23f, 0f, -30f), stone, timber, dark);
        BuildFieldWaterTower(root, new Vector3(-21f, 0f, -34f), timber, dark, rustMaterial());
        AddOrchardTrees(root, bark, leaf, fruit);
        MarkEnvironmentStatic(root);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static Material rustMaterial() => SurfaceMaterial("FieldRust", new Color(0.52f, 0.27f, 0.17f), SurfaceKind.Rust);

    private static void BuildFieldWell(Transform parent, Vector3 position, Material stone, Material timber, Material dark)
    {
        Transform well = ChildRoot(parent, "Old Stone Well");
        well.position = position;
        Transform baseRing = Part(PrimitiveType.Cylinder, "Stone well basin", well, new Vector3(0f, 0.48f, 0f), new Vector3(2.3f, 0.48f, 2.3f), stone);
        baseRing.GetComponent<Collider>().enabled = true;
        Part(PrimitiveType.Cylinder, "Deep well opening", well, new Vector3(0f, 0.76f, 0f), new Vector3(1.58f, 0.06f, 1.58f), dark);
        Block(well, "Well support left", new Vector3(-1.03f, 1.77f, 0f), new Vector3(0.18f, 2.45f, 0.2f), timber);
        Block(well, "Well support right", new Vector3(1.03f, 1.77f, 0f), new Vector3(0.18f, 2.45f, 0.2f), timber);
        Branch(well, new Vector3(-0.95f, 2.75f, 0.06f), new Vector3(0.95f, 2.75f, 0.06f), 0.10f, dark);
        Branch(well, new Vector3(0f, 2.55f, -0.2f), new Vector3(0f, 2.55f, 0.85f), 0.07f, timber);
        Part(PrimitiveType.Cube, "Well roof", well, new Vector3(0f, 3.02f, 0f), new Vector3(2.8f, 0.18f, 1.9f), dark).rotation = Quaternion.Euler(0f, 0f, -8f);
        Part(PrimitiveType.Cylinder, "Well hand crank", well, new Vector3(0f, 2.47f, 0.1f), new Vector3(0.065f, 0.64f, 0.065f), timber).rotation = Quaternion.Euler(0f, 0f, 90f);
    }

    private static void BuildFieldWaterTower(Transform parent, Vector3 position, Material timber, Material dark, Material rust)
    {
        Transform tower = ChildRoot(parent, "Leaning Farm Water Tower");
        tower.position = position;
        Block(tower, "Tower leg front left", new Vector3(-2f, 2.1f, -1.7f), new Vector3(0.24f, 4.3f, 0.24f), timber, new Vector3(0f, 0f, -8f));
        Block(tower, "Tower leg front right", new Vector3(2f, 2.1f, -1.7f), new Vector3(0.24f, 4.3f, 0.24f), timber, new Vector3(0f, 0f, 8f));
        Block(tower, "Tower leg rear left", new Vector3(-2f, 2.1f, 1.7f), new Vector3(0.24f, 4.3f, 0.24f), timber, new Vector3(0f, 0f, -8f));
        Block(tower, "Tower leg rear right", new Vector3(2f, 2.1f, 1.7f), new Vector3(0.24f, 4.3f, 0.24f), timber, new Vector3(0f, 0f, 8f));
        Block(tower, "Tank lower rim", new Vector3(0f, 4.3f, 0f), new Vector3(4.7f, 0.35f, 4.2f), rust);
        Block(tower, "Rusted water tank", new Vector3(0f, 5.55f, 0f), new Vector3(4.2f, 2.2f, 3.8f), dark);
        Block(tower, "Tank roof", new Vector3(0f, 6.72f, 0f), new Vector3(4.35f, 0.22f, 3.95f), rust);
        Block(tower, "Cross brace front", new Vector3(0f, 2.0f, -1.76f), new Vector3(3.9f, 0.16f, 0.18f), dark, new Vector3(0f, 0f, 24f));
        Block(tower, "Cross brace side", new Vector3(2.02f, 2.0f, 0f), new Vector3(0.18f, 0.16f, 3.35f), dark, new Vector3(24f, 0f, 0f));
    }

    private static void AddOrchardTrees(Transform parent, Material bark, Material leaves, Material fruit)
    {
        Random.State old = Random.state;
        Random.InitState(19102026);
        Vector3[] orchard =
        {
            new Vector3(-10f, 0f, -38f), new Vector3(-4f, 0f, -40f), new Vector3(3f, 0f, -39f),
            new Vector3(10f, 0f, -37f), new Vector3(17f, 0f, -40f), new Vector3(28f, 0f, -39f),
            new Vector3(-29f, 0f, -22f), new Vector3(27f, 0f, 13f)
        };
        for (int i = 0; i < orchard.Length; i++)
        {
            Vector3 spot = orchard[i];
            if (NearZone(spot, BuildZoneCenter, 15f) || NearZone(spot, new Vector3(-21f, 0f, -34f), 9f)
                || NearZone(spot, new Vector3(23f, 0f, -30f), 8f)) continue;
            Transform tree = ChildRoot(parent, i % 3 == 0 ? "Leafless Orchard Tree" : "Fruit Orchard Tree");
            tree.position = spot;
            float s = Random.Range(0.82f, 1.18f);
            tree.localScale = Vector3.one * s;
            Transform trunk = Part(PrimitiveType.Cylinder, "Orchard trunk with collision", tree,
                new Vector3(0f, 2.25f, 0f), new Vector3(0.55f, 2.25f, 0.58f), bark);
            CapsuleCollider trunkCollider = trunk.GetComponent<CapsuleCollider>();
            if (trunkCollider != null)
            {
                trunkCollider.radius = 0.95f;
                trunkCollider.height = 4.5f;
            }
            if (i % 3 == 0)
            {
                for (int branch = 0; branch < 5; branch++)
                {
                    float angle = branch * Mathf.PI * 0.4f + 0.2f;
                    Vector3 start = new Vector3(0f, 2.8f + branch * 0.15f, 0f);
                    Vector3 end = new Vector3(Mathf.Cos(angle) * (1.5f + (branch % 2) * 0.4f), 4.2f + branch * 0.12f, Mathf.Sin(angle) * (1.5f + (branch % 2) * 0.4f));
                    Branch(tree, start, end, 0.09f, bark);
                }
            }
            else
            {
                for (int crown = 0; crown < 5; crown++)
                {
                    float angle = crown * Mathf.PI * 0.4f;
                    Vector3 p = new Vector3(Mathf.Cos(angle) * 0.82f, 4.35f + (crown % 2) * 0.35f, Mathf.Sin(angle) * 0.82f);
                    Transform foliage = Part(PrimitiveType.Sphere, "Broadleaf crown", tree, p,
                        new Vector3(Random.Range(1.55f, 2.1f), Random.Range(1.2f, 1.7f), Random.Range(1.45f, 2.0f)), leaves);
                    foliage.GetComponent<Collider>().enabled = false;
                }
                for (int apple = 0; apple < 7; apple++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    Vector3 p = new Vector3(Mathf.Cos(angle) * Random.Range(1.0f, 1.65f), Random.Range(3.45f, 5.0f), Mathf.Sin(angle) * Random.Range(1.0f, 1.65f));
                    Transform berry = Part(PrimitiveType.Sphere, "Wind-fallen red fruit", tree, p, Vector3.one * Random.Range(0.16f, 0.23f), fruit);
                    berry.GetComponent<Collider>().enabled = false;
                }
            }
        }
        Random.state = old;
    }

    private static void AddFieldPolishPass(Scene scene)
    {
        GameObject marker = new GameObject(FieldPolishMarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);
        Transform root = marker.transform;

        GameObject ground = GameObject.Find("Ground_Prototype");
        if (ground != null)
        {
            Vector3 scale = ground.transform.localScale;
            ground.transform.localScale = new Vector3(Mathf.Max(scale.x, 14f), scale.y, Mathf.Max(scale.z, 14f));
        }
        ClearScenicFootprints();

        Material wood = SurfaceMaterial("FieldAgedWood", new Color(0.37f, 0.25f, 0.16f), SurfaceKind.Wood);
        Material darkWood = SurfaceMaterial("FieldCharredTimber", new Color(0.20f, 0.16f, 0.12f), SurfaceKind.Wood);
        Material plaster = SurfaceMaterial("FieldWeatheredPlaster", new Color(0.57f, 0.51f, 0.40f), SurfaceKind.Plaster);
        Material brick = SurfaceMaterial("FieldOldBrick", new Color(0.46f, 0.27f, 0.19f), SurfaceKind.Brick);
        Material rust = SurfaceMaterial("FieldRust", new Color(0.52f, 0.27f, 0.17f), SurfaceKind.Rust);
        Material stone = SurfaceMaterial("FieldTexturedStone", new Color(0.38f, 0.40f, 0.38f), SurfaceKind.Stone);
        Material water = SurfaceMaterial("FieldPondWater", new Color(0.14f, 0.29f, 0.31f), SurfaceKind.Water);
        if (water.HasProperty("_Smoothness")) water.SetFloat("_Smoothness", 0.42f);
        if (water.HasProperty("_Metallic")) water.SetFloat("_Metallic", 0.12f);

        BuildFieldBarn(root, new Vector3(-34f, 0f, 9f), wood, darkWood, rust);
        BuildFieldCabin(root, new Vector3(33f, 0f, 3f), plaster, brick, wood, darkWood);
        BuildFieldGreenhouse(root, new Vector3(-35f, 0f, -12f), darkWood, rust);
        BuildBuildYard(root, wood, stone);
        MeshFilter pond = BuildPond(root, new Vector3(36f, 0f, -26f), water, stone);
        Transform[] treeWind = AddFieldTrees(root);
        AddGroundedFieldDebris(root, wood, stone);
        GroundExistingPickupsAndStones(stone);

        PrototypeApocalypseMotion motion = marker.AddComponent<PrototypeApocalypseMotion>();
        motion.Configure(treeWind, null, new Light[0]);
        motion.ConfigureWater(pond != null ? new[] { pond } : new MeshFilter[0]);
        MarkEnvironmentStatic(root);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void ClearScenicFootprints()
    {
        Vector3[] centers =
        {
            new Vector3(-34f, 0f, 9f), new Vector3(33f, 0f, 3f), new Vector3(-35f, 0f, -12f),
            BuildZoneCenter, new Vector3(36f, 0f, -26f)
        };
        float[] radii = { 9f, 8f, 8f, 10f, 8.5f };
        List<GameObject> remove = new List<GameObject>();
        foreach (Transform item in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (item == null) continue;
            bool tree = item.name == "PineTree_Apocalyptic" || item.name == "DeadTree_Silhouette"
                || item.name.Contains("Resource_Tree") || item.name.Contains("Tree_Prototype");
            bool brushOrRubble = item.name == "Forest rubble" || item.name == "Dead Undergrowth";
            if (!tree && !brushOrRubble)
            {
                PrototypeTreeNode node = item.GetComponent<PrototypeTreeNode>();
                tree = node != null;
            }
            if (!tree && !brushOrRubble) continue;
            for (int i = 0; i < centers.Length; i++)
                if (NearZone(item.position, centers[i], radii[i]))
                {
                    remove.Add(item.gameObject);
                    break;
                }
        }
        foreach (GameObject target in remove) target.SetActive(false);
    }

    private static void BuildFieldBarn(Transform parent, Vector3 position, Material wood, Material dark, Material rust)
    {
        Transform barn = ChildRoot(parent, "Abandoned Field Barn");
        barn.position = position;
        Block(barn, "Stone footing", new Vector3(0f, 0.18f, 0f), new Vector3(10.5f, 0.36f, 8.5f), dark);
        Block(barn, "Back timber wall", new Vector3(0f, 2.4f, -3.9f), new Vector3(9.8f, 4.5f, 0.25f), wood);
        Block(barn, "Left timber wall", new Vector3(-4.8f, 2.4f, 0f), new Vector3(0.25f, 4.5f, 8f), wood);
        Block(barn, "Right timber wall", new Vector3(4.8f, 2.4f, 0.2f), new Vector3(0.25f, 4.5f, 7.5f), wood);
        Block(barn, "Barn front left", new Vector3(-3.25f, 2.4f, 3.9f), new Vector3(3.1f, 4.5f, 0.25f), wood);
        Block(barn, "Barn front right", new Vector3(3.25f, 2.4f, 3.9f), new Vector3(3.1f, 4.5f, 0.25f), wood);
        Block(barn, "Barn sliding door", new Vector3(0f, 1.65f, 4.08f), new Vector3(3.2f, 3.2f, 0.14f), dark);
        Block(barn, "Door brace one", new Vector3(-0.9f, 1.7f, 4.18f), new Vector3(0.16f, 3.0f, 0.08f), rust);
        Block(barn, "Door brace two", new Vector3(0.9f, 1.7f, 4.18f), new Vector3(0.16f, 3.0f, 0.08f), rust);
        Block(barn, "Barn roof left", new Vector3(-2.4f, 5f, 0f), new Vector3(5.5f, 0.32f, 9f), dark, new Vector3(0f, 0f, -24f));
        Block(barn, "Barn roof right", new Vector3(2.4f, 5f, 0f), new Vector3(5.5f, 0.32f, 9f), dark, new Vector3(0f, 0f, 24f));
        Block(barn, "Bent roof ridge", new Vector3(0f, 6.05f, 0f), new Vector3(0.3f, 0.3f, 9.2f), rust, new Vector3(2f, 0f, 0f));
        Block(barn, "Broken loft opening", new Vector3(-4.95f, 3.8f, 1.4f), new Vector3(0.1f, 1.2f, 2.2f), rust, new Vector3(0f, 0f, -10f));
    }

    private static void BuildFieldCabin(Transform parent, Vector3 position, Material plaster, Material brick, Material timber, Material dark)
    {
        Transform cabin = ChildRoot(parent, "Collapsed Ranger Cabin");
        cabin.position = position;
        Block(cabin, "Cabin foundation", new Vector3(0f, 0.18f, 0f), new Vector3(8f, 0.36f, 7f), brick);
        Block(cabin, "Cabin rear wall", new Vector3(0f, 1.7f, -3.1f), new Vector3(7.2f, 3.1f, 0.2f), plaster);
        Block(cabin, "Cabin left wall", new Vector3(-3.5f, 1.7f, 0f), new Vector3(0.2f, 3.1f, 6.1f), plaster);
        Block(cabin, "Cabin right wall", new Vector3(3.5f, 1.5f, 0.6f), new Vector3(0.2f, 2.7f, 4.8f), plaster, new Vector3(0f, 0f, 4f));
        Block(cabin, "Cabin front wall", new Vector3(-2.3f, 1.7f, 3.1f), new Vector3(2.4f, 3.1f, 0.2f), plaster);
        Block(cabin, "Cabin porch beam", new Vector3(0f, 2.8f, 4.1f), new Vector3(6.3f, 0.18f, 0.18f), timber);
        Block(cabin, "Cabin porch post left", new Vector3(-2.7f, 1.3f, 4.1f), new Vector3(0.18f, 2.6f, 0.18f), timber);
        Block(cabin, "Cabin porch post right", new Vector3(2.7f, 1.3f, 4.1f), new Vector3(0.18f, 2.6f, 0.18f), timber);
        Block(cabin, "Cabin roof surviving side", new Vector3(-1.6f, 3.5f, 0f), new Vector3(4.3f, 0.25f, 7.5f), dark, new Vector3(0f, 0f, -19f));
        Block(cabin, "Cabin roof fallen panel", new Vector3(2.3f, 2.9f, 0.3f), new Vector3(2.4f, 0.18f, 3.5f), dark, new Vector3(11f, 0f, 26f));
        Block(cabin, "Boarded front window", new Vector3(1.7f, 1.8f, 3.22f), new Vector3(1.2f, 1.1f, 0.08f), timber, new Vector3(0f, 0f, 4f));
    }

    private static void BuildFieldGreenhouse(Transform parent, Vector3 position, Material frame, Material rust)
    {
        Transform greenhouse = ChildRoot(parent, "Shattered Farm Greenhouse");
        greenhouse.position = position;
        Material glass = Material(new Color(0.16f, 0.28f, 0.27f));
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
            Block(greenhouse, "Greenhouse frame post", new Vector3(x * 3.6f, 1.55f, z * 3f), new Vector3(0.14f, 3.1f, 0.14f), frame);
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            Transform pane = Part(PrimitiveType.Cube, "Dirty greenhouse glass", greenhouse, new Vector3(x * 1.8f, 1.45f, z * 3f), new Vector3(3.4f, 2.3f, 0.045f), glass);
            pane.localRotation = Quaternion.Euler(0f, 0f, x * -3f);
        }
        Block(greenhouse, "Greenhouse ridge beam", new Vector3(0f, 3.2f, 0f), new Vector3(0.16f, 0.16f, 6.5f), rust);
        for (int side = -1; side <= 1; side += 2)
            Block(greenhouse, "Greenhouse roof frame", new Vector3(side * 1.8f, 2.85f, 0f), new Vector3(4.2f, 0.13f, 6.4f), frame, new Vector3(0f, 0f, side * 27f));
        Block(greenhouse, "Broken glass pane on ground", new Vector3(1.4f, 0.04f, 4f), new Vector3(2.6f, 0.05f, 1.7f), glass, new Vector3(0f, 22f, 0f));
        Block(greenhouse, "Rusty side brace", new Vector3(-3.6f, 0.9f, -1.4f), new Vector3(0.1f, 1.8f, 0.1f), rust, new Vector3(0f, 0f, -15f));
    }

    private static void BuildBuildYard(Transform parent, Material wood, Material stone)
    {
        Transform yard = ChildRoot(parent, "Marked Survivor Build Yard");
        yard.position = BuildZoneCenter;
        Part(PrimitiveType.Cylinder, "Build yard compacted soil", yard, new Vector3(0f, 0.015f, 0f), new Vector3(18f, 0.03f, 16f),
            SurfaceMaterial("CompactedBuildSoil", new Color(0.34f, 0.28f, 0.21f), SurfaceKind.Mud));
        for (int i = 0; i < 18; i++)
        {
            float angle = i * Mathf.PI * 2f / 18f;
            Vector3 point = new Vector3(Mathf.Cos(angle) * 8.2f, 0.25f, Mathf.Sin(angle) * 7.2f);
            Transform markerStone = Part(PrimitiveType.Sphere, "Build yard border stone", yard, point,
                new Vector3(0.48f, 0.34f, 0.44f), stone);
            markerStone.rotation = Quaternion.Euler(0f, i * 19f, Random.Range(-9f, 9f));
        }
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 0.5f + 0.25f;
            Vector3 p = new Vector3(Mathf.Cos(angle) * 8.4f, 0.8f, Mathf.Sin(angle) * 7.4f);
            Transform stake = Part(PrimitiveType.Cylinder, "Build yard boundary stake", yard, p,
                new Vector3(0.09f, 0.8f, 0.09f), wood);
            stake.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 7f);
        }
        Block(yard, "Build yard sign board", new Vector3(0f, 1.25f, 8.8f), new Vector3(2.8f, 0.75f, 0.12f), wood);
        Block(yard, "Build yard sign post left", new Vector3(-1.1f, 0.55f, 8.8f), new Vector3(0.12f, 1.1f, 0.12f), wood);
        Block(yard, "Build yard sign post right", new Vector3(1.1f, 0.55f, 8.8f), new Vector3(0.12f, 1.1f, 0.12f), wood);
    }

    private static MeshFilter BuildPond(Transform parent, Vector3 position, Material water, Material stone)
    {
        Transform pond = ChildRoot(parent, "Forest Pond");
        pond.position = position;
        int rings = 8;
        int sides = 48;
        Vector3[] vertices = new Vector3[1 + rings * sides];
        int[] triangles = new int[(sides + (rings - 1) * sides * 2) * 3];
        vertices[0] = Vector3.zero;
        for (int ring = 1; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                float edge = 1f + 0.045f * Mathf.Sin(angle * 3f + 0.6f) + 0.028f * Mathf.Sin(angle * 7f - 0.2f);
                vertices[1 + (ring - 1) * sides + side] = new Vector3(Mathf.Cos(angle) * 7.6f * t * edge, 0f,
                    Mathf.Sin(angle) * 5.1f * t * edge);
            }
        }
        int ti = 0;
        for (int side = 0; side < sides; side++)
        {
            int next = (side + 1) % sides;
            triangles[ti++] = 0; triangles[ti++] = 1 + next; triangles[ti++] = 1 + side;
            for (int ring = 0; ring < rings - 1; ring++)
            {
                int a = 1 + ring * sides + side;
                int b = 1 + ring * sides + next;
                int c = a + sides;
                int d = b + sides;
                triangles[ti++] = a; triangles[ti++] = b; triangles[ti++] = c;
                triangles[ti++] = b; triangles[ti++] = d; triangles[ti++] = c;
            }
        }
        Mesh mesh = new Mesh { name = "Irregular pond surface" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Vector2[] uv = new Vector2[vertices.Length];
        uv[0] = new Vector2(0.5f, 0.5f);
        for (int ring = 1; ring <= rings; ring++)
        for (int side = 0; side < sides; side++)
            uv[1 + (ring - 1) * sides + side] = new Vector2(0.5f + Mathf.Cos(side * Mathf.PI * 2f / sides) * ring / (float)rings,
                0.5f + Mathf.Sin(side * Mathf.PI * 2f / sides) * ring / (float)rings);
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(15.6f, 0.12f, 10.6f));
        GameObject waterObject = new GameObject("Animated Pond Water", typeof(MeshFilter), typeof(MeshRenderer));
        waterObject.transform.SetParent(pond, false);
        waterObject.transform.localPosition = new Vector3(0f, 0.11f, 0f);
        MeshFilter filter = waterObject.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        waterObject.GetComponent<MeshRenderer>().sharedMaterial = water;
        for (int i = 0; i < 28; i++)
        {
            float angle = i * Mathf.PI * 2f / 28f;
            float wobble = 1f + 0.04f * Mathf.Sin(angle * 5f);
            Vector3 p = new Vector3(Mathf.Cos(angle) * 7.7f * wobble, 0.17f, Mathf.Sin(angle) * 5.2f * wobble);
            Transform bank = Part(PrimitiveType.Sphere, "Mossy pond bank rock", pond, p,
                new Vector3(Random.Range(0.35f, 0.85f), Random.Range(0.2f, 0.5f), Random.Range(0.35f, 0.8f)), stone);
            bank.rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, Random.Range(-12f, 12f));
        }
        return filter;
    }

    private static Transform[] AddFieldTrees(Transform parent)
    {
        Material[] bark =
        {
            SurfaceMaterial("FieldBarkDark", new Color(0.22f, 0.15f, 0.11f), SurfaceKind.Wood),
            SurfaceMaterial("FieldBarkGrey", new Color(0.31f, 0.29f, 0.24f), SurfaceKind.Wood)
        };
        Material[] leaves =
        {
            SurfaceMaterial("FieldCanopyDeep", new Color(0.12f, 0.25f, 0.18f), SurfaceKind.Foliage),
            SurfaceMaterial("FieldCanopyMoss", new Color(0.20f, 0.31f, 0.18f), SurfaceKind.Foliage),
            SurfaceMaterial("FieldCanopyCold", new Color(0.12f, 0.24f, 0.24f), SurfaceKind.Foliage)
        };
        Random.State old = Random.state;
        Random.InitState(6102026);
        List<Transform> wind = new List<Transform>();
        foreach (Transform existingCanopy in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (existingCanopy.name != "Soft Evergreen Canopy") continue;
            float size = Random.Range(0.82f, 1.20f);
            existingCanopy.localScale = Vector3.Scale(existingCanopy.localScale, new Vector3(size, Random.Range(0.88f, 1.26f), size));
            wind.Add(existingCanopy);
        }
        for (int i = 0; i < 44; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(43f, 66f);
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            if (NearZone(point, BuildZoneCenter, 14f) || NearZone(point, new Vector3(36f, 0f, -26f), 13f)
                || NearZone(point, new Vector3(-34f, 0f, 9f), 11f)
                || NearZone(point, new Vector3(33f, 0f, 3f), 10f)
                || NearZone(point, new Vector3(-35f, 0f, -12f), 10f)) continue;
            float height = Random.Range(0.72f, 1.62f);
            Transform tree = ChildRoot(parent, "Field Evergreen Variation");
            tree.position = point;
            float width = Random.Range(0.78f, 1.38f);
            tree.localScale = new Vector3(width, height, width * Random.Range(0.82f, 1.15f));
            Transform trunk = Part(PrimitiveType.Cylinder, "Field Tree Trunk", tree, new Vector3(0f, 2.1f, 0f),
                new Vector3(Random.Range(0.25f, 0.4f), 2.1f, Random.Range(0.25f, 0.4f)), bark[Random.Range(0, bark.Length)]);
            trunk.localRotation = Quaternion.Euler(Random.Range(-2f, 2f), 0f, Random.Range(-4f, 4f));
            Transform canopy = ChildRoot(tree, "Soft Evergreen Canopy");
            GameObject meshObject = new GameObject("Natural Canopy Form", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.transform.SetParent(canopy, false);
            meshObject.GetComponent<MeshFilter>().sharedMesh = BuildOrganicCanopyMesh(9100 + i * 73);
            meshObject.GetComponent<MeshRenderer>().sharedMaterial = leaves[Random.Range(0, leaves.Length)];
            wind.Add(canopy);
        }
        Random.state = old;
        return wind.ToArray();
    }

    private static void AddGroundedFieldDebris(Transform parent, Material wood, Material stone)
    {
        Random.State old = Random.state;
        Random.InitState(24102026);
        for (int i = 0; i < 72; i++)
        {
            Vector2 d = Random.insideUnitCircle;
            Vector3 point = new Vector3(d.x * 62f, 0f, d.y * 62f);
            if (NearZone(point, BuildZoneCenter, 12f) || NearZone(point, new Vector3(36f, 0f, -26f), 12f)) continue;
            if (i % 3 == 0)
            {
                float length = Random.Range(0.7f, 2.0f);
                Vector3 direction = new Vector3(Mathf.Cos(Random.value * Mathf.PI * 2f), 0f, Mathf.Sin(Random.value * Mathf.PI * 2f));
                Vector3 start = new Vector3(point.x, 0.10f, point.z) - direction * length * 0.5f;
                Vector3 end = new Vector3(point.x, 0.10f, point.z) + direction * length * 0.5f;
                Branch(parent, start, end, Random.Range(0.07f, 0.13f), wood);
                if (i % 2 == 0)
                    Branch(parent, Vector3.Lerp(start, end, 0.62f), Vector3.Lerp(start, end, 0.62f) + Vector3.Cross(direction, Vector3.up) * Random.Range(0.28f, 0.6f), 0.045f, wood);
            }
            else
            {
                Transform rock = Part(PrimitiveType.Sphere, "Textured Field Stone", parent,
                    new Vector3(point.x, Random.Range(0.17f, 0.28f), point.z),
                    new Vector3(Random.Range(0.35f, 0.9f), Random.Range(0.25f, 0.58f), Random.Range(0.32f, 0.82f)), stone);
                rock.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
            }
        }
        Random.state = old;
    }

    private static void GroundExistingPickupsAndStones(Material stone)
    {
        foreach (PrototypeResourcePickup pickup in Object.FindObjectsByType<PrototypeResourcePickup>(FindObjectsInactive.Include))
            if (pickup != null) pickup.transform.position = new Vector3(pickup.transform.position.x, 0.30f, pickup.transform.position.z);
        foreach (Transform item in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (item == null) continue;
            if (item.name == "Forest rubble" || item.name.Contains("roadside brick and debris") || item.name == "Textured Field Stone")
            {
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null) item.position += Vector3.up * Mathf.Max(0f, 0.035f - renderer.bounds.min.y);
                if (renderer != null) renderer.sharedMaterial = stone;
            }
            if (item.name.Contains("Stone Chunk") || item.name.Contains("Stone Chip"))
            {
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = stone;
            }
        }
    }

    private static void RefineNaturalForest(Scene scene)
    {
        GameObject marker = new GameObject(NaturalForestMarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);
        Material foliage = SurfaceMaterial("NaturalForestFoliage", new Color(0.22f, 0.39f, 0.27f), SurfaceKind.Foliage);
        foliage.color = Color.white;
        SetMatte(foliage);
        int variation = 0;
        foreach (Transform canopy in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (canopy.name != "Soft Evergreen Canopy") continue;
            Transform organic = canopy.Find("Natural Canopy Form");
            if (organic == null)
            {
                organic = new GameObject("Natural Canopy Form").transform;
                organic.SetParent(canopy, false);
            }
            for (int i = 0; i < canopy.childCount; i++)
            {
                Transform oldLobe = canopy.GetChild(i);
                if (oldLobe != organic) oldLobe.gameObject.SetActive(false);
            }
            MeshFilter filter = organic.GetComponent<MeshFilter>();
            if (filter == null) filter = organic.gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = organic.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = organic.gameObject.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildOrganicCanopyMesh(variation++ * 137 + 29);
            renderer.sharedMaterial = foliage;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static Mesh BuildOrganicCanopyMesh(int seed)
    {
        Random.State previous = Random.state;
        Random.InitState(seed);
        const int sides = 20;
        float[] heights = { 0.02f, 0.26f, 0.62f, 1.0f, 1.42f, 1.86f, 2.28f, 2.72f, 3.12f, 3.52f, 3.9f, 4.22f, 4.48f, 4.68f };
        float[] radii = { 0.26f, 0.62f, 1.16f, 1.48f, 1.37f, 1.24f, 1.18f, 1.02f, 0.94f, 0.77f, 0.69f, 0.49f, 0.32f, 0.22f };
        int ringCount = heights.Length;
        Vector3[] vertices = new Vector3[ringCount * sides + 2];
        int[] triangles = new int[(ringCount - 1) * sides * 6 + sides * 6];
        float phase = Random.Range(0f, Mathf.PI * 2f);
        float leanX = Random.Range(-0.18f, 0.18f);
        float leanZ = Random.Range(-0.18f, 0.18f);
        for (int ring = 0; ring < ringCount; ring++)
        {
            float t = ring / (float)(ringCount - 1);
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                float lobes = 1f + 0.105f * Mathf.Sin(angle * 3f + phase + ring * 0.17f)
                    + 0.055f * Mathf.Sin(angle * 5f - phase * 0.7f)
                    + Random.Range(-0.035f, 0.035f);
                float radius = radii[ring] * lobes;
                float centerX = leanX * t + 0.035f * Mathf.Sin(t * 8f + phase);
                float centerZ = leanZ * t + 0.035f * Mathf.Cos(t * 6f + phase);
                vertices[ring * sides + side] = new Vector3(centerX + Mathf.Cos(angle) * radius,
                    heights[ring], centerZ + Mathf.Sin(angle) * radius * 0.90f);
            }
        }
        int bottomCap = ringCount * sides;
        int topCap = bottomCap + 1;
        vertices[bottomCap] = new Vector3(0f, 0.02f, 0f);
        vertices[topCap] = new Vector3(leanX, 4.70f, leanZ);
        int index = 0;
        for (int ring = 0; ring < ringCount - 1; ring++)
        for (int side = 0; side < sides; side++)
        {
            int next = (side + 1) % sides;
            int a = ring * sides + side;
            int b = ring * sides + next;
            int c = (ring + 1) * sides + side;
            int d = (ring + 1) * sides + next;
            triangles[index++] = a; triangles[index++] = c; triangles[index++] = b;
            triangles[index++] = b; triangles[index++] = c; triangles[index++] = d;
        }
        for (int side = 0; side < sides; side++)
        {
            int next = (side + 1) % sides;
            triangles[index++] = bottomCap; triangles[index++] = next; triangles[index++] = side;
            int last = (ringCount - 1) * sides;
            triangles[index++] = topCap; triangles[index++] = last + side; triangles[index++] = last + next;
        }
        Mesh mesh = new Mesh { name = "Irregular Evergreen Canopy" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Vector2[] uv = new Vector2[vertices.Length];
        for (int ring = 0; ring < ringCount; ring++)
        for (int side = 0; side < sides; side++)
            uv[ring * sides + side] = new Vector2(side / (float)sides, heights[ring] / 4.70f);
        uv[bottomCap] = new Vector2(0.5f, 0f);
        uv[topCap] = new Vector2(0.5f, 1f);
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Random.state = previous;
        return mesh;
    }

    private static void AddDistrictAndCharacterPass(Scene scene)
    {
        GameObject dressing = new GameObject(DistrictMarkerName);
        SceneManager.MoveGameObjectToScene(dressing, scene);
        Transform root = dressing.transform;
        Material concrete = SurfaceMaterial("CrackedConcrete", new Color(0.58f, 0.59f, 0.55f), SurfaceKind.Concrete);
        Material plaster = SurfaceMaterial("FadedCementPlaster", new Color(0.49f, 0.50f, 0.46f), SurfaceKind.Plaster);
        Material roof = SurfaceMaterial("CharredRoof", new Color(0.46f, 0.43f, 0.38f), SurfaceKind.Roof);
        Material timber = SurfaceMaterial("AgedTimber", new Color(0.55f, 0.37f, 0.24f), SurfaceKind.Wood);
        Material rust = SurfaceMaterial("FlakingRust", new Color(0.62f, 0.29f, 0.17f), SurfaceKind.Rust);
        Material glass = Material(new Color(0.035f, 0.075f, 0.09f));

        BuildRuralHouse(root, "Abandoned Clinic", new Vector3(-22f, 0f, 43f), plaster, roof, timber, rust, glass, rust, false);
        BuildRuralHouse(root, "Collapsed Motor Workshop", new Vector3(23f, 0f, 48f), plaster, roof, timber, rust, glass, rust, true);
        BuildWaterTower(root, concrete, rust, timber);
        BuildRoadsideBarricade(root, concrete, timber, rust);
        SoftenTreeSilhouettes();
        RefineSurvivor();
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void BuildWaterTower(Transform parent, Material concrete, Material rust, Material timber)
    {
        Transform tower = ChildRoot(parent, "Rusted Water Tower");
        tower.position = new Vector3(0f, 0f, 58f);
        Part(PrimitiveType.Cylinder, "Elevated tank", tower, new Vector3(0f, 8.2f, 0f), new Vector3(3.1f, 2.2f, 3.1f), rust);
        Part(PrimitiveType.Sphere, "Rounded tank crown", tower, new Vector3(0f, 9.2f, 0f), new Vector3(3.08f, 1.05f, 3.08f), rust);
        Part(PrimitiveType.Cylinder, "Tank cap", tower, new Vector3(0f, 9.58f, 0f), new Vector3(1.25f, 0.18f, 1.25f), concrete);
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
            Part(PrimitiveType.Capsule, "Tower support", tower, new Vector3(x * 1.25f, 4.15f, z * 1.25f), new Vector3(0.22f, 4.2f, 0.22f), timber);
        Part(PrimitiveType.Cylinder, "Service catwalk", tower, new Vector3(0f, 6.95f, 0f), new Vector3(3.5f, 0.12f, 3.5f), rust);
        Part(PrimitiveType.Capsule, "Bent drain pipe", tower, new Vector3(1.35f, 3.2f, 1.35f), new Vector3(0.12f, 3.5f, 0.12f), rust);
        for (int step = 0; step < 8; step++)
            Block(tower, "Ladder rung", new Vector3(-1.3f, 0.8f + step * 0.72f, 1.45f), new Vector3(0.66f, 0.055f, 0.07f), concrete, new Vector3(0f, 0f, -4f));
    }

    private static void BuildRoadsideBarricade(Transform parent, Material concrete, Material timber, Material rust)
    {
        Transform block = ChildRoot(parent, "Evacuation Roadblock");
        block.position = new Vector3(0f, 0f, 21f);
        for (int i = 0; i < 5; i++)
        {
            float x = -5f + i * 2.5f;
            Part(PrimitiveType.Capsule, "Rounded concrete jersey barrier", block, new Vector3(x, 0.52f, 0f), new Vector3(0.92f, 0.57f, 0.72f), concrete);
            if (i % 2 == 0) Block(block, "Faded hazard stripe", new Vector3(x, 0.58f, 0.37f), new Vector3(0.45f, 0.5f, 0.04f), rust, new Vector3(0f, 0f, -18f));
        }
        Block(block, "Collapsed timber gate", new Vector3(0f, 1.2f, -0.42f), new Vector3(10.5f, 0.16f, 0.18f), timber, new Vector3(0f, 0f, -12f));
        Block(block, "Broken gate cross brace", new Vector3(1.2f, 1.05f, -0.39f), new Vector3(3.0f, 0.13f, 0.17f), timber, new Vector3(0f, 0f, 29f));
    }

    private static void SoftenTreeSilhouettes()
    {
        foreach (Transform candidate in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            Transform tree = candidate;
            bool generatedPine = candidate.name == "PineTree_Apocalyptic";
            bool resourcePineCanopy = candidate.name == "Tree Visuals" && candidate.Find("Pine Canopy Upgrade") != null;
            if (resourcePineCanopy) tree = candidate.Find("Pine Canopy Upgrade");
            else if (!generatedPine) continue;
            bool hasSoftCrown = tree.Find("Soft Evergreen Canopy") != null;
            if (hasSoftCrown) continue;
            Transform soft = ChildRoot(tree, "Soft Evergreen Canopy");
            Material needles = Material(new Color(0.12f, 0.22f, 0.19f));
            float scale = resourcePineCanopy ? 0.76f : 1f;
            Part(PrimitiveType.Sphere, "Rounded lower boughs", soft, new Vector3(0f, 1.75f * scale, 0f), new Vector3(3.0f, 1.45f, 2.75f) * scale, needles);
            Part(PrimitiveType.Sphere, "Rounded middle boughs", soft, new Vector3(0f, 2.75f * scale, 0f), new Vector3(2.45f, 1.45f, 2.3f) * scale, needles);
            Part(PrimitiveType.Sphere, "Rounded upper boughs", soft, new Vector3(0f, 3.65f * scale, 0f), new Vector3(1.85f, 1.4f, 1.8f) * scale, needles);
            Part(PrimitiveType.Sphere, "Soft tree crown", soft, new Vector3(0f, 4.35f * scale, 0f), new Vector3(1.35f, 1.35f, 1.3f) * scale, needles);
            for (int i = 0; i < tree.childCount; i++)
            {
                Transform child = tree.GetChild(i);
                if (child == soft) continue;
                if (child.name.Contains("Pine Bough") || child.name.Contains("Pine Crown") || child.name.Contains("Needle Tier") || child.name.Contains("Needle Crown")) child.gameObject.SetActive(false);
            }
        }
    }

    private static void RefineSurvivor()
    {
        GameObject player = GameObject.Find("Player_Prototype");
        Transform rig = player != null ? player.transform.Find("Player_Visual_Rig") : null;
        if (rig == null || rig.Find("Field Gear Details") != null) return;
        Transform details = ChildRoot(rig, "Field Gear Details");
        Material fabric = Material(new Color(0.20f, 0.22f, 0.20f));
        Material leather = Material(new Color(0.15f, 0.12f, 0.09f));
        Material canvas = SurfaceMaterial("SurvivorCanvas", new Color(0.43f, 0.38f, 0.27f), SurfaceKind.Wood);
        Part(PrimitiveType.Sphere, "Rounded left shoulder pad", details, new Vector3(-0.30f, 0.28f, 0.03f), new Vector3(0.26f, 0.20f, 0.25f), fabric);
        Part(PrimitiveType.Sphere, "Rounded right shoulder pad", details, new Vector3(0.30f, 0.28f, 0.03f), new Vector3(0.26f, 0.20f, 0.25f), fabric);
        Transform strap = Part(PrimitiveType.Capsule, "Cross body field strap", details, new Vector3(0f, 0.04f, 0.22f), new Vector3(0.10f, 0.46f, 0.075f), leather);
        strap.localRotation = Quaternion.Euler(0f, 0f, -24f);
        Part(PrimitiveType.Sphere, "Canvas bedroll", details, new Vector3(0f, 0.42f, -0.31f), new Vector3(0.42f, 0.17f, 0.20f), canvas);
        Part(PrimitiveType.Capsule, "Left knee pad", details, new Vector3(-0.16f, -0.52f, 0.15f), new Vector3(0.17f, 0.16f, 0.11f), fabric);
        Part(PrimitiveType.Capsule, "Right knee pad", details, new Vector3(0.16f, -0.52f, 0.15f), new Vector3(0.17f, 0.16f, 0.11f), fabric);
        Part(PrimitiveType.Capsule, "Left boot toe", details, new Vector3(-0.16f, -0.83f, 0.20f), new Vector3(0.24f, 0.12f, 0.17f), leather);
        Part(PrimitiveType.Capsule, "Right boot toe", details, new Vector3(0.16f, -0.83f, 0.20f), new Vector3(0.24f, 0.12f, 0.17f), leather);
        PrototypeCharacterAnimation animation = player.GetComponent<PrototypeCharacterAnimation>();
        if (animation != null) animation.SetBodyMotionTarget(rig);
    }

    private static void AddVillageDetails(Scene scene)
    {
        Random.State previousRandom = Random.state;
        Random.InitState(10102026);
        GameObject dressing = new GameObject(VillageMarkerName);
        SceneManager.MoveGameObjectToScene(dressing, scene);
        Transform root = dressing.transform;

        Material plaster = SurfaceMaterial("WeatheredPlaster", new Color(0.71f, 0.65f, 0.52f), SurfaceKind.Plaster);
        Material fadedPlaster = SurfaceMaterial("FadedCementPlaster", new Color(0.49f, 0.50f, 0.46f), SurfaceKind.Plaster);
        Material roof = SurfaceMaterial("CharredRoof", new Color(0.46f, 0.43f, 0.38f), SurfaceKind.Roof);
        Material tile = SurfaceMaterial("BrokenTerracotta", new Color(0.68f, 0.39f, 0.25f), SurfaceKind.Brick);
        Material timber = SurfaceMaterial("AgedTimber", new Color(0.55f, 0.37f, 0.24f), SurfaceKind.Wood);
        Material darkWood = SurfaceMaterial("SootBlackenedWood", new Color(0.36f, 0.27f, 0.20f), SurfaceKind.Wood);
        Material glass = Material(new Color(0.035f, 0.075f, 0.09f));
        Material rust = SurfaceMaterial("FlakingRust", new Color(0.62f, 0.29f, 0.17f), SurfaceKind.Rust);
        Material concrete = SurfaceMaterial("CrackedConcrete", new Color(0.58f, 0.59f, 0.55f), SurfaceKind.Concrete);

        BuildRuralHouse(root, "Ransacked Farmhouse", new Vector3(-10f, 0f, 34f), plaster, roof, timber, darkWood, glass, tile, true);
        BuildRuralHouse(root, "Abandoned Roadside Shop", new Vector3(10f, 0f, 39f), fadedPlaster, roof, timber, darkWood, glass, tile, false);
        BuildRuinedShell(root, "Collapsed Checkpoint", new Vector3(-1f, 0f, 50f), concrete, timber, darkWood);
        AddBrokenFence(root, new Vector3(-4f, 0f, 23f), 11, timber, darkWood, -1);
        AddBrokenFence(root, new Vector3(4f, 0f, 27f), 9, timber, darkWood, 1);
        AddRoadAndGround(root, concrete, darkWood, rust);
        AddStreetPropDetails(root, timber, darkWood, rust, concrete);
        Transform[] grass = AddTallGrass(root);
        AddAshFall(root);
        Light fire = GameObject.Find("Low campfire glow")?.GetComponent<Light>();
        Light streetlamp = GameObject.Find("Street lamp intermittent glow")?.GetComponent<Light>();
        PrototypeApocalypseMotion motion = dressing.AddComponent<PrototypeApocalypseMotion>();
        motion.Configure(grass, fire, streetlamp != null ? new[] { streetlamp } : new Light[0]);
        AddSurvivalToolsToPlayer();

        MarkEnvironmentStatic(root);
        Random.state = previousRandom;
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private enum SurfaceKind { Plaster, Roof, Brick, Wood, Rust, Concrete, Asphalt, Mud, Foliage, Stone, Water }

    private static Material SurfaceMaterial(string name, Color tint, SurfaceKind kind)
    {
        const string artFolder = "Assets/Art";
        const string surfaceFolder = "Assets/Art/Apocalypse";
        if (!AssetDatabase.IsValidFolder(artFolder)) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder(surfaceFolder)) AssetDatabase.CreateFolder(artFolder, "Apocalypse");

        string materialPath = surfaceFolder + "/M_" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material != null)
        {
            SetMatte(material);
            if (kind == SurfaceKind.Mud) ApplyMudRelief(material, surfaceFolder, name);
            EditorUtility.SetDirty(material);
            return material;
        }

        string texturePath = surfaceFolder + "/T_" + name + ".png";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            Texture2D generated = GenerateSurfaceTexture(name, tint, kind, 512);
            File.WriteAllBytes(Path.GetFullPath(texturePath), generated.EncodeToPNG());
            Object.DestroyImmediate(generated);
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer != null)
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.anisoLevel = 4;
                    importer.SaveAndReimport();
                }
            }
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) return Material(tint);
        material = new Material(shader) { name = "M_" + name, color = Color.white };
        if (texture != null) material.mainTexture = texture;
        float tiling = kind == SurfaceKind.Asphalt || kind == SurfaceKind.Mud || kind == SurfaceKind.Stone || kind == SurfaceKind.Water
            ? 3.2f : kind == SurfaceKind.Wood ? 1.5f : 2.1f;
        material.mainTextureScale = new Vector2(tiling, tiling);
        if (kind == SurfaceKind.Mud) ApplyMudRelief(material, surfaceFolder, name);
        SetMatte(material);
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void ApplyMudRelief(Material material, string folder, string name)
    {
        if (material == null || !material.HasProperty("_BumpMap")) return;
        string path = folder + "/T_" + name + "_Relief.png";
        Texture2D relief = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (relief == null)
        {
            const int size = 256;
            Texture2D generated = new Texture2D(size, size, TextureFormat.RGB24, false, true)
            { name = "T_" + name + "_Relief", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                float v = y / (float)size;
                float hL = Mathf.PerlinNoise((u - 1f / size) * 18f, (v + 0.13f) * 18f);
                float hR = Mathf.PerlinNoise((u + 1f / size) * 18f, (v + 0.13f) * 18f);
                float hD = Mathf.PerlinNoise((u + 0.13f) * 18f, (v - 1f / size) * 18f);
                float hU = Mathf.PerlinNoise((u + 0.13f) * 18f, (v + 1f / size) * 18f);
                Vector3 normal = new Vector3((hL - hR) * 3.5f, (hD - hU) * 3.5f, 1f).normalized;
                pixels[y * size + x] = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);
            }
            generated.SetPixels(pixels);
            generated.Apply(false, false);
            File.WriteAllBytes(Path.GetFullPath(path), generated.EncodeToPNG());
            Object.DestroyImmediate(generated);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            relief = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        material.SetTexture("_BumpMap", relief);
        material.SetFloat("_BumpScale", 0.55f);
        material.EnableKeyword("_NORMALMAP");
    }

    private static Texture2D GenerateSurfaceTexture(string name, Color tint, SurfaceKind kind, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            name = "T_" + name,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 4
        };
        Color[] pixels = new Color[size * size];
        int stableHash = 17;
        foreach (char character in name) stableHash = stableHash * 31 + character;
        float seed = Mathf.Abs(stableHash % 10000) * 0.0137f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = x / (float)size;
            float v = y / (float)size;
            float broad = Mathf.PerlinNoise(u * 5.5f + seed, v * 5.5f + seed * 0.7f);
            float fine = Mathf.PerlinNoise(u * 42f + seed * 2f, v * 42f + seed * 1.3f);
            float grain = Mathf.PerlinNoise(u * 4f + seed * 0.5f, v * 68f + seed);
            float shade = 0.78f + broad * 0.28f + fine * 0.10f;

            if (kind == SurfaceKind.Brick || kind == SurfaceKind.Roof)
            {
                int rows = kind == SurfaceKind.Brick ? 13 : 16;
                int columns = kind == SurfaceKind.Brick ? 9 : 12;
                float row = v * rows;
                float offset = ((int)row % 2) * 0.5f;
                float cellX = Mathf.Repeat(u * columns + offset, 1f);
                float cellY = Mathf.Repeat(row, 1f);
                bool mortar = cellX < (kind == SurfaceKind.Brick ? 0.035f : 0.022f)
                    || cellY < (kind == SurfaceKind.Brick ? 0.055f : 0.04f);
                shade = mortar ? 0.47f + fine * 0.18f : 0.72f + broad * 0.42f + fine * 0.12f;
            }
            else if (kind == SurfaceKind.Wood)
                shade = 0.62f + grain * 0.53f + Mathf.Sin((u * 18f + broad * 0.16f) * Mathf.PI * 2f) * 0.07f;
            else if (kind == SurfaceKind.Rust)
                shade = 0.45f + broad * 0.88f + (fine > 0.72f ? 0.20f : 0f);
            else if (kind == SurfaceKind.Concrete || kind == SurfaceKind.Plaster)
                shade = 0.67f + broad * 0.42f + fine * 0.15f - Mathf.Pow(Mathf.PerlinNoise(u * 8f + seed, v * 2.1f + seed), 5f) * 0.20f;
            else if (kind == SurfaceKind.Asphalt)
                shade = 0.50f + broad * 0.40f + fine * 0.12f - (fine > 0.79f ? 0.12f : 0f);
            else if (kind == SurfaceKind.Mud)
                shade = 0.58f + broad * 0.43f + fine * 0.15f;
            else if (kind == SurfaceKind.Foliage)
                shade = 0.48f + broad * 0.58f + fine * 0.16f + (fine > 0.73f ? 0.16f : 0f);
            else if (kind == SurfaceKind.Stone)
            {
                float fleck = Mathf.PerlinNoise(u * 92f + seed * 2.3f, v * 92f + seed * 0.9f);
                float strata = Mathf.PerlinNoise(u * 16f + seed * 0.2f, v * 9f + seed);
                shade = 0.42f + broad * 0.42f + fleck * 0.24f + strata * 0.20f;
                if (fleck > 0.84f) shade += 0.16f;
                if (fleck < 0.17f) shade -= 0.14f;
            }
            else if (kind == SurfaceKind.Water)
            {
                float ripple = Mathf.Sin((u * 5.4f + v * 3.2f + broad * 0.16f) * Mathf.PI * 2f);
                shade = 0.67f + broad * 0.22f + fine * 0.08f + ripple * 0.035f;
            }

            pixels[y * size + x] = new Color(tint.r * shade, tint.g * shade, tint.b * shade, 1f);
        }
        texture.SetPixels(pixels);
        texture.Apply(true, false);
        return texture;
    }

    private static void BuildRuralHouse(Transform parent, string name, Vector3 position, Material plaster,
        Material roof, Material timber, Material darkWood, Material glass, Material tiles, bool farmhouse)
    {
        Transform house = ChildRoot(parent, name);
        house.position = position;
        float width = farmhouse ? 8.5f : 9.2f;
        float depth = farmhouse ? 7.0f : 8.0f;
        float halfW = width * 0.5f;
        float halfD = depth * 0.5f;
        Material brick = Material(new Color(0.31f, 0.16f, 0.11f));
        Material board = Material(new Color(0.20f, 0.16f, 0.12f));

        Block(house, "Stone foundation", new Vector3(0f, 0.34f, 0f), new Vector3(width + 0.25f, 0.68f, depth + 0.25f), darkWood);
        Block(house, "Rear plaster wall", new Vector3(0f, 2.1f, halfD), new Vector3(width, 3.55f, 0.28f), plaster);
        Block(house, "Left side wall", new Vector3(-halfW, 2.1f, 0f), new Vector3(0.28f, 3.55f, depth), plaster);
        Block(house, "Right side wall", new Vector3(halfW, 2.1f, 0f), new Vector3(0.28f, 3.55f, depth), plaster);

        // Front wall is built around an open doorway and two dark, boarded windows.
        float frontZ = -halfD;
        Block(house, "Front left pier", new Vector3(-3.65f, 2.05f, frontZ), new Vector3(1.0f, 3.45f, 0.28f), plaster);
        Block(house, "Front right pier", new Vector3(3.65f, 2.05f, frontZ), new Vector3(1.0f, 3.45f, 0.28f), plaster);
        Block(house, "Front door left", new Vector3(-1.22f, 1.67f, frontZ), new Vector3(2.0f, 2.7f, 0.28f), plaster);
        Block(house, "Front door right", new Vector3(1.18f, 1.67f, frontZ), new Vector3(1.92f, 2.7f, 0.28f), plaster);
        Block(house, "Front lintel", new Vector3(0f, 3.48f, frontZ), new Vector3(2.45f, 0.45f, 0.30f), timber);
        Block(house, "Open doorway shadow", new Vector3(0f, 1.31f, frontZ - 0.045f), new Vector3(1.65f, 2.55f, 0.06f), darkWood);
        Block(house, "Hanging broken door", new Vector3(-0.54f, 1.27f, frontZ - 0.14f), new Vector3(0.52f, 2.45f, 0.12f), board, new Vector3(0f, 0f, -5f));

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 2.65f;
            Block(house, "Window sill", new Vector3(x, 1.15f, frontZ - 0.19f), new Vector3(1.65f, 0.16f, 0.15f), timber);
            Block(house, "Window lintel", new Vector3(x, 2.55f, frontZ - 0.19f), new Vector3(1.65f, 0.16f, 0.15f), timber);
            Block(house, "Window jamb L", new Vector3(x - 0.73f, 1.85f, frontZ - 0.18f), new Vector3(0.15f, 1.25f, 0.16f), darkWood);
            Block(house, "Window jamb R", new Vector3(x + 0.73f, 1.85f, frontZ - 0.18f), new Vector3(0.15f, 1.25f, 0.16f), darkWood);
            Block(house, "Blackened window glass", new Vector3(x, 1.84f, frontZ - 0.11f), new Vector3(1.24f, 1.18f, 0.035f), glass);
            Block(house, "Window crossbar", new Vector3(x, 1.84f, frontZ - 0.25f), new Vector3(0.08f, 1.2f, 0.09f), timber);
            Block(house, "Boarded window plank", new Vector3(x - 0.14f, 1.62f, frontZ - 0.32f), new Vector3(1.35f, 0.16f, 0.07f), board, new Vector3(0f, 0f, -12f * side));
        }

        // Timber framing, exposed brick and broken plaster make the silhouette read as a lived-in settlement.
        for (int i = 0; i < 10; i++)
        {
            float x = -3.75f + i * 0.83f;
            Block(house, "Exposed foundation brick", new Vector3(x, 0.78f, frontZ - 0.17f),
                new Vector3(0.72f, 0.22f, 0.09f), i % 2 == 0 ? brick : tiles, new Vector3(0f, 0f, (i % 3 - 1) * 4f));
        }
        Block(house, "Front corner timber left", new Vector3(-4.08f, 2.05f, frontZ - 0.20f), new Vector3(0.18f, 3.7f, 0.16f), timber);
        Block(house, "Front corner timber right", new Vector3(4.08f, 2.05f, frontZ - 0.20f), new Vector3(0.18f, 3.7f, 0.16f), timber);
        Block(house, "Broken plaster patch", new Vector3(-3.25f, 1.13f, frontZ - 0.19f), new Vector3(0.82f, 0.72f, 0.035f), Material(new Color(0.36f, 0.31f, 0.24f)), new Vector3(0f, 0f, -12f));
        Block(house, "Broken plaster patch 2", new Vector3(3.05f, 2.91f, frontZ - 0.18f), new Vector3(1.0f, 0.40f, 0.035f), Material(new Color(0.35f, 0.33f, 0.29f)), new Vector3(0f, 0f, 9f));
        Block(house, "Door canopy", new Vector3(0.2f, 2.78f, frontZ - 0.56f), new Vector3(2.7f, 0.16f, 0.95f), roof, new Vector3(-9f, 0f, 1f));
        Block(house, "Entry post", new Vector3(-1.15f, 1.4f, frontZ - 0.92f), new Vector3(0.13f, 2.7f, 0.13f), timber, new Vector3(0f, 0f, -7f));
        Block(house, "Entry post 2", new Vector3(1.48f, 1.4f, frontZ - 0.92f), new Vector3(0.13f, 2.7f, 0.13f), timber);

        Transform roofLeft = Part(PrimitiveType.Cube, "Sagging roof left", house, new Vector3(-2.08f, 4.35f, 0f), new Vector3(4.7f, 0.30f, depth + 1.5f), roof);
        roofLeft.localRotation = Quaternion.Euler(0f, 0f, 27f);
        Transform roofRight = Part(PrimitiveType.Cube, "Sagging roof right", house, new Vector3(2.08f, 4.35f, 0f), new Vector3(4.7f, 0.30f, depth + 1.5f), roof);
        roofRight.localRotation = Quaternion.Euler(0f, 0f, -25f);
        Block(house, "Ridge timber", new Vector3(0f, 5.38f, 0f), new Vector3(0.32f, 0.32f, depth + 1.0f), timber);
        for (int side = -1; side <= 1; side += 2)
        for (int z = -3; z <= 3; z++)
        {
            Transform rafter = Part(PrimitiveType.Cylinder, "Exposed roof rafter", house,
                new Vector3(side * 2.2f, 4.34f, z * 1.15f), new Vector3(0.09f, 2.65f, 0.09f), timber);
            rafter.localRotation = Quaternion.Euler(0f, 0f, side * 27f);
        }
        // Individual broken tiles break up the clean roof silhouette.
        for (int i = 0; i < 13; i++)
        {
            float x = -3.7f + i * 0.62f;
            float z = (i % 2 == 0 ? -1f : 1f) * (depth * 0.5f + 0.56f);
            Transform tilePiece = Part(PrimitiveType.Cube, "Loose roof tile", house,
                new Vector3(x, 4.65f + Mathf.Sin(i * 1.7f) * 0.22f, z), new Vector3(0.72f, 0.07f, 0.42f), i % 3 == 0 ? roof : tiles);
            tilePiece.localRotation = Quaternion.Euler(Random.Range(-7f, 7f), Random.Range(-5f, 5f), Random.Range(-9f, 9f));
        }

        Block(house, "Chimney stone stack", new Vector3(-2.85f, 5.0f, 1.0f), new Vector3(0.75f, 2.0f, 0.85f), concreteMaterial());
        Block(house, "Chimney lip", new Vector3(-2.85f, 6.0f, 1.0f), new Vector3(1.0f, 0.17f, 1.05f), darkWood);
        AddBoardedSideWindow(house, -halfW - 0.15f, -0.1f, glass, timber, board);
        AddPointLight(name + " weak window glow", house.position + new Vector3(0f, 1.7f, frontZ - 1.0f), new Color(1f, 0.30f, 0.12f), 0.25f, 5f, house);

        if (farmhouse)
        {
            Block(house, "Farm sign board", new Vector3(3.1f, 3.25f, frontZ - 0.31f), new Vector3(2.0f, 0.48f, 0.14f), darkWood, new Vector3(0f, 0f, 2f));
            Block(house, "Sign hanging chain", new Vector3(2.36f, 3.58f, frontZ - 0.31f), new Vector3(0.06f, 0.42f, 0.06f), timber);
            Block(house, "Sign hanging chain 2", new Vector3(3.85f, 3.58f, frontZ - 0.31f), new Vector3(0.06f, 0.42f, 0.06f), timber);
        }
        else
        {
            for (int i = 0; i < 4; i++)
                Block(house, "Shopfront boarded display", new Vector3(-3.1f + i * 0.47f, 1.65f, frontZ - 0.28f), new Vector3(0.14f, 2.0f, 0.07f), i % 2 == 0 ? board : timber, new Vector3(0f, 0f, Random.Range(-4f, 4f)));
        }
        AddChimneySmoke(house, new Vector3(-2.85f, 6.05f, 1f));
    }

    private static Material concreteMaterial() => Material(new Color(0.30f, 0.30f, 0.27f));

    private static void AddBoardedSideWindow(Transform parent, float x, float z, Material glass, Material timber, Material board)
    {
        Block(parent, "Side window dark opening", new Vector3(x, 2.0f, z), new Vector3(0.04f, 1.25f, 1.55f), glass);
        for (int side = -1; side <= 1; side += 2)
            Block(parent, "Side window board", new Vector3(x - 0.12f, 1.78f, z + side * 0.34f), new Vector3(0.07f, 0.16f, 0.88f), board, new Vector3(0f, 0f, side * 6f));
        Block(parent, "Side window upper frame", new Vector3(x - 0.13f, 2.67f, z), new Vector3(0.10f, 0.13f, 1.75f), timber);
        Block(parent, "Side window lower frame", new Vector3(x - 0.13f, 1.32f, z), new Vector3(0.10f, 0.13f, 1.75f), timber);
    }

    private static void BuildRuinedShell(Transform parent, string name, Vector3 position, Material concrete, Material timber, Material dark)
    {
        Transform ruin = ChildRoot(parent, name);
        ruin.position = position;
        Block(ruin, "Checkpoint foundation", new Vector3(0f, 0.27f, 0f), new Vector3(10f, 0.54f, 6.5f), dark);
        Block(ruin, "Back fractured wall", new Vector3(0f, 1.7f, 3.0f), new Vector3(9.7f, 2.9f, 0.34f), concrete);
        Block(ruin, "Left fractured wall", new Vector3(-4.7f, 1.15f, 0f), new Vector3(0.34f, 1.9f, 5.8f), concrete, new Vector3(0f, 0f, -4f));
        Block(ruin, "Right wall stump", new Vector3(4.7f, 0.8f, 1.8f), new Vector3(0.34f, 1.3f, 2.5f), concrete, new Vector3(0f, 0f, 6f));
        Block(ruin, "Fallen roof slab", new Vector3(0.8f, 1.75f, 0.7f), new Vector3(6.7f, 0.28f, 2.6f), concrete, new Vector3(12f, 0f, 15f));
        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-4.2f, 4.2f);
            float z = Random.Range(-2.8f, 2.8f);
            Transform rubble = Part(i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere, "Checkpoint rubble", ruin,
                new Vector3(x, 0.35f + Random.value * 0.35f, z), new Vector3(Random.Range(0.35f, 1.1f), Random.Range(0.25f, 0.9f), Random.Range(0.3f, 0.85f)), i % 3 == 0 ? timber : concrete);
            rubble.localRotation = Random.rotation;
        }
        Block(ruin, "Bent barricade beam", new Vector3(-3.8f, 1.1f, -3.2f), new Vector3(0.22f, 2.2f, 0.22f), timber, new Vector3(0f, 0f, -28f));
        Block(ruin, "Checkpoint warning panel", new Vector3(3.7f, 1.25f, -3.25f), new Vector3(1.8f, 0.8f, 0.16f), Material(new Color(0.32f, 0.13f, 0.075f)), new Vector3(0f, 0f, 6f));
    }

    private static void AddBrokenFence(Transform parent, Vector3 start, int count, Material wood, Material dark, int direction)
    {
        Transform fence = ChildRoot(parent, "Collapsed roadside fence");
        fence.position = start;
        for (int i = 0; i < count; i++)
        {
            float x = direction * i * 1.0f;
            float lean = (i % 4 == 0) ? direction * 18f : Random.Range(-4f, 4f);
            Block(fence, "Weathered fence post", new Vector3(x, 0.72f, 0f), new Vector3(0.16f, 1.45f, 0.18f), i % 3 == 0 ? dark : wood, new Vector3(0f, 0f, lean));
            if (i < count - 1 && i % 3 != 1)
            {
                Block(fence, "Broken horizontal rail", new Vector3(x + direction * 0.48f, 0.82f, 0f), new Vector3(1.05f, 0.13f, 0.14f), wood, new Vector3(0f, 0f, i % 2 == 0 ? 3f : -7f));
                if (i % 2 == 0) Block(fence, "Upper fence rail", new Vector3(x + direction * 0.48f, 1.25f, 0f), new Vector3(1.02f, 0.11f, 0.14f), dark, new Vector3(0f, 0f, -2f));
            }
        }
    }

    private static void AddRoadAndGround(Transform parent, Material concrete, Material dark, Material rust)
    {
        Transform road = ChildRoot(parent, "Cracked village road");
        road.position = Vector3.zero;
        Material asphalt = SurfaceMaterial("CrackedAsphalt", new Color(0.56f, 0.58f, 0.60f), SurfaceKind.Asphalt);
        Material mud = SurfaceMaterial("WetRoadMud", new Color(0.48f, 0.39f, 0.29f), SurfaceKind.Mud);
        Material crack = Material(new Color(0.055f, 0.06f, 0.065f));
        for (int i = 0; i < 15; i++)
        {
            float z = 8f + i * 3.0f;
            float drift = Mathf.Sin(i * 0.54f) * 1.25f;
            Block(road, "Broken asphalt slab", new Vector3(drift, 0.025f, z), new Vector3(4.2f + (i % 3) * 0.35f, 0.06f, 3.3f), i % 4 == 0 ? mud : asphalt, new Vector3(0f, Mathf.Sin(i) * 2f, 0f));
            if (i % 2 == 0)
                Block(road, "Road fracture", new Vector3(drift + 0.8f, 0.062f, z + 0.8f), new Vector3(1.4f, 0.015f, 0.06f), crack, new Vector3(0f, 0f, Random.Range(-35f, 35f)));
        }
        for (int i = 0; i < 22; i++)
        {
            float x = Random.Range(-7f, 7f);
            float z = Random.Range(14f, 46f);
            Transform debris = Part(i % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere, "Roadside brick and debris", parent,
                new Vector3(x, 0.13f, z), new Vector3(Random.Range(0.18f, 0.8f), Random.Range(0.14f, 0.42f), Random.Range(0.2f, 0.8f)), i % 4 == 0 ? rust : concrete);
            debris.localRotation = Random.rotation;
        }
        // Dirt shoulders and a few broken curb stones frame the new path into the settlement.
        for (int side = -1; side <= 1; side += 2)
        for (int i = 0; i < 12; i++)
        {
            float z = 12f + i * 2.7f;
            Block(road, "Broken roadside curb", new Vector3(side * (3.15f + Random.Range(-0.3f, 0.3f)), 0.11f, z),
                new Vector3(0.28f, 0.22f, Random.Range(1.1f, 2.1f)), dark, new Vector3(0f, Random.Range(-8f, 8f), Random.Range(-5f, 5f)));
        }
    }

    private static void AddStreetPropDetails(Transform parent, Material timber, Material dark, Material rust, Material concrete)
    {
        Transform pole = ChildRoot(parent, "Leaning power pole");
        pole.position = new Vector3(6.6f, 0f, 28f);
        Transform post = Part(PrimitiveType.Cylinder, "Splintered utility pole", pole, new Vector3(0f, 4.4f, 0f), new Vector3(0.23f, 4.5f, 0.23f), timber);
        post.localRotation = Quaternion.Euler(0f, 0f, -7f);
        Block(pole, "Power crossarm", new Vector3(0f, 8.45f, 0f), new Vector3(2.7f, 0.16f, 0.18f), dark);
        for (int i = -1; i <= 1; i++) Block(pole, "Power insulator", new Vector3(i * 0.9f, 8.72f, 0f), new Vector3(0.18f, 0.48f, 0.18f), concrete);
        Transform streetLamp = ChildRoot(parent, "Flickering street lamp");
        streetLamp.position = new Vector3(-5.7f, 0f, 24f);
        Transform lampPost = Part(PrimitiveType.Cylinder, "Rusty streetlamp", streetLamp, new Vector3(0f, 3.4f, 0f), new Vector3(0.13f, 3.5f, 0.13f), dark);
        lampPost.localRotation = Quaternion.Euler(0f, 0f, 2f);
        Block(streetLamp, "Lamp arm", new Vector3(0.65f, 6.8f, 0f), new Vector3(1.4f, 0.1f, 0.1f), dark, new Vector3(0f, 0f, -12f));
        Transform bulb = Part(PrimitiveType.Sphere, "Dying amber bulb", streetLamp, new Vector3(1.2f, 6.65f, 0f), Vector3.one * 0.24f, EmissiveMaterial(new Color(1f, 0.32f, 0.08f)));
        bulb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        AddPointLight("Street lamp intermittent glow", streetLamp.position + new Vector3(1.2f, 6.4f, 0f), new Color(1f, 0.30f, 0.08f), 1.0f, 10f, streetLamp);
        Block(parent, "Abandoned road sign post", new Vector3(4.8f, 1.6f, 18f), new Vector3(0.13f, 3.2f, 0.13f), rust, new Vector3(0f, 0f, 4f));
        Block(parent, "Bent road sign", new Vector3(4.8f, 2.85f, 18f), new Vector3(1.55f, 0.82f, 0.14f), Material(new Color(0.34f, 0.23f, 0.14f)), new Vector3(0f, 0f, 8f));
        Block(parent, "Buried road barrier", new Vector3(-3.8f, 0.62f, 16f), new Vector3(3.4f, 0.2f, 0.18f), rust, new Vector3(0f, 0f, -4f));
        for (int i = 0; i < 8; i++)
            Block(parent, "Barrier stripe", new Vector3(-5.1f + i * 0.38f, 0.64f, 15.88f), new Vector3(0.16f, 0.21f, 0.04f), i % 2 == 0 ? concrete : dark, new Vector3(0f, 0f, -4f));
    }

    private static Transform[] AddTallGrass(Transform parent)
    {
        if (grassMesh == null) grassMesh = BuildGrassMesh();
        Material grassMaterial = Material(new Color(0.27f, 0.29f, 0.17f));
        List<Transform> animated = new List<Transform>();
        Random.State previous = Random.state;
        Random.InitState(6102026);
        for (int i = 0; i < 92; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float radius = Random.Range(10f, 39f);
            Vector3 position = new Vector3(direction.x * radius, 0.025f, direction.y * radius);
            if (NearZone(position, new Vector3(-10f, 0f, 34f), 6f) || NearZone(position, new Vector3(10f, 0f, 39f), 6f)
                || Mathf.Abs(position.x) < 2.8f && position.z > 12f && position.z < 50f) continue;
            GameObject clump = new GameObject("Animated dry grass");
            clump.transform.SetParent(parent, false);
            clump.transform.position = position;
            clump.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            clump.transform.localScale = Vector3.one * Random.Range(0.75f, 1.65f);
            clump.AddComponent<MeshFilter>().sharedMesh = grassMesh;
            clump.AddComponent<MeshRenderer>().sharedMaterial = grassMaterial;
            if (animated.Count < 38) animated.Add(clump.transform);
        }
        Random.state = previous;
        return animated.ToArray();
    }

    private static void AddSurvivalToolsToPlayer()
    {
        GameObject player = GameObject.Find("Player_Prototype");
        Transform rig = player != null ? player.transform.Find("Player_Visual_Rig") : null;
        if (rig == null || rig.Find("Backpack Axe") != null) return;
        Material handle = Material(new Color(0.20f, 0.13f, 0.08f));
        Material steel = Material(new Color(0.48f, 0.50f, 0.47f));
        Transform axe = ChildRoot(rig, "Backpack Axe");
        axe.localPosition = new Vector3(-0.02f, 0.16f, -0.405f);
        Transform haft = Part(PrimitiveType.Cylinder, "Worn axe haft", axe, Vector3.zero, new Vector3(0.075f, 0.68f, 0.075f), handle);
        haft.localRotation = Quaternion.Euler(0f, 0f, 7f);
        Block(axe, "Axe steel head", new Vector3(-0.10f, 0.37f, 0f), new Vector3(0.34f, 0.20f, 0.12f), steel, new Vector3(0f, 0f, -8f));
        Block(axe, "Axe blade edge", new Vector3(-0.25f, 0.37f, 0f), new Vector3(0.08f, 0.25f, 0.12f), steel, new Vector3(0f, 0f, -19f));
        Block(axe, "Axe leather wrap", new Vector3(0f, -0.22f, 0.045f), new Vector3(0.13f, 0.11f, 0.12f), Material(new Color(0.32f, 0.18f, 0.10f)), new Vector3(0f, 0f, 7f));
    }

    private static Mesh BuildGrassMesh()
    {
        const int bladeCount = 7;
        Vector3[] vertices = new Vector3[bladeCount * 4];
        int[] triangles = new int[bladeCount * 12];
        Random.State previous = Random.state;
        Random.InitState(4901);
        for (int i = 0; i < bladeCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(0f, 0.24f);
            float height = Random.Range(0.5f, 1.15f);
            float width = Random.Range(0.035f, 0.09f);
            Vector3 center = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Vector3 side = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * width;
            Vector3 lean = new Vector3(Mathf.Cos(angle + 1.2f), 0f, Mathf.Sin(angle + 1.2f)) * (height * 0.24f);
            int v = i * 4;
            vertices[v] = center - side;
            vertices[v + 1] = center + side;
            vertices[v + 2] = center + lean + side * 0.12f + Vector3.up * height;
            vertices[v + 3] = center + lean - side * 0.12f + Vector3.up * height;
            int t = i * 12;
            triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
            triangles[t + 3] = v; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            triangles[t + 6] = v + 1; triangles[t + 7] = v + 2; triangles[t + 8] = v;
            triangles[t + 9] = v + 2; triangles[t + 10] = v + 3; triangles[t + 11] = v;
        }
        Random.state = previous;
        Mesh mesh = new Mesh { name = "Wind-swept grass clump" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static ParticleSystem AddAshFall(Transform parent)
    {
        GameObject ashObject = new GameObject("Drifting ash and spores");
        ashObject.transform.SetParent(parent, false);
        ashObject.transform.position = new Vector3(0f, 4f, 16f);
        ParticleSystem system = ashObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.duration = 10f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.09f);
        main.startColor = new Color(0.55f, 0.59f, 0.61f, 0.42f);
        main.maxParticles = 240;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 13f;
        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(56f, 0.3f, 62f);
        ParticleSystem.VelocityOverLifetimeModule drift = system.velocityOverLifetime;
        drift.enabled = true;
        drift.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
        drift.y = new ParticleSystem.MinMaxCurve(-0.28f, -0.08f);
        drift.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        ParticleSystemRenderer renderer = ashObject.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) renderer.sharedMaterial = new Material(shader) { color = new Color(0.58f, 0.61f, 0.64f, 0.45f) };
        return system;
    }

    private static void AddChimneySmoke(Transform parent, Vector3 localPosition)
    {
        GameObject smokeObject = new GameObject("Slow chimney smoke");
        smokeObject.transform.SetParent(parent, false);
        smokeObject.transform.localPosition = localPosition;
        ParticleSystem system = smokeObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.42f);
        main.startColor = new Color(0.32f, 0.36f, 0.39f, 0.23f);
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 1.6f;
        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 8f;
        shape.radius = 0.16f;
        ParticleSystemRenderer renderer = smokeObject.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) renderer.sharedMaterial = new Material(shader) { color = new Color(0.38f, 0.41f, 0.43f, 0.24f) };
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

    private static void SetMatte(Material material)
    {
        if (material == null) return;
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.08f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
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
