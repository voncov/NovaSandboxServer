using LiteNetLib;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine;
#nullable enable
#else
using Serilog;
#endif

namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public static class NetworkPacketParser
    {
        public static NetworkPacketBase? Parse(NetPacketReader reader)
        {
            try
            {
                byte protocolVersion = reader.GetByte(); // Protocol : byte
                if (protocolVersion != NetworkPacketBase.ProtocolVersion)
                {
#if UNITY_2021_3_OR_NEWER
                Debug.LogError("Invalid packet received.");
#else
                    Log.Error("Invalid packet received.");
#endif
                    return null;
                }
                byte packetTypeRaw = reader.GetByte(); // PacketType : byte
#if UNITY_2021_3_OR_NEWER
            if (!Enum.IsDefined(typeof(NetworkPacketEnum), (NetworkPacketEnum)packetTypeRaw))
#else
                if (!Enum.IsDefined((NetworkPacketEnum)packetTypeRaw))
#endif
                {
#if UNITY_2021_3_OR_NEWER
                Debug.LogError($"Unknown packet type: {packetTypeRaw}");
#else
                    Log.Error($"Unknown packet type: {packetTypeRaw}");
#endif
                    return null;
                }
                short size = reader.GetShort(); // Size : short
                if (size != reader.AvailableBytes)
                {
#if UNITY_2021_3_OR_NEWER
                Debug.LogError("Invalid packet size received.");
#else
                    Log.Error("Invalid packet size received.");
#endif
                    return null;
                }
                NetworkPacketEnum packetType = (NetworkPacketEnum)packetTypeRaw;
                NetworkPacketBase packet = NetworkProtocolRegistry.Create(packetType);
                packet.Protocol = protocolVersion;
                packet.Size = size;
                packet.Read(reader.GetRemainingBytes());
                return packet;
            }
            catch (Exception ex)
            {
#if UNITY_2021_3_OR_NEWER
            Debug.LogError($"Unable to read packet:\n{ex.Message}\n{ex.StackTrace}");
#else
                Log.Error(ex, "Unable to read packet: ");
#endif
                return null;
            }
        }
    }
}