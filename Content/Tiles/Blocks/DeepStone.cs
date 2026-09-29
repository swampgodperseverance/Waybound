using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepStone : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
            Main.tileMerge[Type][ModContent.TileType<DeepDirt>()] = true;
            AddMapEntry(new Color(68, 94, 100));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepStoneItem>();
			HitSound = SoundID.Tink;

			MineResist = 1.5f;
			MinPick = 40;
			DustType = ModContent.DustType<DeepStoneDust>();
		}
	}
}