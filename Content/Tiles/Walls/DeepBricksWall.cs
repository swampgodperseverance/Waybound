using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Walls
{
	public class DeepBricksWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			AddMapEntry(new Color(43, 51, 65));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepBricksWallItem>();
			DustType = ModContent.DustType<DeepStoneDust>();
		}
	}
}