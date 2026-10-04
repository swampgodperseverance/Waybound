using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Blocks;

public class DeepWood : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileMergeDirt[Type] = false;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		AddMapEntry(new Color(96, 74, 74));
		// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepTreeItem>();
		HitSound = SoundID.Dig;

		MineResist = 2f;
		MinPick = 35;
		DustType = ModContent.DustType<DeepTreeDust>();
	}
}