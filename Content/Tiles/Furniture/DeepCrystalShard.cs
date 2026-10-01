using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
namespace Waybound.Content.Tiles.Furniture
{
	public class DeepCrystalShard : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.addTile(Type);
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(245, 82, 97));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepCrystalShardItem>();
			HitSound = SoundID.Item27;

			DustType = ModContent.DustType<DeepMagicDust>();
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
			if ((i % 10) < 5)
			{
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
		{
			frameXOffset = i % 5 * 18;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 2;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f; //255
			g = 0.41f; //145
			b = 0.46f; //132
		}
	}
}