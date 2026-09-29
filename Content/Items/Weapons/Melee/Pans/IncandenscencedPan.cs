using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class IncandescencedPan : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<IncandescencedPanProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<IncandescencedPanThrownProjectile>();

        public override int BaseDamage => 25;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 95);
        }
        public override int SpinUseTime => 16;
        public override int ThrowUseTime => 24;
        public override int ThrowDebuffType => BuffID.OnFire;
        public override int ThrowDebuffDuration => 600;
        public override float ThrowSpeed => 18f;
        public override float Knockback => 8f;
    }

    public class IncandescencedPanProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 280f;
        public override float ParticleStart => 45f;
        public override float OutlineStart => 65f;
        public override float MinRotationSpeed => 0.03f;
        public override float MaxRotationSpeed => 0.34f;
        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.25f;
        public override float ScaleAtFade => 0.85f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 480;
        public override int BuffInterval => 30;
        public override int BuffStartTime => 70;

        public override Color ParticleColor => new Color(255, 90, 20, 220);
        public override float ParticleSizeMin => 14f;
        public override float ParticleSizeMax => 24f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 18;
        public override int ParticleLifeMax => 30;
    }

    public class IncandescencedPanThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 40f;
        public override float MaxRange => 400f;
        public override float ReturnAccel => 1.0f;
        public override float ReturnMaxSpeed => 24f;
        public override float SpinSpeed => 0.38f;
        public override int AirTime => 600;

        public override Color ParticleColor => new Color(255, 90, 20, 220);
        public override float ParticleSizeMin => 12f;
        public override float ParticleSizeMax => 22f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;

        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public override Color HitTextColor => Color.OrangeRed;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            if (Main.myPlayer != Projectile.owner) return;
            if (!target.active || target.CountsAsACritter || target.immortal || target.dontTakeDamage) return;

            int count = Main.rand.Next(2, 4);

            Vector2 baseDir = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitY);
            baseDir.Y = -Math.Abs(baseDir.Y) - 0.3f;
            baseDir.Normalize();

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = baseDir.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f));
                dir.Y = -Math.Abs(dir.Y) - 0.2f;
                dir.Normalize();

                float speed = Main.rand.NextFloat(6f, 11f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    dir * speed,
                    ModContent.ProjectileType<IncandescencedPanP>(),
                    Projectile.damage / 2,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }

        public class IncandescencedPanP : ModProjectile
        {
            public override void SetStaticDefaults()
            {
                Main.projFrames[Type] = 4;
                ProjectileID.Sets.TrailCacheLength[Type] = 8;
                ProjectileID.Sets.TrailingMode[Type] = 2;
            }

            public override void SetDefaults()
            {
                Projectile.width = 32;
                Projectile.height = 26;
                Projectile.friendly = true;
                Projectile.hostile = false;
                Projectile.tileCollide = true;
                Projectile.ignoreWater = true;
                Projectile.penetrate = 2;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = 12;
                Projectile.timeLeft = 120;
                Projectile.DamageType = DamageClass.Melee;
                Projectile.noEnchantmentVisuals = true;
                Projectile.extraUpdates = 1;
                Projectile.damage = 10;
            }

            public override void AI()
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 5)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
                }

                if (Projectile.velocity.Length() > 0.1f)
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;

                Projectile.velocity.Y += 0.22f;

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(1))
                {
                    Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                    Vector2 vel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.4f, 1.4f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawn.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(12f, 22f)),
                        new Color(255, 90, 20, 220),
                        Main.rand.Next(16, 28)
                    ));
                }

                Lighting.AddLight(Projectile.Center, 1.0f, 0.35f, 0.05f);
            }

            public override bool OnTileCollide(Vector2 oldVelocity)
            {
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                return true;
            }

            public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
            {
                if (target.active && !target.CountsAsACritter && !target.immortal && !target.dontTakeDamage)
                    target.AddBuff(BuffID.OnFire, 600);

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
            }

            public override void OnKill(int timeLeft)
            {
                if (Main.netMode == NetmodeID.Server) return;

                for (int i = 0; i < 14; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 spawn = Projectile.Center + dir * Main.rand.NextFloat(2f, 14f);
                    Vector2 vel = dir * Main.rand.NextFloat(2f, 5f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawn.ToNumerics(),
                        vel.ToNumerics(),
                        angle,
                        new System.Numerics.Vector2(Main.rand.NextFloat(12f, 22f)),
                        new Color(255, 90, 20, 220),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;

                int frameHeight = texture.Height / Main.projFrames[Type];
                Rectangle frame = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
                Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

                SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                for (int k = 0; k < Projectile.oldPos.Length - 1; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;

                    Vector2 drawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
                    float progress = k / (float)Projectile.oldPos.Length;

                    Color trailColor = Color.Lerp(
                        new Color(255, 90, 20, 0),
                        new Color(40, 0, 0, 0),
                        progress
                    );

                    Main.EntitySpriteDraw(
                        texture,
                        drawPos,
                        frame,
                        trailColor * 0.6f,
                        Projectile.oldRot[k],
                        origin,
                        Projectile.scale * (1f - progress * 0.5f),
                        effects,
                        0
                    );

                    Main.EntitySpriteDraw(
                        texture,
                        drawPos - Projectile.oldPos[k] * 0.5f + Projectile.oldPos[k + 1] * 0.5f,
                        frame,
                        trailColor * 0.45f,
                        (Projectile.oldRot[k] + Projectile.oldRot[k + 1]) * 0.5f,
                        origin,
                        Projectile.scale * (1f - progress * 0.5f),
                        effects,
                        0
                    );
                }

                Vector2 drawPos2 = Projectile.Center - Main.screenPosition;
                Color color = Color.White * Projectile.Opacity;

                Color outlineColor = new Color(255, 120, 30) * 0.7f * Projectile.Opacity;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(1.8f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i + Main.GlobalTimeWrappedHourly * 4f);
                    Main.EntitySpriteDraw(texture, drawPos2 + offset, frame, outlineColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
                }

                Main.EntitySpriteDraw(texture, drawPos2, frame, color, Projectile.rotation, origin, Projectile.scale, effects, 0);

                return false;
            }
        }
    
}