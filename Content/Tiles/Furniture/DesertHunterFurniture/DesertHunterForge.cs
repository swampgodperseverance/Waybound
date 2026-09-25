using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture.DesertHunterFurniture
{
	public class DesertHunterForge : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.LavaDeath = false;
			//TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed; ???????????????
			TileObjectData.addTile(Type);
			Main.tileSolidTop[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			HitSound = SoundID.Tink;
			DustType = ModContent.DustType<Dusts.DesertHunterDust>();

			AddMapEntry(new Color(96, 74, 74));
		}
		private readonly int AnimationFrameHeight = 54;
        private readonly int AnimationFrameWidth = 52;

		public override void KillMultiTile(int x, int y, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 48, 32, ModContent.ItemType<Items.Placeable.Furniture.DesertHunterFurniture.DesertHunterForgeI>());
		}
		/*public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) 
        {
        Tile tile = Framing.GetTileSafely(i, j);
            int uniqueAnimationFrame = Main.tileFrame[Type] + i;
                if (i % 2 == 0)
                    uniqueAnimationFrame += 2;
                if (i % 3 == 0)
                    uniqueAnimationFrame += 2;
                uniqueAnimationFrame %= 4;

            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }*/
	}
}