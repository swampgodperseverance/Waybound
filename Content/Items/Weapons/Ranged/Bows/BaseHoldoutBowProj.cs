using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Abstractions
{
    public abstract class BaseHoldoutBow : ModItem
    {
        public abstract int ProjectileType { get; }
        public abstract int Damage { get; }
        public abstract int UseTime { get; }
        public abstract int ShotCooldown { get; }
        public abstract float HoldoutDistance { get; }
        public abstract float BaseOffset { get; }
        public abstract int AmmoType { get; }
        public abstract int Rarity { get; }
        public abstract int Value { get; }
        public abstract float KnockBack { get; }
        public abstract SoundStyle UseSound { get; }
        public abstract SoundStyle ShotSound { get; }
        public abstract int DustType { get; }
        public virtual int SmokeDustType => DustID.Smoke;
        public virtual float FadeInTime => 20f;
        public virtual float AimResponsiveness => 0.7f;
        public virtual float RecoilAmount => 0.2f;
        public virtual float ChargeSpeedMultMin => 0.7f;
        public virtual float ChargeSpeedMultMax => 0.8f;
        public virtual float ChargeDamageMultMin => 0.5f;
        public virtual float ChargeDamageMultMax => 1.2f;
        public virtual float SpawnOffset => 35f;
        public virtual float ProjectileSpeed => 8f;
        public virtual int DustCount => 8;
        public virtual int SmokeDustCount => 4;
        public virtual float DustScale => 1.5f;
        public virtual float SmokeDustScale => 0.8f;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = Damage;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 26;
            Item.height = 28;
            Item.useTime = UseTime;
            Item.useAnimation = UseTime;
            Item.reuseDelay = 0;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = KnockBack;
            Item.value = Value;
            Item.rare = Rarity;
            Item.UseSound = UseSound;
            Item.shoot = ProjectileType;
            Item.shootSpeed = 1f;
            Item.useAmmo = AmmoType;
            Item.autoReuse = false;
            Item.channel = true;
            Item.noUseGraphic = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(
                source,
                position,
                velocity,
                ProjectileType,
                damage,
                knockback,
                player.whoAmI
            );
            return false;
        }
    }

    public abstract class BaseHoldoutProjectile : ModProjectile
    {
        private int _shotTimer;
        private float _targetHoldoutDistance;
        private float _recoilProgress;
        private float _smoothHoldDistance;
        private float _fadeInTimer;
        private float _maxFadeInTime;

        public abstract int ShotCooldown { get; }
        public abstract float HoldoutDistance { get; }
        public abstract float BaseOffset { get; }
        public abstract int DustType { get; }
        public abstract int ProjectileType { get; }
        public abstract SoundStyle ShotSound { get; }
        public virtual int SmokeDustType => DustID.Smoke;
        public virtual float FadeInTime => 20f;
        public virtual float AimResponsiveness => 0.7f;
        public virtual float RecoilAmount => 0.2f;
        public virtual float ChargeSpeedMultMin => 0.7f;
        public virtual float ChargeSpeedMultMax => 0.8f;
        public virtual float ChargeDamageMultMin => 0.5f;
        public virtual float ChargeDamageMultMax => 1.2f;
        public virtual float SpawnOffset => 35f;
        public virtual float ProjectileSpeed => 8f;
        public virtual int DustCount => 8;
        public virtual int SmokeDustCount => 4;
        public virtual float DustScale => 1.5f;
        public virtual float SmokeDustScale => 0.8f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 84;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
            Projectile.Opacity = 0f;

            _targetHoldoutDistance = HoldoutDistance;
            _maxFadeInTime = FadeInTime;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 120;

            _fadeInTimer = Math.Min(_fadeInTimer + 1f, _maxFadeInTime);
            Projectile.Opacity = MathHelper.Clamp(_fadeInTimer / _maxFadeInTime, 0f, 1f);

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true, true);

            if (Main.myPlayer == Projectile.owner)
            {
                UpdateAim(playerCenter, player);
                Projectile.netUpdate = true;
            }

            float aimDirection = Projectile.velocity.X > 0f ? 1f : -1f;
            if (Math.Abs(Projectile.velocity.X) < 0.01f) aimDirection = player.direction;

            player.ChangeDir((int)aimDirection);
            Projectile.spriteDirection = (int)aimDirection;
            Projectile.direction = (int)aimDirection;

            float targetDistance = _targetHoldoutDistance;
            if (_recoilProgress > 0f)
            {
                _recoilProgress -= 0.05f;
                targetDistance += 0.5f;
            }
            else
            {
                _recoilProgress = 0f;
                targetDistance = MathHelper.Lerp(targetDistance, HoldoutDistance, 0.05f);
            }

            _smoothHoldDistance = MathHelper.Lerp(_smoothHoldDistance, targetDistance, 0.15f);

            Vector2 holdPosition = playerCenter + Projectile.velocity * (_smoothHoldDistance + BaseOffset);

            Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
            if (Projectile.spriteDirection == -1)
            {
                holdPosition -= perpendicular * 4f;
            }
            else
            {
                holdPosition += perpendicular * 4f;
            }

            Projectile.Center = holdPosition;

            float rot = Projectile.velocity.ToRotation();
            if (Projectile.spriteDirection == -1)
                rot += MathHelper.Pi;
            Projectile.rotation = rot;

            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);

            float itemRot = Projectile.velocity.ToRotation();
            if (player.direction == -1)
                itemRot += MathHelper.Pi;
            player.itemRotation = itemRot;

            _shotTimer++;

            if (_recoilProgress > 0f)
            {
                _recoilProgress -= 0.05f;
            }
            else
            {
                _recoilProgress = 0f;
                _targetHoldoutDistance = MathHelper.Lerp(_targetHoldoutDistance, HoldoutDistance, 0.05f);
            }

            if (_shotTimer >= ShotCooldown)
            {
                _shotTimer = 0;
                FireShot(player);
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0f, 0.3f, 0f));
        }

        private void UpdateAim(Vector2 source, Player player)
        {
            Vector2 aimVector = Vector2.Normalize(Main.MouseWorld - source);

            if (aimVector.HasNaNs())
                aimVector = -Vector2.UnitY;

            aimVector = Vector2.Normalize(Vector2.Lerp(
                aimVector,
                Vector2.Normalize(Projectile.velocity),
                AimResponsiveness
            ));

            if (aimVector != Projectile.velocity)
                Projectile.netUpdate = true;

            Projectile.velocity = aimVector;
        }

        private void FireShot(Player player)
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

            _recoilProgress = RecoilAmount;
            _targetHoldoutDistance = 2f;

            SoundEngine.PlaySound(ShotSound with { Pitch = 0.1f + charge * 0.3f }, Projectile.Center);

            for (int i = 0; i < DustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    spawnPos - new Vector2(4, 4),
                    8, 8,
                    DustType,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    0,
                    default,
                    DustScale
                );
                dust.noGravity = true;
                dust.alpha = 80;
            }

            for (int i = 0; i < SmokeDustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    spawnPos - new Vector2(4, 4),
                    8, 8,
                    SmokeDustType,
                    Main.rand.NextFloat(-0.5f, 0f),
                    Main.rand.NextFloat(-0.2f, 0.2f),
                    0,
                    default,
                    SmokeDustScale
                );
                dust.noGravity = true;
                dust.alpha = 200;
            }
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity < 0.01f)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
            color *= Projectile.Opacity;

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                color,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}