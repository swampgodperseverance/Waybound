using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    // I mean it's fine and addon friendly
    public abstract class BaseRapierItem : ModItem
    {
        protected abstract int HoldoutType { get; }

        protected abstract void SetRapierDefaults();

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;
            Item.autoReuse = true;
            Item.shoot = HoldoutType;
            Item.shootSpeed = 1f;

            SetRapierDefaults();
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[HoldoutType] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public abstract class BaseRapierHoldout : ModProjectile
    {
        public abstract string RapierTexture { get; }
        public override string Texture => RapierTexture;

        public virtual bool UseSpecial => true;
        public virtual float BaseHoldoutDistance => 30f;
        public virtual float PullBackDistance => 10f;
        public virtual float BladeLength => 60f;
        public virtual float AimResponsiveness => 0.5f;
        public virtual float EmpoweredDistance => 215f;

        protected virtual float GetThrustDistance(int i) => i switch { 0 => 105f, 1 => 120f, _ => 140f };
        protected virtual int WindupFrames(int i) => i == 2 ? 22 : 4;
        protected virtual int ThrustFrames(int i) => 3;
        protected virtual int HoldFrames(int i) => i == 2 ? 4 : 2;
        protected virtual int RecoverFrames(int i) => i == 2 ? 14 : 6;
        protected virtual int EmpoweredThrustFrames => 4;
        protected virtual int EmpoweredHoldFrames => 8;
        protected virtual int SpecialWindowFrames => 10;
        protected virtual int EndFrames => 10;

        protected virtual float GetDamageMultiplier() => empowered ? 2f : (thrustIndex == 2 ? 1.2f : 0.7f);

        protected virtual Color GetLungeParticleColor() => new Color(180, 140, 90, 255);
        protected virtual Color GetTipParticleColor() => new Color(160, 120, 70, 255);
        protected virtual Color GetTrailParticleColor() => new Color(170, 130, 80, 200);
        protected virtual Color GetGlowColor() => new Color(200, 160, 100, 180);
        protected virtual Color GetGreenPulseColor() => new Color(80, 255, 120, 220);

        protected virtual bool IsSpecialInput(Player player) => player.controlUseTile;
        protected abstract bool IsCorrectItem(Player player);

        protected virtual void OnEmpoweredStrike(Player player) { }

        protected enum Phase { Windup, Thrust, Hold, Recover, End }

        protected Phase phase;
        protected int phaseTimer;
        protected int thrustIndex;
        protected float holdoutDistance;
        protected float phaseStartDist;
        protected float targetDist;
        protected int hitStop;
        protected bool hitThisThrust;
        protected bool empowered;
        protected bool specialLocked;
        protected bool prevSpecialInput;
        protected bool windowFxDone;
        protected Vector2 lastPos;
        protected float fadeIn;
        protected float glowProgress;
        protected float specialGlow;
        protected float trailStrength;
        private bool initialized;

        protected bool Attacking => phase == Phase.Thrust || phase == Phase.Hold;

        protected bool SpecialWindowOpen =>
            UseSpecial && phase == Phase.Windup && thrustIndex == 2 && !specialLocked && !empowered
            && phaseTimer >= WindupFrames(2) - SpecialWindowFrames;

        protected Vector2 Tip => Projectile.Center + Projectile.velocity * BladeLength * 0.5f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 2;
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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            bool owner = Projectile.owner == Main.myPlayer;

            if (!player.active || player.dead || player.CCed || !IsCorrectItem(player))
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.timeLeft < 30)
                Projectile.timeLeft = 120;

            Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true, true);

            if (!initialized)
            {
                initialized = true;
                holdoutDistance = BaseHoldoutDistance;
                lastPos = Projectile.Center;
                if (Projectile.velocity.LengthSquared() < 0.01f)
                    Projectile.velocity = Vector2.UnitX * player.direction;
                Projectile.velocity.Normalize();
                EnterPhase(Phase.Windup, player);
            }

            fadeIn = MathHelper.Lerp(fadeIn, 1f, 0.3f);
            lastPos = Projectile.Center;

            if (!owner && Projectile.ai[0] == 1f && !empowered)
                Empower(player);

            if (owner)
            {
                if (phase == Phase.Windup)
                    UpdateAim(center, AimResponsiveness);
                HandleSpecialInput(player);
            }

            if (hitStop > 0)
                hitStop--;
            else
            {
                phaseTimer++;
                if (phaseTimer >= PhaseDuration())
                    NextPhase(player);
            }

            UpdateDistance();
            Projectile.Center = center + Projectile.velocity * holdoutDistance;

            SpawnPhaseEffects();

            float targetGlow = Attacking ? 1f : 0.15f;
            glowProgress = MathHelper.Lerp(glowProgress, targetGlow, 0.25f);
            trailStrength = MathHelper.Lerp(trailStrength, Attacking ? 1f : 0f, 0.25f);

            bool specialOn = SpecialWindowOpen || (empowered && (phase == Phase.Thrust || phase == Phase.Hold));
            specialGlow = MathHelper.Lerp(specialGlow, specialOn ? 1f : 0f, specialOn ? 0.35f : 0.12f);

            int dir = Projectile.velocity.X > 0f ? 1 : -1;
            player.ChangeDir(dir);
            Projectile.spriteDirection = dir;
            Projectile.direction = dir;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (dir == -1)
                Projectile.rotation += MathHelper.PiOver2;

            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private int PhaseDuration() => phase switch
        {
            Phase.Windup => WindupFrames(thrustIndex),
            Phase.Thrust => empowered ? EmpoweredThrustFrames : ThrustFrames(thrustIndex),
            Phase.Hold => empowered ? EmpoweredHoldFrames : HoldFrames(thrustIndex),
            Phase.Recover => RecoverFrames(thrustIndex),
            _ => EndFrames
        };

        private void NextPhase(Player player)
        {
            switch (phase)
            {
                case Phase.Windup:
                    EnterPhase(Phase.Thrust, player);
                    break;
                case Phase.Thrust:
                    EnterPhase(Phase.Hold, player);
                    break;
                case Phase.Hold:
                    EnterPhase(Phase.Recover, player);
                    break;
                case Phase.Recover:
                    if (thrustIndex < 2)
                    {
                        thrustIndex++;
                        EnterPhase(Phase.Windup, player);
                    }
                    else
                        EnterPhase(Phase.End, player);
                    break;
                case Phase.End:
                    Projectile.Kill();
                    break;
            }
        }

        private void EnterPhase(Phase newPhase, Player player)
        {
            phase = newPhase;
            phaseTimer = 0;
            phaseStartDist = holdoutDistance;

            switch (newPhase)
            {
                case Phase.Windup:
                    windowFxDone = false;
                    if (thrustIndex == 2)
                        specialLocked = false;
                    break;

                case Phase.Thrust:
                    targetDist = empowered ? EmpoweredDistance : GetThrustDistance(thrustIndex);
                    Projectile.ResetLocalNPCHitImmunity();
                    hitThisThrust = false;

                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 c = player.RotatedRelativePoint(player.MountedCenter, true, true);
                        UpdateAim(c, 1f);
                    }

                    SoundEngine.PlaySound(
                        (empowered ? SoundID.Item28 : SoundID.Item1) with { Pitch = 0.25f + thrustIndex * 0.2f, Volume = 0.9f },
                        Projectile.Center);
                    SpawnThrustBurst();
                    break;

                case Phase.Hold:
                    if (empowered)
                    {
                        SpawnEmpoweredBurst();
                        if (Projectile.owner == Main.myPlayer)
                        {
                            OnEmpoweredStrike(player);
                            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                                Projectile.Center, Projectile.velocity, 7f, 8f, 12, 1200f, "RapierEmpowered"));
                        }
                    }
                    break;
            }
        }

        private void UpdateDistance()
        {
            float t = MathHelper.Clamp(phaseTimer / (float)Math.Max(1, PhaseDuration()), 0f, 1f);

            switch (phase)
            {
                case Phase.Windup:
                    float pull = BaseHoldoutDistance - PullBackDistance * (thrustIndex == 2 ? 2f : 1f);
                    holdoutDistance = MathHelper.Lerp(phaseStartDist, pull, EaseFunctions.EaseOutQuad(t));
                    break;
                case Phase.Thrust:
                    holdoutDistance = MathHelper.Lerp(phaseStartDist, targetDist, EaseFunctions.EaseOutCubic(t));
                    break;
                case Phase.Hold:
                    holdoutDistance = targetDist;
                    break;
                case Phase.Recover:
                    holdoutDistance = MathHelper.Lerp(targetDist, BaseHoldoutDistance, EaseFunctions.EaseInOutQuad(t));
                    break;
                default:
                    holdoutDistance = BaseHoldoutDistance;
                    break;
            }
        }

        private void HandleSpecialInput(Player player)
        {
            bool input = IsSpecialInput(player);
            bool pressed = input && !prevSpecialInput;
            prevSpecialInput = input;

            if (!pressed || !UseSpecial || phase != Phase.Windup || thrustIndex != 2 || specialLocked || empowered)
                return;

            if (SpecialWindowOpen)
            {
                Projectile.ai[0] = 1f;
                Projectile.netUpdate = true;
                Empower(player);
            }
            else
            {
                specialLocked = true;
                for (int i = 0; i < 10; i++)
                    Spark(Tip + Main.rand.NextVector2Circular(14f, 14f), Main.rand.NextVector2Circular(1.5f, 1.5f),
                        new Color(110, 110, 120, 200), Main.rand.NextFloat(6f, 10f), 14);
            }
        }

        private void Empower(Player player)
        {
            empowered = true;
            EnterPhase(Phase.Thrust, player);
        }

        private void UpdateAim(Vector2 source, float responsiveness)
        {
            Vector2 aim = Main.MouseWorld - source;
            if (aim.LengthSquared() < 1f)
                return;
            aim.Normalize();

            Vector2 cur = Projectile.velocity.SafeNormalize(aim);
            Vector2 result = Vector2.Lerp(cur, aim, responsiveness).SafeNormalize(aim);

            if (Vector2.DistanceSquared(result, Projectile.velocity) > 0.000001f)
                Projectile.netUpdate = true;

            Projectile.velocity = result;
        }

        public override bool? CanDamage() => Attacking ? null : false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!Attacking)
                return false;

            Vector2 dir = Projectile.velocity;
            Vector2 a = Projectile.Center - dir * BladeLength * 0.5f;
            Vector2 b = Tip;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), a, b,
                empowered ? 26f : 16f, ref point);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= GetDamageMultiplier();
            if (empowered)
                modifiers.Knockback *= 1.6f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hitThisThrust)
            {
                hitThisThrust = true;
                hitStop = empowered ? 6 : 2;
            }

            Vector2 dir = Projectile.velocity;
            int count = empowered ? 16 : 7;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = dir.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(3f, 9f);
                Spark(target.Center + Main.rand.NextVector2Circular(10f, 10f), vel,
                    (empowered ? GetGreenPulseColor() : GetTipParticleColor()), Main.rand.NextFloat(8f, 14f), Main.rand.Next(14, 26));
            }
        }

        protected static void Spark(Vector2 pos, Vector2 vel, Color color, float scale, int life)
        {
            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                color,
                life));
        }

        private void SpawnPhaseEffects()
        {
            if (phase == Phase.Thrust)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 p = Vector2.Lerp(lastPos, Projectile.Center, i / 6f) + Main.rand.NextVector2Circular(2f, 2f);
                    Color c = empowered ? GetGreenPulseColor() : GetTrailParticleColor();
                    Spark(p, Vector2.Zero, c * fadeIn, Main.rand.NextFloat(10f, 18f), Main.rand.Next(10, 20));
                }
            }

            if (UseSpecial && phase == Phase.Windup && thrustIndex == 2 && !specialLocked && !empowered)
            {
                int dur = WindupFrames(2);
                float t = MathHelper.Clamp(phaseTimer / (float)dur, 0f, 1f);
                float radius = MathHelper.Lerp(64f, 6f, EaseFunctions.EaseInOutQuad(t));
                bool open = SpecialWindowOpen;

                if (open && !windowFxDone)
                {
                    windowFxDone = true;
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
                    for (int i = 0; i < 18; i++)
                        Spark(Tip, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f),
                            GetGreenPulseColor(), Main.rand.NextFloat(8f, 14f), 16);
                }

                int pts = 14;
                float spin = phaseTimer * 0.25f;
                Color c = open ? GetGreenPulseColor() : GetGlowColor() * 0.4f;
                for (int i = 0; i < pts; i++)
                {
                    Vector2 p = Tip + new Vector2(radius, 0f).RotatedBy(spin + MathHelper.TwoPi * i / pts);
                    Spark(p, Vector2.Zero, c * fadeIn, open ? 9f : 5f, 3);
                }
            }
        }

        private void SpawnThrustBurst()
        {
            Vector2 dir = Projectile.velocity;
            int count = empowered ? 30 : 10;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = dir.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 8f);
                Spark(Tip - dir * 20f + Main.rand.NextVector2Circular(8f, 8f), vel,
                    (empowered ? GetGreenPulseColor() : GetLungeParticleColor()) * fadeIn,
                    Main.rand.NextFloat(8f, 16f), Main.rand.Next(16, 30));
            }
        }

        private void SpawnEmpoweredBurst()
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 11f);
                Spark(Tip, vel, GetGreenPulseColor(), Main.rand.NextFloat(10f, 20f), Main.rand.Next(20, 40));
            }
            for (int i = 0; i < 14; i++)
                Spark(Tip, Projectile.velocity * (6f + i * 1.2f), GetTipParticleColor(), 14f, 22);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float alpha = fadeIn * (phase == Phase.End ? 1f - phaseTimer / (float)EndFrames : 1f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (hitStop > 0)
                drawPos += Main.rand.NextVector2Circular(1.6f, 1.6f);
            else if (phase == Phase.Windup && thrustIndex == 2 && !empowered)
                drawPos += Main.rand.NextVector2Circular(0.9f, 0.9f) * (phaseTimer / (float)WindupFrames(2));

            if (trailStrength > 0.02f)
            {
                int len = Projectile.oldPos.Length;
                for (int k = 1; k < len; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;
                    float f = 1f - k / (float)len;
                    Color c = (empowered ? GetGreenPulseColor() : GetGlowColor()) * (f * 0.55f * trailStrength * alpha);
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                        c, Projectile.oldRot[k], origin, Projectile.scale, effects, 0);
                }
            }

            if (glowProgress > 0.01f)
            {
                Color glowColor = GetGlowColor() * glowProgress * alpha;

                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = new Vector2(2.8f, 0f).RotatedBy(MathHelper.TwoPi * i / 8f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor * 0.35f, Projectile.rotation, origin, Projectile.scale * 1.04f, effects, 0);
                }
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = new Vector2(1.4f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.015f, effects, 0);
                }
            }

            if (UseSpecial && specialGlow > 0.01f)
            {
                float s = EaseFunctions.EaseInOutQuad(specialGlow);
                float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.7f);
                Color green = GetGreenPulseColor() * s * alpha * pulse;

                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = new Vector2(4f * s, 0f).RotatedBy(MathHelper.TwoPi * i / 10f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, green * 0.3f, Projectile.rotation, origin, Projectile.scale * (1f + 0.07f * s), effects, 0);
                }
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = new Vector2(1.8f * s, 0f).RotatedBy(MathHelper.TwoPi * i / 8f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, green * 0.7f, Projectile.rotation, origin, Projectile.scale * (1f + 0.03f * s), effects, 0);
                }
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor * alpha, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }
}