using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Plants
{
	public class DeepbloomVine : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileCut[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(229, 207, 124));
			HitSound = SoundID.Grass;
			DustType = ModContent.DustType<DeepLeafDust>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 1.00f; //229
			g = 0.90f; //207
			b = 0.54f; //124
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 2;
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override void RandomUpdate(int i, int j)
		{
			if (!Main.tile[i, j - 1].HasTile
			|| Main.tile[i, j - 1].Slope != 0)
			{
				WorldGen.KillTile(i, j, noItem: true);
			}

			if (!Main.tile[i, j + 1].HasTile && Main.tile[i, j].Slope == 0 && Main.rand.NextBool(25))
			{
				WorldGen.PlaceTile(i, j + 1, (ushort)ModContent.TileType<DeepbloomVine>());
			}
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			if (!Main.tile[i, j - 1].HasTile
			|| Main.tile[i, j - 1].Slope != 0)
			{
				WorldGen.KillTile(i, j, noItem: true);
			}
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (!Main.tile[i, j - 1].HasTile
			|| Main.tile[i, j - 1].Slope != 0)
			{
				WorldGen.KillTile(i, j, noItem: true);
			}

			Player player = Main.LocalPlayer;
			Vector2 pos = player.Center / 16f;
			float dist1 = Vector2.Distance(new Vector2(pos.X, pos.Y + 1), new Vector2(i + 0.5f, j + 0.5f));
			float dist2 = Vector2.Distance(new Vector2(pos.X, pos.Y - 1), new Vector2(i + 0.5f, j + 0.5f));
			if (dist1 < 1 || dist2 < 1)
			{
				player.AddBuff(ModContent.BuffType<Caught>(), 20);
			}
		}
	}
}