using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Tiles.Furniture
{
	public class DeepBrickFireplace : ModTile
	{
		public override void SetStaticDefaults()
		{
            Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Brick Fireplace");
			AddMapEntry(new Color(96, 74, 74), name);

			DustType = ModContent.DustType<DeepStoneDust>();
			AdjTiles = new int[] { TileID.Tables };
		}
        
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 54)
            {
                r = 0.80f;
                g = 0.51f;
                b = 0.56f;
            }
        }

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 64, ModContent.ItemType<DeepBrickFireplaceI>());

		public override void HitWire(int i, int j)
        {
            int left = i - Main.tile[i, j].TileFrameX / 18 % 3;
            int top = j - Main.tile[i, j].TileFrameY / 18 % 2;
            for (int x = left; x < left + 3; x++)
            {
                for (int y = top; y < top + 2; y++)
                {

                    if (Main.tile[x, y].TileFrameX >= 54)
                        Main.tile[x, y].TileFrameX -= 54;
                    else
                        Main.tile[x, y].TileFrameX += 54;
                }
            }
            if (Wiring.running)
            {
                Wiring.SkipWire(left, top);
                Wiring.SkipWire(left, top + 1);
                Wiring.SkipWire(left + 1, top);
                Wiring.SkipWire(left + 1, top + 1);
            }
            NetMessage.SendTileSquare(-1, left, top + 1, 2);
        }
	}
}

