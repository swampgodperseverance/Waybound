using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture
{
	public class ScorchedWoodBathtub : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(115, 115, 120), CreateMapEntryName());
			DustType = DustID.Ash;
			AdjTiles = [TileID.Bathtubs];
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
            r = 0.15f;
            g = 0.15f;
            b = 0.18f;
        }

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	}
}