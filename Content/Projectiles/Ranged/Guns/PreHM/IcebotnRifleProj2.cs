using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Ranged.Guns.PreHM
{
    public class IcebornRifleProj2 : ModProjectile
    {
        private const int SpinDuration = 129;
        private const float InitialSpinSpeed = 0.62f;
        private const float MinSpinSpeed = 0.15f;
        private float currentSpinSpeed;
        private bool hasExploded;
        private Vector2 oldPos = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 0;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            currentSpinSpeed = InitialSpinSpeed;
            hasExploded = false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (!hasExploded)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (other.active &&
                        other.whoAmI != Projectile.whoAmI &&
                        other.friendly &&
                        other.owner == Projectile.owner &&
                        other.damage > 0 &&
                        other.DamageType == DamageClass.Ranged &&
                        other.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        hasExploded = true;
                        Projectile.Kill();
                        return;
                    }
                }
            }

            if (Projectile.ai[0] <= SpinDuration)
            {
                float progress = Projectile.ai[0] / SpinDuration;
                float spinEase = EaseFunctions.EaseOutCubic(progress);
                currentSpinSpeed = MathHelper.Lerp(InitialSpinSpeed, MinSpinSpeed, spinEase);
                Projectile.rotation += currentSpinSpeed * Projectile.direction;
                Projectile.velocity *= 0.982f;
                Projectile.velocity.Y += 0.14f;
                return;
            }

            float slowProgress = MathHelper.Clamp((Projectile.ai[0] - SpinDuration) / 70f, 0f, 1f);
            float slowEase = EaseFunctions.EaseOutCubic(slowProgress);

            Projectile.velocity *= MathHelper.Lerp(0.982f, 0.88f, slowEase);
            Projectile.velocity.Y += 0.14f * (1f - slowEase);

            currentSpinSpeed = MathHelper.Lerp(MinSpinSpeed, 0.008f, slowEase);
            Projectile.rotation += currentSpinSpeed * Projectile.direction;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;

            if (Projectile.ai[0] > SpinDuration + 70 && !hasExploded)
            {
                hasExploded = true;
                Projectile.Kill();
            }
        }

        private void SpawnBigBlueFlash()
        {
            for (int i = 0; i < 3; i++)
            {
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(11f, 15.5f)), 
                    color: new Color(180, 225, 255, 255),
                    duration: Main.rand.Next(16, 22)
                ));
            }

            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 22f);

                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: (Projectile.Center + offset).ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(6.5f, 9.5f)),
                    color: new Color(140, 205, 255, 240),
                    duration: Main.rand.Next(18, 26)
                ));
            }

            // === Мягкое внешнее свечение (очень большое и прозрачное) ===
            for (int i = 0; i < 4; i++)
            {
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(14f, 19f)),
                    color: new Color(160, 220, 255, 140),
                    duration: Main.rand.Next(12, 17)
                ));
            }
        }

        private void SpawnSnowFlakes(int count, float scaleMult, float speedMult = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(42f, 42f);
                Vector2 vel = Main.rand.NextVector2Circular(6.0f, 6.0f) * speedMult
                              - Vector2.UnitY * Main.rand.NextFloat(1.6f, 4.5f) * speedMult;
                ParticleSystem.SnowFlakeBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: vel.ToNumerics(),
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(3.0f, 5.4f) * scaleMult),
                    color: Color.White * Main.rand.NextFloat(0.95f, 1.3f),
                    duration: Main.rand.Next(40, 60)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SpawnBigBlueFlash();

                SpawnSnowFlakes(42, 3.9f, 1.25f);

                for (int i = 0; i < 14; i++)  
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 vel = Main.rand.NextVector2Circular(7.5f, 7.5f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, new Color(100, 190, 255), Main.rand.NextFloat(1.5f, 2.3f));
                    d.noGravity = true;
                }

                for (int i = 0; i < 8; i++)   
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(22f, 22f);
                    Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Frost, vel, 60, new Color(160, 220, 255), Main.rand.NextFloat(1.2f, 1.7f));
                    d.noGravity = true;
                }
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.95f, Pitch = -0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.55f, Pitch = 0.15f }, Projectile.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = lightColor;

            if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.16f);

            if (oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                float trailAlpha = 0.7f;
                Color trailColor = new Color(70, 170, 255, 0) * trailAlpha;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 5.8f;

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.9f, trailScaleY),
                    SpriteEffects.None,
                    0f
                );

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.5f,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.5f, trailScaleY * 1.4f),
                    SpriteEffects.None,
                    0f
                );
            }

            Color outline = new Color(90, 180, 255) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.8f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i);
                Main.EntitySpriteDraw(
                    texture,
                    drawPos + offset,
                    null,
                    outline,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0f
                );
            }

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                drawColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );
            return false;
        }
    }
}