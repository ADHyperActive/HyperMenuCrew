using System;
using System.Linq;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using ModMenuCrew.Features;
using ModMenuCrew.Patches;
using ModMenuCrew.UI.Controls;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Styles;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using InnerNet;

namespace ModMenuCrew
{
    /// <summary>
    /// Main plugin entry point for ModMenuCrew.
    /// BepInEx discovers this class automatically via the [BepInPlugin] attribute.
    /// 
    /// <b>HOW IT WORKS:</b>
    /// 1. BepInEx loads the plugin and calls <see cref="Load"/>.
    /// 2. Load() registers our MonoBehaviour (<see cref="DebuggerComponent"/>) into the IL2CPP runtime.
    /// 3. DebuggerComponent.Awake() creates the window, tabs, and feature managers.
    /// 4. Every frame, Update() handles hotkeys and game state; OnGUI() renders the IMGUI window.
    /// 
    /// <b>HOW TO ADD A NEW FEATURE:</b>
    /// - Add a new property (bool/float) to DebuggerComponent for your feature state.
    /// - Create a draw method (e.g., DrawMyFeatureTab) that uses GUILayout + GuiStyles.
    /// - Register it as a tab in InitializeTabsForGameIMGUI() with tabControl.AddTab(...).
    /// - Apply the feature logic in UpdateGameState() or via a Harmony patch.
    /// </summary>
    [BepInPlugin(Id, "ModMenuCrew - Showcase", ModVersion)]
    [BepInProcess("Among Us.exe")]
    public class ModMenuCrewPlugin : BasePlugin
    {
        public const string Id = "com.crewmod.showcase";
        public const string ModVersion = "6.0.8-SHOWCASE";

        public DebuggerComponent Component { get; private set; } = null!;
        public static ModMenuCrewPlugin Instance { get; private set; }
        public Harmony Harmony { get; } = new Harmony(Id);

        /// <summary>
        /// Called once by BepInEx when the plugin is loaded.
        /// Registers the IL2CPP component, applies Harmony patches, and initializes config.
        /// </summary>
        public override void Load()
        {
            Instance = this;
            Log.LogInfo($"Plugin {Id} v{ModVersion} is loading...");

            try { ClassInjector.RegisterTypeInIl2Cpp<DebuggerComponent>(); } catch { }
            Component = AddComponent<DebuggerComponent>();
            Harmony.PatchAll();

            if (Config != null)
                LobbyHarmonyPatches.InitializeConfig(Config);

            Log.LogInfo($"Plugin {Id} v{ModVersion} loaded successfully.");
        }

        /// <summary>
        /// Called when BepInEx unloads the plugin. Cleans up resources and unpatches Harmony.
        /// </summary>
        public override bool Unload()
        {
            try
            {
                Component?.CleanupResources();
                Harmony?.UnpatchSelf();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModMenuCrew] Error during unload: {ex}");
            }
            return base.Unload();
        }

        /// <summary>
        /// Core MonoBehaviour attached to a persistent GameObject.
        /// Handles the IMGUI menu rendering, hotkey detection, and per-frame feature updates.
        /// 
        /// <b>KEY CONCEPTS:</b>
        /// - <see cref="DragWindow"/>: Draggable IMGUI window with minimize/close buttons.
        /// - <see cref="TabControl"/>: Tab bar that switches between content panels.
        /// - <see cref="CheatManager"/>: Manages cheat toggles (vision, cursor teleport, etc.).
        /// - <see cref="TeleportManager"/>: Handles teleportation to players and map locations.
        /// 
        /// <b>HOW TO ADD A NEW TAB:</b>
        /// <code>
        /// // In InitializeTabsForGameIMGUI():
        /// tabControl.AddTab("MyTab", DrawMyTabContent, "Tooltip description");
        /// 
        /// // Then define the draw method:
        /// private void DrawMyTabContent()
        /// {
        ///     GUILayout.BeginVertical(GuiStyles.SectionStyle);
        ///     GUILayout.Label("My Section", GuiStyles.HeaderStyle);
        ///     GuiStyles.DrawSeparator();
        ///     
        ///     if (GUILayout.Button("My Button", GuiStyles.ButtonStyle))
        ///     {
        ///         // Your action here
        ///         ShowNotification("Button clicked!");
        ///     }
        ///     
        ///     GUILayout.EndVertical();
        /// }
        /// </code>
        /// 
        /// <b>HOW TO ADD A NEW TOGGLE:</b>
        /// <code>
        /// // 1. Add a property:
        /// public bool MyFeature { get; set; }
        /// 
        /// // 2. Draw it inside a tab:
        /// MyFeature = GuiStyles.DrawBetterToggle(MyFeature, "My Feature", "What this does");
        /// 
        /// // 3. Apply the effect in UpdateGameState():
        /// if (MyFeature) { /* apply effect */ }
        /// </code>
        /// 
        /// <b>HOW TO ADD A NEW SLIDER:</b>
        /// <code>
        /// GUILayout.Label($"My Value: {myValue:F1}", GuiStyles.LabelStyle);
        /// myValue = GUILayout.HorizontalSlider(myValue, min, max, GuiStyles.SliderStyle, GUI.skin.horizontalSliderThumb);
        /// </code>
        /// </summary>
        public class DebuggerComponent : MonoBehaviour
        {
            // ═══════════════════════════════════════════════════════════════
            // FEATURE STATE — Add new bool/float properties here for your features
            // ═══════════════════════════════════════════════════════════════
            public bool IsNoclipping { get; set; }
            public float PlayerSpeed { get; set; } = 2.1f;
            public float KillCooldown { get; set; } = 25f;
            public bool InfiniteVision { get; set; }
            public bool NoKillCooldown { get; set; }

            // ═══════════════════════════════════════════════════════════════
            // INTERNAL REFERENCES
            // ═══════════════════════════════════════════════════════════════
            private DragWindow mainWindow;
            private TabControl tabControl;
            private TabControl lobbyTabControl;
            private TeleportManager teleportManager;
            private CheatManager cheatManager;
            private Il2CppSystem.Collections.Generic.List<string> pendingNotifications = new();

            public DebuggerComponent(IntPtr ptr) : base(ptr) { }

            /// <summary>Releases all managed references for clean shutdown.</summary>
            public void CleanupResources()
            {
                teleportManager = null;
                cheatManager = null;
                mainWindow = null;
                tabControl = null;
                lobbyTabControl = null;
            }

            void OnDestroy() => CleanupResources();

            /// <summary>
            /// Called once when the component is created.
            /// Initializes feature managers, the IMGUI window, and all tabs.
            /// </summary>
            void Awake()
            {
                try
                {
                    InitializeFeatureManagers();
                    InitializeMainWindowIMGUI();
                    InitializeTabsForGameIMGUI();
                    ModMenuCrewPlugin.Instance.Log.LogInfo("DebuggerComponent initialized (SHOWCASE).");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModMenuCrew] Critical error in Awake: {ex}");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // INITIALIZATION
            // ═══════════════════════════════════════════════════════════════

            private void InitializeFeatureManagers()
            {
                teleportManager = new TeleportManager();
                cheatManager = new CheatManager();
            }

            /// <summary>
            /// Creates the main draggable IMGUI window.
            /// The window starts hidden (Enabled = false) and is toggled with F1.
            /// </summary>
            private void InitializeMainWindowIMGUI()
            {
                mainWindow = new DragWindow(
                    new Rect(24, 24, 514, 0),
                    $"ModMenuCrew v{ModMenuCrewPlugin.ModVersion}",
                    DrawMainModWindowIMGUI
                )
                {
                    Enabled = false
                };
                mainWindow.SetViewportMinHeight(160f);
            }

            /// <summary>
            /// Registers all in-game tabs. This is where you add new tabs.
            /// Each tab has: a name, a draw callback, and an optional tooltip.
            /// </summary>
            private void InitializeTabsForGameIMGUI()
            {
                tabControl = new TabControl();

                // ── Built-in tabs ──
                tabControl.AddTab("Game", DrawGameTabIMGUI, "General game controls and basic settings");
                tabControl.AddTab("Movement", DrawMovementTabIMGUI, "Movement controls and teleportation");
                tabControl.AddTab("Sabotage", DrawSabotageTabIMGUI, "Sabotage and door controls");

                if (cheatManager != null)
                    tabControl.AddTab("Cheats", cheatManager.DrawCheatsTab, "Cheats and advanced features");

                // ── HOW TO ADD YOUR OWN TAB ──
                // tabControl.AddTab("MyTab", DrawMyTabMethod, "Description shown on hover");
            }

            // ═══════════════════════════════════════════════════════════════
            // MAIN WINDOW CONTENT
            // ═══════════════════════════════════════════════════════════════

            /// <summary>
            /// Root draw callback for the main window.
            /// Shows lobby UI when not in a game, or the tab bar when in a game.
            /// </summary>
            private void DrawMainModWindowIMGUI()
            {
                if (ShipStatus.Instance == null)
                    DrawLobbyUI();
                else if (tabControl != null)
                    tabControl.Draw();
                else
                    GUILayout.Label("Error: Game tabs not initialized.", GuiStyles.ErrorStyle);
            }

            // ═══════════════════════════════════════════════════════════════
            // LOBBY UI
            // ═══════════════════════════════════════════════════════════════

            private void DrawLobbyUI()
            {
                if (lobbyTabControl == null)
                {
                    lobbyTabControl = new TabControl();
                    lobbyTabControl.AddTab("Lobby Info", () => DrawLobbyInfoContent(), "Lobby information");
                }
                lobbyTabControl.Draw();
            }

            private void DrawLobbyInfoContent()
            {
                GUILayout.BeginVertical(GuiStyles.SectionStyle);
                try
                {
                    GUILayout.Label($"Lobby Settings: {DateTime.Now:HH:mm}", GuiStyles.HeaderStyle);

                    if (GUILayout.Button("ModMenuCrew - <color=#44AAFF>crewcore.online</color>", GuiStyles.LabelStyle))
                        Application.OpenURL("https://crewcore.online");

                    GuiStyles.DrawSeparator();
                    GUILayout.Label("<color=#FF6600>SHOWCASE VERSION</color>", GuiStyles.HeaderStyle);
                    GUILayout.Label("This is a demonstration version with limited features.", GuiStyles.LabelStyle);
                    GUILayout.Label("Visit crewcore.online for the full version!", GuiStyles.LabelStyle);
                    GuiStyles.DrawSeparator();

                    DrawBanControls();
                }
                finally
                {
                    GUILayout.EndVertical();
                }
            }

            private void DrawBanControls()
            {
                var playerBanData = DataManager.Player?.ban;
                if (playerBanData == null) return;

                bool isLobby = LobbyBehaviour.Instance != null;
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
                if (isLobby && isHost) return;

                GUILayout.Label($"Ban Time Remaining: {playerBanData.BanMinutesLeft} minutes", GuiStyles.LabelStyle);

                if (GUILayout.Button("Add Ban Time (+10 pts)", GuiStyles.ButtonStyle))
                    AddBanPoints(playerBanData, 10);

                if (playerBanData.BanPoints > 0 && GUILayout.Button("Remove ALL Bans", GuiStyles.ButtonStyle))
                    RemoveAllBans(playerBanData);
            }

            private void AddBanPoints(PlayerBanData data, int points)
            {
                if (data == null) return;
                data.BanPoints += points;
                data.OnBanPointsChanged?.Invoke();
                data.PreviousGameStartDate = new Il2CppSystem.DateTime(DateTime.UtcNow.Ticks);
                ShowNotification($"Ban points added: {points}. Total: {data.BanPoints}");
            }

            private void RemoveAllBans(PlayerBanData data)
            {
                if (data == null) return;
                data.BanPoints = 0f;
                data.OnBanPointsChanged?.Invoke();
                data.PreviousGameStartDate = new Il2CppSystem.DateTime(DateTime.MinValue.Ticks);
                ShowNotification("All bans removed!");
            }

            // ═══════════════════════════════════════════════════════════════
            // GAME TAB — Force end, meetings, vision, speed
            // ═══════════════════════════════════════════════════════════════

            private void DrawGameTabIMGUI()
            {
                GUILayout.BeginVertical(GuiStyles.SectionStyle);
                GUILayout.Label("Game Controls", GuiStyles.HeaderStyle);
                GuiStyles.DrawSeparator();

                if (GUILayout.Button("Force Game End", GuiStyles.ButtonStyle))
                {
                    GameEndManager.ForceGameEnd(GameOverReason.ImpostorsByKill);
                    ShowNotification("Game end forced!");
                }

                if (PlayerControl.LocalPlayer != null && GUILayout.Button("Call Emergency Meeting", GuiStyles.ButtonStyle))
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                    ShowNotification("Emergency meeting called!");
                }

                GUILayout.BeginHorizontal();
                bool prevVision = InfiniteVision;
                InfiniteVision = GuiStyles.DrawBetterToggle(InfiniteVision, "Infinite Vision", "Removes fog of war for full map visibility");
                GuiStyles.DrawStatusIndicator(InfiniteVision);
                GUILayout.EndHorizontal();

                if (prevVision != InfiniteVision && HudManager.Instance?.ShadowQuad != null)
                    HudManager.Instance.ShadowQuad.gameObject.SetActive(!InfiniteVision);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Player Speed: {PlayerSpeed:F2}x", GuiStyles.LabelStyle);
                PlayerSpeed = GUILayout.HorizontalSlider(PlayerSpeed, 0.5f, 6f, GuiStyles.SliderStyle, GUI.skin.horizontalSliderThumb);
                GUILayout.EndHorizontal();

                GuiStyles.DrawSeparator();
                GUILayout.EndVertical();
            }

            // ═══════════════════════════════════════════════════════════════
            // MOVEMENT TAB — Noclip, teleport to players and locations
            // ═══════════════════════════════════════════════════════════════

            private void DrawMovementTabIMGUI()
            {
                GUILayout.BeginVertical(GuiStyles.SectionStyle);
                GUILayout.Label("Movement Controls", GuiStyles.HeaderStyle);
                GuiStyles.DrawSeparator();

                if (ShipStatus.Instance == null)
                {
                    GUILayout.Label("Available during a game.", GuiStyles.SubHeaderStyle);
                    GUILayout.Label("Join or start a match to use movement features.", GuiStyles.LabelStyle);
                    GUILayout.EndVertical();
                    return;
                }

                if (PlayerControl.LocalPlayer != null)
                {
                    IsNoclipping = GuiStyles.DrawBetterToggle(IsNoclipping, "Enable Noclip", "Walk through walls and obstacles");
                    if (PlayerControl.LocalPlayer.Collider != null)
                        PlayerControl.LocalPlayer.Collider.enabled = !IsNoclipping;
                }

                if (teleportManager != null)
                {
                    if (GUILayout.Button("Teleport to Nearest Player", GuiStyles.ButtonStyle))
                    {
                        teleportManager.TeleportToPlayer(teleportManager.GetClosestPlayer());
                        ShowNotification("Teleported to nearest player!");
                    }

                    foreach (var location in teleportManager.Locations)
                    {
                        if (GUILayout.Button($"Teleport to {location.Key}", GuiStyles.ButtonStyle))
                        {
                            teleportManager.TeleportToLocation(location.Key);
                            ShowNotification($"Teleported to {location.Key}!");
                        }
                    }
                }

                GuiStyles.DrawSeparator();
                GUILayout.EndVertical();
            }

            // ═══════════════════════════════════════════════════════════════
            // SABOTAGE TAB — Door controls per room
            // ═══════════════════════════════════════════════════════════════

            private void DrawSabotageTabIMGUI()
            {
                GUILayout.BeginVertical(GuiStyles.SectionStyle);
                GUILayout.Label("Sabotage Controls", GuiStyles.HeaderStyle);
                GuiStyles.DrawSeparator();

                if (ShipStatus.Instance == null)
                {
                    GUILayout.Label("Available during a game.", GuiStyles.SubHeaderStyle);
                    GUILayout.Label("Join or start a match to control doors.", GuiStyles.LabelStyle);
                    GUILayout.EndVertical();
                    return;
                }

                if (GUILayout.Button("Close Cafeteria Doors", GuiStyles.ButtonStyle))
                {
                    SystemManager.CloseDoorsOfType(SystemTypes.Cafeteria);
                    ShowNotification("Cafeteria doors closed!");
                }
                if (GUILayout.Button("Close Storage Doors", GuiStyles.ButtonStyle))
                {
                    SystemManager.CloseDoorsOfType(SystemTypes.Storage);
                    ShowNotification("Storage doors closed!");
                }
                if (GUILayout.Button("Close Medbay Doors", GuiStyles.ButtonStyle))
                {
                    SystemManager.CloseDoorsOfType(SystemTypes.MedBay);
                    ShowNotification("Medbay doors closed!");
                }
                if (GUILayout.Button("Close Security Doors", GuiStyles.ButtonStyle))
                {
                    SystemManager.CloseDoorsOfType(SystemTypes.Security);
                    ShowNotification("Security doors closed!");
                }

                GuiStyles.DrawSeparator();
                GUILayout.EndVertical();
            }

            // ═══════════════════════════════════════════════════════════════
            // PER-FRAME LOGIC
            // ═══════════════════════════════════════════════════════════════

            /// <summary>
            /// Applies feature state to the game every frame.
            /// Add your per-frame feature logic here.
            /// </summary>
            private void UpdateGameState()
            {
                if (PlayerControl.LocalPlayer == null) return;

                try
                {
                    if (HudManager.Instance?.ShadowQuad != null)
                        HudManager.Instance.ShadowQuad.gameObject.SetActive(!InfiniteVision);

                    if (PlayerControl.LocalPlayer.MyPhysics != null)
                        PlayerControl.LocalPlayer.MyPhysics.Speed = PlayerSpeed;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModMenuCrew] Error in UpdateGameState: {ex}");
                }
            }

            /// <summary>
            /// Displays an in-game notification via the HUD notifier.
            /// Falls back to a pending queue if the HUD is not yet available.
            /// </summary>
            private void ShowNotification(string message)
            {
                try
                {
                    Debug.Log($"[ModMenuCrew] {message}");
                    if (HudManager.Instance?.Notifier != null)
                        HudManager.Instance.Notifier.AddDisconnectMessage(message);
                    else
                        pendingNotifications?.Add(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModMenuCrew] Notification error: {ex}");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // UNITY CALLBACKS
            // ═══════════════════════════════════════════════════════════════

            /// <summary>
            /// Called every frame. Handles hotkeys, feature updates, and safety resets.
            /// </summary>
            void Update()
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.F1) && mainWindow != null)
                        mainWindow.Enabled = !mainWindow.Enabled;

                    if (mainWindow is { Enabled: true } && cheatManager != null)
                        cheatManager.Update();

                    GameCheats.CheckTeleportInput();
                    UpdateGameState();

                    if (IsNoclipping && PlayerControl.LocalPlayer?.Collider != null && ShipStatus.Instance == null)
                    {
                        PlayerControl.LocalPlayer.Collider.enabled = true;
                        IsNoclipping = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModMenuCrew] Update error: {ex}");
                }
            }

            /// <summary>Called by Unity for IMGUI rendering. Draws the main window if enabled.</summary>
            void OnGUI()
            {
                if (mainWindow is { Enabled: true })
                    mainWindow.OnGUI();
            }
        }
    }
}