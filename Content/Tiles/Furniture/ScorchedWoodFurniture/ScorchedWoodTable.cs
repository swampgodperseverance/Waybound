using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture
{
	public class ScorchedWoodTable : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;

			AdjTiles = new int[] { TileID.Tables };

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.StyleHorizontal = true;
			AddMapEntry(new Color(120, 171, 191));
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

			LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Desert Hunter Table");
            AddMapEntry(new Color(115, 115, 120), name);
            DustType = DustID.Ash;
        }

		public override void NumDust(int x, int y, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

	}
}