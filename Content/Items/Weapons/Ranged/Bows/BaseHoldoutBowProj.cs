using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
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
        public virtual bool ItemGlow => false;
        public virtual Color ItemGlowColor => new Color(255, 225, 160);

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

        public override void PostUpdate()
        {
            if (!ItemGlow) return;

            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + Item.whoAmI);
            Lighting.AddLight(Item.Center, ItemGlowColor.ToVector3() * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (!ItemGlow) return;

            Texture2D texture = TextureAssets.Item[Type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.7f + 0.3f * MathF.Sin(time * 3f + whoAmI);
            Color glow = ItemGlowColor with { A = 0 };

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(2f, 0f).RotatedBy(time * 1.2f + MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, position + off, null, glow * (0.22f * pulse), rotation, origin, scale * 1.03f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, position, null, glow * (0.18f * pulse), rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);
        }
    }

    public abstract class BaseHoldoutProjectile : ModProjectile
    {
        public int _shotTimer;
        public float _targetHoldoutDistance;
        public float _recoilProgress;
        private float _smoothHoldDistance;
        private float _fadeInTimer;
        private float _maxFadeInTime;
        private float _flash;

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
        public virtual Vector3 LightColor => new Vector3(0.35f, 0.35f, 0.35f);
        public virtual Color GlowColor => new Color(255, 225, 160);
        public virtual float GlowIntensity => 1f;
        public virtual bool AimGlow => false;
        public virtual bool ChargeSparkles => true;

        protected float ChargeProgress => ShotCooldown <= 0 ? 0f : MathHelper.Clamp(_shotTimer / (float)ShotCooldown, 0f, 1f);

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
            _flash = Math.Max(0f, _flash - 0.08f);

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

            float charge = ChargeProgress;

            if (Main.netMode != NetmodeID.Server && charge > 0.6f)
            {
                float tremble = (charge - 0.6f) / 0.4f;
                holdPosition += Main.rand.NextVector2Circular(1f, 1f) * (tremble * tremble * 0.9f);
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

            if (_fadeInTimer == 1f)
                EquipVisuals();

            ChargeVisuals(ChargeProgress);

            if (_shotTimer >= ShotCooldown)
            {
                _shotTimer = 0;
                ShotVisuals(player);
                FireShot(player);
            }

            Lighting.AddLight(Projectile.Center, LightColor * (1f + ChargeProgress * 0.9f + _flash * 0.8f));
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

        private void EquipVisuals()
        {
            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dir * 10f, DustType, dir * 1.6f, 100, default, DustScale * 0.6f);
                dust.noGravity = true;
            }
        }

        private void ChargeVisuals(float charge)
        {
            if (Main.netMode == NetmodeID.Server || !ChargeSparkles) return;
            if (charge < 0.35f || Projectile.Opacity < 0.5f) return;

            int chance = Math.Max(1, (int)MathHelper.Lerp(7f, 1f, charge));
            if (!Main.rand.NextBool(chance)) return;

            Vector2 focus = Projectile.Center + Projectile.velocity * SpawnOffset * 0.5f;
            Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(18f, 34f);
            Dust dust = Dust.NewDustPerfect(focus + off, DustType, -off * 0.07f, 100, default, DustScale * 0.55f);
            dust.noGravity = true;
            dust.fadeIn = 0.8f;
        }

        protected virtual void ShotVisuals(Player player)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (!player.HasAmmo(player.HeldItem)) return;

            _flash = 1f;

            Vector2 muzzle = Projectile.Center + Projectile.velocity * SpawnOffset;
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(1.5f, 4.5f);
                Dust dust = Dust.NewDustPerfect(muzzle, DustType, vel, 80, default, DustScale * 0.7f);
                dust.noGravity = true;
            }
        }

        protected virtual void FireShot(Player player)
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

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);

            float charge = ChargeProgress;
            float intensity = GlowIntensity;
            float time = Main.GlobalTimeWrappedHourly;

            color = Color.Lerp(color, Color.White, charge * 0.2f * Math.Min(intensity, 1f));
            color *= Projectile.Opacity;

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            float strength = (charge * charge * 0.55f + _flash * 0.7f) * intensity * Projectile.Opacity;

            if (strength > 0.01f)
            {
                Color glow = GlowColor with { A = 0 };
                float pulse = 0.85f + 0.15f * MathF.Sin(time * 7f + charge * 4f);
                float spread = 1.5f + charge * 2.5f + _flash * 4f;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = new Vector2(spread, 0f).RotatedBy(MathHelper.PiOver2 * i + time * 1.5f);
                    Main.EntitySpriteDraw(texture, position + off, null, glow * (strength * 0.5f * pulse), Projectile.rotation, origin,
                        Projectile.scale * (1f + charge * 0.04f), effects, 0);
                }

                if (_flash > 0.01f)
                {
                    Main.EntitySpriteDraw(texture, position, null, glow * (_flash * 0.5f * intensity * Projectile.Opacity), Projectile.rotation, origin,
                        Projectile.scale * (1.05f + (1f - _flash) * 0.25f), effects, 0);
                }

                if (AimGlow && charge > 0.2f)
                {
                    Texture2D ray = TextureAssets.Extra[98].Value;
                    Vector2 rayOrigin = new Vector2(ray.Width / 2f, ray.Height);
                    Vector2 start = Projectile.Center + Projectile.velocity * SpawnOffset - Main.screenPosition;
                    float rayAlpha = (charge - 0.2f) / 0.8f * 0.35f * pulse * Projectile.Opacity;
                    Main.EntitySpriteDraw(ray, start, null, glow * rayAlpha, Projectile.velocity.ToRotation() + MathHelper.PiOver2, rayOrigin,
                        new Vector2(0.1f + 0.05f * pulse, 0.3f + charge * 1.1f), SpriteEffects.None, 0);
                }
            }

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