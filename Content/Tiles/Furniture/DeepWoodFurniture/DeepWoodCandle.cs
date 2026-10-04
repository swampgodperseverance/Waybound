using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
	public class DeepWoodCandle : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 20 };
			TileObjectData.newTile.DrawYOffset = -4;
            TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Wood Candle");
			AddMapEntry(new Color(96, 74, 74), name);

			DustType = ModContent.DustType<DeepTreeDust>();
			AdjTiles = new int[]{ TileID.Candles };
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 18)
            {
                r = 1.20f;
                g = 0.75f;
                b = 0.84f;
            }
        }
		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;


		 public override void HitWire(int i, int j)
        {
            if (Main.tile[i, j].TileFrameX >= 18)
                Main.tile[i, j].TileFrameX -= 18;
            else
                Main.tile[i, j].TileFrameX += 18;
        }
        public override bool RightClick(int i, int j)
        {
            if (Main.tile[i, j].TileFrameX >= 18)
                Main.tile[i, j].TileFrameX -= 18;
            else
                Main.tile[i, j].TileFrameX += 18;
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<DeepWoodCandleI>();
        }

	}
}