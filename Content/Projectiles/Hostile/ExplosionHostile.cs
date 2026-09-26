using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Hostile
{
    public class ExplosionHostile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1.5f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 5;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.5f, 1f, 0.5f);
            if (Projectile.ai[0] > 0)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Player player = Main.player[Projectile.owner];
            player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, Projectile.Center)) / 1000;

            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f + Main.rand.NextFloat(-0.12f, 0.12f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 9.5f);
                Vector2 pos = Projectile.Center;

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    Color.White,
                    0.5f,
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(14f, 24f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6.5f);
                Vector2 pos = Projectile.Center;

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    Color.White,
                    0.5f,
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(14f, 24f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            for (int i = 0; i < Main.rand.Next(5, 8); i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.2f, 3.5f);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, Main.rand.Next(61, 64));
            }

            Projectile.ai[0] = 1;
        }
    }
}