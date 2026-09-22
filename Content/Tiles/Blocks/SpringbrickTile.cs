using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Tiles.Blocks {
  
        public class SpringbrickTile : ModTile
        {
            private static readonly Color GlowColor = new Color(120, 200, 255);

            public override void SetStaticDefaults()
            {
                TileID.Sets.HellSpecial[Type] = true;
                TileID.Sets.ChecksForMerge[Type] = true;
                Main.tileSolid[Type] = true;
                Main.tileBlockLight[Type] = true;
                AddMapEntry(new Color(120, 200, 255));
                DustType = DustID.Ice;
                HitSound = SoundID.Tink;
            }

            public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
            {
                if (Main.dedServ)
                    return;

                if (!Main.rand.NextBool(20))
                    return;

                Vector2 worldPos = new Vector2(i * 16 + 8, j * 16 + 8);

                Vector2 vel = new Vector2(
                    Main.rand.NextFloat(-0.35f, 0.35f),
                    Main.rand.NextFloat(-0.9f, -0.4f)
                );

                float scale = Main.rand.NextFloat(3.5f, 6.5f);

                ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                    worldPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    GlowColor * 0.8f,
                    60
                ));
            }
        }
    
}   
