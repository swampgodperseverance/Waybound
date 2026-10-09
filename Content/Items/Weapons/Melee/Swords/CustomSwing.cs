using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{
    public abstract class CustomSwing : ModProjectile
    {
        //For diagonal swords only
        public virtual float SwordLength
        {
            get
            {
                if (autoLength <= 0f)
                {
                    if (Main.dedServ)
                        return 64f;
                    Main.instance.LoadProjectile(Type);
                    Texture2D tex = TextureAssets.Projectile[Type].Value;
                    autoLength = MathF.Sqrt(tex.Width * tex.Width + tex.Height * tex.Height);
                }
                return autoLength;
            }
        }
        private float autoLength;

        public virtual float MaxTime => 32f;               
        public virtual bool DebugMode => false;             
        public virtual float BaseScale => 1f;
        public virtual float SwingArc => 3.5f;               
        public virtual float AngleOffset => 0.3f;          
        public virtual float HitWidth => 10f;              
        public virtual float ActiveWindowStart => 0.12f;    
        public virtual float ActiveWindowEnd => 0.92f;
        public virtual int LocalHitCooldown => -1;           

        public virtual float FinisherArcMultiplier => 1.15f;
        public virtual float FinisherScaleMultiplier => 1.2f;
        public virtual float SwingEase(float t)
        {
            float smooth = t * t * (3f - 2f * t);
            return MathHelper.Lerp(t, smooth, 0.7f);
        }

        public virtual float ScaleCurve(float t) => 1f + 0.2f * MathF.Sin(MathF.PI * t);
        public virtual bool DrawTrail => true;
        public virtual int TrailDrawLength => Projectile.oldPos.Length / 3;
        public virtual Color TrailColor => new Color(120, 200, 255);
        public virtual Color TrailCoreColor => new Color(180, 230, 255);
        public virtual bool UseLight => true;

        public virtual bool SpawnSwingParticles => true;
        public virtual Color ParticleColor => new Color(120, 200, 255);
        public virtual Color ParticleCoreColor => new Color(180, 230, 255);
        public virtual float ParticleWidth => 1f;
        public virtual float ParticleSizeMultiplier => 1f;
        public virtual float ParticleSpeedMultiplier => 1f;
        public virtual int ParticleLife => 22;
        public virtual int ParticleCount => 2;

        public virtual int DustType => -1;
        public virtual Color DustColor => default;
        public virtual bool DustNoGravity => false;

        public virtual SoundStyle? SwingSound => SoundID.Item1; // i should add the new sound Sky has sent byt i dont want to since the new git version has it and i dont want to fix merging errors so later i hope


        public Player Player => Main.player[Projectile.owner];
        public virtual ref float Timer => ref Projectile.ai[0];
        public virtual ref float SwingDirection => ref Projectile.ai[1];

        public int SwingSign => Projectile.ai[1] < 0f ? -1 : 1;
        public bool IsFinisher => Math.Abs(Projectile.ai[1]) > 1.5f;  // big swing
        public float Progress => Player.itemAnimationMax > 0
            ? MathHelper.Clamp(1f - Player.itemAnimation / (float)Player.itemAnimationMax, 0f, 1f)
            : 1f;
        public float AimRotation
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public float BladeAngle => Projectile.rotation - MathHelper.PiOver4;

        public Vector2 GetBladePoint(float along = 1f)
            => Projectile.Center + BladeAngle.ToRotationVector2() * SwordLength * Projectile.scale * along;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new(8);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.noEnchantmentVisuals = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LocalHitCooldown;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Player owner = Player;

            if (!owner.active || owner.dead || owner.noItems || owner.CCed || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                AimRotation = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f)).ToRotation();
                Projectile.velocity = Vector2.Zero;
                OnSwingStart();
            }

            Projectile.direction = Projectile.spriteDirection = owner.direction;
            Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter, true);

            float progress = Progress;
            float arc = SwingArc * (IsFinisher ? FinisherArcMultiplier : 1f);
            float bladeAngle = AimRotation
                + (SwingEase(progress) - 0.5f) * SwingSign * owner.direction * arc
                - owner.direction * AngleOffset;

            Projectile.rotation = bladeAngle * owner.gravDir + MathHelper.PiOver4;

            float scale = BaseScale * ScaleCurve(progress) * owner.GetAdjustedItemScale(owner.HeldItem);
            if (IsFinisher)
                scale *= FinisherScaleMultiplier;
            Projectile.scale = scale;

            Projectile.timeLeft = owner.itemAnimation;
            Timer++;

            SetPlayerValues();

            bool active = progress >= ActiveWindowStart && progress <= ActiveWindowEnd;
            if (active)
            {
                if (SpawnSwingParticles)
                    SpawnTrailParticles();
                SpawnDust();
            }

            if (UseLight && Main.netMode != NetmodeID.Server)
                Lighting.AddLight(GetBladePoint(0.85f), TrailColor.ToVector3() * (0.3f + 0.5f * MathF.Sin(MathF.PI * progress)));

            if (DebugMode)
            {
                for (float a = 0f; a <= 1f; a += 0.1f)
                {
                    Dust d = Dust.NewDustPerfect(GetBladePoint(a), DustID.RedTorch, Vector2.Zero);
                    d.noGravity = true;
                }
            }
        }

        public virtual void OnSwingStart()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (SwingSound is SoundStyle style)
                SoundEngine.PlaySound(style with { Pitch = IsFinisher ? -0.35f : 0f, PitchVariance = 0.1f }, Projectile.Center);
        }

        public virtual void SetPlayerValues()
        {
            Player.heldProj = Projectile.whoAmI;
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 - MathHelper.PiOver4);
            Player.itemRotation = MathHelper.WrapAngle(Projectile.rotation - MathHelper.PiOver4);
            if (Player.direction == -1)
            {
                Player.itemRotation += MathHelper.TwoPi;
            }
        }
        public virtual void SpawnTrailParticles()
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.TrailBuffer == null)
                return;

            Vector2 tipDirection = BladeAngle.ToRotationVector2();
            Vector2 perp = new Vector2(-tipDirection.Y, tipDirection.X);
            int count = ParticleCount * (IsFinisher ? 2 : 1);

            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat(0.35f, 1f);
                Vector2 spawnPos = GetBladePoint(along);
                spawnPos += perp * Main.rand.NextFloat(-6f, 6f) * ParticleWidth;

                Vector2 vel = (-tipDirection * Main.rand.NextFloat(0.3f, 1.2f) + Main.rand.NextVector2Circular(0.6f, 0.6f)) * ParticleSpeedMultiplier;

                float size = Main.rand.NextFloat(22f, 42f) * (0.5f + along * 0.7f) * ParticleWidth * ParticleSizeMultiplier;

                ParticleSystem.TrailBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(size, size * Main.rand.NextFloat(0.4f, 0.7f)),
                    ParticleColor * Main.rand.NextFloat(0.85f, 1.2f),
                    Main.rand.Next(ParticleLife - 6, ParticleLife + 4)
                ));
            }

            if (Main.rand.NextBool(IsFinisher ? 2 : 3))
            {
                Vector2 spawnPos = GetBladePoint(Main.rand.NextFloat(0.6f, 1f));

                ParticleSystem.TrailBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    (-tipDirection * 0.4f * ParticleSpeedMultiplier).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(Main.rand.NextFloat(34f, 52f) * ParticleWidth * ParticleSizeMultiplier, Main.rand.NextFloat(10f, 18f)),
                    ParticleCoreColor * 1.15f,
                    Main.rand.Next(ParticleLife - 8, ParticleLife)
                ));
            }
        }

        public virtual void SpawnDust()
        {
            if (DustType < 0 || Main.netMode == NetmodeID.Server || !Main.rand.NextBool(IsFinisher ? 1 : 2))
                return;

            Dust d = Dust.NewDustPerfect(
                GetBladePoint(Main.rand.NextFloat(0.35f, 1f)),
                DustType,
                Main.rand.NextVector2Circular(1.5f, 1.5f),
                150,
                DustColor,
                Main.rand.NextFloat(0.9f, 1.4f));
            d.noGravity = DustNoGravity;
        }

        public virtual void SpawnHitEffects(NPC target, NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.TrailBuffer == null)
                return;

            Vector2 pos = target.Hitbox.ClosestPointInRect(Projectile.Center);
            Vector2 away = (pos - Player.Center).SafeNormalize(Vector2.UnitX);
            int count = (IsFinisher ? 14 : 7) + (hit.Crit ? 5 : 0);

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = away.RotatedByRandom(1.1f) * Main.rand.NextFloat(2f, 7f);
                float size = Main.rand.NextFloat(14f, 28f) * ParticleSizeMultiplier;

                ParticleSystem.TrailBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    vel.ToRotation(),
                    new SystemVector2(size, size * 0.4f),
                    (i % 3 == 0 ? ParticleCoreColor : ParticleColor) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(14, 24)
                ));
            }
        }
        // Lmao change this it's a duplicate
        public static bool AValidTarget(NPC target) => target.active && !target.CountsAsACritter && !target.immortal && target.chaseable && !target.dontTakeDamage;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = Progress;
            if (progress < ActiveWindowStart || progress > ActiveWindowEnd)
                return false;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, GetBladePoint(), HitWidth * Projectile.scale, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Player.Center.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (AValidTarget(target) && Projectile.CanHitWithMeleeWeapon(target) && Main.myPlayer == Projectile.owner)
            {
                NPCLoader.OnHitByItem(target, Player, Player.HeldItem, hit, damageDone);
            }

            SpawnHitEffects(target, hit);
            OnSwingHit(target, hit, damageDone);
        }

        public virtual void OnSwingHit(NPC target, NPC.HitInfo hit, int damageDone) { }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(Projectile.spriteDirection == -1 ? texture.Width : 0f, texture.Height);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraRotation = Projectile.spriteDirection == -1 ? MathHelper.PiOver2 : 0f;

            if (DrawTrail && TrailDrawLength > 1)
            {
                int drawLength = Math.Min(TrailDrawLength, Projectile.oldPos.Length);
                float fadeDenom = MathF.Max(1f, drawLength - 1);

                for (int k = drawLength - 1; k >= 0; k--)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;

                    float t = 1f - k / fadeDenom;
                    float alpha = t * t * Projectile.Opacity;

                    Color trailColor = TrailColor with { A = 0 };
                    trailColor *= alpha;

                    float scale = Projectile.scale * MathHelper.Lerp(0.55f, 1f, t);
                    Vector2 drawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;

                    Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k] + extraRotation, origin, scale, spriteEffects, 0f);
                }
            }

            float pulse = 0.35f + 0.25f * MathF.Sin(MathF.PI * Progress);
            Color glow = TrailColor * Projectile.Opacity * pulse;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glow, Projectile.rotation + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            Color bladeColor = Color.Lerp(Color.Lerp(lightColor, Color.White, 0.7f), TrailCoreColor with { A = 255 }, 0.2f) * Projectile.Opacity;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, bladeColor, Projectile.rotation + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
    namespace Waybound.Content.Items.Weapons.Melee.Swords
    {
        public abstract class CustomSwingItem : ModItem
        {
            protected abstract int SwingProjectileType { get; }

            protected virtual int ComboLength => 3;
            protected virtual float FinisherDamageMultiplier => 1.35f;
            protected virtual float FinisherKnockbackMultiplier => 1.25f;
            protected virtual int ComboResetTicks => 75;

            private int comboStep;
            private bool flip;
            private uint lastSwingTick;

            public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

            public override void SetDefaults()
            {
                Item.DamageType = DamageClass.Melee;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noMelee = true;
                Item.noUseGraphic = true;
                Item.autoReuse = true;
                Item.UseSound = null;
                Item.shoot = SwingProjectileType;
                Item.shootSpeed = 1f;
            }

            public override bool MeleePrefix() => true;

            public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
            {
                if (Main.GameUpdateCount - lastSwingTick > ComboResetTicks)
                {
                    comboStep = 0;
                    flip = false;
                }
                lastSwingTick = Main.GameUpdateCount;

                bool finisher = ComboLength > 1 && comboStep == ComboLength - 1;

                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner == player.whoAmI && p.type == type)
                        p.Kill();
                }

                float swingDir = flip ? -1f : 1f;
                if (finisher)
                    swingDir *= 2f;

                Projectile.NewProjectile(
                    source,
                    player.MountedCenter,
                    velocity.SafeNormalize(new Vector2(player.direction, 0f)),
                    type,
                    finisher ? (int)(damage * FinisherDamageMultiplier) : damage,
                    finisher ? knockback * FinisherKnockbackMultiplier : knockback,
                    player.whoAmI,
                    0f,
                    swingDir);

                flip = !flip;
                comboStep = (comboStep + 1) % ComboLength;
                return false;
            }
        }
    }
}