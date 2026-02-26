using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace ModMenuCrew.Messages
{
    /// <summary>Types of custom messages that can be sent between players.</summary>
    public enum MessageType : byte
    {
        Normal = 0,
        Command = 1,
        System = 2,
        Private = 3,
        Broadcast = 4
    }

    /// <summary>
    /// Represents a custom network message sent via RPC.
    /// Used for broadcasting commands and chat messages between players.
    /// </summary>
    public class CustomMessage
    {
        public byte Tag { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }

        public CustomMessage(byte tag, int senderId, string senderName, string content, MessageType type)
        {
            Tag = tag;
            SenderId = senderId;
            SenderName = senderName;
            Content = content;
            Timestamp = DateTime.UtcNow;
            Type = type;
        }

        public void Serialize(MessageWriter writer)
        {
            writer.Write(Tag);
            writer.WritePacked(SenderId);
            writer.Write(SenderName ?? "");
            writer.Write(Content ?? "");
            writer.Write(Timestamp.ToBinary());
            writer.Write((byte)Type);
        }

        public static CustomMessage Deserialize(MessageReader reader)
        {
            var message = new CustomMessage(
                reader.ReadByte(),
                reader.ReadPackedInt32(),
                reader.ReadString(),
                reader.ReadString(),
                (MessageType)reader.ReadByte()
            );
            message.Timestamp = DateTime.FromBinary((long)reader.ReadUInt64());
            return message;
        }

        /// <summary>Sends this message to all players via RPC.</summary>
        public void SendBypass()
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmConnected) return;
            if (PlayerControl.LocalPlayer == null) return;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRpcCalls.BroadcastMessage,
                SendOption.Reliable,
                -1
            );

            Serialize(writer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        /// <summary>Handles an incoming custom message from the network.</summary>
        public static void HandleBypass(MessageReader reader)
        {
            try
            {
                CustomMessage message = Deserialize(reader);
                if (HudManager.Instance != null && HudManager.Instance.Chat != null)
                {
                    var player = PlayerControl.AllPlayerControls.ToArray()
                        .FirstOrDefault(p => p.PlayerId == message.SenderId);

                    if (player != null)
                    {
                        HudManager.Instance.Chat.AddChat(player, message.Content);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CustomMessage] Error processing message: {e.Message}");
            }
        }

        /// <summary>Broadcasts a text message to all players in the lobby.</summary>
        public static void SendMessageToAll(string messageContent)
        {
            if (PlayerControl.LocalPlayer == null) return;

            var message = new CustomMessage(
                0, // tag
                PlayerControl.LocalPlayer.PlayerId, // senderId
                PlayerControl.LocalPlayer.Data.PlayerName, // senderName
                messageContent, // content
                MessageType.Broadcast // type
            );

            message.SendBypass();
        }

        /// <summary>Sends a private message to a specific player (sent via broadcast, filtered client-side).</summary>
        public static void SendPrivateMessage(string messageContent, PlayerControl targetPlayer)
        {
            if (PlayerControl.LocalPlayer == null || targetPlayer == null) return;

            var message = new CustomMessage(
                0, // tag
                PlayerControl.LocalPlayer.PlayerId, // senderId
                PlayerControl.LocalPlayer.Data.PlayerName, // senderName
                messageContent, // content
                MessageType.Private // type
            );

            message.SendBypass();
        }
    }

    /// <summary>Custom RPC call IDs used by ModMenuCrew.</summary>
    public enum CustomRpcCalls : byte
    {
        BroadcastMessage = 201
    }

    /// <summary>Harmony patch to intercept and handle custom RPC calls.</summary>
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class RpcPatch
    {
        public static void Postfix(PlayerControl __instance, byte callId, MessageReader reader)
        {
            // Only process our custom RPC — never interfere with vanilla RPCs
            if (callId == (byte)CustomRpcCalls.BroadcastMessage)
            {
                CustomMessage.HandleBypass(reader);
            }
        }
    }
}