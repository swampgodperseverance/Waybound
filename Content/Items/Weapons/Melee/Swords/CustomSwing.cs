using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{
    public abstract class CustomSwing : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
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
            Projectile.friendly = true;
        }

        public Player Player => Main.player[Projectile.owner];
        public virtual ref float Timer => ref Projectile.ai[0];
        public virtual ref float SwingDirection => ref Projectile.ai[1];
        public virtual float SwordLength => 16f;
        public virtual float MaxTime => 32f;
        public virtual bool DebugMode => false;
        public virtual float BaseScale => 1f;

        public override void AI()
        {
            Projectile.Center = Player.RotatedRelativePoint(Player.Center);
            Projectile.spriteDirection = Projectile.direction;
            Projectile.scale = BaseScale + (float)Player.itemAnimation / (float)Player.itemAnimationMax;
            Projectile.rotation = Vector2.Normalize(Projectile.velocity).ToRotation() + ((float)Player.itemAnimation / (float)Player.itemAnimationMax - 0.5f) * (SwingDirection * -Player.direction) * 3.5f - Player.direction * 0.3f + MathHelper.PiOver4;
            Projectile.rotation *= Player.gravDir;
            Projectile.timeLeft = Player.itemAnimation;
            if (DebugMode)
            {
                float rotation = Projectile.rotation - MathHelper.PiOver4;
            }
            SetPlayerValues();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (AValidTarget(target) && Projectile.CanHitWithMeleeWeapon(target) && Main.myPlayer == Projectile.owner)
            {
                NPCLoader.OnHitByItem(target, Player, Player.HeldItem, hit, damageDone);
            }
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
        public static bool AValidTarget(NPC target) => target.active && !target.CountsAsACritter && !target.immortal && target.chaseable && !target.dontTakeDamage;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float rotation = Projectile.rotation - MathHelper.PiOver4;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + rotation.ToRotationVector2() * SwordLength * Projectile.scale, 4f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(Projectile.spriteDirection == -1 ? texture.Width : 0f, texture.Height);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraRotation = Projectile.spriteDirection == -1 ? MathHelper.PiOver2 : 0f;
            Color trailColor = Color.White with { A = 0 } * Projectile.Opacity;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                trailColor *= 0.75f;
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition, null, trailColor, Projectile.oldRot[i] + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}