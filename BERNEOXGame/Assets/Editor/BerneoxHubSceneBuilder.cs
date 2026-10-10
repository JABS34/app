using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BerneoxHubSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BERNEOXHub.unity";

    [MenuItem("BERNEOX/Crear inicio y catálogo")]
    public static void CreateScene()
    {
        if (SceneExists())
        {
            if (!Application.isBatchMode)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorUtility.DisplayDialog("Inicio BERNEOX listo", "Se abrió la escena BERNEOXHub. Pulsa Play para ver la apertura y el catálogo.", "Aceptar");
            }
            return;
        }

        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        CreateNewScene();
        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Inicio BERNEOX creado", "Se creó BERNEOXHub sin modificar SampleScene ni el prototipo Zombie Survival. Pulsa Play para ver la apertura y el catálogo.", "Aceptar");
    }

    // Public entry point for a safe Unity batch-mode generation and compile check.
    public static void CreateSceneForBuild()
    {
        if (SceneExists()) throw new InvalidOperationException("Refusing to overwrite existing scene: " + ScenePath);
        CreateNewScene();
    }

    private static void CreateNewScene()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "BERNEOXHub";
        GameObject root = new GameObject("BERNEOX Hub Runtime");
        root.AddComponent<BerneoxHubController>();
        root.AddComponent<BerneoxSceneOrientation>().SetOrientation(ScreenOrientation.Portrait);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        // The hub becomes the app's entry scene; existing build scenes remain enabled and intact.
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (existing.path == ScenePath) continue;
            scenes.Add(existing);
        }
        EditorBuildSettings.scenes = scenes.ToArray();

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        AssetDatabase.SaveAssets();
    }

    private static bool SceneExists()
    {
        return File.Exists(Path.Combine(Application.dataPath, "Scenes", "BERNEOXHub.unity"));
    }
}
