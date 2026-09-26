using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.NPCs.Bosses.Themis
{
    public class SickleOfThemis : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            DrawOriginOffsetY = -6;
        }

        private int DashCount { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private int Timer { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }

        private float outlinePulse = 0f;
        private float rotationSpeed = 4f;

        public override void AI()
        {
            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);

            rotationSpeed = MathHelper.Lerp(rotationSpeed, 4f + Projectile.velocity.Length() * 1.5f, 0.1f);
            Projectile.rotation += MathHelper.ToRadians(rotationSpeed);
            Timer++;

            if (DashCount >= 3 && Timer > 40)
            {
                Projectile.velocity *= 0.94f;
                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.12f);
                Projectile.alpha += 8;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else if (Timer < 40)
            {
                Projectile.velocity *= 0.85f;
                outlinePulse = (float)Math.Sin(Timer * 0.15f) * (Timer / 40f);
            }
            else if (Timer == 40 && DashCount < 3)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 targetDir = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = targetDir.RotatedByRandom(MathHelper.ToRadians(5f)) * 22f;
                SpawnShortFlash();
                DashCount++;
                Projectile.netUpdate = true;
            }
            else if (Timer > 40)
            {
                Projectile.velocity *= 0.96f;
                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.1f);
                if (Projectile.velocity.Length() < 2f && DashCount < 3)
                    Timer = 0;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                int rocketType = ModContent.ProjectileType<ThemisRocket>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.whoAmI == Projectile.whoAmI)
                        continue;

                    if (other.type == rocketType && Projectile.Hitbox.Intersects(other.Hitbox))
                    {
                        other.Kill();
                        SpawnShortFlash();
                        SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.45f, Pitch = 0.2f }, Projectile.Center);
                    }

                    if (other.type == Projectile.type && Projectile.whoAmI < other.whoAmI && Projectile.Hitbox.Intersects(other.Hitbox))
                    {
                        SpawnBigGreyFlash();
                        SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.15f, 0.1f) }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.2f, 0.15f) }, Projectile.Center);

                        Vector2 bounceDir = (Projectile.Center - other.Center).SafeNormalize(Vector2.UnitX);
                        Projectile.velocity = bounceDir * 12f;
                        other.velocity = -bounceDir * 12f;
                        Timer = 0;
                        if (other.ModProjectile is SickleOfThemis otherSickle)
                            otherSickle.Timer = 0;
                        Projectile.netUpdate = true;
                        other.netUpdate = true;
                    }
                }
            }
        }

        private void SpawnShortFlash()
        {
            for (int i = 0; i < 3; i++)
            {
                // Размеры уменьшены почти в два раза
                float s = 30f + i * 12f;
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(s + Main.rand.NextFloat(-3f, 3f)),
                    color: Color.White,
                    duration: 12 + i));
            }
        }

        private void SpawnBigGreyFlash()
        {
            for (int i = 0; i < 4; i++)
            {
                float s = 90f + i * 28f;
                int alpha = 210 - i * 35;
                if (alpha < 0) alpha = 0;

                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(s + Main.rand.NextFloat(-8f, 8f)),
                    color: new Color(160, 160, 160, alpha),
                    duration: 20 + i * 2
                ));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            float fade = (255f - Projectile.alpha) / 255f;

            if (fade <= 0.01f)
                return false;

            if (outlinePulse > 0.05f)
            {
                Color outlineColor = new Color(255, 140, 25, 0) * outlinePulse * 0.6f * fade;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 circularOffset = new Vector2(4f * outlinePulse, 0f).RotatedBy(i * MathHelper.PiOver2);
                    Vector2 outlinePos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + circularOffset;
                    Main.EntitySpriteDraw(
                        texture,
                        outlinePos,
                        null,
                        outlineColor,
                        Projectile.rotation,
                        drawOrigin,
                        Projectile.scale * (1f + outlinePulse * 0.1f),
                        SpriteEffects.None,
                        0
                    );
                }
            }

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float trailFade = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * trailFade * 0.4f * fade;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    color,
                    Projectile.oldRot[k],
                    drawOrigin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }

            Vector2 mainPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.EntitySpriteDraw(
                texture,
                mainPos,
                null,
                Projectile.GetAlpha(lightColor) * fade,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
