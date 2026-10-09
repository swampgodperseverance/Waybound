using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Waybound.Content.NPCs.Bosses.Korochun;

namespace Waybound.Common.NetCodeUtil.Packets;

sealed class KorochunSyncPacket : NetPacket
{
    public KorochunSyncPacket(int whoAmI, Vector2 center, Vector2 velocity, Vector2 target, Vector2 targetVelocity, float side)
    {
        Writer.Write((short)whoAmI);
        Writer.WriteVector2(center);
        Writer.WriteVector2(velocity);
        Writer.WriteVector2(target);
        Writer.WriteVector2(targetVelocity);
        Writer.Write(side);
    }

    public override void Read(BinaryReader reader, int sender)
    {
        int whoAmI = reader.ReadInt16();
        Vector2 center = reader.ReadVector2();
        Vector2 velocity = reader.ReadVector2();
        Vector2 target = reader.ReadVector2();
        Vector2 targetVelocity = reader.ReadVector2();
        float side = reader.ReadSingle();

        if (Main.netMode != NetmodeID.MultiplayerClient) { return; }
        if (whoAmI < 0 || whoAmI >= Main.maxNPCs) { return; }
        NPC npc = Main.npc[whoAmI];
        if (!npc.active || npc.ModNPC is not Korochun boss) { return; }
        boss.ApplySync(center, velocity, target, targetVelocity, side);
    }
}