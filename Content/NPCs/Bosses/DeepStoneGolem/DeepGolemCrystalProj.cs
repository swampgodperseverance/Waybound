using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
    public class DeepGolemCrystalProj : ModProjectile
    {
        //Its just old victima proj but simply bettuh
        private const int TelegraphTime = 60;
        private const int AccelTime = 65;
        private const float StartSpeed = 0.8f;
        private const float MaxSpeed = 5f;
        private const float TelegraphLength = 480f;
        private bool locked;
        private int spawnTime;
        private Vector2 lockedDirection;
        private float currentSpeed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.scale = 1f;
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
                target.AddBuff(ModContent.BuffType<DeepFire>(), 180);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 90, false);
            }
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

                    if (spawnTime % 3 == 0 && ParticleSystem.CrystalBuffer != null && Main.netMode != NetmodeID.Server)
                    {
                        Vector2 vel = lockedDirection * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                        float size = Main.rand.NextFloat(18f, 30f);

                        ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                            (Projectile.Center + lockedDirection * Main.rand.NextFloat(4f, 24f)).ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                            new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                            Main.rand.Next(22, 38)
                        ));
                    }
                }
                else
                {
                    locked = true;
                    Projectile.netUpdate = true;
                    currentSpeed = StartSpeed;
                    Projectile.velocity = lockedDirection * currentSpeed;
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

                if (spawnTime % 2 == 0 && ParticleSystem.CrystalBuffer != null && Main.netMode != NetmodeID.Server)
                {
                    Vector2 vel = -lockedDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                    float size = Main.rand.NextFloat(20f, 32f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                        new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                        Main.rand.Next(18, 32)
                    ));
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
                float fade = MathF.Sin(progress * MathHelper.Pi);
                Color lineColor = new Color(255, 120, 100, 0) * (0.75f * fade);

                Vector2 start = Projectile.Center;
                Vector2 end = Projectile.Center + lockedDirection * TelegraphLength;
                Vector2 delta = end - start;
                float length = delta.Length();
                float rot = delta.ToRotation();
                Vector2 lineOrigin = new Vector2(0f, glowTex.Height * 0.5f);
                Vector2 lineScale = new Vector2(length / glowTex.Width, 0.08f * fade);

                Main.EntitySpriteDraw(glowTex, start - Main.screenPosition, null, lineColor, rot, lineOrigin, lineScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(glowTex, start - Main.screenPosition, null, lineColor * 0.5f, rot, lineOrigin, new Vector2(lineScale.X, lineScale.Y * 3f), SpriteEffects.None, 0);

                float warnScale = 0.35f + fade * 0.5f;
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null,
                    new Color(255, 200, 150, 0) * fade, 0f, glowTex.Size() * 0.5f, warnScale, SpriteEffects.None, 0);

                return false;
            }

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            if (locked)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    float t = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                    Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.55f * t * t;
                    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * (0.85f + t * 0.15f), SpriteEffects.None, 0);
                }
            }

            Vector2 pos = (Projectile.position - Main.screenPosition) + drawOrigin;
            Color baseTint = locked ? Color.White : Color.White * 0.7f;
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(baseTint), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (ParticleSystem.CrystalBuffer != null && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f);
                    float size = Main.rand.NextFloat(22f, 38f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                        new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.9f, 1.15f),
                        Main.rand.Next(20, 34)
                    ));
                }
            }

            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
    }
}