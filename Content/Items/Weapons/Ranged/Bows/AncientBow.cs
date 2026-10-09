using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class AncientBow : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<AncientBowHoldout>();
        public override int Damage => 24;
        public override int UseTime => 22;
        public override int ShotCooldown => 50;
        public override float HoldoutDistance => 10f;
        public override float BaseOffset => 12f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.LightRed;
        public override int Value => Item.buyPrice(gold: 2);
        public override float KnockBack => 3f;
        public override SoundStyle UseSound => SoundID.Item5 with { Volume = 0.5f };
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.GoldFlame;
        public override bool ItemGlow => true;
        public override Color ItemGlowColor => new Color(255, 205, 110);
    }

    public class AncientBowHoldout : BaseHoldoutProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/AncientBow";

        public override int ShotCooldown => 50;
        public override float HoldoutDistance => 10f;
        public override float BaseOffset => 12f;
        public override int DustType => DustID.GoldFlame;
        public override int ProjectileType => ModContent.ProjectileType<AncientArrow>();
        public override SoundStyle ShotSound => SoundID.Item5 with { Volume = 0.9f, Pitch = -0.15f };

        public override float ProjectileSpeed => 12f;
        public override float ChargeSpeedMultMin => 1f;
        public override float ChargeSpeedMultMax => 1f;
        public override float ChargeDamageMultMin => 1f;
        public override float ChargeDamageMultMax => 1f;
        public override float RecoilAmount => 0.4f;
        public override float DustScale => 1f;
        public override Vector3 LightColor => new Vector3(0.45f, 0.35f, 0.12f);
        public override Color GlowColor => new Color(255, 205, 110);
        public override float GlowIntensity => 1f;
        public override bool AimGlow => true;

        protected override void FireShot(Player player)
        {
            base.FireShot(player);

            if (Main.myPlayer != player.whoAmI || !player.HasAmmo(player.HeldItem))
                return;

            Vector2 spawnPos = Projectile.Center + Projectile.velocity * SpawnOffset;
            if (Collision.SolidCollision(spawnPos, 4, 4))
                spawnPos = player.Center + Projectile.velocity * (SpawnOffset + 5f);

            for (int sign = -1; sign <= 1; sign += 2)
            {
                Vector2 velocity = Projectile.velocity.RotatedBy(sign * 0.14f) * (ProjectileSpeed * 0.75f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ProjectileType,
                    Math.Max(1, Projectile.damage / 3),
                    Projectile.knockBack * 0.4f,
                    player.whoAmI,
                    1f
                );
            }
        }
    }

    public class AncientArrow : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WoodenArrowFriendly;

        private static readonly Color Gold = new Color(255, 205, 110);
        private static readonly Color Ghost = new Color(150, 220, 255);

        private bool Spectral => Projectile.ai[0] == 1f;
        private Color Tone => Spectral ? Ghost : Gold;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.aiStyle = -1;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                if (Spectral)
                {
                    Projectile.penetrate = 1;
                    Projectile.timeLeft = 120;
                }
            }

            Projectile.ai[1]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (!Spectral && Projectile.ai[1] > 40f)
                Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.06f, 16f);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Spectral ? DustID.BlueTorch : DustID.GoldFlame,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    100, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }

            Vector3 light = Tone.ToVector3() * 0.5f;
            Lighting.AddLight(Projectile.Center, light);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(target.width * 0.4f, target.height * 0.4f),
                    Spectral ? DustID.BlueTorch : DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3f, 3f), 80, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.position);

            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    Spectral ? DustID.BlueTorch : DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3.5f, 3.5f), 80, default, Main.rand.NextFloat(0.9f, 1.4f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Color tone = Tone with { A = 0 };

            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float k = i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    tone * ((1f - k) * 0.45f), Projectile.oldRot[i], origin, Projectile.scale * (1f - k * 0.35f), SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex, pos, null, tone * 0.5f, Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0);

            Color body = Spectral
                ? Color.Lerp(lightColor, Ghost, 0.5f) * 0.85f
                : Color.Lerp(lightColor, Color.White, 0.35f);
            Main.EntitySpriteDraw(tex, pos, null, body, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}