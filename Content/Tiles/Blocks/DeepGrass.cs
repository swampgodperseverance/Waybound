using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable;
using Waybound.Content.Items.Placeable.Blocks;

namespace Waybound.Content.Tiles.Blocks
{
	public class DeepGrass : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			AddMapEntry(new Color(32, 143, 72));
			HitSound = SoundID.Dig;
            MinPick = 40;
            MineResist = 1.25f;
			DustType = ModContent.DustType<DeepStoneDust>();
            // ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepDirtItem>();
            // RegisterItemDrop(deepDirtItem, 3);
            // TODO: Add this to the deepDirtItem
            RegisterItemDrop(ModContent.ItemType<DeepDirtItem>(), 0, 1, 2, 3, 4);
	            
            Main.tileMerge[Type][ModContent.TileType<DeepDirt>()] = true;

			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<DeepDirt>();
			TileID.Sets.Conversion.Grass[Type] = true;
		}
		
		

		public override void RandomUpdate(int i, int j)
		{
			List<Point> adjacents = OpenAdjacents(i, j, (ushort)ModContent.TileType<DeepDirt>());
			if (adjacents.Count > 0)
			{
				for (int k = 0; k < adjacents.Count; ++k)
				{
					Point p = adjacents[k];
					if (HasOpening(p.X, p.Y) && Main.rand.Next(100) < 75)
					{
						Framing.GetTileSafely(p.X, p.Y).TileType = (ushort)ModContent.TileType<DeepGrass>();
						if (Main.netMode == NetmodeID.Server)
							NetMessage.SendTileSquare(-1, p.X, p.Y, 1, TileChangeType.None);
					}
				}
			}
		}

		private List<Point> OpenAdjacents(int i, int j, ushort type)
		{
			var p = new List<Point>();
			for (int k = -1; k < 2; ++k)
				for (int l = -1; l < 2; ++l)
					if (!(l == 0 && k == 0) && Framing.GetTileSafely(i + k, j + l).HasTile && Framing.GetTileSafely(i + k, j + l).TileType == type)
						p.Add(new Point(i + k, j + l));
			return p;
		}

		private bool HasOpening(int i, int j)
		{
			for (int k = -1; k < 2; ++k)
				for (int l = -1; l < 2; ++l)
					if (!Framing.GetTileSafely(i + k, j + l).HasTile)
						return true;
			return false;
		}
    }
}