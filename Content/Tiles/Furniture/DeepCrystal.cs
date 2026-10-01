using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Plants;

namespace Waybound.Content.Tiles.Furniture
{
	public class DeepCrystal : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<DeepCrystal2>()] = true;
			AddMapEntry(new Color(245, 82, 97));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepCrystalItem>();
			HitSound = SoundID.Tink;

			MineResist = 2f;
			MinPick = 110;
			DustType = ModContent.DustType<DeepMagicDust>();
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override bool CanExplode(int i, int j) => false;

		public override void RandomUpdate(int i, int j)
		{
			if (!Main.tile[i, j - 1].HasTile && Main.rand.NextBool(25))
			{
				WorldGen.PlaceTile(i, j - 1, (ushort)ModContent.TileType<DeepCrystalShard>());
			}

			if (!Main.tile[i, j - 1].HasTile
				&& !Main.tile[i, j - 2].HasTile
				&& !Main.tile[i + 1, j - 1].HasTile
				&& !Main.tile[i + 1, j - 2].HasTile
				&& Main.tile[i + 1, j].HasTile
				&& Main.tile[i + 1, j].TileType == Main.tile[i, j].TileType
				&& Main.tile[i, j].Slope == 0
				&& Main.tile[i + 1, j].Slope == 0
			 	&& Main.rand.NextBool(20))
			{
				WorldGen.PlaceObject(i, j - 1, (ushort)ModContent.TileType<CrystalMushroom>(), true, 0, -1, -1);
			}
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Player player = Main.LocalPlayer;
			Vector2 pos = player.Center / 16f;
			float dist1 = Vector2.Distance(new Vector2(pos.X, pos.Y + 1), new Vector2(i + 0.5f, j + 0.5f));
			float dist2 = Vector2.Distance(new Vector2(pos.X, pos.Y - 1), new Vector2(i + 0.5f, j + 0.5f));
			if (dist1 < 1.3f || dist2 < 1.3f)
			{
				player.AddBuff(ModContent.BuffType<Buffs.Debuffs.DeepFire>(), 125);
			}
		}

		/*public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int height = tile.TileFrameY == 36 ? 18 : 16;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>("VictimaMod2/Content/Tiles/deepCrystal").Value, new Vector2((i * 16) - (int)Main.screenPosition.X, (j * 16) - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height), Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }*/

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}
	}
}