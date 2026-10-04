using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.NPCs.Bosses.DeepStoneGolem;

namespace Waybound.Content.Tiles.BossSummon
{
	public class DeepSecurityPillar : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);
			Main.tileSolidTop[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			HitSound = SoundID.Tink;

			MineResist = 3f;
			MinPick = 40;
			DustType = ModContent.DustType<DeepStoneDust>();

			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Security Pillar");
			AddMapEntry(new Color(68, 94, 100), name);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Player player = Main.LocalPlayer;

			int type = ModContent.NPCType<DeepStoneGolem>();
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.SpawnOnPlayer(player.whoAmI, type);
			}
			else
			{
				NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
			}
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

		public override bool CanExplode(int i, int j)
		{
			return true;
		}
	}
}