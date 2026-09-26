using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Accessories;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Shields
{
    public class ShieldOfDesertHunterP : ModProjectile
    {
        public int animationTimer;
        public int scaleCounter = 0;

        private const int FadeInTime = 30;
        private const int FadeOutTime = 30;

        private int spawnTimer = 0;
        private bool dying = false;
        private int deathTimer = 0;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Minotaur Shield");
        }

        public override void SetDefaults()
        {
            Projectile.width = 68;
            Projectile.height = 72;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Main.projFrames[Projectile.type] = 8;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!dying && !player.HasBuff(ModContent.BuffType<ShieldOfDesertHunterB>()))
            {
                dying = true;
                deathTimer = FadeOutTime;
            }

            // Fade in
            if (!dying && spawnTimer < FadeInTime)
            {
                spawnTimer++;

                float t = spawnTimer / (float)FadeInTime;
                Projectile.alpha = (int)MathHelper.Lerp(255f, 75f, t);
                Projectile.scale = MathHelper.Lerp(0.6f, 1f, t);

                SpawnAppearParticles(t);
            }
            else if (!dying)
            {
                // Normal breathing scale (only after fade in finishes)
                Projectile.ai[1]++;

                if (Projectile.ai[1] > 1)
                {
                    if (Projectile.scale <= 1.25f && scaleCounter == 0)
                    {
                        Projectile.scale += 0.005f;
                    }
                    else if (Projectile.scale >= 1.1f && scaleCounter == 1)
                    {
                        Projectile.scale -= 0.005f;
                    }
                    Projectile.ai[1] = 0;
                }

                if (Projectile.scale >= 1.25f)
                {
                    scaleCounter = 1;
                }
                else if (Projectile.scale <= 1.1f)
                {
                    scaleCounter = 0;
                }
            }

            // Fade out
            if (dying)
            {
                deathTimer--;

                float t = deathTimer / (float)FadeOutTime;
                Projectile.alpha = (int)MathHelper.Lerp(255f, 75f, t);
                Projectile.scale = MathHelper.Lerp(1.25f, 0.5f, 1f - t);

                SpawnDisappearParticles(1f - t);

                if (deathTimer <= 0)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Projectile.Center = player.Center;

            if (Projectile.frame == 0)
            {
                animationTimer = 40;
            }
            else
            {
                animationTimer = 5;
            }

            if (++Projectile.frameCounter >= animationTimer)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            if (!dying)
                SpawnOutlineParticles();
        }

        private void SpawnAppearParticles(float t)
        {
            int count = (int)MathHelper.Lerp(2f, 6f, t);
            float radius = (Projectile.width * 0.5f) * Projectile.scale + Main.rand.NextFloat(2f, 8f);
            float fade = 1f - (Projectile.alpha / 255f);

            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dir = angle.ToRotationVector2();
                Vector2 pos = Projectile.Center + dir * radius;
                Vector2 vel = -dir * Main.rand.NextFloat(3f, 6f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 220, 150) * fade,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(4f, 10f)),
                        new Color(255, 180, 80, (byte)(200 * fade)),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
        }

        private void SpawnDisappearParticles(float t)
        {
            int count = (int)MathHelper.Lerp(6f, 2f, t);
            float radius = (Projectile.width * 0.5f) * Projectile.scale + Main.rand.NextFloat(2f, 8f);
            float fade = 1f - (Projectile.alpha / 255f);

            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dir = angle.ToRotationVector2();
                Vector2 pos = Projectile.Center + dir * radius;
                Vector2 vel = dir * Main.rand.NextFloat(2f, 6f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 180, 100) * fade,
                    Main.rand.NextFloat(0.2f, 0.45f),
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(6f, 14f)),
                        new Color(255, 140, 50, (byte)(200 * fade)),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
        }

        private void SpawnOutlineParticles()
        {
            if (Projectile.alpha > 200)
                return;

            int outlineCount = 4;
            float radius = (Projectile.width * 0.5f) * Projectile.scale + Main.rand.NextFloat(-2f, 3f);
            float fade = 1f - (Projectile.alpha / 255f);

            for (int i = 0; i < outlineCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 pos = Projectile.Center + offset;

                Vector2 vel = -angle.ToRotationVector2() * Main.rand.NextFloat(0.2f, 0.8f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 210, 130) * fade,
                    Main.rand.NextFloat(0.15f, 0.3f) * Projectile.scale,
                    1f
                );

                if (Main.rand.NextBool(3))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                        new Color(255, 180, 80, (byte)(180 * fade)),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int i = 0; i < 50; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 10f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(0f, Projectile.width * 0.5f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 200, 120),
                    Main.rand.NextFloat(0.2f, 0.45f),
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(6f, 14f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(10f, 20f)),
                    new Color(255, 120, 40, 220),
                    Main.rand.Next(20, 34)
                ));
            }

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width / 2, Projectile.height / 2, 31, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 120, default(Color), Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1f;
            }

            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X, Projectile.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), 61);
            }
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X, Projectile.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), 62);
            }
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X, Projectile.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), 63);
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}