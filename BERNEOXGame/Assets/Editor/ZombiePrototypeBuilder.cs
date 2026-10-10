using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ZombiePrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/ZombiePrototype.unity";

    [MenuItem("BERNEOX/Crear escena de prototipo Zombie Survival")]
    public static void CreateScene()
    {
        if (File.Exists(Path.Combine(Application.dataPath, "Scenes", "ZombiePrototype.unity")))
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorUtility.DisplayDialog("Prototipo listo", "Se abrió la escena ZombiePrototype existente sin modificarla.", "Aceptar");
            return;
        }
        if (!EditorUtility.DisplayDialog("Crear prototipo", "Se creará la escena ZombiePrototype con bosque, recursos recolectables, árboles talables y rondas de zombis. SampleScene y el inicio BERNEOX se conservarán.", "Crear", "Cancelar")) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ZombiePrototype";
        RenderSettings.ambientLight = new Color(0.38f, 0.40f, 0.43f);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Prototype";
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("GroundMaterial", new Color(0.24f, 0.29f, 0.25f));

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player_Prototype";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.GetComponent<Renderer>().sharedMaterial = CreateMaterial("PlayerMaterial", new Color(0.16f, 0.48f, 0.72f));
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController playerController = player.AddComponent<CharacterController>();
        playerController.height = 2f;
        playerController.radius = 0.42f;
        playerController.center = Vector3.zero;
        player.AddComponent<PrototypePlayerController>();
        PrototypePlayerHealth playerHealth = player.AddComponent<PrototypePlayerHealth>();
        player.AddComponent<PrototypeMeleeAttack>();
        PrototypeResourceInventory playerInventory = player.AddComponent<PrototypeResourceInventory>();
        playerInventory.AddStarterKnife();
        player.AddComponent<PrototypePickupInteractor>();

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.48f, 0.62f, 0.70f);
        cameraObject.AddComponent<AudioListener>();
        var follow = cameraObject.AddComponent<PrototypeFollowCamera>();
        follow.SetTarget(player.transform);

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        new GameObject("Touch Joystick").AddComponent<PrototypeJoystick>();
        new GameObject("Combat HUD").AddComponent<PrototypeCombatHud>();
        PrototypeHordeDirector hordeDirector = new GameObject("Horde Director").AddComponent<PrototypeHordeDirector>();
        PrototypeShelterBuilder shelterBuilder = new GameObject("Shelter Builder").AddComponent<PrototypeShelterBuilder>();
        Material zombieMaterial = CreateMaterial("ZombieMaterial", new Color(0.38f, 0.49f, 0.39f));
        Material zombieDetail = CreateMaterial("ZombieDetail", new Color(0.22f, 0.31f, 0.27f));
        Material zombieEyes = CreateMaterial("ZombieEyes", new Color(0.95f, 0.28f, 0.38f));
        GameObject zombieTemplate = CreateZombie(0, new Vector3(0f, 1f, 12f), playerHealth, zombieMaterial, zombieDetail, zombieEyes);
        zombieTemplate.name = "Zombie_Wave_Template";
        zombieTemplate.SetActive(false);
        hordeDirector.Initialize(playerHealth, zombieTemplate);
        shelterBuilder.Initialize(player.transform, playerInventory, hordeDirector);
        PrototypeResourcePickup.Spawn(new Vector3(2f, 0f, 1.5f), PrototypeResourceType.Wood, 3);
        PrototypeResourcePickup.Spawn(new Vector3(-2f, 0f, 2f), PrototypeResourceType.Scrap, 1);
        PrototypeResourcePickup.Spawn(new Vector3(-1f, 0f, 3f), PrototypeResourceType.Stone, 3);
        PrototypeResourcePickup.Spawn(new Vector3(1f, 0f, 3.5f), PrototypeResourceType.Fiber, 4);
        PrototypeResourcePickup.Spawn(new Vector3(0f, 0f, 4f), PrototypeResourceType.Food, 1);
        CreateCraftingStation(new Vector3(3f, 0f, 7f), CreateMaterial("WorkbenchMaterial", new Color(0.38f, 0.23f, 0.15f)));
        Material treeBark = CreateMaterial("TreeBark", new Color(0.30f, 0.17f, 0.12f));
        Material treeCanopy = CreateMaterial("TreeCanopy", new Color(0.15f, 0.32f, 0.2f));
        CreateTree(new Vector3(-22f, 0f, 5f), treeBark, treeCanopy);
        CreateTree(new Vector3(21f, 0f, 7f), treeBark, treeCanopy);
        CreateTree(new Vector3(-26f, 0f, 17f), treeBark, treeCanopy);
        CreateTree(new Vector3(24f, 0f, 18f), treeBark, treeCanopy);
        CreateTree(new Vector3(-30f, 0f, 3f), treeBark, treeCanopy);
        CreateTree(new Vector3(30f, 0f, 8f), treeBark, treeCanopy);
        new GameObject("Scene Orientation").AddComponent<BerneoxSceneOrientation>().SetOrientation(ScreenOrientation.LandscapeLeft);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        EditorSceneManager.SaveScene(scene, ScenePath);
        var buildScenes = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            if (existing.path != ScenePath) buildScenes.Add(existing);
        buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        EditorUtility.DisplayDialog("Prototipo creado", "ZombiePrototype incluye recolección, árboles, mochila de 12 espacios, mesa para fabricar bate/hacha y hordas. Prepárate antes de la primera ronda; hay tres minutos entre hordas. En Play, muévete con WASD/joystick, ataca con Espacio, interactúa con E y abre la mochila con I. Los valores de dificultad son provisionales.", "Aceptar");
    }

    private static GameObject CreateZombie(int index, Vector3 position, PrototypePlayerHealth target,
        Material bodyMaterial, Material detailMaterial, Material eyeMaterial)
    {
        GameObject zombie = new GameObject("Zombie_Prototype_" + index.ToString("00"));
        zombie.transform.position = position;
        CharacterController controller = zombie.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.38f;
        controller.center = Vector3.zero;
        controller.stepOffset = 0.3f;

        Transform visual = CreatePart(PrimitiveType.Capsule, "Zombie Body", zombie.transform,
            Vector3.zero, new Vector3(0.74f, 1f, 0.74f), bodyMaterial);
        CreatePart(PrimitiveType.Sphere, "Zombie Head", visual, new Vector3(0f, 0.86f, 0.05f),
            new Vector3(0.54f, 0.48f, 0.5f), detailMaterial);
        CreatePart(PrimitiveType.Capsule, "Zombie Left Arm", visual, new Vector3(-0.47f, 0.14f, 0.02f),
            new Vector3(0.21f, 0.53f, 0.21f), bodyMaterial).localRotation = Quaternion.Euler(0f, 0f, -28f);
        CreatePart(PrimitiveType.Capsule, "Zombie Right Arm", visual, new Vector3(0.47f, 0.17f, 0.02f),
            new Vector3(0.21f, 0.55f, 0.21f), bodyMaterial).localRotation = Quaternion.Euler(0f, 0f, 28f);
        CreatePart(PrimitiveType.Sphere, "Zombie Left Eye", visual, new Vector3(-0.12f, 0.9f, 0.28f),
            Vector3.one * 0.075f, eyeMaterial);
        CreatePart(PrimitiveType.Sphere, "Zombie Right Eye", visual, new Vector3(0.12f, 0.9f, 0.28f),
            Vector3.one * 0.075f, eyeMaterial);

        PrototypeZombieEnemy enemy = zombie.AddComponent<PrototypeZombieEnemy>();
        enemy.Initialize(target, visual);
        return zombie;
    }

    private static GameObject CreateTree(Vector3 position, Material barkMaterial, Material canopyMaterial)
    {
        GameObject tree = new GameObject("Resource_Tree");
        tree.transform.position = position;
        CapsuleCollider trunkCollider = tree.AddComponent<CapsuleCollider>();
        trunkCollider.center = new Vector3(0f, 2.1f, 0f);
        trunkCollider.height = 4.2f;
        trunkCollider.radius = 0.48f;
        Transform visuals = new GameObject("Tree Visuals").transform;
        visuals.SetParent(tree.transform, false);
        CreatePart(PrimitiveType.Cylinder, "Trunk", visuals, new Vector3(0f, 2f, 0f), new Vector3(0.42f, 2f, 0.42f), barkMaterial);
        CreatePart(PrimitiveType.Sphere, "Canopy", visuals, new Vector3(0f, 4.4f, 0f), new Vector3(3.2f, 3f, 3.2f), canopyMaterial);
        CreatePart(PrimitiveType.Sphere, "Canopy Crown", visuals, new Vector3(0f, 5.8f, 0f), new Vector3(2.2f, 2.3f, 2.2f), canopyMaterial);
        tree.AddComponent<PrototypeTreeNode>().Initialize(visuals);
        return tree;
    }

    private static GameObject CreateCraftingStation(Vector3 position, Material material)
    {
        GameObject station = new GameObject("Workbench_Prototype");
        station.transform.position = position;
        station.AddComponent<PrototypeCraftingStation>();
        BoxCollider interactionCollider = station.AddComponent<BoxCollider>();
        interactionCollider.center = new Vector3(0f, 0.8f, 0f);
        interactionCollider.size = new Vector3(3.2f, 2f, 2f);
        interactionCollider.isTrigger = true;
        Transform visuals = new GameObject("Workbench Visuals").transform;
        visuals.SetParent(station.transform, false);
        CreatePart(PrimitiveType.Cube, "Workbench Top", visuals, new Vector3(0f, 1.25f, 0f), new Vector3(2.8f, 0.25f, 1.6f), material);
        CreatePart(PrimitiveType.Cube, "Workbench Leg", visuals, new Vector3(-1f, 0.6f, -0.5f), new Vector3(0.2f, 1.2f, 0.2f), material);
        CreatePart(PrimitiveType.Cube, "Workbench Leg", visuals, new Vector3(1f, 0.6f, -0.5f), new Vector3(0.2f, 1.2f, 0.2f), material);
        CreatePart(PrimitiveType.Cube, "Workbench Leg", visuals, new Vector3(-1f, 0.6f, 0.5f), new Vector3(0.2f, 1.2f, 0.2f), material);
        CreatePart(PrimitiveType.Cube, "Workbench Leg", visuals, new Vector3(1f, 0.6f, 0.5f), new Vector3(0.2f, 1.2f, 0.2f), material);
        return station;
    }

    private static Transform CreatePart(PrimitiveType type, string name, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        return part.transform;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var material = new Material(shader ? shader : Shader.Find("Standard")) { name = name, color = color };
        return material;
    }
}
