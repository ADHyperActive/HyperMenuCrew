using Hazel;
using UnityEngine;

namespace ModMenuCrew;

/// <summary>
/// Utility class for controlling doors and ship systems.
/// </summary>
public static class SystemManager
{
    /// <summary>
    /// Closes the doors for a specific room/system type.
    /// </summary>
    public static void CloseDoorsOfType(SystemTypes type)
    {
        if (!ShipStatus.Instance)
        {
            ShowNotification("Error: ShipStatus not available!");
            return;
        }
        if (!IsDoorSystem(type))
        {
            ShowNotification($"{type} does not have closeable doors.");
            return;
        }
        try
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(
                ShipStatus.Instance.NetId, 27, SendOption.Reliable, AmongUsClient.Instance.HostId);
            messageWriter.Write((byte)type);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

            ShipStatus.Instance.RpcCloseDoorsOfType(type);
            ShowNotification($"{type} doors closed!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SystemManager] Error closing {type} doors: {e}");
            ShowNotification($"Error closing {type} doors!");
        }
    }

    /// <summary>
    /// Checks if the given system type has closeable doors.
    /// </summary>
    private static bool IsDoorSystem(SystemTypes type)
    {
        switch (type)
        {
            case SystemTypes.Electrical:
            case SystemTypes.MedBay:
            case SystemTypes.Security:
            case SystemTypes.Storage:
            case SystemTypes.Cafeteria:
            case SystemTypes.UpperEngine:
            case SystemTypes.LowerEngine:
                return true;
            default:
                return false;
        }
    }

    /// <summary>Shows an in-game notification via the HUD notifier.</summary>
    private static void ShowNotification(string message)
    {
        try
        {
            var hud = HudManager.Instance;
            if (hud != null && hud.Notifier != null)
            {
                hud.Notifier.AddDisconnectMessage(message);
            }
            else
            {
                Debug.LogWarning("[SystemManager] HudManager or Notifier is null.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SystemManager] Notification error: {e}");
        }
    }
}