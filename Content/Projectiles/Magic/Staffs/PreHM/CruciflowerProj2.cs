using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Magic.Staffs.PreHM;

namespace Waybound.Content.Projectiles.Magic.Staffs.PreHM
{
    public class CruciflowerProj2 : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/Magic/Staffs/PreHM/CruciflowerProj2";

        private const float HoverHeight = -280f;
        private const float MaxCursorOffset = 40f;
        private const float FollowResponsiveness = 0.07f;
        private const int Lifetime = 300;
        private const int FadeInTime = 45;
        private const int FadeOutTime = 60;
        private const int ShootInterval = 1;

        private ref float Timer => ref Projectile.ai[0];
        private ref float ShootTimer => ref Projectile.ai[1];

        private float glowPulse;
        private float glowIntensity;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.aiStyle = -1;
            Projectile.hide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 2.35f;
        }

        public override void AI()
        {
            Timer++;
            ShootTimer++;

            Player player = Main.player[Projectile.owner];
            Vector2 anchor = player.Center + new Vector2(0f, HoverHeight);
            Vector2 toMouse = Main.MouseWorld - anchor;

            if (toMouse.Length() > MaxCursorOffset)
                toMouse = Vector2.Normalize(toMouse) * MaxCursorOffset;

            Vector2 desired = anchor + toMouse;
            Projectile.Center = Vector2.Lerp(Projectile.Center, desired, FollowResponsiveness);
            Projectile.velocity = Vector2.Zero;

            float fade = 1f;
            if (Timer < FadeInTime)
                fade = (float)Math.Pow(Timer / (float)FadeInTime, 0.65f);
            else if (Projectile.timeLeft < FadeOutTime)
                fade = Projectile.timeLeft / (float)FadeOutTime;

            Projectile.Opacity = MathHelper.Clamp(fade, 0f, 1f);

            Projectile.frame = 0;

            if (Timer > FadeInTime && Projectile.timeLeft > FadeOutTime)
            {
                if (ShootTimer >= ShootInterval)
                {
                    ShootTimer = 0f;
                    FireOne(player);
                    FireOne(player);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.4f * Projectile.Opacity, 0.65f * Projectile.Opacity, 1.05f * Projectile.Opacity);

            glowPulse += 0.045f;
            glowIntensity = MathHelper.Lerp(glowIntensity, Projectile.Opacity, 0.08f);
        }

        private void FireOne(Player player)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            float dir = Main.MouseWorld.X >= player.Center.X ? 1f : -1f;
            Vector2 baseDir = new Vector2(dir * 0.35f, 1f);
            baseDir.Normalize();

            float spread = MathHelper.ToRadians(Main.rand.NextFloat(-18f, 18f));
            Vector2 velocity = baseDir.RotatedBy(spread) * Main.rand.NextFloat(5.2f, 9.8f);

            Vector2 spawnPos = Projectile.Center + new Vector2(
                Main.rand.NextFloat(-48f, 48f),
                Main.rand.NextFloat(6f, 28f)
            );

            int dmg = (int)(Projectile.damage * 0.48f);

            int idx = Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                spawnPos,
                velocity,
                ModContent.ProjectileType<CruciflowerProj3>(),
                dmg,
                0f,
                player.whoAmI
            );

            if (idx >= 0 && idx < Main.maxProjectiles)
            {
                Main.projectile[idx].scale = 0.48f;
                Main.projectile[idx].Opacity = 0.7f;
                Main.projectile[idx].netUpdate = true;
            }

            if (Main.rand.NextBool(7))
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.18f, Pitch = 0.25f }, Projectile.Center);

            glowIntensity = Math.Min(glowIntensity + 0.2f, 1.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / 3;
            Vector2 origin = new Vector2(tex.Width * 0.5f, frameHeight * 0.5f);
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float opacity = Projectile.Opacity;
            float time = Timer * 0.04f;

            DrawBackGlow(pos);

            Rectangle frame0 = new Rectangle(0, 0, tex.Width, frameHeight);

            Main.EntitySpriteDraw(tex, pos, frame0, Color.White * opacity, 0f, origin, Projectile.scale, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(tex, pos, frame0, new Color(130, 195, 255) * (opacity * 0.4f), 0f, origin, Projectile.scale * 1.12f, SpriteEffects.None, 0f);

            Rectangle frame1 = new Rectangle(0, frameHeight, tex.Width, frameHeight);

            Vector2[] offsets1 = new Vector2[]
            {
                new Vector2(-18f, -8f),
                new Vector2( 16f, -6f),
                new Vector2(-12f,  10f),
                new Vector2( 14f,  9f),
                new Vector2(-20f,  2f),
                new Vector2( 20f, -12f),
                new Vector2( -6f, -16f),
                new Vector2(  8f,  14f)
            };

            Color outline1 = new Color(150, 205, 255) * (opacity * 0.10f);

            for (int i = 0; i < offsets1.Length; i++)
            {
                float waveX = MathF.Sin(time * 1.2f + i * 1.8f) * 3.5f;
                float waveY = MathF.Cos(time * 0.95f + i * 1.5f) * 2.8f;

                Vector2 finalOffset = offsets1[i] + new Vector2(waveX, waveY);
                float layerOp = opacity * (0.62f - i * 0.04f);

                Vector2 layerPos = pos + finalOffset;
                float layerScale = Projectile.scale * 0.82f;

                for (int j = 0; j < 4; j++)
                {
                    Vector2 off = new Vector2(1.4f, 0f).RotatedBy(MathHelper.TwoPi / 4f * j) * layerScale;
                    Main.EntitySpriteDraw(tex, layerPos + off, frame1, outline1, 0f, origin, layerScale, SpriteEffects.None, 0f);
                }

                Main.EntitySpriteDraw(tex, layerPos, frame1, Color.White * layerOp, 0f, origin, layerScale, SpriteEffects.None, 0f);
            }

            Rectangle frame2 = new Rectangle(0, frameHeight * 2, tex.Width, frameHeight);

            Vector2[] offsets2 = new Vector2[]
            {
                new Vector2(-22f,  2f),
                new Vector2( 20f,  1f),
                new Vector2( -8f, -14f),
                new Vector2(  9f,  13f),
                new Vector2(-15f,  12f),
                new Vector2( 24f, -8f),
                new Vector2(-24f, -6f),
                new Vector2( 12f, -18f),
                new Vector2( -4f,  16f),
                new Vector2( 18f,  15f)
            };

            Color outline2 = new Color(150, 205, 255) * (opacity * 0.08f);

            for (int i = 0; i < offsets2.Length; i++)
            {
                float waveX = MathF.Sin(time * 1.05f + i * 2.1f + 1.3f) * 4.0f;
                float waveY = MathF.Cos(time * 1.15f + i * 1.7f) * 3.2f;

                Vector2 finalOffset = offsets2[i] + new Vector2(waveX, waveY);
                float layerOp = opacity * (0.55f - i * 0.035f);

                Vector2 layerPos = pos + finalOffset;
                float layerScale = Projectile.scale * 0.75f;

                for (int j = 0; j < 4; j++)
                {
                    Vector2 off = new Vector2(1.2f, 0f).RotatedBy(MathHelper.TwoPi / 4f * j) * layerScale;
                    Main.EntitySpriteDraw(tex, layerPos + off, frame2, outline2, 0f, origin, layerScale, SpriteEffects.None, 0f);
                }

                Main.EntitySpriteDraw(tex, layerPos, frame2, Color.White * layerOp, 0f, origin, layerScale, SpriteEffects.None, 0f);
            }

            return false;
        }

        private void DrawBackGlow(Vector2 pos)
        {
            Texture2D glow = ModContent.Request<Texture2D>(
                "Waybound/Particles/Megaspark",
                AssetRequestMode.ImmediateLoad
            ).Value;

            Vector2 glowOrigin = glow.Size() * 0.5f;
            float aspect = glow.Width / (float)glow.Height;

            float swell = MathF.Sin(MathHelper.Pi * (1f - Projectile.timeLeft / (float)Lifetime));
            float scale = Projectile.scale * 0.07f * (0.4f + swell * 1.2f);
            float alpha = glowIntensity * swell;

            Vector2 scaleVec = new Vector2(scale / aspect, scale);

            Main.EntitySpriteDraw(glow, pos, null,
                new Color(60, 130, 235) * (alpha * 0.75f),
                glowPulse * 0.15f, glowOrigin, scaleVec, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(glow, pos, null,
                new Color(110, 185, 255) * (alpha * 0.9f),
                -glowPulse * 0.25f, glowOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(glow, pos, null,
                new Color(175, 220, 255) * alpha,
                glowPulse * 0.4f, glowOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(glow, pos, null,
                new Color(235, 248, 255) * alpha,
                -glowPulse * 0.6f, glowOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);
        }
    }
    public class CruciflowerProj3 : ModProjectile
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

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.25f * Projectile.Opacity, 0.55f * Projectile.Opacity, 0.95f * Projectile.Opacity);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.IceTorch,
                    Projectile.velocity * 0.05f,
                    100,
                    new Color(140, 210, 255),
                    Main.rand.NextFloat(0.5f, 0.8f)
                );
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.alpha = 120;
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
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 80, new Color(120, 200, 255), 0.9f);
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
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 60, new Color(150, 220, 255), Main.rand.NextFloat(0.6f, 1.0f));
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float alpha = Projectile.Opacity;

            if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.35f);

            if (oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;

                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 3.2f;

                Color trailColor = new Color(90, 180, 255, 0) * 0.6f * alpha;

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
                    new Color(180, 230, 255, 0) * 0.5f * alpha,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.28f, trailScaleY * 1.15f),
                    SpriteEffects.None,
                    0f
                );
            }

            Color outlineColor = new Color(100, 190, 255) * 0.5f * alpha;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i) * Projectile.scale;
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
                Color.White * alpha,
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