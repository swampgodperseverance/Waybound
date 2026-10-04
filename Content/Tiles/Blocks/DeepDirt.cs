using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Plants;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepDirt : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
            Main.tileMerge[Type][ModContent.TileType<DeepStone>()] = true;
            Main.tileMerge[Type][ModContent.TileType<DeepGrass>()] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			AddMapEntry(new Color(37, 53, 62));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepDirtItem>();
			HitSound = SoundID.Dig;
            MinPick = 40;
            MineResist = 1f;
			DustType = ModContent.DustType<DeepDirtDust>();
		}

		public override void RandomUpdate(int i, int j)
		{
			if (!Main.tile[i, j + 1].HasTile && Main.tile[i, j].Slope == 0 && Main.rand.NextBool(150))
			{
				WorldGen.PlaceTile(i, j + 1, (ushort)ModContent.TileType<DeepbloomVine>());
			}
		}
	}
}