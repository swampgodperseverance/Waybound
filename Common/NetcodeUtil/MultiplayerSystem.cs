using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;

namespace Waybound.Common.NetCodeUtil;

sealed class MultiplayerSystem : ModSystem {
    public static MultiplayerSystem Instance => GetInstance<MultiplayerSystem>();

    private static readonly List<NetPacket> packets = [];
    private static readonly Dictionary<Type, NetPacket> packetsByType = [];

    public override void Load() {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(NetPacket)))) {
            NetPacket instance = (NetPacket)FormatterServices.GetUninitializedObject(type);
            instance.Id = packets.Count;
            packetsByType[type] = instance;
            packets.Add(instance);
            ContentInstance.Register(instance);
        }
    }
    public override void Unload() {
        packets?.Clear();
        packetsByType?.Clear();
    }
    // Get
    public static NetPacket GetPacket(byte id) => packets[id];
    public static NetPacket GetPacket(Type type) => packetsByType[type];
    public static T GetPacket<T>() where T : NetPacket => ModContent.GetInstance<T>();

    // Send
    public static void SendPacket<T>(T packet, int toClient = -1, int ignoreClient = -1, Func<Player, bool>? sendDelegate = null) where T : NetPacket {
        if (Main.netMode == NetmodeID.SinglePlayer) { return; };

        ModPacket modPacket = Instance.Mod.GetPacket();

        modPacket.Write((byte)packet.Id);
        packet.WriteAndDispose(modPacket);

        try {
            if (Main.netMode == NetmodeID.MultiplayerClient) { modPacket.Send(); }
            else if (toClient != -1) { modPacket.Send(toClient, ignoreClient); }
            else {
                for (int i = 0; i < Main.player.Length; i++) {
                    Player player = Main.player[i];
                    if (i != ignoreClient && Netplay.Clients[i].State >= 10 && (sendDelegate?.Invoke(player) ?? true)) { modPacket.Send(i); };
                }
            }
        }
        catch { }
    }

    internal static void HandlePacket(BinaryReader reader, int sender) {
        byte packetId = reader.ReadByte();
        if (packetId > packets.Count) { return; }
        NetPacket packet = packets[packetId];
        packet.Read(reader, sender);
    }
};