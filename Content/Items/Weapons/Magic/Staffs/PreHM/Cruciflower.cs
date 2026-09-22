using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Rarities;
using Waybound.Content.Items.Weapons.Magic.Staffs;
using Waybound.Content.Projectiles.Magic.Staffs.PreHM;
using Waybound.Content.Projectiles.Ranged.Guns.PreHM;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;
namespace Waybound.Content.Items.Weapons.Magic.Staffs.PreHM
{
    public class Cruciflower : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 28;
            Item.DamageType = DamageClass.Magic;
            Item.width = 48;
            Item.height = 52;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3.5f;
            Item.value = Item.buyPrice(silver: 75);
            Item.rare = RarityType<IceShimer>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<CruciflowerHoldout>();
            Item.shootSpeed = 1f;
            Item.mana = 12;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) => false;
        public override Vector2? HoldoutOffset() => new Vector2(-2f, 0f);
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(
            source,
            position,
            velocity,
            ModContent.ProjectileType<CruciflowerHoldout>(),
            damage,
            knockback,
            player.whoAmI
            );
            return false;
        }
    }
    public class CruciflowerHoldout : BaseHoldoutStaffProj
    {
        public override string GlowTexture => "Waybound/Content/Items/Weapons/Magic/Staffs/PreHM/Cruciflower";
        public override string GlowTexture2 => "Waybound/Content/Items/Weapons/Magic/Staffs/PreHM/Cruciflower_Glow";
        public override int MaxCharge => 60;
        public override float HoldoutDistance => 20f;
        public override float AimResponsiveness => 0.8f;
        public override SoundStyle ChargeSound => SoundID.Item28;
        public override SoundStyle ShootSound => SoundID.Item28;
        public override int ProjectileType => ModContent.ProjectileType<CruciflowerProj1>();
        public override float ProjectileSpeed => 7f;
        public override int ManaCostDivider => 3;
        public override float KnockbackForce => 0f;
        public override float SpriteRotationOffset => MathHelper.PiOver4;
        public override Color ChargeColor => new Color(150, 225, 255);
        public override bool ApplyPlayerRecoil => false;
        public override bool DrawMainTexture => false;

        public int SuccessfulHits = 0;
        public bool CloudMode = false;
        public int CloudModeTimer = 0;
        private int cloudWhoAmI = -1;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!CloudMode && SuccessfulHits >= 10)
            {
                CloudMode = true;
                CloudModeTimer = 0;
                SuccessfulHits = 0;

                if (Projectile.owner == Main.myPlayer)
                {
                    int idx = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        player.Center + new Vector2(0f, -280f),
                        Vector2.Zero,
                        ModContent.ProjectileType<CruciflowerProj2>(),
                        Projectile.damage,
                        0f,
                        player.whoAmI
                    );
                    cloudWhoAmI = idx;
                }

                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f, Pitch = 0.3f }, player.Center);
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.5f, Pitch = -0.2f }, player.Center);
            }

            if (CloudMode)
            {
                CloudModeTimer++;

                Projectile cloud = null;
                if (cloudWhoAmI >= 0 && cloudWhoAmI < Main.maxProjectiles && Main.projectile[cloudWhoAmI].active)
                {
                    cloud = Main.projectile[cloudWhoAmI];
                }
                else
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                            Main.projectile[i].type == ModContent.ProjectileType<CruciflowerProj2>())
                        {
                            cloud = Main.projectile[i];
                            cloudWhoAmI = i;
                            break;
                        }
                    }
                }

                if (cloud == null)
                {
                    CloudMode = false;
                    return;
                }

                Vector2 toCloud = cloud.Center - player.Center;
                float desiredRot = toCloud.ToRotation();

                Projectile.rotation = Projectile.rotation.AngleLerp(desiredRot + SpriteRotationOffset, 0.12f);
                Projectile.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation - SpriteRotationOffset);

                float raiseProgress = MathHelper.Clamp(CloudModeTimer / 40f, 0f, 1f);
                float extraHeight = MathHelper.Lerp(0f, 18f, raiseProgress);
                Projectile.Center = player.Center + Projectile.velocity * (HoldoutDistance + extraHeight);

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, desiredRot - MathHelper.PiOver2);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, desiredRot - MathHelper.PiOver2);

                player.itemRotation = desiredRot - MathHelper.PiOver2;
                player.itemTime = 0;
                player.itemAnimation = 0;

                Projectile.timeLeft = 2;
            }
            else
            {
                base.AI();
            }
        }

        public override bool? CanDamage() => CloudMode ? false : null;

        public override bool ShouldUpdatePosition() => !CloudMode;

        public override Color GlowColor => CloudMode
            ? Color.Lerp(new Color(110, 195, 255), new Color(180, 240, 255), 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f))
            : new Color(110, 195, 255);

        public override float GlowScaleMultiplier => CloudMode ? 1.55f : 1.2f;

        protected override void SpawnChargeParticles()
        {
            if (Main.dedServ) return;
            float chargeRatio = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
            int count = (int)(MathF.Pow(chargeRatio, 3.2f) * 3f);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Projectile.velocity * Main.rand.Next(6, 14) * 0.13f;
                Vector2 pos = Projectile.Center + offset + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
                vel.Y -= Main.rand.NextFloat(0.3f, 0.9f);
                float scale = Main.rand.NextFloat(1.1f, 1.9f);
                ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    ChargeColor,
                    45
                ));
            }
        }

        protected override void SpawnAmbientParticles()
        {
            if (Main.dedServ || charge <= 0 || !Main.rand.NextBool(5)) return;
            Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(16f, 16f);
            Vector2 vel = Main.rand.NextVector2Circular(0.4f, 0.4f);
            float scale = Main.rand.NextFloat(0.9f, 1.6f);
            ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                ChargeColor * 0.85f,
                35
            ));
        }

        protected override void SpawnBurstParticles()
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * 3.2f;
                float scale = Main.rand.NextFloat(8f, 12f);
                ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    ChargeColor,
                    55
                ));
            }
        }
    }
}