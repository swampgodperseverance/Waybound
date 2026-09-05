using System.IO;
using Terraria;
using Terraria.ID;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.NetCodeUtil;
using Waybound.Helpers;

namespace Waybound.Common.NetcodeUtil.Packets;

internal sealed class ThunderSigilActiveEffect : NetPacket {
    public ThunderSigilActiveEffect(Player player, bool value) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(value);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out Player player)) { return; };
        if (!reader.TryReadBoolean(out bool value)) { return; };
        player.GetModPlayer<ThunderSigilPlayer>().activeEffect = value;
        if (Main.netMode == NetmodeID.Server) { MultiplayerSystem.SendPacket(new ThunderSigilActiveEffect(player, value), ignoreClient: sender); }
    }
}