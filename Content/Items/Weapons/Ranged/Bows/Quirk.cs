using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Ranged.Bows;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class Quirk : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<QuirkHoldout>();
        public override int Damage => 32;
        public override int UseTime => 40;
        public override int ShotCooldown => 35;
        public override float HoldoutDistance => 8f;
        public override float BaseOffset => 12f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Blue;
        public override int Value => Item.sellPrice(0, 0, 80, 0);
        public override float KnockBack => 2f;
        public override SoundStyle UseSound => SoundID.Item5;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.Ice;
    }

    public class QuirkHoldout : BaseHoldoutProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/Quirk";
        public override int ShotCooldown => 35;
        public override Vector3 LightColor => new Vector3(0.45f, 0.45f, 0.45f);
        public override float HoldoutDistance => 5f;
        public override float BaseOffset => 10f;
        public override int DustType => DustID.Ice;
        public override int ProjectileType => ProjectileID.WoodenArrowFriendly;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override float RecoilAmount => 0.08f;

        // Glow
        private float _glowPulse;
        private const float GlowPulseDecay = 0.88f;          
        private const float GlowPulseStrength = 0.55f;      

        public override void AI()
        {
            base.AI();

            if (_glowPulse > 0.01f)
                _glowPulse *= GlowPulseDecay;
            else
                _glowPulse = 0f;
        }

        protected override void FireShot(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return;

            Item heldItem = player.HeldItem;
            if (!player.HasAmmo(heldItem))
                return;

            Vector2 spawnPos = Projectile.Center + Projectile.velocity * SpawnOffset;
            if (Collision.SolidCollision(spawnPos, 4, 4))
            {
                spawnPos = player.Center + Projectile.velocity * (SpawnOffset + 5f);
            }

            Vector2 velocity = Projectile.velocity * ProjectileSpeed;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;

            float charge = MathHelper.Clamp((float)_shotTimer / ShotCooldown, 0f, 1f);
            float speedMult = ChargeSpeedMultMin + charge * (ChargeSpeedMultMax - ChargeSpeedMultMin);
            float damageMult = ChargeDamageMultMin + charge * (ChargeDamageMultMax - ChargeDamageMultMin);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                velocity * speedMult,
                ProjectileType,
                (int)(damage * damageMult),
                knockback * charge,
                player.whoAmI
            );

            if (Main.rand.NextFloat() < 0.30f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity * speedMult * 1.30f,
                    ModContent.ProjectileType<QuirkProj>(),
                    (int)(damage * damageMult * 0.85f),
                    knockback * charge,
                    player.whoAmI
                );

                _glowPulse = 1f;
            }

            _recoilProgress = RecoilAmount;
            _targetHoldoutDistance = 2f;

            SoundEngine.PlaySound(ShotSound with { Pitch = 0.1f + charge * 0.3f }, Projectile.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    float size = Main.rand.NextFloat(14f, 22f);
                    Vector2 vel = Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(1.6f, 1.6f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                        new Color(100, 210, 255, 175) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(16, 26)
                    ));
                }

                for (int i = 0; i < 6; i++)
                {
                    float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(4f, 10f);
                    Vector2 pSpawn = spawnPos + trailOffset;
                    Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.3f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pSpawn.ToNumerics(),
                        vel.ToNumerics(),
                        trailAngle,
                        new SystemVector2(Main.rand.NextFloat(12f, 18f)),
                        new Color(70, 190, 255, 150) * Main.rand.NextFloat(0.85f, 1.15f),
                        Main.rand.Next(12, 20)
                    ));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );

            if (_glowPulse > 0.12f)
            {
                Texture2D glow = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Ranged/Bows/Quirk_Glow").Value;

                float intensity = _glowPulse * GlowPulseStrength;
                Color glowColor = new Color(120, 220, 255, 0) * intensity; 

                Main.EntitySpriteDraw(
                    glow,
                    drawPos,
                    null,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0
                );
            }

            return false; 
        }
    }
}