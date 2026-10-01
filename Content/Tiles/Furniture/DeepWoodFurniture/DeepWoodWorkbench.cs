using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
	public class DeepWoodWorkbench : ModTile
	{
		public override void SetStaticDefaults()
		{
			// Properties
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;

			DustType = DustID.t_BorealWood;
			AdjTiles = new int[] { TileID.WorkBenches };

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = new[] { 18 };
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

			// Etc
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Wood Work Bench");
			AddMapEntry(new Color(96, 74, 74), name);
		}

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.60f;
            g = 0.375f;
            b = 0.36f;
        }

        public override void NumDust(int x, int y, bool fail, ref int num) => num = fail ? 1 : 3;


	}
}