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
using Waybound.Common.GlobalPlayer;
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
        public bool noParticles;
        public Color colorLineBG, colorLinesAround;

        private Texture2D texture;
        private Texture2D rayTexture;
        private int framesAmount = 3;
        private float textureTimer;

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

        private float Charge01 => MathHelper.Clamp((Projectile.scale - 0.25f) / 0.75f, 0f, 1f);

        private void EnsureTexture()
        {
            if (texture == null || texture.IsDisposed)
                texture = Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad).Value;

            if (rayTexture == null || rayTexture.IsDisposed)
                rayTexture = Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
        }

        private static void BeginAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        /// <summary>
        /// Fuck this laser that's my whole summary
        /// </summary>
        private static void EndAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// ignore, could be deleted even
        private void DrawFlare(Vector2 worldPos, float scale, float charge, float spin, int rays, float lengthMul)
        {
            Vector2 pos = worldPos - Main.screenPosition;
            Vector2 rayOrigin = new Vector2(rayTexture.Width / 2f, rayTexture.Height);
            float time = Main.GlobalTimeWrappedHourly;

            for (int i = 0; i < rays; i++)
            {
                float ang = MathHelper.TwoPi / rays * i + spin;
                float pulse = 0.85f + 0.15f * MathF.Sin(time * 9f + i * 1.7f);
                Color c = Color.Lerp(colorLinesAround, Color.White, 0.35f) * (0.35f + 0.35f * charge);
                Main.EntitySpriteDraw(rayTexture, pos, null, c, ang, rayOrigin,
                    new Vector2((0.18f + 0.18f * charge) * scale * 1.6f, (0.45f + 0.8f * charge) * scale * 1.4f * lengthMul * pulse),
                    SpriteEffects.None, 0);
            }
        }

        public void DrawLaser(Vector2 start, Vector2 unit, float rotation = 0f)
        {
            EnsureTexture();
            if (texture == null || rayTexture == null) return;

            float scale = Math.Max(Projectile.scale, 0.05f);
            float charge = Charge01;
            float time = Main.GlobalTimeWrappedHourly;
            int colW = texture.Width / framesAmount;
            Vector2 org = new Vector2(colW, 1f) / 2f;

            unit = unit.SafeNormalize(Vector2.UnitX);
            float r = unit.ToRotation() + rotation;
            float length = Math.Max(Distance - moveDistance, 0f);
            bool hasBeam = length > 0f;

            float step = Math.Max(scale, 1.5f);

            BeginAdditive();

            if (hasBeam)
            {
                for (float i = 0; i <= length; i += step)
                {
                    Vector2 p = start + i * unit - Main.screenPosition;

                    float wave = 1f
                        + (0.06f + 0.07f * charge) * MathF.Sin(i * 0.045f - time * 14f)
                        + 0.035f * MathF.Sin(i * 0.13f + time * 23f);

                    float taper = MathHelper.Clamp(i / 28f, 0.35f, 1f);
                    float w = scale * wave * taper;

                    Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                        colorLineBG * (0.28f + 0.22f * charge), r, org, new Vector2(w * 2.1f, step), 0, 0);

                    Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                        colorLineBG * 0.9f, r, org, new Vector2(w, step), 0, 0);

                    Main.EntitySpriteDraw(texture, p, new Rectangle(54, (int)textureTimer * 4 % texture.Height, colW, 1),
                        colorLinesAround, r, org, new Vector2(w * 1.05f, step), 0, 0);

                    Main.EntitySpriteDraw(texture, p, new Rectangle(0, ((int)textureTimer + (int)i / 12) % texture.Height, colW, 1),
                        Color.White, r, org, new Vector2(w * 0.55f, step), 0, 0);

                    if (charge > 0.5f)
                    {
                        Main.EntitySpriteDraw(texture, p, new Rectangle(0, ((int)textureTimer * 2 + (int)i / 6) % texture.Height, colW, 1),
                            Color.White * ((charge - 0.5f) * 1.6f), r, org, new Vector2(w * 0.25f, step), 0, 0);
                    }
                }
            }

            float capPulse = 1f + 0.12f * MathF.Sin(time * 16f);
            Vector2 endPos = start + unit * length;
            for (int i = 0; i < 180; i += 6)
            {
                float a = r + MathHelper.ToRadians(i);

                Main.EntitySpriteDraw(texture, start - Main.screenPosition, new Rectangle(27, 0, colW, 1),
                    colorLineBG, a, org, scale * 1.6f * capPulse, 0, 0);
                Main.EntitySpriteDraw(texture, start - Main.screenPosition, new Rectangle(0, (int)textureTimer, colW, 1),
                    Color.White, a, org, scale * 1.3f * capPulse, 0, 0);

                if (hasBeam)
                {
                    Main.EntitySpriteDraw(texture, endPos - Main.screenPosition, new Rectangle(27, 0, colW, 1),
                        colorLineBG, a, org, scale * 2f * capPulse, 0, 0);
                    Main.EntitySpriteDraw(texture, endPos - Main.screenPosition, new Rectangle(0, ((int)textureTimer + (int)length / 12) % texture.Height, colW, 1),
                        Color.White, a, org, scale * 1.6f * capPulse, 0, 0);
                }
            }

            // I TRIED TO DO LIKE VISUALS AT THE EDGE AND ON THE BEAM START POSITION BUT I DROPPED THIS IDEA SO NEVERMIND
        }

        public override void AI()
        {
            EnsureTexture();

            Player player = Main.player[Projectile.owner];
            Projectile.scale = player.GetModPlayer<PlayerLaserGun>().laserScale;

            if (Projectile.scale < 0.05f)
                Projectile.scale = 0.05f;

            Projectile.position = player.Center + Projectile.velocity * 22 - new Vector2(0f, Projectile.width).RotatedBy(Projectile.rotation);

            if (texture != null)
                textureTimer = (textureTimer + 0.5f + Charge01 * 0.8f) % texture.Height;

            UpdatePlayer(player);
            SetLaserPosition(player);
            CastLights();
            SpawnEffects(player);
        }

        private void SpawnEffects(Player player)
        {
            if (Main.netMode == NetmodeID.Server) return;

            float length = Distance - moveDistance;
            if (length <= 0f) return;

            float scale = Math.Max(Projectile.scale, 0.05f);
            float charge = Charge01;
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 start = player.Center + unit * moveDistance;
            Vector2 end = player.Center + unit * Distance;

            if (noParticles)
            {
                Vector3 baseLight = new Vector3(colorLinesAround.R, colorLinesAround.G, colorLinesAround.B) / 255f;
                Lighting.AddLight(end, baseLight * (0.8f + charge * 0.9f));
                Lighting.AddLight(start, baseLight * (0.5f + charge * 0.5f));
                return;
            }

            for (int j = 0; j < Main.rand.Next(2, 5 + (int)(charge * 3)); j++)
            {
                Dust d = Dust.NewDustPerfect(end, laserDust,
                    unit.RotatedBy(MathHelper.ToRadians(180f + Main.rand.NextFloat(-70f, 70f))) * Main.rand.NextFloat(4f, 9f + charge * 5f),
                    0, default, Main.rand.NextFloat(0.85f, 1.125f) + charge * 0.3f);
                d.noGravity = true;

                Dust d2 = Dust.NewDustPerfect(end, laserDust,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 4f),
                    0, default, Main.rand.NextFloat(0.65f, 0.85f));
                d2.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust w = Dust.NewDustPerfect(end, DustID.WhiteTorch,
                    unit.RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(3f, 8f),
                    0, Color.White, Main.rand.NextFloat(0.8f, 1.3f));
                w.noGravity = true;
            }

            if (charge > 0.25f && Main.rand.NextBool(Math.Max(1, 4 - (int)(charge * 3f))))
            {
                Vector2 p = start + unit * Main.rand.NextFloat(0f, length);
                Vector2 side = unit.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1));
                Dust d = Dust.NewDustPerfect(p + side * Main.rand.NextFloat(0f, 8f * scale), laserDust,
                    side * Main.rand.NextFloat(0.5f, 2.5f) + unit * Main.rand.NextFloat(-1f, 2f),
                    0, default, Main.rand.NextFloat(0.6f, 1.1f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(Math.Max(1, 3 - (int)(charge * 2f))))
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(28f, 55f) * (0.6f + scale * 0.6f);
                Dust d = Dust.NewDustPerfect(start + off, laserDust, -off * 0.07f + unit * 1.5f, 0, default, 0.7f + charge * 0.6f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(30 - (int)(26f * scale)))
            {
                Gore gore = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), start,
                    Vector2.Zero, Main.rand.NextFromList(GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3), 1f);
                gore.position -= new Vector2(gore.Width, gore.Height) / 2 + unit * Main.rand.NextFloat(0f, moveDistance - 5);
                gore.scale = Main.rand.NextFloat(0.175f, 0.375f);
                gore.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f));
                gore.alpha = 120;
            }

            Vector3 lc = new Vector3(colorLinesAround.R, colorLinesAround.G, colorLinesAround.B) / 255f;
            Lighting.AddLight(end, lc * (0.8f + charge * 0.9f));
            Lighting.AddLight(start, lc * (0.5f + charge * 0.5f));

            if (Projectile.owner == Main.myPlayer && charge > 0.9f && Main.GameUpdateCount % 20 == 0)
                player.GetModPlayer<ScreenShakePlayer>().TriggerShake(8, 0.7f);
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

            if (Projectile.scale > 0.25f && player.controlUseItem)
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