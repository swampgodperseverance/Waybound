using System.IO;
using Terraria;
using Terraria.ID;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.NetCodeUtil;
using Waybound.Helpers;

namespace Waybound.Common.NetcodeUtil.Packets;

internal sealed class ThunderSigilDodge : NetPacket {
    public ThunderSigilDodge(Player player) => Writer.TryWriteSenderPlayer(player);

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out Player player)) { return; };
        player.GetModPlayer<ThunderSigilPlayer>().DodgeEffect();
        if (Main.netMode == NetmodeID.Server) { MultiplayerSystem.SendPacket(new ThunderSigilDodge(player), ignoreClient: sender); };
    }
};