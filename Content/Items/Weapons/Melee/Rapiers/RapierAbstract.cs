using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SysVector2 = System.Numerics.Vector2;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public abstract class BaseRapierHoldout : ModProjectile
    {
        public abstract string RapierTexture { get; }
        public abstract int MaxCharge { get; }
        public abstract float MinLungeDistance { get; }
        public abstract float MaxLungeDistance { get; }
        public abstract float LungeOutSpeed { get; }
        public abstract float LungeReturnSpeed { get; }
        public abstract bool UseGreenPulse { get; }

        public virtual float AimResponsiveness => 0.88f;
        public virtual float BaseHoldoutDistance => 36f;

        public override string Texture => RapierTexture;

        protected int charge;
        protected float holdoutDistance;
        protected float targetDistance;
        protected int lungeTimer;
        protected bool isLunging;
        protected bool returning;
        protected Vector2 lastPos;
        protected bool particlesSpawned;
        protected float glowProgress;
        protected float fadeIn;
        protected float fadeOut = 1f;
        protected bool isFadingOut;
        protected float lungeStrength = 1f;

        protected int greenPulseTimer;
        protected float greenPulseIntensity;
        protected bool greenPulseActive;
        protected bool specialTriggered;

        private bool initialized;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || player.dead || !player.active || !IsCorrectItem(player))
            {
                isFadingOut = true;
            }

            if (isFadingOut)
            {
                fadeOut -= 0.08f;
                if (fadeOut <= 0f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                fadeIn = MathHelper.Lerp(fadeIn, 1f, 0.15f);
            }

            if (Projectile.timeLeft < 30)
                Projectile.timeLeft = 120;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true, true);
            lastPos = Projectile.Center;

            if (!initialized)
            {
                holdoutDistance = BaseHoldoutDistance;
                targetDistance = BaseHoldoutDistance;
                Projectile.velocity = Vector2.UnitX * player.direction;
                UpdateAim(playerCenter);
                initialized = true;
            }

            if (player.channel && Projectile.owner == Main.myPlayer && !isFadingOut)
            {
                if (!isLunging)
                {
                    charge++;
                    UpdateAim(playerCenter);

                    if (charge >= MaxCharge)
                    {
                        StartLunge();
                    }
                }
                else
                {
                    lungeTimer++;

                    if (!returning)
                    {
                        holdoutDistance = MathHelper.Lerp(holdoutDistance, targetDistance, LungeOutSpeed);

                        if (holdoutDistance > targetDistance - 18f)
                        {
                            returning = true;
                            targetDistance = BaseHoldoutDistance;
                        }
                    }
                    else
                    {
                        holdoutDistance = MathHelper.Lerp(holdoutDistance, targetDistance, LungeReturnSpeed);

                        if (holdoutDistance < BaseHoldoutDistance + 8f)
                        {
                            isLunging = false;
                            charge = 0;
                            holdoutDistance = BaseHoldoutDistance;
                            lungeStrength = 1f;
                            greenPulseActive = false;
                            greenPulseIntensity = 0f;
                            specialTriggered = false;
                        }
                    }

                    if (!particlesSpawned && lungeTimer == 1)
                    {
                        particlesSpawned = true;
                        SpawnLungeParticles(playerCenter);
                    }

                    if (isLunging)
                    {
                        SpawnTrailParticles();
                        UpdateGreenPulse(player);
                    }
                }
            }

            float targetGlow = (isLunging || charge >= MaxCharge - 10) ? 1f : (charge / (float)MaxCharge) * 0.4f;
            glowProgress = MathHelper.Lerp(glowProgress, targetGlow, 0.08f);

            Projectile.Center = playerCenter + Projectile.velocity * holdoutDistance;

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
                Projectile.spriteDirection = 1;
            }
            else
            {
                player.ChangeDir(-1);
                Projectile.spriteDirection = -1;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.PiOver2;

            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void StartLunge()
        {
            isLunging = true;
            returning = false;
            lungeTimer = 0;
            lungeStrength = MathHelper.Clamp(charge / (float)MaxCharge, 0.55f, 1f);
            targetDistance = MathHelper.Lerp(MinLungeDistance, MaxLungeDistance, lungeStrength);
            particlesSpawned = false;
            greenPulseActive = false;
            greenPulseIntensity = 0f;
            greenPulseTimer = 0;
            specialTriggered = false;
        }

        private void UpdateGreenPulse(Player player)
        {
            if (!UseGreenPulse)
                return;

            if (!greenPulseActive && Main.rand.NextBool(18))
            {
                greenPulseActive = true;
                greenPulseTimer = Main.rand.Next(14, 26);
                greenPulseIntensity = 1f;
                OnGreenPulseStart();
            }

            if (greenPulseActive)
            {
                greenPulseTimer--;
                greenPulseIntensity = MathHelper.Lerp(greenPulseIntensity, 0f, 0.1f);

                if (!specialTriggered && IsSpecialInput(player))
                {
                    specialTriggered = true;
                    OnSpecialDuringGreenPulse(player);
                }

                if (greenPulseTimer <= 0)
                {
                    greenPulseActive = false;
                    greenPulseIntensity = 0f;
                    OnGreenPulseEnd();
                }
                else
                {
                    OnGreenPulseUpdate();
                }
            }
        }

        protected virtual bool IsSpecialInput(Player player)
        {
            return player.controlUseTile;
        }

        protected virtual void OnGreenPulseStart() { }
        protected virtual void OnGreenPulseUpdate() { }
        protected virtual void OnGreenPulseEnd() { }
        protected virtual void OnSpecialDuringGreenPulse(Player player) { }

        protected abstract bool IsCorrectItem(Player player);

        private void UpdateAim(Vector2 source)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
            if (aim.HasNaNs())
                aim = -Vector2.UnitY;

            aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, AimResponsiveness));

            if (aim != Projectile.velocity)
                Projectile.netUpdate = true;

            Projectile.velocity = aim;
        }

        protected virtual void SpawnLungeParticles(Vector2 playerCenter)
        {
            Vector2 tip = Projectile.Center;
            int baseCount = (int)MathHelper.Lerp(20, 55, lungeStrength);
            int tipCount = (int)MathHelper.Lerp(25, 70, lungeStrength);

            for (int i = 0; i < baseCount; i++)
            {
                Vector2 spawnPos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                float scale = Main.rand.NextFloat(8f, 16f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    Vector2.Zero.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    GetLungeParticleColor() * fadeIn,
                    Main.rand.Next(35, 65)
                ));
            }

            for (int i = 0; i < tipCount; i++)
            {
                Vector2 spawnPos = tip + Main.rand.NextVector2Circular(30f, 30f);
                float scale = Main.rand.NextFloat(10f, 20f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    Vector2.Zero.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    GetTipParticleColor() * fadeIn,
                    Main.rand.Next(40, 75)
                ));
            }
        }

        protected virtual void SpawnTrailParticles()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPos = Vector2.Lerp(lastPos, Projectile.Center, i / 8f) + Main.rand.NextVector2Circular(2f, 2f);
                float scale = Main.rand.NextFloat(14f, 26f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    Vector2.Zero.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    GetTrailParticleColor() * fadeIn,
                    Main.rand.Next(14, 28)
                ));
            }
        }

        protected virtual Color GetLungeParticleColor() => new Color(180, 140, 90, 255);
        protected virtual Color GetTipParticleColor() => new Color(160, 120, 70, 255);
        protected virtual Color GetTrailParticleColor() => new Color(170, 130, 80, 200);
        protected virtual Color GetGlowColor() => new Color(200, 160, 100, 180);
        protected virtual Color GetGreenPulseColor() => new Color(80, 255, 120, 200);

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (isLunging)
            {
                int size = (int)MathHelper.Lerp(22, 48, lungeStrength);
                hitbox.Inflate(size, size);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (isLunging)
            {
                float damageMult = MathHelper.Lerp(0.85f, 1.55f, lungeStrength);
                modifiers.FinalDamage *= damageMult;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float alpha = fadeIn * fadeOut;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Обычный glow (меньше и аккуратнее)
            if (glowProgress > 0.01f)
            {
                Color glowColor = GetGlowColor() * glowProgress * alpha;

                for (int i = 0; i < 8; i++)
                {
                    float rot = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = new Vector2(2.8f, 0f).RotatedBy(rot);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor * 0.35f, Projectile.rotation, origin, Projectile.scale * 1.04f, effects, 0);
                }

                for (int i = 0; i < 6; i++)
                {
                    float rot = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = new Vector2(1.4f, 0f).RotatedBy(rot);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.015f, effects, 0);
                }
            }

            // Зелёный пульс (меньше, резче, плавное появление)
            if (UseGreenPulse && greenPulseIntensity > 0.01f)
            {
                // Плавность появления через smoothstep-подобную кривую
                float smooth = greenPulseIntensity * greenPulseIntensity * (3f - 2f * greenPulseIntensity);
                Color green = GetGreenPulseColor() * smooth * alpha;

                // Тонкий внешний слой
                for (int i = 0; i < 10; i++)
                {
                    float rot = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = new Vector2(3.5f * smooth, 0f).RotatedBy(rot);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, green * 0.3f, Projectile.rotation, origin, Projectile.scale * (1f + 0.06f * smooth), effects, 0);
                }

                // Резкий внутренний
                for (int i = 0; i < 8; i++)
                {
                    float rot = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = new Vector2(1.8f * smooth, 0f).RotatedBy(rot);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, green * 0.65f, Projectile.rotation, origin, Projectile.scale * (1f + 0.03f * smooth), effects, 0);
                }
            }

            // Основной спрайт
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor * alpha, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}