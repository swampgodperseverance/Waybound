using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Furniture
{
	public class DeepCrystal2 : ModTile
	{
		public override string Texture => "Waybound/Content/Tiles/Furniture/DeepCrystal";

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<DeepCrystal>()] = true;
			AddMapEntry(new Color(245, 82, 97));
			// ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<deepCrystalItem>();
			HitSound = SoundID.Item27;

			MineResist = 2f;
			MinPick = 10000;
			DustType = ModContent.DustType<DeepMagicDust>();
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override bool CanExplode(int i, int j) => false;

		public override bool Slope(int i, int j) => false;

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

        public override void NearbyEffects(int i, int j, bool closer)
		{
			Player player = Main.LocalPlayer;
			Vector2 pos = player.Center / 16f;
			float dist1 = Vector2.Distance(new Vector2(pos.X, pos.Y + 1), new Vector2(i + 0.5f, j + 0.5f));
			float dist2 = Vector2.Distance(new Vector2(pos.X, pos.Y - 1), new Vector2(i + 0.5f, j + 0.5f));
			if (dist1 < 1.4f || dist2 < 1.4f)
			{
				player.AddBuff(ModContent.BuffType<DeepFire>(), 125);
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}
	}
}