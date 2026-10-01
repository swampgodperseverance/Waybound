using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
	public class DeepWoodPiano : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Wood Piano");
			AddMapEntry(new Color(96, 74, 74), name);

			DustType = ModContent.DustType<DeepTreeDust>();
			AdjTiles = new int[] { TileID.Pianos };
		}

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.60f;
            g = 0.375f;
            b = 0.36f;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	}
}