using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Blocks;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepTree : ModTile
	{
		
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			AddMapEntry(new Color(96, 74, 74));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepTreeItem>();
			HitSound = SoundID.Dig;
			
			RegisterItemDrop(ModContent.ItemType<DeepTreeItem>(), 0, 1, 2, 3, 4);


			MineResist = 1.5f;
			DustType = ModContent.DustType<DeepTreeDust>();
		}

	}
}