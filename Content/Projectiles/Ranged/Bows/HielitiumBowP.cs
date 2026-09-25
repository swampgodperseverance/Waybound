using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Ranged.Bows
{
    public class HielitiumBowP : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/Ranged/Bows/HielitiumBowP";

        private const int StraightTime = 20;
        private Vector2 targetPos;
        private bool hasTarget;
        private Vector2 lastPos;
        private float fadeIn;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.arrow = true;
            Projectile.extraUpdates = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            targetPos = Main.MouseWorld;
            hasTarget = true;
            lastPos = Projectile.Center;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            fadeIn = MathHelper.Lerp(fadeIn, 1f, 0.15f);

            if (Projectile.ai[0] <= StraightTime)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Projectile.velocity * 0.1f, 100, default, 0.9f);
                    d.noGravity = true;
                }
                SpawnTrailParticles();
                lastPos = Projectile.Center;
                Lighting.AddLight(Projectile.Center, 0.2f, 0.45f, 0.7f);
                return;
            }

            if (hasTarget)
            {
                Vector2 toTarget = targetPos - Projectile.Center;
                float dist = toTarget.Length();

                if (dist < 12f)
                {
                    hasTarget = false;
                }
                else
                {
                    float progress = MathHelper.Clamp((Projectile.ai[0] - StraightTime) / 35f, 0f, 1f);
                    float ease = progress * progress * (3f - 2f * progress);

                    Vector2 desiredDir = toTarget.SafeNormalize(Vector2.UnitX);
                    float currentSpeed = Projectile.velocity.Length();
                    float targetSpeed = MathHelper.Lerp(currentSpeed, 22f, ease * 0.15f);
                    targetSpeed = MathHelper.Clamp(targetSpeed, 10f, 24f);

                    Vector2 newVel = Vector2.Lerp(
                        Projectile.velocity.SafeNormalize(Vector2.UnitX),
                        desiredDir,
                        0.12f + ease * 0.25f
                    ).SafeNormalize(Vector2.UnitX) * targetSpeed;

                    Projectile.velocity = newVel;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Projectile.velocity * 0.08f, 80, default, 1.1f);
                d.noGravity = true;
            }

            SpawnTrailParticles();
            lastPos = Projectile.Center;
            Lighting.AddLight(Projectile.Center, 0.25f, 0.5f, 0.8f);
        }

        private void SpawnTrailParticles()
        {
            if (Main.rand.NextBool(2))
                return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPos = Vector2.Lerp(lastPos, Projectile.Center, i / 3f) + Main.rand.NextVector2Circular(2.5f, 2.5f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    System.Numerics.Vector2.Zero,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(10f, 16f)),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(12, 20)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3.5f, 3.5f), 60, default, 1.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float alpha = fadeIn;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float fade = 1f - (float)i / Projectile.oldPos.Length;
                float trailScale = Projectile.scale * (1f - i * 0.05f);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null,
                    new Color(150, 215, 255, 0) * fade * 0.5f * alpha,
                    Projectile.oldRot[i],
                    origin,
                    trailScale,
                    SpriteEffects.None,
                    0
                );
            }

            Color outline = new Color(120, 200, 255, 180) * alpha;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(2.5f, 0).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                    null, outline * 0.65f, Projectile.rotation, origin, Projectile.scale * 1.04f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                null, lightColor * alpha, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}