using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class EcosystemShoot : ModProjectile
    {
        public override string Texture
        {
            get
            {
                return "Terraria/Images/Extra_57";
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;   
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.12f;
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;

            if (Projectile.velocity.X != 0f)
            {
                Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
            }

            Projectile.oldRot[0] = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft <= 25)
            {
                float fade = Projectile.timeLeft / 25f;
                Projectile.alpha = (int)(255 * (1f - fade));
                Projectile.scale = MathHelper.Lerp(0.3f, 0.8f, fade);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++) 
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.JungleTorch,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    0,
                    new Color(30, 180, 60), 
                    1.1f
                );
            }

            Projectile.position = Projectile.Center;
            Projectile.width = 50; 
            Projectile.height = 50;
            Projectile.position.X -= Projectile.width / 2;
            Projectile.position.Y -= Projectile.height / 2;
            Projectile.Damage();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color jungleColor = new Color(40, 200, 70);
            jungleColor.A = 0;

            Texture2D texture = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = MathHelper.Lerp(0.9f, 0f, progress) * (1f - Projectile.alpha / 255f);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + new Vector2(Projectile.width, Projectile.height) / 2f - Main.screenPosition,
                    null,
                    jungleColor * alpha,
                    Projectile.oldRot[i] + MathHelper.PiOver2,
                    texture.Size() / 2f,
                    Projectile.scale * MathHelper.Lerp(0.4f, 0.05f, progress), 
                    SpriteEffects.None,
                    0f
                );
            }

            texture = ModContent.Request<Texture2D>(Texture).Value;

            Color drawColor = jungleColor * (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale * 0.35f, 
                SpriteEffects.None,
                0f
            );

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor * 0.7f,
                -Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale * 0.35f,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}