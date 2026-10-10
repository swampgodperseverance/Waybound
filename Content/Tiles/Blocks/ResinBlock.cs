using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Plants;

namespace Waybound.Content.Tiles.Blocks
{
	public class ResinBlock : ModTile
	{
		public override void SetStaticDefaults()
		{
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileMerge[Type][TileID.Stone] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			AddMapEntry(new Color(37, 53, 62));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepDirtItem>();
			HitSound = SoundID.Dig;
            MinPick = 40;
            MineResist = 1f;
		}

	}
}