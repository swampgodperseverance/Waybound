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

namespace Waybound.Content.Projectiles.Ranged.Thrown
{
    public class AxeOfDisgraceProj : ModProjectile
    {
        private const int SpinDuration = 75;
        private const float InitialSpinSpeed = 0.55f;
        private const float MinSpinSpeed = 0.04f;
        private const float DropSpeed = 28f;
        private const float DropSpinSpeed = 0.72f;

        private float currentSpinSpeed;
        private bool hasDropped;
        private bool hasSpawnedPreDropParticles;
        private Vector2 oldPos = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 0;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            currentSpinSpeed = InitialSpinSpeed;
            hasDropped = false;
            hasSpawnedPreDropParticles = false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] <= SpinDuration)
            {
                float progress = Projectile.ai[0] / SpinDuration;
                float spinEase = EaseFunctions.EaseOutCubic(progress);
                currentSpinSpeed = MathHelper.Lerp(InitialSpinSpeed, MinSpinSpeed, spinEase);

                Projectile.rotation += currentSpinSpeed * Projectile.direction;
                Projectile.velocity *= 0.985f;
                Projectile.velocity.Y += 0.15f;
                return;
            }

            if (!hasDropped)
            {
                hasDropped = true;
                Projectile.velocity = new Vector2(Projectile.velocity.X * 0.25f, DropSpeed);
                currentSpinSpeed = DropSpinSpeed;

                if (!hasSpawnedPreDropParticles && Main.netMode != NetmodeID.Server)
                {
                    hasSpawnedPreDropParticles = true;
                    SpawnSnowFlakes(12, 2.6f);
                }

                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.4f }, Projectile.Center);
            }

            Projectile.rotation += currentSpinSpeed * Projectile.direction;
            currentSpinSpeed = MathHelper.Lerp(currentSpinSpeed, DropSpinSpeed, 0.08f);

            if (Projectile.velocity.Y < DropSpeed * 1.5f)
                Projectile.velocity.Y += 1.1f;

            Projectile.velocity.X *= 0.97f;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
        }

        private void SpawnSnowFlakes(int count, float scaleMult)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = Main.rand.NextVector2Circular(3.8f, 3.8f) - Vector2.UnitY * Main.rand.NextFloat(1.4f, 4.2f);

                ParticleSystem.SnowFlakeBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: vel.ToNumerics(),
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(2.2f, 3.8f) * scaleMult),
                    color: Color.White * Main.rand.NextFloat(0.9f, 1.2f),
                    duration: Main.rand.Next(30, 48)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SpawnSnowFlakes(26, 3.4f);

                for (int i = 0; i < 16; i++)
                {
    
                }
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.4f }, Projectile.Center);
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

            if (hasDropped)
            {
                if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                    oldPos = Projectile.Center;
                else if (!Main.gamePaused)
                    oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.16f);

                if (oldPos != Projectile.Center)
                {
                    Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;

                    float trailAlpha = 0.7f;
                    Color trailColor = new Color(200, 235, 255, 0) * trailAlpha;

                    float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                    float trailScaleY = trailLength / trailTex.Height * 5.4f;

                    Main.EntitySpriteDraw(
                        trailTex,
                        Projectile.Center - Main.screenPosition,
                        new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                        trailColor,
                        (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                        new Vector2(trailTex.Width * 0.5f, 0f),
                        new Vector2(Projectile.scale * 0.85f, trailScaleY),
                        SpriteEffects.None,
                        0f
                    );

                    Main.EntitySpriteDraw(
                        trailTex,
                        Projectile.Center - Main.screenPosition,
                        new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                        trailColor * 0.45f,
                        (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                        new Vector2(trailTex.Width * 0.5f, 0f),
                        new Vector2(Projectile.scale * 0.45f, trailScaleY * 1.35f),
                        SpriteEffects.None,
                        0f
                    );
                }
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

            if (hasDropped)
            {
                Color outline = Color.White * 0.4f;
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
            }

            return false;
        }
    }
}