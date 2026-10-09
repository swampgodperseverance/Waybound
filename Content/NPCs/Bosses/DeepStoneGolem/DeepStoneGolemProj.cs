using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
    public class DeepStoneGolemProj : ModProjectile
    {
        //Its just old victima proj but simply bettuh
        private const int TelegraphTime = 60;
        private const int AccelTime = 40;
        private const float StartSpeed = 1.5f;
        private const float MaxSpeed = 14f;

        private bool locked;
        private int spawnTime;
        private Vector2 lockedDirection;
        private float currentSpeed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.scale = 1.25f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            spawnTime = 0;
            currentSpeed = 0f;
            lockedDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            locked = false;
            Projectile.velocity = Vector2.Zero;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool(2))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 240);
            }
        }

        private void SpawnCrystal(Vector2 pos, Vector2 vel, float sizeMin, float sizeMax)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            float size = Main.rand.NextFloat(sizeMin, sizeMax);

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.2f),
                Main.rand.Next(16, 30)
            ));
        }

        public override void AI()
        {
            spawnTime++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
            Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);

            if (!locked)
            {
                if (spawnTime < TelegraphTime)
                {
                    Projectile.velocity = Vector2.Zero;
                    if (spawnTime % 4 == 0)
                    {
                        Vector2 dustVel = lockedDirection * Main.rand.NextFloat(5f, 12f);
                        SpawnCrystal(Projectile.Center, dustVel, 18f, 30f);
                    }
                }
                else
                {
                    locked = true;
                    Projectile.netUpdate = true;
                    currentSpeed = StartSpeed;
                    Projectile.velocity = lockedDirection * currentSpeed;

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f) * 6f;
                            SpawnCrystal(Projectile.Center, speed, 20f, 34f);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
                }
            }
            else
            {
                float accelProgress = MathHelper.Clamp((spawnTime - TelegraphTime) / (float)AccelTime, 0f, 1f);
                float eased = accelProgress * accelProgress * (3f - 2f * accelProgress);
                currentSpeed = MathHelper.Lerp(StartSpeed, MaxSpeed, eased);

                if (Projectile.velocity.LengthSquared() > 0.01f)
                    lockedDirection = Projectile.velocity.SafeNormalize(lockedDirection);

                Projectile.velocity = lockedDirection * currentSpeed;

                if (spawnTime % 2 == 0)
                {
                    Vector2 dustVel = -lockedDirection * Main.rand.NextFloat(0.5f, 2f);
                    SpawnCrystal(Projectile.Center, dustVel, 18f, 30f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;

            if (!locked && spawnTime < TelegraphTime)
            {
                float progress = spawnTime / (float)TelegraphTime;
                float pulse = 0.55f + MathF.Sin(progress * MathHelper.Pi * 5f) * 0.45f;
                float fade = MathF.Sin(progress * MathHelper.Pi);
                Color lineColor = new Color(255, 168, 135, 0) * (0.9f * pulse * fade);

                Vector2 start = Projectile.Center;
                Vector2 end = Projectile.Center + lockedDirection * 1800f;
                Vector2 delta = end - start;
                float length = delta.Length();
                float rot = delta.ToRotation();

                Vector2 lineOrigin = new Vector2(0f, glowTex.Height * 0.5f);
                Vector2 lineScale = new Vector2(length / glowTex.Width, 0.11f * pulse);

                Main.EntitySpriteDraw(glowTex, start - Main.screenPosition, null, lineColor, rot, lineOrigin, lineScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(glowTex, start - Main.screenPosition, null, lineColor * 0.55f, rot, lineOrigin, new Vector2(lineScale.X, lineScale.Y * 3.5f), SpriteEffects.None, 0);

                float ringProgress = (spawnTime % 20) / 20f;
                float ringScale = 0.3f + ringProgress * 1.1f;
                float ringAlpha = (1f - ringProgress) * fade;
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null,
                    new Color(255, 200, 170, 0) * ringAlpha, 0f, glowTex.Size() * 0.5f, ringScale, SpriteEffects.None, 0);

                float warnScale = 0.45f + pulse * 0.6f;
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null,
                    new Color(255, 220, 190, 0) * pulse * fade, 0f, glowTex.Size() * 0.5f, warnScale, SpriteEffects.None, 0);
            }

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            if (locked)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    float t = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                    Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.65f * t * t;
                    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * (0.8f + t * 0.2f), SpriteEffects.None, 0);
                }
            }

            Vector2 pos = (Projectile.position - Main.screenPosition) + drawOrigin;
            Color baseTint = locked ? Color.White : Color.White * 0.7f;
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(baseTint), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.4f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server && ParticleSystem.CrystalBuffer != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 6f);
                    SpawnCrystal(Projectile.Center, vel, 22f, 38f);
                }
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
    }
}