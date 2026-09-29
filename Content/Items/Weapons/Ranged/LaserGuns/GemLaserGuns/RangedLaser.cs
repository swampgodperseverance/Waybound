using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns
{
    public abstract class RangedLaser : ModProjectile
    {
        public float moveDistance = 90f;
        public float moveSpeed = 2f;
        public float rotateToDecrease = 22.5f;
        public int maxDistance = 250;
        public int laserDust;
        public Color colorLineBG, colorLinesAround;

        private Texture2D texture;
        private int framesAmount = 3;
        private float textureTimer;
        private float wasDisplayed;

        public float Distance
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public float rotation
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public float rotate
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private void EnsureTexture()
        {
            if (texture == null || texture.IsDisposed)
            {
                texture = Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public void DrawLaser(Vector2 start, Vector2 unit, float rotation = 0f)
        {
            EnsureTexture();
            if (texture == null) return;

            float scale = Math.Max(Projectile.scale, 0.05f);

            unit = unit.SafeNormalize(Vector2.UnitX);
            float r = unit.ToRotation() + rotation;
            wasDisplayed = 0;

            for (float i = 0; i <= Distance - moveDistance; i += scale)
            {
                var origin = start + i * unit;

                Main.EntitySpriteDraw(texture, origin - Main.screenPosition,
                    new Rectangle(27, 0, texture.Width / framesAmount, 1),
                    colorLineBG, r, new Vector2(texture.Width / framesAmount, 1f) / 2, scale, 0, 0);

                Main.EntitySpriteDraw(texture, origin - Main.screenPosition,
                    new Rectangle(54, (int)textureTimer * 4 % texture.Height, texture.Width / framesAmount, 1),
                    colorLinesAround, r, new Vector2(texture.Width / framesAmount, 1f) / 2, scale, 0, 0);

                Main.EntitySpriteDraw(texture, origin - Main.screenPosition,
                    new Rectangle(0, ((int)textureTimer + (int)i / 12) % texture.Height, texture.Width / framesAmount, 1),
                    Color.White, r, new Vector2(texture.Width / framesAmount, 1f) / 2, scale, 0, 0);

                wasDisplayed = 1;
            }

            for (int i = 0; i < 180; i += 6)
            {
                Main.EntitySpriteDraw(texture, start - Main.screenPosition,
                    new Rectangle(27, 0, texture.Width / framesAmount, 1),
                    colorLineBG, r + MathHelper.ToRadians(i), new Vector2(texture.Width / framesAmount, 1f) / 2, scale * 1.5f, 0, 0);

                Main.EntitySpriteDraw(texture, start - Main.screenPosition,
                    new Rectangle(0, (int)textureTimer, texture.Width / framesAmount, 1),
                    Color.White, r + MathHelper.ToRadians(i), new Vector2(texture.Width / framesAmount, 1f) / 2, scale * 1.5f, 0, 0);

                if (wasDisplayed == 1)
                {
                    Main.EntitySpriteDraw(texture, start + unit * (Distance - moveDistance) - Main.screenPosition,
                        new Rectangle(27, 0, texture.Width / framesAmount, 1),
                        colorLineBG, r + MathHelper.ToRadians(i), new Vector2(texture.Width / framesAmount, 1f) / 2, scale * 1.5f, 0, 0);

                    Main.EntitySpriteDraw(texture, start + unit * (Distance - moveDistance) - Main.screenPosition,
                        new Rectangle(0, ((int)textureTimer + (int)(Distance - moveDistance) / 12) % texture.Height, texture.Width / framesAmount, 1),
                        Color.White, r + MathHelper.ToRadians(i), new Vector2(texture.Width / framesAmount, 1f) / 2, scale * 1.5f, 0, 0);
                }
            }

            if (wasDisplayed == 1)
            {
                for (int j = 0; j < Main.rand.Next(2, 5); j++)
                {
                    int dust = Dust.NewDust(start + unit * (Distance - moveDistance), 0, 0,
                        laserDust, 0, 0, 0, default, Main.rand.NextFloat(0.85f, 1.125f));
                    Vector2 dustVel = unit.RotatedBy(MathHelper.ToRadians(180f + Main.rand.NextFloat(-70f, 70f))) * Main.rand.NextFloat(4f, 9f);
                    Main.dust[dust].velocity = dustVel;
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].position -= new Vector2(Main.dust[dust].frame.Width, Main.dust[dust].frame.Width) * Main.dust[dust].scale / 2;

                    dust = Dust.NewDust(start + unit * (Distance - moveDistance), 0, 0,
                        laserDust, 0, 0, 0, default, Main.rand.NextFloat(0.65f, 0.85f));
                    dustVel = new Vector2(1f, 0f).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(360f))) * Main.rand.NextFloat(2f, 4f);
                    Main.dust[dust].velocity = dustVel;
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].position -= new Vector2(Main.dust[dust].frame.Width, Main.dust[dust].frame.Width) * Main.dust[dust].scale / 2;
                }
            }

            if (Main.rand.NextBool(30 - (int)(26f * scale)))
            {
                Gore gore = Gore.NewGorePerfect(null, start,
                    Vector2.Zero, Main.rand.NextFromList(GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3), 1f);
                gore.velocity *= 0.65f;
                gore.position -= new Vector2(gore.Width, gore.Height) / 2 + unit * Main.rand.NextFloat(0f, moveDistance - 5);
                gore.scale = Main.rand.NextFloat(0.175f, 0.375f);
                gore.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f));
                gore.alpha = 120;
            }
        }

        public override void AI()
        {
            EnsureTexture(); // важно

            Player player = Main.player[Projectile.owner];
            Projectile.scale = player.GetModPlayer<PlayerLaserGun>().laserScale;

            if (Projectile.scale < 0.05f)
                Projectile.scale = 0.05f;

            Projectile.position = player.Center + Projectile.velocity * 22 - new Vector2(0f, Projectile.width).RotatedBy(Projectile.rotation);

            if (texture != null)
                textureTimer = (textureTimer + 0.5f) % texture.Height;

            UpdatePlayer(player);
            SetLaserPosition(player);
            CastLights();
        }

        private void CastLights()
        {
            if (texture == null) return;

            DelegateMethods.v3_1 = new Vector3(colorLinesAround.R / 255f, colorLinesAround.G / 255f, colorLinesAround.B / 255f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Distance - moveDistance), texture.Width / framesAmount, DelegateMethods.CastLight);
        }

        private void SetLaserPosition(Player player)
        {
            float step = maxDistance / 2f;
            for (Distance = 0; Distance <= maxDistance; Distance += step)
            {
                var start = player.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * Distance;
                if (!Collision.CanHitLine(player.Center, 1, 1, start, 1, 1) && !Collision.CanHit(player.Center, 1, 1, start, 1, 1))
                {
                    Distance -= step;
                    if (step < 0.25f)
                        break;
                    step /= 2f;
                }
            }
            if (Distance > maxDistance)
                Distance = maxDistance;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (texture == null) return false;

            Player player = Main.player[Projectile.owner];
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float point = 1f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                player.Center + unit * moveDistance,
                player.Center + unit * Distance,
                texture.Width / framesAmount * Projectile.scale, ref point);
        }

        private void UpdatePlayer(Player player)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 diff = Main.MouseWorld - player.Center;
                rotation = MathHelper.ToDegrees(Projectile.velocity.ToRotation());
                rotate = MathHelper.ToDegrees(diff.ToRotation());

                if (Math.Abs(rotation - rotate) >= 180)
                {
                    if (rotation < 0) rotation = 360 - Math.Abs(rotation);
                    if (rotate < 0) rotate = 360 - Math.Abs(rotate);
                }

                float lerpFactor = 1f / Math.Max(1f, Math.Abs(rotation - rotate)) * moveSpeed * (1.25f - Projectile.scale) * (Math.Min(Math.Abs(rotation - rotate), rotateToDecrease) / rotateToDecrease);
                Projectile.velocity = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(MathHelper.Lerp(rotation, rotate, lerpFactor)));
                Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                player.direction = Projectile.direction;
                Projectile.netUpdate = true;
            }

            int dir = Projectile.direction;
            player.ChangeDir(dir);
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * dir, Projectile.velocity.X * dir);

            if (Projectile.scale > 0.25f && Main.mouseLeft)
            {
                player.itemAnimation = player.itemAnimationMax;
                player.itemTime = player.itemTimeMax;
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 2);
            }

            player.HeldItem.color = Color.Lerp(Color.White, new Color(255, 110, 110), Projectile.scale);
            player.lastVisualizedSelectedItem = player.HeldItem;
            Projectile.damage = (int)(player.HeldItem.damage * Projectile.scale);
        }

        public override bool ShouldUpdatePosition() => false;

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 unit = Projectile.velocity;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * Distance, (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
        }
    }
}