#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TenjikuDevaYuddha.Editor
{
    /// <summary>
    /// Editor utility to automatically create and configure the required scenes
    /// with all necessary GameObjects and components.
    /// Access via menu: Tenjiku Deva Yuddha > Setup Scenes
    /// </summary>
    public static class SceneSetupEditor
    {
        [MenuItem("Tenjiku Deva Yuddha/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            // Ensure Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            CreateMainMenuScene();
            CreateGameScene();
            SetupBuildSettings();

            Debug.Log("[SceneSetup] ✅ All scenes created and configured!");
            EditorUtility.DisplayDialog("Tenjiku Deva Yuddha",
                "All scenes created successfully!\n\n" +
                "• MainMenu scene\n" +
                "• GameScene\n\n" +
                "Build Settings updated.\n" +
                "Press Play to start the game from MainMenu.",
                "OK");
        }

        [MenuItem("Tenjiku Deva Yuddha/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera setup
            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.05f, 0.03f, 0.1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
            }

            // Main Menu UI
            var menuObj = new GameObject("MainMenuUI");
            menuObj.AddComponent<TenjikuDevaYuddha.Core.MainMenuUI>();

            // Persistent Managers (will be created by MainMenuUI if needed)
            // But we add them here for clarity
            var managersObj = new GameObject("--- GAME MANAGERS ---");
            managersObj.AddComponent<TenjikuDevaYuddha.Core.GameManager>();
            managersObj.AddComponent<TenjikuDevaYuddha.Core.SaveLoadManager>();

            // Ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.25f, 0.35f);

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[SceneSetup] MainMenu scene created.");
        }

        [MenuItem("Tenjiku Deva Yuddha/Create Game Scene")]
        public static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ─── Camera ───
            var cam = Camera.main;
            if (cam != null)
            {
                cam.gameObject.AddComponent<TenjikuDevaYuddha.Core.CameraController>();
                cam.backgroundColor = new Color(0.1f, 0.15f, 0.25f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.transform.position = new Vector3(32, 25, 17);
                cam.transform.rotation = Quaternion.Euler(55, 0, 0);
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 200f;
            }

            // ─── Directional Light (Sun) ───
            var lightObj = GameObject.Find("Directional Light");
            if (lightObj != null)
            {
                var light = lightObj.GetComponent<Light>();
                light.color = new Color(1f, 0.95f, 0.85f);
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // ─── Game Systems Root ───
            var systemsRoot = new GameObject("--- GAME SYSTEMS ---");

            // Scene Initializer
            var initObj = new GameObject("GameSceneInitializer");
            initObj.transform.SetParent(systemsRoot.transform);
            initObj.AddComponent<TenjikuDevaYuddha.Core.GameSceneInitializer>();

            // Grid Manager
            var gridObj = new GameObject("GridManager");
            gridObj.transform.SetParent(systemsRoot.transform);
            gridObj.AddComponent<TenjikuDevaYuddha.Core.GridManager>();

            // Building System
            var buildObj = new GameObject("BuildingSystem");
            buildObj.transform.SetParent(systemsRoot.transform);
            buildObj.AddComponent<TenjikuDevaYuddha.Core.BuildingSystem>();

            // Resource Manager
            var resObj = new GameObject("ResourceManager");
            resObj.transform.SetParent(systemsRoot.transform);
            resObj.AddComponent<TenjikuDevaYuddha.Core.ResourceManager>();

            // Dashavatar Manager
            var avatarObj = new GameObject("DashavatarManager");
            avatarObj.transform.SetParent(systemsRoot.transform);
            avatarObj.AddComponent<TenjikuDevaYuddha.Core.DashavatarManager>();

            // Veda Research System
            var vedaObj = new GameObject("VedaResearchSystem");
            vedaObj.transform.SetParent(systemsRoot.transform);
            vedaObj.AddComponent<TenjikuDevaYuddha.Core.VedaResearchSystem>();

            // Rudra Power System
            var rudraObj = new GameObject("RudraPowerSystem");
            rudraObj.transform.SetParent(systemsRoot.transform);
            rudraObj.AddComponent<TenjikuDevaYuddha.Core.RudraPowerSystem>();

            // Battle System
            var battleObj = new GameObject("BattleSystem");
            battleObj.transform.SetParent(systemsRoot.transform);
            battleObj.AddComponent<TenjikuDevaYuddha.Core.BattleSystem>();

            // ─── UI ───
            var uiRoot = new GameObject("--- UI ---");
            var hudObj = new GameObject("GameHUD");
            hudObj.transform.SetParent(uiRoot.transform);
            hudObj.AddComponent<TenjikuDevaYuddha.Core.GameHUD>();

            // ─── Lighting ───
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.45f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.18f, 0.15f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.85f);
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 80f;

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
            Debug.Log("[SceneSetup] GameScene created with all systems.");
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[SceneSetup] Build settings updated with MainMenu and GameScene.");
        }

        // ─── Quick Play ───
        [MenuItem("Tenjiku Deva Yuddha/Play from Main Menu")]
        public static void PlayFromMainMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                SetupAllScenes();
            }

            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
#endif
