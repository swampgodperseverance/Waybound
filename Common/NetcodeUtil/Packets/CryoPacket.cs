//using System.IO;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ID;
//using Waybound.Common.Water;

//namespace Waybound.Common.NetCodeUtil.Packets;

//internal sealed class CryoPacket : NetPacket
//{
//    private byte playerIndex;
//    private Vector2 destination;

//    public CryoPacket() { }

//    public CryoPacket(byte playerIndex, Vector2 destination)
//    {
//        this.playerIndex = playerIndex;
//        this.destination = destination;

//        Writer.Write(playerIndex);
//        Writer.WriteVector2(destination);
//    }

//    public override void Read(BinaryReader reader, int sender)
//    {
//        byte idx = reader.ReadByte();
//        Vector2 dest = reader.ReadVector2();

//        if (Main.netMode == NetmodeID.Server)
//        {
//            MultiplayerSystem.SendPacket(
//                new CryoPacket(idx, dest),
//                -1,
//                sender
//            );
//            return;
//        }

//        Player target = Main.player[idx];
//        if (target.active)
//        {
//            CryoWaterTeleportPlayer modPlayer = target.GetModPlayer<CryoWaterTeleportPlayer>();
//            Vector2 oldPos = target.Center;
//            modPlayer.ExecuteTeleport(oldPos, dest);
//        }
//    }
//}