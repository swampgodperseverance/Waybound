using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Waybound.Common.Water;

namespace Waybound.Common.NetCodeUtil.Packets;

sealed class NPCExtraValuePacket : NetPacket
{

    public NPCExtraValuePacket(int whoAmI, int extraValue)
    {
        Writer.Write(whoAmI);
        Writer.Write(extraValue);
    }

    public override void Read(BinaryReader reader, int sender)
    {
        int whoAmI = reader.ReadInt32();
        int extraValue = reader.ReadInt32();
        if (whoAmI >= 0 && whoAmI <= 200)
        {
            Main.npc[whoAmI].extraValue = extraValue;
            Main.npc[whoAmI].moneyPing(Main.npc[whoAmI].Center + VelocityToPoint(Main.npc[whoAmI].Center, Main.player[Main.npc[whoAmI].target].Center, 25f));
        }
    }
    public static Vector2 VelocityToPoint(Vector2 a, Vector2 b, float speed)
    {
        Vector2 vector2 = b - a;
        Vector2 velocity = vector2 * (speed / vector2.Length());
        return !velocity.HasNaNs() ? velocity : Vector2.Zero;
    }
}