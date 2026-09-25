using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Ores;
using Waybound.Particles;

namespace Waybound.Content.Tiles.Ores
{
	public class HielitiumOre : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 410;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 500;
			Main.tileMergeDirt[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Hielitium Ore");
			AddMapEntry(new Color(119, 158, 201), name);

			DustType = 13;
			//ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<hielitiumOreItem>();
			HitSound = SoundID.Tink;

			MineResist = 1.5f;
			MinPick = 70;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly || Main.netMode == NetmodeID.Server)
                return;

            Vector2 pos = new Vector2(i * 16 + 8, j * 16 + 8);

            for (int k = 0; k < 14; k++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.2f, 3.2f);
                float size = Main.rand.NextFloat(16f, 30f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(40, 90, 160, 220) * Main.rand.NextFloat(0.85f, 1.15f),
                    Main.rand.Next(18, 32)
                ));
            }
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.dedServ)
                return;

            if (!Main.rand.NextBool(28))
                return;

            Vector2 worldPos = new Vector2(i * 16 + 8, j * 16 + 8);
            Vector2 vel = new Vector2(
                Main.rand.NextFloat(-0.25f, 0.25f),
                Main.rand.NextFloat(-1.8f, -1.0f)
            );

            float scale = Main.rand.NextFloat(4f, 7.5f);

            ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                worldPos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(scale),
                new Color(40, 90, 160, 220) * 0.85f,
                Main.rand.Next(90, 130)
            ));
        }
    }
}