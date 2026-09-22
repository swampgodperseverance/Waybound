using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Armor
{
    public class IceSpike : ModProjectile
    {
        private float breath;
        private float glowPulse;
        private float glowIntensity = 1f;
        private Vector2 oldPos;
        private bool launching;
        private float turnSpeed;
        private float spawnProgress;
        private float deathFlash;
        private const int HoverLifetime = 600;
        private const int SpawnTime = 28;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = HoverLifetime;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.Opacity = 0f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
            breath = Main.rand.NextFloat(MathHelper.TwoPi);
            glowPulse = Main.rand.NextFloat(MathHelper.TwoPi);
            spawnProgress = 0f;

            if (Main.netMode != NetmodeID.Server)
            {
                SpawnSnowFlakes(6, 0.55f, 0.7f);
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.3f, Pitch = 0.45f }, Projectile.Center);
            }
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            if (spawnProgress < 1f)
            {
                spawnProgress += 1f / SpawnTime;
                if (spawnProgress > 1f)
                    spawnProgress = 1f;
                float ease = 1f - MathF.Pow(1f - spawnProgress, 2.4f);
                Projectile.Opacity = ease;
                glowIntensity = ease * 1.35f;
            }

            breath += 0.045f;
            glowPulse += 0.06f;
            float breathe = 0.92f + MathF.Sin(breath) * 0.08f;
            float spawnScale = MathHelper.Lerp(0.15f, 1f, 1f - MathF.Pow(1f - spawnProgress, 3f));
            Projectile.scale = breathe * spawnScale;

            int slot = (int)Projectile.ai[0];
            Vector2 baseOffset = slot switch
            {
                0 => new Vector2(-28f, -46f),
                1 => new Vector2(0f, -58f),
                _ => new Vector2(28f, -46f)
            };

            float bob = MathF.Sin(breath * 0.85f + slot * 1.2f) * 3.5f;
            float sway = MathF.Cos(breath * 0.55f + slot) * 2.2f;

            if (Projectile.ai[1] <= 0)
            {
                launching = false;
                Vector2 desired = owner.Center + baseOffset + new Vector2(sway, bob);
                Projectile.Center = Vector2.Lerp(Projectile.Center, desired, 0.12f);
                Projectile.velocity = Vector2.Zero;

                float targetRot = MathHelper.Pi + MathF.Sin(breath * 0.4f) * 0.12f;
                Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, 0.08f);

                if (spawnProgress >= 1f)
                    glowIntensity = MathHelper.Lerp(glowIntensity, 1f, 0.1f);
            }
            else
            {
                int targetId = (int)Projectile.ai[1] - 1;
                if (targetId < 0 || targetId >= Main.maxNPCs || !Main.npc[targetId].active)
                {
                    Projectile.ai[1] = 0;
                    launching = false;
                    return;
                }

                NPC target = Main.npc[targetId];
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();

                if (!launching)
                {
                    launching = true;
                    turnSpeed = 0.04f;
                    Projectile.timeLeft = 180;
                    SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.35f, Pitch = 0.3f }, Projectile.Center);
                }

                turnSpeed = MathHelper.Clamp(turnSpeed + 0.008f, 0.04f, 0.22f);
                float desiredRot = toTarget.ToRotation() + MathHelper.PiOver2;
                Projectile.rotation = Projectile.rotation.AngleLerp(desiredRot, turnSpeed);

                float speed = MathHelper.Lerp(3.5f, 14f, MathHelper.Clamp(turnSpeed * 4f, 0f, 1f));
                Vector2 move = Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver2) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, move, 0.18f);

                if (dist < 18f)
                {
                    Projectile.Kill();
                    return;
                }

                glowIntensity = MathHelper.Lerp(glowIntensity, 1.45f, 0.12f);
            }

            Lighting.AddLight(Projectile.Center, 0.25f * glowIntensity * Projectile.Opacity, 0.5f * glowIntensity * Projectile.Opacity, 0.9f * glowIntensity * Projectile.Opacity);

            if (Main.rand.NextBool(5) && spawnProgress > 0.4f)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 6f),
                    DustID.IceTorch,
                    Vector2.Zero,
                    100,
                    new Color(150, 220, 255),
                    Main.rand.NextFloat(0.45f, 0.75f) * Projectile.Opacity
                );
                d.noGravity = true;
                d.velocity *= 0.2f;
            }

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            SpawnSnowFlakes(7, 0.7f, 0.85f);
            SpawnSnowFlakes(4, 1.05f, 1.15f);

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.45f, Pitch = 0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.25f, Pitch = 0.55f }, Projectile.Center);
        }

        private void SpawnSnowFlakes(int count, float scaleMult, float speedMult = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f) * speedMult
                              - Vector2.UnitY * Main.rand.NextFloat(1.2f, 3.5f) * speedMult;
                ParticleSystem.SnowFlakeBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: vel.ToNumerics(),
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(5.5f, 8.5f) * scaleMult),
                    color: Color.White * Main.rand.NextFloat(0.95f, 1.3f),
                    duration: Main.rand.Next(35, 55)
                ));
            }
        }   

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = Projectile.Opacity;
            float scale = Projectile.scale;

            float spawnGlow = 1f;
            if (spawnProgress < 1f)
                spawnGlow = 0.6f + spawnProgress * 1.1f;

            if (oldPos != Projectile.Center && oldPos != Vector2.Zero && spawnProgress > 0.5f)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 2.8f;
                Color trailColor = new Color(80, 170, 255, 0) * 0.55f * alpha * glowIntensity;

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(scale * 0.45f, trailScaleY),
                    SpriteEffects.None,
                    0f
                );
                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    new Color(170, 225, 255, 0) * 0.4f * alpha * glowIntensity,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(scale * 0.22f, trailScaleY * 1.1f),
                    SpriteEffects.None,
                    0f
                );
            }

            Texture2D megaspark = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
            Vector2 megaOrigin = megaspark.Size() * 0.5f;
            float aspect = megaspark.Width / (float)megaspark.Height;

            float gScale = scale * 0.055f * (0.7f + MathF.Sin(glowPulse) * 0.15f) * spawnGlow;
            Vector2 gScaleVec = new Vector2(gScale / aspect, gScale);
            float gAlpha = glowIntensity * 0.7f * alpha;

            if (spawnProgress < 1f)
            {
                float burst = (1f - spawnProgress) * 1.8f;
                Vector2 burstScale = gScaleVec * (1.4f + burst * 1.6f);
                Main.EntitySpriteDraw(megaspark, drawPos, null,
                    new Color(180, 230, 255) * (gAlpha * 0.9f * (1f - spawnProgress)),
                    glowPulse * 0.5f, megaOrigin, burstScale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(megaspark, drawPos, null,
                new Color(60, 130, 235) * (gAlpha * 0.6f),
                glowPulse * 0.2f, megaOrigin, gScaleVec, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(megaspark, drawPos, null,
                new Color(130, 200, 255) * (gAlpha * 0.85f),
                -glowPulse * 0.3f, megaOrigin, gScaleVec * 0.65f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(megaspark, drawPos, null,
                new Color(200, 235, 255) * (gAlpha * 0.5f),
                glowPulse * 0.45f, megaOrigin, gScaleVec * 0.35f, SpriteEffects.None, 0f);

            Color outline = new Color(120, 200, 255) * (0.45f * alpha * glowIntensity);
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1.5f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i) * scale;
                Main.EntitySpriteDraw(tex, drawPos + off, null, outline, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(tex, drawPos, null, Color.White * alpha, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(tex, drawPos, null, new Color(160, 220, 255) * (0.35f * alpha * glowIntensity), Projectile.rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);

            return false;
        }
    }
}