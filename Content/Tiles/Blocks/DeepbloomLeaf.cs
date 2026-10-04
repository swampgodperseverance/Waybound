using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Plants;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepbloomLeaf : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(229, 207, 124));
			HitSound = SoundID.Grass;
			

			MineResist = 0.1f;
			DustType = ModContent.DustType<DeepLeafDust>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 1.00f; //229
			g = 0.90f; //207
			b = 0.54f; //124
		}

		public override bool CanDrop(int i, int j)
		{
			return false;
		}

		public override void RandomUpdate(int i, int j)
		{
			if (!Main.tile[i, j + 1].HasTile && Main.rand.NextBool(50))
			{
				WorldGen.PlaceTile(i, j + 1, (ushort)ModContent.TileType<DeepbloomVine>());
			}
		}
	}
}