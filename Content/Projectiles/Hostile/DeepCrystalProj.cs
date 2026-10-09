using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Projectiles.Hostile
{
    public class DeepCrystalProj : ModProjectile
    {
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
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Generic;
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

        private void SpawnTrail()
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            float size = Main.rand.NextFloat(12f, 22f);

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                (Projectile.Center + Main.rand.NextVector2Circular(4f, 4f)).ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                Main.rand.Next(10, 18)
            ));
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
            Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);

            SpawnTrail();
            if (Main.rand.NextBool(2))
                SpawnTrail();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float t = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.5f * t * t;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * (0.85f + t * 0.15f), SpriteEffects.None, 0);
            }

            Vector2 pos = (Projectile.position - Main.screenPosition) + drawOrigin + new Vector2(0, 0);
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor) * 1, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server && ParticleSystem.CrystalBuffer != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                    float size = Main.rand.NextFloat(16f, 28f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                        new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.9f, 1.15f),
                        Main.rand.Next(14, 24)
                    ));
                }
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
    }
}