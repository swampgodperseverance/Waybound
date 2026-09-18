using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Projectiles.Ranged.Guns.PreHM
{
    public class IcebornRifleProj : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;          
            Projectile.scale = 1f;
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
    
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.25f, 0.55f, 0.95f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.IceTorch,
                    Projectile.velocity * 0.05f,
                    100,
                    new Color(140, 210, 255),
                    Main.rand.NextFloat(0.7f, 1.1f)
                );
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 80, new Color(120, 200, 255), 1.2f);
                    d.noGravity = true;
                }
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 60, new Color(150, 220, 255), Main.rand.NextFloat(1.0f, 1.5f));
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.35f); 

            if (oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;

                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 3.2f; 

                Color trailColor = new Color(90, 180, 255, 0) * 0.85f;

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.55f, trailScaleY),
                    SpriteEffects.None,
                    0f
                );

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    new Color(180, 230, 255, 0) * 0.7f,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.28f, trailScaleY * 1.15f),
                    SpriteEffects.None,
                    0f
                );
            }

            Color outlineColor = new Color(100, 190, 255) * 0.65f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i);
                Main.EntitySpriteDraw(
                    texture,
                    drawPos + offset,
                    null,
                    outlineColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}