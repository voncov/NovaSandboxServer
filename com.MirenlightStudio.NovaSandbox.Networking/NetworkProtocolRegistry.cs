using System.Runtime.CompilerServices;

#if !UNITY_2021_3_OR_NEWER
using Serilog;
using com.MirenlightStudio.NovaSandbox.Networking.Packets;
#else
using UnityEngine;
using System;
using System.Collections.Generic;
#nullable enable
#endif

namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public static class NetworkProtocolRegistry
    {
        private static readonly Dictionary<NetworkPacketEnum, Func<NetworkPacketBase>> _registry = new();

        public static void Register(NetworkPacketEnum type, Func<NetworkPacketBase> factory)
        {
            _registry[type] = factory;
        }

        public static bool EnsureLoaded()
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(AuthPacket).TypeHandle);
                RuntimeHelpers.RunClassConstructor(typeof(AuthResponsePacket).TypeHandle);
                RuntimeHelpers.RunClassConstructor(typeof(BannedResponsePacket).TypeHandle);
                return true;
            }
            catch (Exception ex)
            {
#if !UNITY_2021_3_OR_NEWER
                Log.Fatal(ex, "Error while ensuring packets: ");
#else
            Debug.LogError($"Error while ensuring packets:\n{ex.Message}\n{ex.StackTrace}");
#endif
            }
            return false;
        }

        public static NetworkPacketBase Create(NetworkPacketEnum type)
        {
            if (_registry.TryGetValue(type, out Func<NetworkPacketBase>? factory))
            {
                return factory!();
            }
            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}