using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Walls;

namespace Waybound.Content.Tiles.Walls
{
	public class DesertHunterBlockWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			AddMapEntry(new Color(44, 22, 65));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<desertHunterBlockWallI>();
			DustType = ModContent.DustType<Dusts.DesertHunterDust>();
		}
	}
}