using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Magic.Staffs.PreHM
{
    public class Deepslate : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Magic;
            Item.width = 48;
            Item.height = 52;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3.8f;
            Item.value = Item.buyPrice(silver: 95);
            Item.rare = ItemRarityID.Green; 
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<DeepslateHoldout>();
            Item.shootSpeed = 1f;
            Item.mana = 5;
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
                ModContent.ProjectileType<DeepslateHoldout>(),
                damage,
                knockback,
                player.whoAmI
            );
            return false;
        }
    }

    public class DeepslateHoldout : BaseHoldoutStaffProj
    {
        public override string GlowTexture => "Waybound/Content/Items/Weapons/Magic/Staffs/PreHM/Deepslate";
        public override string GlowTexture2 => "Waybound/Content/Items/Weapons/Magic/Staffs/PreHM/Deepslate_Glow";

        public override int MaxCharge => 78;                    
        public override float HoldoutDistance => 24f;
        public override float AimResponsiveness => 0.85f;
        public override SoundStyle ChargeSound => SoundID.Item28;
        public override SoundStyle ShootSound => SoundID.Item43;
        public override int ProjectileType => ModContent.ProjectileType<DeepslateProj1>();
        public override float ProjectileSpeed => 12f;
        public override int ManaCostDivider => 3;
        public override float KnockbackForce => 2.8f;
        public override float SpriteRotationOffset => MathHelper.PiOver4;
        public override bool ApplyPlayerRecoil => false;
        public override bool DrawMainTexture => false;

        public override Color GlowColor => new Color(255, 110, 165);
        public override Color ChargeColor => new Color(255, 145, 190);
        public override float GlowScaleMultiplier => 1.3f;

        private int phase = 0;          
        private int burstTimer = 0;
        private int shotsFired = 0;
        private bool didFanShot = false;
        private int cooldownTimer = 0;
        private float smoothCharge;
        private float smoothPulse;
        private int flashTimer = 0;
        private float flashIntensity = 0f;

        private int angleDir = 1;
        private float dirPi = 0f;

        private Vector2 shakeOffset = Vector2.Zero;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            bool channeling = player.channel && Projectile.owner == Main.myPlayer;
            int manaCost = Math.Max(1, player.HeldItem.mana);

            if (!channeling || player.statMana < manaCost)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.timeLeft == 20)
                Projectile.timeLeft = 120;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true, true);
            UpdateAim(playerCenter);

            if (Main.myPlayer == Projectile.owner)
                Projectile.netUpdate = true;

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
                angleDir = 1;
                dirPi = 0f;
                Projectile.spriteDirection = 1;
            }
            else
            {
                player.ChangeDir(-1);
                angleDir = -1;
                dirPi = MathHelper.Pi;
                Projectile.spriteDirection = -1;
            }

            shakeOffset = Vector2.Zero;
            if (phase == 1)
            {
                float chargeRatio = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
                smoothCharge = MathHelper.Lerp(smoothCharge, chargeRatio, 0.06f);
                smoothPulse = MathHelper.Lerp(smoothPulse, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f), 0.1f);
                float shakeStr = chargeRatio * chargeRatio * 2.8f;
                shakeOffset = Main.rand.NextVector2Circular(shakeStr, shakeStr);
            }

            Projectile.Center = playerCenter + Projectile.velocity * HoldoutDistance + shakeOffset;
            Projectile.rotation = Projectile.velocity.ToRotation() + SpriteRotationOffset * angleDir + dirPi;

            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            Vector2 tip = Projectile.Center + Projectile.velocity * 28f;

            if (flashTimer > 0)
            {
                flashTimer--;
                flashIntensity = flashTimer / 14f;
            }


            if (phase == 3)
            {
                cooldownTimer--;
                if (cooldownTimer <= 0)
                {
                    phase = 0;
                    shotsFired = 0;
                    burstTimer = 0;
                }
                return;
            }
            if (phase == 0)
            {
                burstTimer++;

                if ((shotsFired == 0 && burstTimer >= 3) || (shotsFired > 0 && burstTimer >= 6))
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 shootVel = Projectile.velocity * (ProjectileSpeed * 0.95f);
                        shootVel = shootVel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-2.8f, 2.8f)));

                        int idx = Projectile.NewProjectile(
                            Projectile.GetSource_FromAI(),
                            tip,
                            shootVel,
                            ModContent.ProjectileType<DeepslateProj1>(),
                            Projectile.damage,
                            KnockbackForce,
                            player.whoAmI
                        );

                        if (idx >= 0 && idx < Main.maxProjectiles)
                        {
                            Main.projectile[idx].scale = Main.rand.NextFloat(0.95f, 1.12f);
                            Main.projectile[idx].netUpdate = true;
                        }
                    }

                    SoundEngine.PlaySound(ShootSound with { Volume = 0.5f, Pitch = 0.2f + shotsFired * 0.06f }, tip);
                    shotsFired++;
                    burstTimer = 0;

                    if (shotsFired >= 5)
                    {
                        phase = 1;
                        charge = 0;
                        charge2 = 0;
                        shotsFired = 0;
                    }
                }

                if (Main.rand.NextBool(3))
                    SpawnAmbientParticles();
            }

            else if (phase == 1)
            {
                charge++;
                charge2++;

                if (charge == MaxCharge - 12)
                    SoundEngine.PlaySound(ChargeSound with { Volume = 0.75f, Pitch = 0.45f }, Projectile.Center);

                if (charge >= MaxCharge)
                    phase = 2;

                SpawnSuctionParticles();
                SpawnAmbientParticles();
            }
     
            else if (phase == 2 && !didFanShot)
            {
                didFanShot = true;

                if (Projectile.owner == Main.myPlayer)
                {
                    player.CheckMana(manaCost / ManaCostDivider, true, false);

                    for (int i = -1; i <= 1; i++)
                    {
                        float spread = MathHelper.ToRadians(i * 12f);
                        Vector2 shootVel = Projectile.velocity.RotatedBy(spread) * (ProjectileSpeed * 1.2f);

                        int idx = Projectile.NewProjectile(
                            Projectile.GetSource_FromAI(),
                            tip,
                            shootVel,
                            ModContent.ProjectileType<DeepslateProj2>(),
                            (int)(Projectile.damage * 1.3f),
                            KnockbackForce + 1.2f,
                            player.whoAmI
                        );

                        if (idx >= 0 && idx < Main.maxProjectiles)
                        {
                            Main.projectile[idx].scale = Main.rand.NextFloat(1.1f, 1.3f);
                            Main.projectile[idx].netUpdate = true;
                        }
                    }
                }

                SoundEngine.PlaySound(ShootSound with { Volume = 0.95f, Pitch = -0.15f }, tip);
                SoundEngine.PlaySound(SoundID.Item68 with { Volume = 0.4f, Pitch = 0.35f }, tip);

                SpawnBurstParticles();
                TriggerFlash();                 
                phase = 3;
                cooldownTimer = 30;
                charge = 0;
                charge2 = 0;
                didFanShot = false;
            }
            float intensity = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
            Lighting.AddLight(Projectile.Center, GlowColor.ToVector3() * (0.5f + intensity * 1.1f));
        }

        private void UpdateAim(Vector2 source)
        {
            Vector2 aim = Main.MouseWorld - source;
            if (aim.HasNaNs() || aim == Vector2.Zero)
                aim = -Vector2.UnitY;
            else
                aim.Normalize();

            Vector2 newVelocity = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, aim, AimResponsiveness));
            if (newVelocity.HasNaNs())
                newVelocity = aim;

            if (newVelocity != Projectile.velocity)
                Projectile.netUpdate = true;

            Projectile.velocity = newVelocity;
        }

        private void TriggerFlash()
        {
            flashTimer = 14;
            flashIntensity = 1f;
        }

public override void PostDraw(Color lightColor)
{
    Texture2D glow = GetCachedTexture(ref cachedGlow, ref cachedGlowPath, GlowTexture);
    Vector2 position = Projectile.Center - Main.screenPosition;
    SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    float pulse = smoothPulse;
    float pulseAmount = smoothCharge * (0.15f + pulse * 0.35f);

    Color baseGlow = Color.White * (0.6f + smoothCharge * 0.4f);

    Main.EntitySpriteDraw(
        glow,
        position,
        null,
        baseGlow,
        Projectile.rotation,
        glow.Size() * 0.5f,
        Projectile.scale * GlowScaleMultiplier * (1f + pulseAmount * 0.2f),
        effects,
        0f
    );

    if (!string.IsNullOrEmpty(GlowTexture2))
    {
        Texture2D glow2 = GetCachedTexture(ref cachedGlow2, ref cachedGlow2Path, GlowTexture2);

        float pulse2 = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + MathHelper.PiOver2);
        float scalePulse = 1f + pulse2 * 0.25f * smoothCharge;
        Color glowColor = GlowColor * (0.4f + smoothCharge * 0.8f * (0.7f + pulse2 * 0.3f));

        Main.EntitySpriteDraw(
            glow2,
            position,
            null,
            glowColor,
            Projectile.rotation,
            glow2.Size() * 0.5f,
            Projectile.scale * GlowScaleMultiplier * scalePulse,
            effects,
            0f
        );
    }
}

        private void SpawnSuctionParticles()
        {
            if (Main.dedServ) return;

            float chargeRatio = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
            int count = (int)(chargeRatio * 4f);

            for (int i = 0; i < count; i++)
            {
                float radius = Main.rand.NextFloat(50f, 120f);
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
                Vector2 spawnPos = Projectile.Center + offset;
                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(6f, 12f);

                ParticleSystem.MegasparkBuffer?.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(Main.rand.NextFloat(0.9f, 1.7f)),
                    new Color(255, 130, 180, 220),
                    Main.rand.Next(18, 30)
                ));
            }
        }

        protected override void SpawnAmbientParticles()
        {
            if (Main.dedServ || !Main.rand.NextBool(5)) return;

            Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
            Vector2 vel = Main.rand.NextVector2Circular(0.3f, 0.3f);

            ParticleSystem.MegasparkBuffer?.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(Main.rand.NextFloat(0.8f, 1.3f)),
                new Color(255, 150, 195, 180),
                28
            ));
        }

        protected override void SpawnBurstParticles()
        {
            if (Main.dedServ) return;

            Vector2 tip = Projectile.Center + Projectile.velocity * 28f;

            for (int i = 0; i < 14; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.4f, 1.4f) * 3.8f;
                ParticleSystem.MegasparkBuffer?.Create(new ParticleInfo(
                    tip.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(Main.rand.NextFloat(6f, 10f)),
                    new Color(255, 100, 160),
                    45
                ));
            }
        }

        protected override void SpawnChargeParticles() { }
    }
}