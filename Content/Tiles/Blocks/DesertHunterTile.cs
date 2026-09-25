using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace Waybound.Content.Tiles.Blocks
{
	public class DesertHunterTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
			Main.tileMerge[Type][ModContent.TileType<DesertHunterBlock>()] = true;
			Main.tileMerge[ModContent.TileType<DesertHunterBlock>()][Type] = true;
			Main.tileBlendAll[Type] = true;

			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;


			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			AddMapEntry(new Color(120, 171, 191));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<desertHunterTileI>();
			HitSound = SoundID.Dig;

			MineResist = 1f;
			MinPick = 35;
			DustType = ModContent.DustType<Dusts.DesertHunterDust>();
		}
		public override void RandomUpdate(int i, int j)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            Tile tileBelow2 = Framing.GetTileSafely(i, j + 2);
            Tile tileAbove = Framing.GetTileSafely(i, j - 1);
		}
	}
}