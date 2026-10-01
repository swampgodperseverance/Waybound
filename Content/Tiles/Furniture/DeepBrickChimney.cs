using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Tiles.Furniture
{
	public class DeepBrickChimney : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Brick Chimney");
			AddMapEntry(new Color(96, 74, 74), name);

			DustType = ModContent.DustType<DeepStoneDust>();
		}

        private readonly int AnimationFrameHeight = 56;
        private readonly int AnimationFrameWidth = 54;



        

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 64, ModContent.ItemType<DeepBrickChimneyI>());

		public override void HitWire(int i, int j)
        {
            int left = i - Main.tile[i, j].TileFrameX / 18 % 3;
            int top = j - Main.tile[i, j].TileFrameY / 18 % 3;
            for (int x = left; x < left + 3; x++)
            {
                for (int y = top; y < top + 3; y++)
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
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) 
        {
        Tile tile = Framing.GetTileSafely(i, j);
            int uniqueAnimationFrame = Main.tileFrame[Type] + i;
                if (i % 2 == 0)
                    uniqueAnimationFrame += 3;
                if (i % 3 == 0)
                    uniqueAnimationFrame += 3;
                if (i % 4 == 0)
                    uniqueAnimationFrame += 3;
                uniqueAnimationFrame %= 6;

            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }
        public override void AnimateTile(ref int frame, ref int frameCounter) 
        {
			frame = Main.tileFrame[TileID.Chimney];
		}
	}
}

