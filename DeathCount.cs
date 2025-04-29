using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace DeathCount
{
    [BepInPlugin("Ara.DeathCount", "DeathCount", "1.0")]
    public class DeathCount : BaseUnityPlugin
    {
        internal static DeathCount Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        // Death tracking data
        private int totalDeaths = 0;

        // Death detection variables
        private float deathCooldown = 0f;
        private const float DEATH_COOLDOWN_TIME = 2.0f;

        // UI variables
        private GUIStyle deathCountStyle;
        private GUIStyle debugStyle;
        private Color deathCountColor = new Color(1f, 0.5f, 0f); // Orange color similar to game UI

        // Debug mode - force display for testing
        private bool forceDisplay = false;

        // Tracking for OnGUI calls
        private float uiUpdateTimer = 0f;
        private int uiCallCount = 0;

        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            // Always start at 0 deaths
            totalDeaths = 0;

            // Listen for scene changes to detect level loading
            SceneManager.sceneLoaded += OnSceneLoaded;

            Patch();

            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Log the scene name for debugging purposes
            Logger.LogInfo($"Scene loaded: {scene.name}");

            // For scenes that look like game levels, log more details
            if (scene.name.StartsWith("Level - ") && !scene.name.Contains("Lobby"))
            {
                Logger.LogInfo($"POTENTIAL GAME LEVEL LOADED: {scene.name}");

                // Reset UI tracking data on new level
                uiCallCount = 0;
                uiUpdateTimer = 0f;
            }
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
            Logger.LogInfo("Applied Harmony patches for death tracking");
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
            Logger.LogInfo("Removed Harmony patches");
        }

        private void InitializeGUIStyles()
        {
            if (deathCountStyle == null)
            {
                deathCountStyle = new GUIStyle();
                deathCountStyle.fontSize = 20;
                deathCountStyle.fontStyle = FontStyle.Bold;
                deathCountStyle.normal.textColor = deathCountColor;
                deathCountStyle.alignment = TextAnchor.UpperRight;

                deathCountStyle.font = Font.CreateDynamicFontFromOSFont("Arial", 20);
            }

            if (debugStyle == null)
            {
                debugStyle = new GUIStyle();
                debugStyle.fontSize = 12;
                debugStyle.normal.textColor = Color.yellow;
                debugStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.5f));
            }
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            // Track OnGUI calls
            uiCallCount++;
            InitializeGUIStyles();

            // Determine if we should show the death counter based on scene name
            string currentScene = SceneManager.GetActiveScene().name;
            bool isGameLevel = currentScene.StartsWith("Level - ") && !currentScene.Contains("Lobby");

            // Show the counter if we're in a game level or if force display is enabled
            if (isGameLevel || forceDisplay)
            {
                // Position under the 0/2 counter on the right side (exactly as in your screenshot)
                // Calculate position based on screen size
                float xPos = Screen.width - 125; // Right side positioning
                float yPos = 275; // Below the existing UI element

                // Draw death counter text - match the game's UI style
                GUI.Label(new Rect(xPos, yPos, 80, 25), "DEATHS", deathCountStyle);
                GUI.Label(new Rect(xPos, yPos + 25, 80, 25), totalDeaths.ToString(), deathCountStyle);
            }
        }

        // Called from the Harmony patches when a player death is detected
        public void RegisterDeath()
        {
            // Check if we're in cooldown to prevent double-counting
            if (deathCooldown <= 0)
            {
                totalDeaths++;
                deathCooldown = DEATH_COOLDOWN_TIME;
                Logger.LogInfo($"Player died! Total deaths: {totalDeaths}");
            }
        }

        public void DeleteDeath()
        {
            if (totalDeaths > 0 && deathCooldown <= 0)
            {
                totalDeaths--;
                deathCooldown = DEATH_COOLDOWN_TIME;
                Logger.LogInfo($"Death removed! Total deaths: {totalDeaths}");
            }
        }

        private void Update()
        {
            // Update death cooldown
            if (deathCooldown > 0)
            {
                deathCooldown -= Time.deltaTime;
            }

            // Track UI updates
            uiUpdateTimer += Time.deltaTime;
            if (uiUpdateTimer >= 5f)
            {
                Logger.LogInfo($"UI update: {uiCallCount} OnGUI calls in 5 seconds");
                uiUpdateTimer = 0f;
                uiCallCount = 0;
            }

            // Toggle force display with F10
            if (Input.GetKeyDown(KeyCode.F10))
            {
                forceDisplay = !forceDisplay;
                Logger.LogInfo($"Force display toggled to: {forceDisplay}");
            }

            // Delete a death with F9
            if (Input.GetKeyDown(KeyCode.F9))
            {
                DeleteDeath();
            }

            // For testing: Simulate death with F8
            if (Input.GetKeyDown(KeyCode.F8))
            {
                RegisterDeath();
            }
        }

        private void OnDestroy()
        {
            // Clean up
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Patch PlayerAvatar.PlayerDeath to detect when the player dies
    [HarmonyPatch(typeof(PlayerAvatar), "PlayerDeath")]
    class PlayerDeathPatch
    {
        static void Prefix()
        {
            DeathCount.Instance.RegisterDeath();
            DeathCount.Logger.LogInfo("Death detected: PlayerDeath called");
        }
    }
}
