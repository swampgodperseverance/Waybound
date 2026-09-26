using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Particles;

namespace Waybound.Content.NPCs.Bosses.Themis
{
    public class ThemisRocket : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            DrawOriginOffsetY = -3;
            Projectile.alpha = 255;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[1] = MathHelper.ToRadians(Main.rand.NextFloat(-90, 90));
            base.OnSpawn(source);
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0) Projectile.alpha = 0;
            }

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            Projectile.ai[0]++;

            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 43)
                Projectile.tileCollide = true;

            if (Projectile.ai[0] < 10)
                Projectile.velocity = (Projectile.velocity + (Projectile.velocity.RotatedBy(Projectile.ai[1])).SafeNormalize(Vector2.UnitX) * 1f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            else if (Projectile.ai[0] < 40)
                Projectile.velocity = (Projectile.velocity + (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 1.5f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            Projectile.velocity *= 1.015f;

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 8f);
                Vector2 vel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f);

                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(18f, 30f)),
                    new Color(255, 140, 40, 220),
                    Main.rand.Next(24, 40)
                ));

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

            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 perturbedSpeed = new Vector2(0, 0);
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<ExplosionHostile>(), 20, 4, Main.myPlayer);
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 18; i++)
                {
                    ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        Main.rand.NextVector2Circular(5f, 5f).ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(22f, 40f)),
                        new Color(255, 130, 30, 230),
                        Main.rand.Next(30, 55)
                    ));
                }

                for (int i = 0; i < 22; i++)
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        Main.rand.NextVector2Circular(8f, 8f).ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(16f, 28f)),
                        new Color(255, 120, 30, 210),
                        Main.rand.Next(22, 40)
                    ));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, Projectile.height / 2);

            float currentAlpha = (255f - Projectile.alpha) / 255f;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);

                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;

                Color color = Projectile.GetAlpha(lightColor) * progress * 0.5f * currentAlpha;
                float trailScale = Projectile.scale * progress;

                spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, trailScale, SpriteEffects.None, 0f);
            }

            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            spriteBatch.Draw(texture, mainDrawPos, null, Projectile.GetAlpha(lightColor) * currentAlpha, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
