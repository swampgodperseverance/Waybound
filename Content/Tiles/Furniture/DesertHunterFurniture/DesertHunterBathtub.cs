using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture.DesertHunterFurniture
{
	public class DesertHunterBathtub : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(96, 74, 74), CreateMapEntryName());
			DustType = ModContent.DustType<Dusts.DesertHunterDust>();
			AdjTiles = [TileID.Bathtubs];
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		//public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 64, ModContent.ItemType<Content.Items.Placeable.Furniture.DesertHunterFurniture.DesertHunterBathtubI>());
	}
}