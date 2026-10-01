using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.BossSummon;

namespace Waybound.Content.Tiles.BossSummon
{
	public class KronosSummonTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);
			Main.tileSolidTop[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			HitSound = SoundID.Tink;
			MinPick = 10000;
			DustType = ModContent.DustType<DeepMagicDust>();

			AddMapEntry(new Color(245, 82, 97));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 4;
		}

		//public override bool RightClick(int i, int j)
		//{
		//	Player player = Main.LocalPlayer;

		//	if (!NPC.AnyNPCs(ModContent.NPCType<kronos>()))
		//	{
		//		for (int b = 0; b < 58; b++)
		//		{
		//			if (player.inventory[b].type == ModContent.ItemType<CrystalbringerCore>() && player.inventory[b].stack > 0)
		//			{
		//				int type = ModContent.NPCType<kronos>();
		//				if (Main.netMode != NetmodeID.MultiplayerClient)
		//				{
		//					NPC.SpawnOnPlayer(player.whoAmI, type);
		//				}
		//				else
		//				{
		//					NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
		//				}

		//				player.inventory[b].stack--;
		//				break;
		//			}
		//		}
		//	}
		//	return true;
		//}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<CrystalbringerCore>();
		}

        public override bool CanExplode(int i, int j) => false;

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    }
}