using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ZombiePrototypeBuilder
{
    [MenuItem("BERNEOX/Crear escena de prototipo Zombie Survival")]
    public static void CreateScene()
    {
        if (!EditorUtility.DisplayDialog("Crear prototipo", "Se creará una escena nueva llamada ZombiePrototype. SampleScene se conservará sin cambios.", "Crear", "Cancelar")) return;

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
        player.AddComponent<CharacterController>();
        player.AddComponent<PrototypePlayerController>();

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
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/ZombiePrototype.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/ZombiePrototype.unity", true) };
        EditorUtility.DisplayDialog("Prototipo creado", "La escena ZombiePrototype ya está lista. El personaje provisional se mueve con WASD/flechas o el joystick táctil en Play.", "Aceptar");
    }

    private static Material CreateMaterial(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var material = new Material(shader ? shader : Shader.Find("Standard")) { name = name, color = color };
        return material;
    }
}
