using InnerNet;
using UnityEngine;

/// <summary>
/// Utility class for forcing game end conditions. Host-only.
/// </summary>
public static class GameEndManager
{
    /// <summary>
    /// Forces the game to end with the specified reason. Only works if you are the host.
    /// </summary>
    /// <param name="endReason">The reason for the game ending (e.g., ImpostorsByKill).</param>
    public static void ForceGameEnd(GameOverReason endReason)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            Debug.LogWarning("[GameEndManager] Only the host can force game end.");
            return;
        }

        if (GameManager.Instance == null || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
            return;

        GameManager.Instance.RpcEndGame(endReason, showAd: false);
    }
}