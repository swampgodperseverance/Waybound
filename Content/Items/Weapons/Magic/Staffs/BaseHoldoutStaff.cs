using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Waybound.Content.Items.Weapons.Magic.Staffs
{
    public abstract class BaseHoldoutStaffProj : ModProjectile
    {
        public abstract string GlowTexture { get; }
        public virtual string GlowTexture2 => null;
        public abstract int MaxCharge { get; }
        public virtual float HoldoutDistance => 40f;
        public virtual float AimResponsiveness => 0.75f;
        public virtual SoundStyle ChargeSound => SoundID.Item28;
        public virtual SoundStyle ShootSound => SoundID.Item28;
        public abstract int ProjectileType { get; }
        public virtual float ProjectileSpeed { get; }
        public virtual int ManaCostDivider => 3;
        public virtual float KnockbackForce => 0f;
        public virtual float SpriteRotationOffset => MathHelper.PiOver4;
        public virtual float GlowScaleMultiplier => 1f;
        public virtual Color GlowColor => new Color(120, 200, 255);
        public virtual Color ChargeColor => new Color(160, 230, 255);
        public virtual bool ApplyPlayerRecoil => false;
        public virtual float PlayerRecoilStrength => 0.25f;
        public virtual bool DrawMainTexture => false;
        protected int charge;
        protected int charge2;
        private int angleDir = -1;
        private float dirPi = MathHelper.Pi;
        private float holdoutDistance;
        private bool playedChargeSound;
        private static Texture2D cachedGlow;
        private static Texture2D cachedGlow2;
        private static string cachedGlowPath;
        private static string cachedGlow2Path;
        public override string Texture => GlowTexture;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 66;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.usesLocalNPCImmunity = true;
            holdoutDistance = HoldoutDistance;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(charge);
            writer.Write(charge2);
            writer.Write(holdoutDistance);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            charge = reader.ReadInt32();
            charge2 = reader.ReadInt32();
            holdoutDistance = reader.ReadSingle();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            bool channeling = player.channel && Projectile.owner == Main.myPlayer;
            int manaCost = Math.Max(1, player.HeldItem.mana);
            if (channeling && player.statMana >= manaCost)
            {
                charge++;
                bool fullyCharged = charge >= MaxCharge;
                if (fullyCharged)
                {
                    if (charge == MaxCharge)
                    {
                        charge2 = 0;
                        playedChargeSound = false;
                        player.CheckMana(manaCost / ManaCostDivider, true, false);

                        Vector2 shootVelocity = Projectile.velocity * ProjectileSpeed;

                        if (Projectile.owner == Main.myPlayer)
                        {
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromAI(),
                                Projectile.Center,
                                shootVelocity,
                                ProjectileType,
                                Projectile.damage,
                                KnockbackForce,
                                player.whoAmI
                            );
                        }

                        SoundEngine.PlaySound(ShootSound, Projectile.Center);
                        SpawnBurstParticles();

                        if (ApplyPlayerRecoil && !player.noKnockback)
                            player.velocity -= shootVelocity * PlayerRecoilStrength;
                    }
                    else if (charge > MaxCharge + 12)
                    {
                        charge = 0;
                    }
                }
                else
                {
                    charge2++;
                    if (!playedChargeSound && charge == MaxCharge - 10)
                    {
                        SoundEngine.PlaySound(ChargeSound, Projectile.Center);
                        playedChargeSound = true;
                    }
                }
            }
            if (!player.channel || player.statMana < manaCost)
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
            else if (Projectile.velocity.X < 0f)
            {
                player.ChangeDir(-1);
                angleDir = -1;
                dirPi = MathHelper.Pi;
                Projectile.spriteDirection = -1;
            }
            Projectile.Center = playerCenter + Projectile.velocity * holdoutDistance;
            Projectile.rotation = Projectile.velocity.ToRotation() + SpriteRotationOffset * angleDir + dirPi;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            SpawnChargeParticles();
            SpawnAmbientParticles();
            float intensity = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
            Lighting.AddLight(Projectile.Center, GlowColor.ToVector3() * (0.4f + intensity * 0.8f));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return DrawMainTexture;
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D glow = GetCachedTexture(ref cachedGlow, ref cachedGlowPath, GlowTexture);
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float chargeRatio = MathHelper.Clamp(charge2 / (float)MaxCharge, 0f, 1f);
            float trans = MathF.Pow(chargeRatio, 3.5f);
            Main.EntitySpriteDraw(
            glow,
            position,
            null,
            Color.White,
            Projectile.rotation,
            glow.Size() * 0.5f,
            Projectile.scale * GlowScaleMultiplier,
            effects,
            0f
            );
            if (!string.IsNullOrEmpty(GlowTexture2))
            {
                Texture2D glow2 = GetCachedTexture(ref cachedGlow2, ref cachedGlow2Path, GlowTexture2);
                Color glowColor = GlowColor * (0.6f + trans * 0.9f);
                Main.EntitySpriteDraw(
                glow2,
                position,
                null,
                glowColor,
                Projectile.rotation,
                glow2.Size() * 0.5f,
                Projectile.scale * GlowScaleMultiplier,
                effects,
                0f
                );
            }
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
        protected virtual void SpawnChargeParticles() { }
        protected virtual void SpawnAmbientParticles() { }
        protected virtual void SpawnBurstParticles() { }
        private static Texture2D GetCachedTexture(ref Texture2D cache, ref string cachedPath, string path)
        {
            if (cache == null || cache.IsDisposed || cachedPath != path)
            {
                cache = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
                cachedPath = path;
            }
            return cache;
        }
    }
}