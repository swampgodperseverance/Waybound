using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepWoodBeam : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			TileID.Sets.IsBeam[Type] = true;
			AddMapEntry(new Color(96, 74, 74));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepBeamItem>();
			HitSound = SoundID.Dig;

			MineResist = 2f;
			MinPick = 35;
			DustType = ModContent.DustType<DeepTreeDust>();
		}
	}
}