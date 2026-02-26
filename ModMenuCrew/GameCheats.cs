using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.Features
{
    /// <summary>
    /// Static utility class containing all cheat implementations.
    /// Each region groups related cheats: meetings, tasks, movement, and visual.
    /// 
    /// <b>HOW TO ADD A NEW CHEAT:</b>
    /// 1. Create a public static method in the appropriate region.
    /// 2. Call it from a button in CheatManager.DrawGeneralCheatsSection().
    /// 3. Use LogCheat() for console output.
    /// </summary>
    public static class GameCheats
    {
        public const byte RPC_SET_SCANNER = 15;
        private static readonly System.Random random = new System.Random();
        public static bool TeleportToCursorEnabled = false;

        private static void LogCheat(string message) => Debug.Log($"[Cheat] {message}");

        #region Meeting Cheats
        /// <summary>
        /// Closes the current meeting or exile screen and restores gameplay.
        /// </summary>
        public static void CloseMeeting()
        {
            try
            {
                if (MeetingHud.Instance)
                {
                    MeetingHud.Instance.DespawnOnDestroy = false;
                    UnityEngine.Object.Destroy(MeetingHud.Instance.gameObject);
                    DestroyableSingleton<HudManager>.Instance.StartCoroutine(
                        DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
                    PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
                    ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                    DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
                    LogCheat("Meeting closed successfully.");
                }
                else if (ExileController.Instance != null)
                {
                    ExileController.Instance.ReEnableGameplay();
                    ExileController.Instance.WrapUp();
                    LogCheat("Exile ended.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in CloseMeeting: {e}");
            }
        }
        #endregion

        #region Task Cheats
        /// <summary>
        /// Completes all tasks for the local player.
        /// </summary>
        public static void CompleteAllTasks()
        {
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                Debug.LogWarning("[GameCheats] Local player not found.");
                return;
            }
            if (AmongUsClient.Instance == null)
            {
                Debug.LogWarning("[GameCheats] AmongUsClient not initialized.");
                return;
            }
            try
            {
                var taskList = PlayerControl.LocalPlayer.Data.Tasks;
                if (taskList == null || taskList.Count == 0)
                {
                    Debug.LogWarning("[GameCheats] No tasks found to complete.");
                    return;
                }
                HudManager.Instance.StartCoroutine(CompleteAllTasksWithDelay(0.2f).WrapToIl2Cpp());
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameCheats] Error completing tasks: {e}");
            }
        }

        /// <summary>
        /// Completes all tasks with a small delay between each completion to avoid detection.
        /// </summary>
        public static IEnumerator CompleteAllTasksWithDelay(float perTaskDelay = 0.2f)
        {
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || AmongUsClient.Instance == null)
                yield break;

            bool isHost = AmongUsClient.Instance.AmHost;
            var taskList = PlayerControl.LocalPlayer.Data.Tasks;
            if (taskList == null || taskList.Count == 0)
                yield break;

            var taskInfosSnapshot = taskList.ToArray();
            var idsToComplete = new List<int>();
            foreach (var ti in taskInfosSnapshot)
                if (ti != null && !ti.Complete)
                    idsToComplete.Add((int)ti.Id);

            foreach (var id in idsToComplete)
            {
                var currentList = PlayerControl.LocalPlayer.Data.Tasks;
                if (currentList == null) break;

                object match = null;
                for (int i = 0, c = currentList.Count; i < c; i++)
                {
                    var cur = currentList[i];
                    if (cur != null && (int)cur.Id == id) { match = cur; break; }
                }
                if (match == null) continue;

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.Reliable, -1);
                writer.WritePacked(id);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                try { ((dynamic)match).Complete = true; } catch { }
                LogCheat($"Task {id} completed. (Host: {isHost})");

                yield return new WaitForSeconds(Mathf.Max(0.05f, perTaskDelay));
            }

            var myTasksSnapshot = PlayerControl.LocalPlayer.myTasks.ToArray();
            foreach (var task in myTasksSnapshot)
            {
                if (task == null || task.IsComplete) continue;

                if (task is NormalPlayerTask normalTask)
                {
                    while (normalTask.TaskStep < normalTask.MaxStep)
                    {
                        normalTask.NextStep();
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                task.Complete();
                yield return new WaitForSeconds(0.05f);
            }

            PlayerControl.LocalPlayer.Data.MarkDirty();
            LogCheat("All tasks completed.");
        }
        #endregion

        #region Movement Cheats
        /// <summary>
        /// Teleports the local player to the mouse cursor position.
        /// Sends an RPC to sync the position with other clients.
        /// </summary>
        public static void TeleportToCursor()
        {
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.NetTransform == null || Camera.main == null)
            {
                Debug.LogWarning("[GameCheats] Local player or camera not found.");
                return;
            }
            try
            {
                Vector2 mousePos = Input.mousePosition;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                if (!Physics2D.OverlapPoint(worldPos, Constants.ShipAndAllObjectsMask))
                {
                    PlayerControl.LocalPlayer.NetTransform.SnapTo(worldPos);
                    LogCheat($"Teleported to {worldPos}.");

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SnapTo, SendOption.Reliable, -1);
                    writer.Write(worldPos.x);
                    writer.Write(worldPos.y);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in TeleportToCursor: {e}");
            }
        }

        /// <summary>
        /// Checks for right-click input to trigger cursor teleport when enabled.
        /// Called every frame from DebuggerComponent.Update().
        /// </summary>
        public static void CheckTeleportInput()
        {
            if (TeleportToCursorEnabled && Input.GetMouseButtonDown(1))
            {
                TeleportToCursor();
            }
        }
        #endregion

        #region Visual Cheats
        /// <summary>
        /// Reveals all impostors by coloring their names red and showing their role.
        /// </summary>
        public static void RevealImpostors()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data?.Role != null)
                {
                    Color roleColor = player.Data.Role.IsImpostor ? Color.red : Color.white;
                    string roleName = player.Data.Role.Role.ToString();

                    string roleHex = ColorUtility.ToHtmlStringRGB(roleColor);
                    string roleLine = $"<size=65%><color=#{roleHex}><i>{roleName}</i></color></size>";
                    string display = roleLine + "\n" + player.Data.PlayerName;

                    player.cosmetics.nameText.color = Color.white;
                    player.cosmetics.nameText.text = display;
                }
            }
            LogCheat("Impostors revealed.");
        }

        /// <summary>
        /// Increases the player's field of view by adjusting the camera orthographic size.
        /// </summary>
        public static void IncreaseVision(float multiplier)
        {
            if (Camera.main != null)
            {
                Camera.main.orthographicSize = 3.0f * multiplier;
                LogCheat($"Vision set to {multiplier}x.");
            }
        }

        /// <summary>
        /// Resets vision to the default 3.0 orthographic size.
        /// </summary>
        public static void ResetVision()
        {
            if (Camera.main != null)
            {
                Camera.main.orthographicSize = 3.0f;
                LogCheat("Vision reset to default.");
            }
        }
        #endregion
    }
}
