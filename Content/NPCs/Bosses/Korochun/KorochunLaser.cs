using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.NPCs.Bosses.Korochun
{
    public class KorochunLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        private const int Life = 70;
        private const float BeamLength = 2600f;

        private static readonly Color colorLineBG = new Color(14, 48, 120);
        private static readonly Color colorLinesAround = new Color(60, 130, 200);

        private static Texture2D texture;
        private static Texture2D rayTexture;
        private static int framesAmount = 3;
        private float textureTimer;

        private float Angle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float Power
        {
            get
            {
                float age = Life - Projectile.timeLeft;
                float grow = MathHelper.Clamp(age / 9f, 0f, 1f);
                grow = 1f - (1f - grow) * (1f - grow);
                float shrink = MathHelper.Clamp(Projectile.timeLeft / 16f, 0f, 1f);
                float pulse = 1f + 0.05f * MathF.Sin(age * 1.5f);
                return grow * shrink * pulse;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Life;
        }

        public override void AI()
        {
            int bossIndex = (int)Projectile.ai[0];
            if (bossIndex < 0 || bossIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC boss = Main.npc[bossIndex];
            if (!boss.active || boss.ModNPC is not Korochun korochun)
            {
                Projectile.Kill();
                return;
            }

            Angle += boss.direction * 0.002f;

            Projectile.Center = korochun.Mouth;
            Projectile.rotation = Angle;
            Projectile.velocity = Angle.ToRotationVector2();

            EnsureTexture();
            if (texture != null)
                textureTimer = (textureTimer + 0.55f + Power * 0.9f) % texture.Height;

            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2 start = Projectile.Center;
            Vector2 dir = Projectile.velocity;
            float th = Power;

            for (int i = 0; i < 2; i++)
            {
                float d = Main.rand.NextFloat(0f, 1400f);
                Vector2 side = dir.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1));
                Dust dust = Dust.NewDustPerfect(
                    start + dir * d + side * Main.rand.NextFloat(0f, 35f * th),
                    Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.IceTorch,
                    side * Main.rand.NextFloat(0.4f, 2.2f) + dir * Main.rand.NextFloat(-0.8f, 3f),
                    60, Color.White, Main.rand.NextFloat(0.9f, 1.5f));
                dust.noGravity = true;
            }

            for (float d = 0f; d < BeamLength; d += 220f)
                Lighting.AddLight(start + dir * d, 0.12f * th, 0.28f * th, 0.5f * th);
        }

        public override bool? CanDamage() => Power > 0.35f ? null : false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity * BeamLength;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 70f * Power, ref point);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Chilled, 240);
        }

        private void EnsureTexture()
        {
            if (texture == null || texture.IsDisposed)
                texture = Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad).Value;
            if (rayTexture == null || rayTexture.IsDisposed)
                rayTexture = Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float th = Power;
            if (th <= 0.01f)
                return false;

            EnsureTexture();
            if (texture == null)
                return false;

            DrawLaser(Projectile.Center, Projectile.velocity, th);
            return false;
        }

        private void DrawLaser(Vector2 start, Vector2 unit, float charge)
        {
            float scale = MathHelper.Lerp(0.7f, 1.35f, charge);
            float time = Main.GlobalTimeWrappedHourly;
            int colW = texture.Width / framesAmount;
            Vector2 org = new Vector2(colW, 1f) / 2f;

            unit = unit.SafeNormalize(Vector2.UnitX);
            float r = unit.ToRotation();
            float length = BeamLength;

            const float step = 4f;
            float thickness = 26f * scale;

            BeginAdditive();

            for (float i = 0; i <= length; i += step)
            {
                Vector2 p = start + i * unit - Main.screenPosition;

                float wave = 1f
                    + (0.06f + 0.07f * charge) * MathF.Sin(i * 0.045f - time * 14f)
                    + 0.035f * MathF.Sin(i * 0.13f + time * 23f);

                float taper = MathHelper.Clamp(i / 28f, 0.35f, 1f);
                float tail = MathHelper.Clamp((length - i) / 180f, 0f, 1f);
                float w = scale * wave * taper * tail;
                float th = thickness * wave * taper * tail;

                Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                    colorLineBG * (0.28f + 0.22f * charge), r, org, new Vector2(w * 2.1f, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                    colorLineBG * 0.9f * charge, r, org, new Vector2(w, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(54, (int)textureTimer * 4 % texture.Height, colW, 1),
                    colorLinesAround * charge, r, org, new Vector2(w * 1.05f, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(0, ((int)textureTimer + (int)i / 12) % texture.Height, colW, 1),
                    Color.White * charge, r, org, new Vector2(w * 0.55f, th), SpriteEffects.None, 0);

                if (charge > 0.5f)
                {
                    Main.EntitySpriteDraw(texture, p, new Rectangle(0, ((int)textureTimer * 2 + (int)i / 6) % texture.Height, colW, 1),
                        Color.White * ((charge - 0.5f) * 1.6f), r, org, new Vector2(w * 0.25f, th), SpriteEffects.None, 0);
                }
            }

            float capPulse = 1f + 0.12f * MathF.Sin(time * 16f);
            Vector2 endPos = start + unit * length;

            for (int i = 0; i < 180; i += 6)
            {
                float a = r + MathHelper.ToRadians(i);

                Main.EntitySpriteDraw(texture, endPos - Main.screenPosition, new Rectangle(27, 0, colW, 1),
                    colorLineBG * charge, a, org, new Vector2(thickness * 1.4f, thickness * 2f * capPulse), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, endPos - Main.screenPosition, new Rectangle(0, ((int)textureTimer + (int)length / 12) % texture.Height, colW, 1),
                    Color.White * charge, a, org, new Vector2(thickness * 1.1f, thickness * 1.6f * capPulse), SpriteEffects.None, 0);
            }

            EndAdditive();
        }

        private static void BeginAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void EndAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DrawBeam(Vector2 mouthWorld, float ang, float length,
                                    float scale, float power, float alpha = 1f)
        {
            if (power <= 0.01f || alpha <= 0.01f)
                return;

            if (texture == null || texture.IsDisposed)
                texture = Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad).Value;
            if (texture == null)
                return;

            float charge = MathHelper.Clamp(power * scale * alpha, 0.05f, 1.2f);
            Vector2 unit = ang.ToRotationVector2();
            float time = Main.GlobalTimeWrappedHourly;
            float texTimer = (time * 28f) % texture.Height;

            int colW = texture.Width / framesAmount;
            Vector2 org = new Vector2(colW, 1f) / 2f;
            float r = unit.ToRotation();
            const float step = 3f;
            float thickness = 14f * scale;

            BeginAdditive();

            for (float i = 0; i <= length; i += step)
            {
                Vector2 p = mouthWorld + i * unit - Main.screenPosition;

                float wave = 1f
                    + (0.05f + 0.06f * charge) * MathF.Sin(i * 0.045f - time * 14f)
                    + 0.03f * MathF.Sin(i * 0.13f + time * 23f);

                float taper = MathHelper.Clamp(i / 28f, 0.35f, 1f);
                float tail = MathHelper.Clamp((length - i) / 180f, 0f, 1f);
                float w = scale * wave * taper * tail * charge;
                float th = thickness * wave * taper * tail;

                Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                    colorLineBG * (0.25f + 0.2f * charge), r, org, new Vector2(w * 1.8f, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(27, 0, colW, 1),
                    colorLineBG * 0.7f * charge, r, org, new Vector2(w * 0.9f, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(54, (int)texTimer * 4 % texture.Height, colW, 1),
                    colorLinesAround * charge, r, org, new Vector2(w * 0.95f, th), SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, p, new Rectangle(0, ((int)texTimer + (int)i / 12) % texture.Height, colW, 1),
                    Color.White * charge, r, org, new Vector2(w * 0.45f, th), SpriteEffects.None, 0);
            }

            float capPulse = 1f + 0.1f * MathF.Sin(time * 16f);
            for (int i = 0; i < 180; i += 8)
            {
                float a = r + MathHelper.ToRadians(i);
                Main.EntitySpriteDraw(texture, mouthWorld - Main.screenPosition, new Rectangle(27, 0, colW, 1),
                    colorLineBG * charge * 0.7f, a, org, new Vector2(thickness * 1.2f, thickness * 1.3f * capPulse), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, mouthWorld - Main.screenPosition, new Rectangle(0, (int)texTimer, colW, 1),
                    Color.White * charge * 0.7f, a, org, new Vector2(thickness * 0.9f, thickness * 1.1f * capPulse), SpriteEffects.None, 0);
            }

            EndAdditive();
        }
    }
}