using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public abstract class SpinnablePanItem : ModItem
    {
        public abstract int SpinProjectileType { get; }
        public abstract int ThrowProjectileType { get; }

        public virtual int SpinUseTime => 20;
        public virtual int ThrowUseTime => 30;
        public virtual float ThrowSpeed => 14f;
        public virtual int BaseDamage => 15;
        public virtual float Knockback => 6f;

        public virtual int ThrowDebuffType => BuffID.OnFire;
        public virtual int ThrowDebuffDuration => 480;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 26;
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = SpinUseTime;
            Item.useAnimation = SpinUseTime;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = Knockback;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = SpinProjectileType;
            Item.shootSpeed = 0f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = ThrowUseTime;
                Item.useAnimation = ThrowUseTime;
                Item.shoot = ThrowProjectileType;
                Item.shootSpeed = ThrowSpeed;
                Item.channel = false;
                Item.autoReuse = false;
                Item.UseSound = SoundID.Item1;
            }
            else
            {
                Item.useTime = SpinUseTime;
                Item.useAnimation = SpinUseTime;
                Item.shoot = SpinProjectileType;
                Item.shootSpeed = 0f;
                Item.channel = true;
                Item.autoReuse = true;
                Item.UseSound = null;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
                Projectile.NewProjectile(source, player.MountedCenter, dir * ThrowSpeed, ThrowProjectileType, damage, knockback, player.whoAmI);
                return false;
            }

            if (player.ownedProjectileCounts[SpinProjectileType] > 0)
                return false;

            Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, SpinProjectileType, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public abstract class SpinnablePanProjectile : ModProjectile
    {
        protected Player Player => Main.player[Projectile.owner];
        protected ref float Timer => ref Projectile.ai[0];
        protected ref float CurrentSpinSpeed => ref Projectile.localAI[2];
        public virtual float SpinAccelRate => 0.006f;
public virtual float SpinDecelRate => 0.010f;
        public virtual int ProjWidth => 32;
        public virtual int ProjHeight => 26;
        public virtual float MaxSpinTime => 260f;
        public virtual float ParticleStart => 55f;
        public virtual float OutlineStart => 75f;
        public virtual float OrbitRadius => 8f;
        public virtual float SpriteLength => 26f;
        public virtual float MinRotationSpeed => 0.02f;
        public virtual float MaxRotationSpeed => 0.28f;
        public virtual float ScaleStart => 1f;
        public virtual float ScalePeak => 1.15f;
        public virtual float ScaleAtFade => 0.85f;
        public virtual int BaseBuffType => BuffID.WellFed;
        public virtual int BaseBuffDuration => 360;
        public virtual int BuffInterval => 40;
        public virtual int BuffStartTime => 90;
        public virtual Color ParticleColor => new Color(255, 120, 30, 210);
        public virtual float ParticleSizeMin => 12f;
        public virtual float ParticleSizeMax => 20f;
        public virtual float ParticleSizeBonus => 6f;
        public virtual int ParticleLifeMin => 16;
        public virtual int ParticleLifeMax => 28;
        public virtual int ParticleSpawnRate => 2;
        public virtual int ParticleCountMin => 1;
        public virtual int ParticleCountMax => 5;
        public virtual bool CanShoot => false;
        public virtual int ShootInterval => 45;
        public virtual float ShootSpeed => 10f;
        public virtual int ShootProjectileType => -1;
        public virtual int ShootDamageDivider => 2;
        public virtual float ShootMinProgress => 0.55f;
        public virtual float ShootMinSpinSpeed => 0.18f;
        public virtual SoundStyle? SpinSound => SoundID.Item1;
        public virtual int SpinSoundInterval => 6;
        public virtual float SpinAccelFactor => 0.08f;
        public virtual float SpinDecelFactor => 0.04f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = ProjWidth;
            Projectile.height = ProjHeight;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 200;
            Projectile.noEnchantmentVisuals = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            if (!Player.active || Player.dead || Player.noItems || Player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Timer++;
            Projectile.direction = Player.direction;
            Projectile.spriteDirection = Player.direction;

            float progress = MathHelper.Clamp(Timer / MaxSpinTime, 0f, 1f);
            float eased = EaseInOutCubic(progress);
            bool stillSpinning = Player.channel && Timer <= MaxSpinTime;

            float targetSpeed = stillSpinning
                ? MathHelper.Lerp(MinRotationSpeed, MaxRotationSpeed, eased)
                : 0f;

            if (CurrentSpinSpeed < targetSpeed)
                CurrentSpinSpeed = Math.Min(targetSpeed, CurrentSpinSpeed + SpinAccelRate);
            else if (CurrentSpinSpeed > targetSpeed)
                CurrentSpinSpeed = Math.Max(targetSpeed, CurrentSpinSpeed - SpinDecelRate);

            if (!stillSpinning && Math.Abs(CurrentSpinSpeed) < 0.002f)
                CurrentSpinSpeed = 0f;

            Projectile.rotation += CurrentSpinSpeed * Player.direction;

            Projectile.Center = Player.MountedCenter;

            float targetScale = MathHelper.Lerp(ScaleStart, ScalePeak, eased);
            Projectile.scale = MathHelper.Lerp(Projectile.scale, targetScale, 0.08f);

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Timer * 0.1f);
            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, pulse, 0.12f);

            SpawnSpinParticles(eased);
            UpdateLight(eased);
            UpdateBuffs();
            UpdateSound();

            if (CanShoot
                && ShootProjectileType > 0
                && Main.myPlayer == Projectile.owner
                && stillSpinning
                && Timer % ShootInterval == 0
                && eased >= ShootMinProgress
                && Math.Abs(CurrentSpinSpeed) >= ShootMinSpinSpeed)
            {
                ShootProjectile();
            }

            if (!Player.channel || Timer > MaxSpinTime)
                Player.reuseDelay = 10;

            if (!stillSpinning)
            {
                Projectile.localAI[0]++;
                float fade = MathHelper.Clamp(Projectile.localAI[0] / 35f, 0f, 1f);
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, fade * 0.2f);
                Projectile.scale = MathHelper.Lerp(Projectile.scale, ScaleAtFade, fade * 0.12f);

                if (fade >= 1f && Math.Abs(CurrentSpinSpeed) < 0.005f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                Projectile.localAI[0] = 0f;
                Projectile.timeLeft = 30;
            }

            UpdatePlayerVisuals();
        }

        protected virtual void SpawnSpinParticles(float eased)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Timer <= ParticleStart) return;
            if (Timer % ParticleSpawnRate != 0) return;

            float spinStrength = MathHelper.Clamp((Timer - ParticleStart) / 80f, 0f, 1f) * eased;
            int count = (int)MathHelper.Lerp(ParticleCountMin, ParticleCountMax, spinStrength);

            float tang = Projectile.rotation + MathHelper.PiOver2 * Player.direction;
            Vector2 trailDir = tang.ToRotationVector2();
            Vector2 orbitDir = Projectile.rotation.ToRotationVector2();

            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat(0.35f, 1f);
                Vector2 edge = orbitDir * (OrbitRadius + SpriteLength * along);
                Vector2 spawnPos = Projectile.Center + edge;
                Vector2 vel = -trailDir * Main.rand.NextFloat(1.2f, 3.5f + 2.5f * spinStrength)
                    + Main.rand.NextVector2Circular(0.4f, 0.4f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(ParticleSizeMin, ParticleSizeMax + ParticleSizeBonus * spinStrength)),
                    ParticleColor,
                    Main.rand.Next(ParticleLifeMin, ParticleLifeMax)
                ));
            }
        }

        protected virtual void UpdateLight(float eased)
        {
            if (Timer <= OutlineStart) return;
            float outlineStrength = MathHelper.Clamp((Timer - OutlineStart) / 60f, 0f, 1f);
            Vector2 lightPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (OrbitRadius + SpriteLength * 0.5f);
            Lighting.AddLight(lightPos, 1.0f * outlineStrength * Projectile.Opacity, 0.3f * outlineStrength * Projectile.Opacity, 0.04f);
        }

        protected virtual void UpdateBuffs()
        {
            if (BaseBuffType <= 0) return;
            if (Timer > BuffStartTime && Timer % BuffInterval == 0)
                Player.AddBuff(BaseBuffType, BaseBuffDuration);
        }

        protected virtual void UpdateSound()
        {
            if (SpinSound == null) return;
            if (Timer % SpinSoundInterval != 0) return;
            if (Math.Abs(CurrentSpinSpeed) < 0.08f) return;

            if (Projectile.localAI[1] <= 0)
            {
                SoundEngine.PlaySound(SpinSound.Value with
                {
                    Volume = 0.8f + Main.rand.NextFloat(-0.05f, 0.05f),
                    Pitch = Main.rand.NextFloat(-0.2f, 0.1f)
                }, Projectile.Center);
                Projectile.localAI[1] = 20;
            }
            else
            {
                Projectile.localAI[1]--;
            }
        }

        protected virtual void ShootProjectile()
        {
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Vector2 spawnPos = Projectile.Center + dir * (OrbitRadius + SpriteLength);
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                dir * ShootSpeed,
                ShootProjectileType,
                Projectile.damage / ShootDamageDivider,
                Projectile.knockBack,
                Projectile.owner
            );
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.7f }, spawnPos);
        }

        protected virtual void UpdatePlayerVisuals()
        {
            if (Player.channel)
                Projectile.timeLeft = 30;

            Player.ChangeDir(Projectile.direction);
            Player.heldProj = Projectile.whoAmI;
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            float armRot = Projectile.rotation;
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot - MathHelper.PiOver2);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 orbitDir = Projectile.rotation.ToRotationVector2();
            Vector2 bladeStart = Projectile.Center + orbitDir * (OrbitRadius - 6f);
            Vector2 bladeEnd = Projectile.Center + orbitDir * (OrbitRadius + SpriteLength);
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                bladeStart,
                bladeEnd,
                14f,
                ref point
            );
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.active || target.CountsAsACritter || target.immortal || target.dontTakeDamage)
                return;
            target.AddBuff(BuffID.OnFire, 480);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(Projectile.spriteDirection == 1 ? texture.Width : 0f, texture.Height);

            float drawRotation;
            if (Projectile.spriteDirection == 1)
                drawRotation = Projectile.rotation - MathHelper.PiOver2 + MathHelper.Pi;
            else
                drawRotation = Projectile.rotation;

            Vector2 orbitDir = Projectile.rotation.ToRotationVector2();
            Vector2 drawCenter = Projectile.Center + orbitDir * OrbitRadius;
            Vector2 drawPos = drawCenter - Main.screenPosition;
            Color color = Color.White * Projectile.Opacity;

            DrawTrail(texture, origin, effects);
            DrawGlow(texture, origin, effects, drawPos, drawRotation);
            Main.EntitySpriteDraw(texture, drawPos, null, color, drawRotation, origin, Projectile.scale, effects, 0);
            return false;
        }

        protected virtual void DrawTrail(Texture2D texture, Vector2 origin, SpriteEffects effects)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(255, 130, 40, 0) * 0.25f * fade * Projectile.Opacity;
                Vector2 oldOrbitDir = Projectile.oldRot[i].ToRotationVector2();
                Vector2 oldDrawCenter = Projectile.oldPos[i] + Projectile.Size / 2f + oldOrbitDir * OrbitRadius;

                float oldDrawRotation;
                if (Projectile.spriteDirection == 1)
                    oldDrawRotation = Projectile.oldRot[i] - MathHelper.PiOver2 + MathHelper.Pi;
                else
                    oldDrawRotation = Projectile.oldRot[i];

                Main.EntitySpriteDraw(texture, oldDrawCenter - Main.screenPosition, null, trail, oldDrawRotation, origin, Projectile.scale * (1f - i * 0.03f), effects, 0);
            }
        }

        protected virtual void DrawGlow(Texture2D texture, Vector2 origin, SpriteEffects effects, Vector2 drawPos, float drawRotation)
        {
            if (Timer <= OutlineStart) return;
            float strength = MathHelper.Clamp((Timer - OutlineStart) / 55f, 0f, 1f) * Projectile.Opacity;
            Color glow = new Color(255, 110, 25, 0) * strength * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.2f * strength, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, drawRotation, origin, Projectile.scale * 1.05f, effects, 0);
            }
        }

        protected static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - (float)Math.Pow(-2f * t + 2f, 3) / 2f;
        }
    }

    public abstract class ThrownPanProjectile : ModProjectile
    {
        protected Player Player => Main.player[Projectile.owner];
        protected ref float Timer => ref Projectile.ai[0];
        protected ref float Returning => ref Projectile.ai[1];

        public virtual int ProjWidth => 32;
        public virtual int ProjHeight => 26;
        public virtual float MaxOutTime => 45f;
        public virtual float MaxRange => 420f;
        public virtual float ReturnAccel => 0.9f;
        public virtual float ReturnMaxSpeed => 22f;
        public virtual float SpinSpeed => 0.35f;
        public virtual int AirTime => 600;
        public virtual Color ParticleColor => new Color(255, 120, 30, 210);
        public virtual float ParticleSizeMin => 10f;
        public virtual float ParticleSizeMax => 18f;
        public virtual int ParticleLifeMin => 14;
        public virtual int ParticleLifeMax => 24;
        public virtual string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public virtual Color HitTextColor => Color.OrangeRed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = ProjWidth;
            Projectile.height = ProjHeight;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.timeLeft = AirTime;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Timer++;

            Projectile.rotation += SpinSpeed * (Projectile.velocity.X >= 0 ? 1f : -1f);
            Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;

            if (Returning == 0f)
            {
                float dist = Vector2.Distance(Projectile.Center, Player.Center);
                if (Timer >= MaxOutTime || dist >= MaxRange)
                    Returning = 1f;
            }
            else
            {
                Vector2 toPlayer = Player.Center - Projectile.Center;
                float dist = toPlayer.Length();

                if (dist < 24f)
                {
                    Projectile.Kill();
                    return;
                }

                toPlayer.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * ReturnMaxSpeed, 0.12f);
                Projectile.velocity += toPlayer * ReturnAccel * 0.15f;

                if (Projectile.velocity.Length() > ReturnMaxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * ReturnMaxSpeed;
            }

            SpawnAirParticles();
            Lighting.AddLight(Projectile.Center, 0.9f, 0.3f, 0.05f);
        }

        protected virtual void SpawnAirParticles()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (!Main.rand.NextBool(2)) return;

            Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
            Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.2f, 1.2f);

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                spawn.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(Main.rand.NextFloat(ParticleSizeMin, ParticleSizeMax)),
                ParticleColor,
                Main.rand.Next(ParticleLifeMin, ParticleLifeMax)
            ));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Returning == 0f)
                Returning = 1f;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.active || target.CountsAsACritter || target.immortal || target.dontTakeDamage)
                return;

            Item held = Player.HeldItem;
            if (held?.ModItem is SpinnablePanItem panItem && panItem.ThrowDebuffType > 0)
            {
                target.AddBuff(panItem.ThrowDebuffType, panItem.ThrowDebuffDuration);
            }

            string word = HitWords[Main.rand.Next(HitWords.Length)];
            CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 10, 10), HitTextColor, word, false);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);

            if (Returning == 0f)
                Returning = 1f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = -0.2f }, Projectile.Center);
            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 14; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dir = angle.ToRotationVector2();
                Vector2 spawn = Projectile.Center + dir * Main.rand.NextFloat(4f, 16f);
                Vector2 vel = dir * Main.rand.NextFloat(1.5f, 4f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawn.ToNumerics(),
                    vel.ToNumerics(),
                    angle,
                    new System.Numerics.Vector2(Main.rand.NextFloat(ParticleSizeMin, ParticleSizeMax)),
                    ParticleColor,
                    Main.rand.Next(ParticleLifeMin, ParticleLifeMax)
                ));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(Projectile.spriteDirection == 1 ? texture.Width : 0f, texture.Height);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Color.White * Projectile.Opacity;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(255, 130, 40, 0) * 0.35f * fade * Projectile.Opacity;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trail, Projectile.oldRot[i], origin, Projectile.scale, effects, 0);
            }

            Color glow = new Color(255, 110, 25, 0) * 0.6f * Projectile.Opacity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class PanPlayer : ModPlayer
    {
        public float PanBonusDamage;
        public int PanCombo;
        public int PanComboTimer;

        public override void PostUpdate()
        {
            if (PanComboTimer > 0)
            {
                PanComboTimer--;
                if (PanComboTimer <= 0)
                {
                    PanCombo = 0;
                    PanBonusDamage = 0f;
                }
            }
            else
            {
                PanBonusDamage = 0f;
            }
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (item.ModItem is SpinnablePanItem && PanBonusDamage > 0f)
                damage += PanBonusDamage;
        }
    }
}