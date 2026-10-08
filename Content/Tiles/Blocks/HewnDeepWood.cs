using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Blocks;

public class HewnDeepWood : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.ChecksForMerge[Type] = false;
		TileID.Sets.NeedsGrassFraming[Type] = true;
		TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.DirtiestBlock; // 0.000000000000000000001% merge
		Main.tileMergeDirt[Type] = false;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		AddMapEntry(new Color(96, 74, 74));
		HitSound = SoundID.Dig;

		MineResist = 2f;
		MinPick = 35;
		DustType = ModContent.DustType<DeepTreeDust>();
	}
}