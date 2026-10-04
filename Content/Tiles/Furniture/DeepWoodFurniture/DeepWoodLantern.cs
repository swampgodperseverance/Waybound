using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
	public class DeepWoodLantern : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
            TileObjectData.newSubTile.LavaDeath = false;
            TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.Allowed;
            TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Wood Lantern");
			AddMapEntry(new Color(96, 74, 74), name);

			DustType = ModContent.DustType<DeepTreeDust>();
			AdjTiles = new int[] { TileID.HangingLanterns };
		}

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 16)
            {
                r = 1.20f;
                g = 0.75f;
                b = 0.84f;
            }
        }

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;


		public override void HitWire(int i, int j)
        {
            int left = i - Main.tile[i, j].TileFrameX / 18 % 1;
            int top = j - Main.tile[i, j].TileFrameY / 18 % 2;
            for (int x = left; x < left + 1; x++)
            {
                for (int y = top; y < top + 2; y++)
                {

                    if (Main.tile[x, y].TileFrameX >= 18)
                        Main.tile[x, y].TileFrameX -= 18;
                    else
                        Main.tile[x, y].TileFrameX += 18;
                }
            }
            if (Wiring.running)
            {
                Wiring.SkipWire(left, top);
                Wiring.SkipWire(left, top + 1);
            }
            NetMessage.SendTileSquare(-1, left, top + 1, 2);
        }
	}
}