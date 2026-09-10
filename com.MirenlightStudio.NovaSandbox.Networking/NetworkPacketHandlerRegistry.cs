using LiteNetLib;

#if !UNITY_2021_3_OR_NEWER
using Serilog;
#else
using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
#endif

namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public static class NetworkPacketHandlerRegistry
    {
        private static readonly Dictionary<NetworkPacketEnum, Action<NetworkPacketBase, NetPeer>?> _handlers = new();

        public static void Register(NetworkPacketEnum type, Action<NetworkPacketBase, NetPeer> handler)
        {
            _handlers[type] = handler;
        }

        public static void Unregister(NetworkPacketEnum type)
        {
            _handlers[type] = null;
        }

        public static void Handle(NetworkPacketEnum type, NetworkPacketBase packet, NetPeer peer)
        {
            if (_handlers.TryGetValue(type, out Action<NetworkPacketBase, NetPeer>? handler))
            {
                handler!.Invoke(packet, peer);
            }
            else
            {
#if !UNITY_2021_3_OR_NEWER
                Log.Error($"No handler registered for packet type: {type}");
#else
            Debug.LogError($"No handler registered for packet type: {type}");
#endif
            }
        }
    }
}