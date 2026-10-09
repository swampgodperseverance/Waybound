using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
    public class DeepCrystalBall : ModProjectile
    {
        public int damage;
        private bool launched;
        private Vector2 launchTarget;
        private int age;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            DrawOriginOffsetY = -2;
            Projectile.width = 36;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool(2))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 240);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 150, false);
            }
        }

        public override void AI()
        {
            if (Projectile.friendly)
            {
                damage = Projectile.damage / 2;
            }
            else
            {
                damage = Projectile.damage;
            }

            Projectile.rotation += Projectile.direction * 0.1f;

            Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);

            age++;

            if (!launched)
            {
                Projectile.velocity.Y *= 0.94f;
                Projectile.velocity.X *= 0.94f;

                if (MathF.Abs(Projectile.velocity.Y) < 0.25f)
                {
                    launched = true;
                    Projectile.netUpdate = true;

                    Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                    if (target != null && target.active && !target.dead)
                        launchTarget = target.Center;
                    else
                        launchTarget = Projectile.Center + new Vector2(0, 200f);
                }
            }
            else
            {
                Vector2 toTarget = launchTarget - Projectile.Center;
                if (toTarget.LengthSquared() > 1f)
                {
                    Vector2 desired = toTarget.SafeNormalize(Vector2.UnitY) * 4f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.08f);
                }

                if (Projectile.Center.Y >= launchTarget.Y - 6f)
                {
                    Explode();
                    return;
                }
            }

            if (age % 5 == 0 && ParticleSystem.CrystalBuffer != null && Main.netMode != NetmodeID.Server)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                float size = Main.rand.NextFloat(16f, 26f);

                ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                    (Projectile.Center + offset).ToNumerics(),
                    (Main.rand.NextVector2Circular(0.6f, 0.6f)).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                    new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                    Main.rand.Next(18, 30)
                ));
            }
        }

        private void Explode()
        {
            int shardCount = Main.masterMode ? 4 : (Main.expertMode ? 4 : 3);
            float halfSpread = MathHelper.Pi * 0.35f;

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Vector2 aimDir = Vector2.UnitY;
            if (target != null && target.active && !target.dead)
                aimDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);

            float baseAngle = aimDir.ToRotation();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < shardCount; i++)
                {
                    float t = shardCount == 1 ? 0.5f : i / (float)(shardCount - 1);
                    float angle = baseAngle + MathHelper.Lerp(-halfSpread, halfSpread, t);
                    Vector2 dir = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 7f);
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, dir, ModContent.ProjectileType<DeepGolemCrystalProj>(), damage, 1f, Main.myPlayer);
                    Main.projectile[p].friendly = Projectile.friendly;
                    Main.projectile[p].hostile = Projectile.hostile;
                }
            }

            if (ParticleSystem.CrystalBuffer != null && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 60; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 8f);
                    float size = Main.rand.NextFloat(22f, 40f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        speed.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                        new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(20, 36)
                    ));
                }
            }

            SpawnRingFlash();
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            Projectile.Kill();
        }

        private void SpawnRingFlash()
        {
            Texture2D ringTex = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ring").Value;
            Vector2 origin = ringTex.Size() * 0.5f;

            float ringScale = 0.4f;
            int baseDamage = Math.Max(1, damage / 2);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int ring = 0; ring < 3; ring++)
                {
                    float angleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                    float scaleBase = ringScale + ring * 0.35f;

                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<DeepCrystalBallRing>(),
                        baseDamage,
                        0f,
                        Main.myPlayer,
                        angleOffset,
                        scaleBase,
                        ring * 0.7f
                    );

                    if (proj >= 0 && proj < Main.maxProjectiles)
                    {
                        Main.projectile[proj].friendly = Projectile.friendly;
                        Main.projectile[proj].hostile = Projectile.hostile;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            float pulse = 0.75f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.25f;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float t = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.6f * t * t;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * (0.85f + t * 0.15f), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 168, 135, 0) * (0.5f * pulse), 0f, glowOrigin, 1.1f * pulse, SpriteEffects.None, 0);

            Vector2 pos = (Projectile.position - Main.screenPosition) + drawOrigin;
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.45f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
    }

    public class DeepCrystalBallRing : ModProjectile
    {
        private int lifeTime;
        private int maxLifeTime = 30;

        public override string Texture => "Waybound/Assets/Textures/Ring";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override bool CanHitPlayer(Player target) => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            lifeTime++;

            if (lifeTime >= maxLifeTime)
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = Vector2.Zero;
            Lighting.AddLight(Projectile.Center, 0.9f, 0.6f, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ringTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = ringTex.Size() * 0.5f;

            float progress = lifeTime / (float)maxLifeTime;
            float fade = 1f - progress;

            float baseScale = Projectile.ai[1];
            float rotationOffset = Projectile.ai[0];
            float delay = Projectile.ai[2];

            float delayedProgress = MathHelper.Clamp((lifeTime - delay * 6f) / (float)maxLifeTime, 0f, 1f);
            float delayedEased = 1f - MathF.Pow(1f - delayedProgress, 2.5f);

            float scale = baseScale * (0.15f + delayedEased * 1.4f);
            float alpha = fade * fade;

            Color outer = new Color(255, 168, 135, 0) * (alpha * 0.4f);
            Color mid = new Color(255, 190, 160, 0) * (alpha * 0.6f);
            Color core = new Color(255, 230, 210, 0) * (alpha * 0.9f);

            float rot = rotationOffset + progress * 1.2f;

            Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition, null,
                outer, rot, origin, new Vector2(scale * 1.3f, scale * 0.9f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition, null,
                mid, rot + 0.15f, origin, new Vector2(scale * 1.1f, scale * 0.75f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition, null,
                core, rot + 0.3f, origin, new Vector2(scale * 0.85f, scale * 0.55f), SpriteEffects.None, 0);

            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;
            float flashAlpha = MathF.Sin(MathHelper.Clamp(progress * 2.5f, 0f, 1f) * MathHelper.Pi) * fade;
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 200, 170, 0) * (flashAlpha * 0.75f), 0f, glowOrigin, scale * 0.9f, SpriteEffects.None, 0);

            return false;
        }
    }
}