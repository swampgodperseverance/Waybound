using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.Projectiles.Ranged.Bows
{
    public class QuirkProj : ModProjectile
    {
        private const int AimDuration = 90;
        private const float ShootSpeed = 22f;
        private const float MinRayLength = 20f;
        private const float MaxRayLength = 140f;

        private const float RepelRadius = 48f;
        private const float RepelStrength = 1.35f;

        private Asset<Texture2D> rayTexture;
        private Vector2 oldPos = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 250;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.ai[0]++;

            if (Projectile.ai[0] <= AimDuration)
            {
                float fadeIn = MathHelper.Clamp(Projectile.ai[0] / 18f, 0f, 1f);
                float fadeOut = MathHelper.Clamp((AimDuration - Projectile.ai[0]) / 15f, 0f, 1f);
                float alphaProgress = fadeIn * fadeOut;
                Projectile.alpha = (int)(255 * (1f - EaseFunctions.EaseOutCubic(alphaProgress)));

                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                float distance = toMouse.Length();
                Vector2 dir = distance > 0.1f ? Vector2.Normalize(toMouse) : Vector2.UnitX;

                float rayLength = MathHelper.Clamp(distance * 0.38f, MinRayLength, MaxRayLength);
                Projectile.rotation = dir.ToRotation();
                Projectile.localAI[0] = rayLength;

                float breathe = 1f + (float)Math.Sin(Projectile.ai[0] * 0.14f) * 0.03f;
                Projectile.localAI[0] *= breathe;

                Projectile.velocity *= 0.88f;
                if (Projectile.velocity.Length() < 0.4f)
                    Projectile.velocity *= 0.7f;

                ApplyRepulsion();
                return;
            }

            if (Projectile.ai[0] == AimDuration + 1)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                    if (toMouse.LengthSquared() < 0.1f)
                        toMouse = Vector2.UnitX * owner.direction;

                    Projectile.velocity = Vector2.Normalize(toMouse) * ShootSpeed;
                    Projectile.netUpdate = true;
                }

                SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
                Projectile.localAI[0] = 0f;
                Projectile.alpha = 0;
            }

            if (Projectile.ai[0] > AimDuration)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();

                float lifeFade = MathHelper.Clamp(Projectile.timeLeft / 35f, 0f, 1f);
                Projectile.alpha = (int)(255 * (1f - EaseFunctions.EaseOutQuad(lifeFade)));

                ApplyRepulsion();

                if (oldPos == Vector2.Zero)
                    oldPos = Projectile.Center;
            }
        }

        private void ApplyRepulsion()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (i == Projectile.whoAmI || !other.active || other.type != Projectile.type)
                    continue;

                Vector2 delta = Projectile.Center - other.Center;
                float dist = delta.Length();

                if (dist < RepelRadius && dist > 0.1f)
                {
                    float strength = (1f - dist / RepelRadius) * RepelStrength;
                    Vector2 push = Vector2.Normalize(delta) * strength;
                    Projectile.velocity += push;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (rayTexture == null)
                rayTexture = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray", AssetRequestMode.ImmediateLoad);

            Texture2D ray = rayTexture.Value;
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            float overallAlpha = 1f - (Projectile.alpha / 255f);

            if (Projectile.ai[0] <= AimDuration && Projectile.localAI[0] > 1f)
            {
                float rayLength = Projectile.localAI[0];
                float rayProgress = Projectile.ai[0] / AimDuration;
                float lengthEase = EaseFunctions.EaseOutCubic(MathHelper.Clamp(rayProgress * 1.4f, 0f, 1f));
                float drawLength = rayLength * lengthEase;

                float rayFadeOut = MathHelper.Clamp((AimDuration - Projectile.ai[0]) / 12f, 0f, 1f);
                float rayAlpha = overallAlpha * EaseFunctions.EaseOutCubic(rayFadeOut);

                Color rayColor = new Color(200, 230, 255) * (0.9f * rayAlpha);
                float pulse = 0.88f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9f) * 0.12f;
                rayColor *= pulse;

                Vector2 start = Projectile.Center;
                float scaleY = drawLength / ray.Height;
                float scaleX = Projectile.scale * 0.75f;

                float rayRotation = Projectile.rotation - MathHelper.PiOver2 + MathHelper.Pi;

                Main.EntitySpriteDraw(
                    ray,
                    start - Main.screenPosition,
                    null,
                    rayColor,
                    rayRotation,
                    new Vector2(ray.Width * 0.5f, ray.Height),
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f
                );

                Main.EntitySpriteDraw(
                    ray,
                    start - Main.screenPosition,
                    null,
                    Color.White * (0.4f * rayAlpha * pulse),
                    rayRotation,
                    new Vector2(ray.Width * 0.5f, ray.Height),
                    new Vector2(scaleX * 0.4f, scaleY),
                    SpriteEffects.None,
                    0f
                );
            }

            if (Projectile.ai[0] > AimDuration)
            {
                if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                    oldPos = Projectile.Center;
                else if (!Main.gamePaused)
                    oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.18f);

                if (oldPos != Projectile.Center)
                {
                    Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;

                    float trailProgress = MathHelper.Clamp((Projectile.ai[0] - AimDuration) / 16f, 0f, 1f);
                    float trailAlpha = EaseFunctions.EaseOutCubic(trailProgress) * 0.48f * overallAlpha;
                    float lifeFade = MathHelper.Clamp(Projectile.timeLeft / 40f, 0f, 1f);
                    trailAlpha *= lifeFade;

                    Color trailColor = new Color(180, 220, 255, 0) * trailAlpha;
                    float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                    float trailScaleY = trailLength / trailTex.Height * 2.6f;

                    Main.EntitySpriteDraw(
                        trailTex,
                        Projectile.Center - Main.screenPosition,
                        new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                        trailColor,
                        (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                        new Vector2(trailTex.Width * 0.5f, 0f),
                        new Vector2(Projectile.scale * 0.52f, trailScaleY),
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            if (overallAlpha > 0.05f)
            {
                Color outlineColor = Color.White * (0.55f * overallAlpha);

                for (int i = 0; i < 4; i++)
                {
                    float offset = 1.4f + i * 0.6f;
                    float layerAlpha = 1f - i * 0.22f;

                    Vector2[] offsets = new Vector2[]
                    {
                        new Vector2( offset,  0),
                        new Vector2(-offset,  0),
                        new Vector2( 0,  offset),
                        new Vector2( 0, -offset),
                        new Vector2( offset * 0.7f,  offset * 0.7f),
                        new Vector2(-offset * 0.7f,  offset * 0.7f),
                        new Vector2( offset * 0.7f, -offset * 0.7f),
                        new Vector2(-offset * 0.7f, -offset * 0.7f),
                    };

                    foreach (var off in offsets)
                    {
                        Main.EntitySpriteDraw(
                            texture,
                            Projectile.Center + off - Main.screenPosition,
                            null,
                            outlineColor * layerAlpha * 0.45f,
                            Projectile.rotation,
                            texture.Size() * 0.5f,
                            Projectile.scale * (1f + i * 0.015f),
                            SpriteEffects.None,
                            0f
                        );
                    }
                }
            }

            Color drawColor = lightColor * overallAlpha;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * (0.25f * overallAlpha),
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale * 0.92f,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void OnKill(int timeLeft)
        {
        }
    }
}