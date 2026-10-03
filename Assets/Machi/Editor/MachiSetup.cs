using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Machi.EditorTools
{
    /// <summary>Creates and opens the Main scene the first time the project is opened.</summary>
    [InitializeOnLoad]
    public static class MachiSetup
    {
        const string ScenePath = "Assets/Machi/Scenes/Main.unity";

        static MachiSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath) && !EditorApplication.isPlayingOrWillChangePlaymode) CreateMainScene();
            };
        }

        [MenuItem("Machi/Create Or Open Main Scene")]
        public static void CreateMainScene()
        {
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
                var sun = new GameObject("Sun", typeof(Light));
                sun.GetComponent<Light>().type = LightType.Directional;
                // GameRoot builds everything else at runtime (see GameRoot.AutoCreate).
                new GameObject("GameRoot", typeof(GameRoot));
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log("[Machi] created " + ScenePath);
            }
            else EditorSceneManager.OpenScene(ScenePath);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        [MenuItem("Machi/Delete Save File")]
        public static void DeleteSave()
        {
            SaveStore.Delete();
            Debug.Log("[Machi] save deleted");
        }

        [MenuItem("Machi/Open Save Folder")]
        public static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
