using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.GlobalPlayer;
using Waybound.Content.Items.Weapons.Ranged.Guns.HM;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Ranged.Guns.HM
{
    public class TesseractProj1 : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/Ranged/Guns/HM/TesseractProj1";

        private const float MaxCharge = 1f;
        private const float ChargeRate = 0.014f;
        private const float MaxScale = 2f;

        private Vector2[] trailPos = new Vector2[5];
        private float[] trailRot = new float[5];
        private int trailIndex;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 0;
            Projectile.scale = 1f;
            Projectile.alpha = 35;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[1] == 0)
            {
                if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<Tesseract>() || !player.channel)
                {
                    Launch(player);
                    return;
                }

                if (Projectile.ai[0] < MaxCharge)
                {
                    Projectile.ai[0] += ChargeRate;
                    if (Projectile.ai[0] > MaxCharge)
                        Projectile.ai[0] = MaxCharge;
                }

                float charge = Projectile.ai[0];
                Projectile.scale = 1f + charge * (MaxScale - 1f);

                float animSpeed = 0.15f + charge * 0.7f;
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= Math.Max(1, (int)(7f / (animSpeed * 7f))))
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % 8;
                }

                Projectile.rotation += (0.028f + charge * 0.1f) * player.direction;

                Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                float dist = 99f + charge * 6f;
                Projectile.Center = player.MountedCenter + dir * dist;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                if (charge > 0.2f && Main.rand.NextBool(8))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f * Projectile.scale, 4f * Projectile.scale),
                        DustID.WhiteTorch, Vector2.Zero, 120, Color.White, 0.45f + charge * 0.35f);
                    d.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, 0.3f + charge * 0.5f, 0.3f + charge * 0.5f, 0.35f + charge * 0.55f);
                return;
            }

            trailPos[trailIndex] = Projectile.Center;
            trailRot[trailIndex] = Projectile.rotation;
            trailIndex = (trailIndex + 1) % trailPos.Length;

            float flyCharge = Projectile.ai[0];
            Projectile.rotation += 0.09f * Projectile.direction * (0.65f + flyCharge * 0.4f);
            Projectile.velocity.Y += 0.06f;
            Projectile.velocity *= 0.9985f;

            float anim = 0.28f + flyCharge * 0.55f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= Math.Max(1, (int)(5f / anim)))
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 8;
            }

            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Projectile.velocity * 0.08f, 90, Color.White, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.45f, 0.45f, 0.5f);
        }

        private void Launch(Player player)
        {
            float charge = Projectile.ai[0];
            Projectile.ai[1] = 1;

            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            float speed = MathHelper.Lerp(19f, 11f, charge);
            float dist = 99f + charge * 6f;
            Projectile.Center = player.MountedCenter + dir * dist;
            Projectile.velocity = dir * speed;

            Projectile.damage = (int)(Projectile.damage * (0.7f + charge * 1.5f));
            Projectile.knockBack *= 0.75f + charge * 0.85f;
            Projectile.scale = 1f + charge * (MaxScale - 1f);
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 2 + (int)(charge * 2);
            Projectile.alpha = 30;

            player.GetModPlayer<TesseractPlayer>().GlowTimer = 40;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<TesseractHeld>())
                {
                    Main.projectile[i].localAI[0] = 12;
                    break;
                }
            }

            SoundEngine.PlaySound(SoundID.Item67 with { Volume = 0.65f, Pitch = -0.1f + charge * 0.3f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.4f, Pitch = 0.4f }, player.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 flashPos = player.MountedCenter + dir * 40f;
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: flashPos.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(28f, 40f)),
                    color: new Color(255, 255, 255, 220),
                    duration: 9));
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: flashPos.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(Main.rand.NextFloat(18f, 26f)),
                    color: new Color(255, 255, 255, 180),
                    duration: 8));
            }
        }

        private void SpawnShortFlash()
        {
            for (int i = 0; i < 3; i++)
            {
                float s = 70f + i * 25f;
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(s + Main.rand.NextFloat(-8f, 8f)),
                    color: Color.White,
                    duration: 12 + i));
            }
        }

        private void SpawnBigWhiteFlash()
        {
            for (int i = 0; i < 4; i++)
            {
                float s = 140f + i * 55f;
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: Projectile.Center.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(s + Main.rand.NextFloat(-15f, 15f)),
                    color: new Color(255, 255, 255, 255 - i * 20),
                    duration: 18 + i * 2));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            float charge = Projectile.ai[0];
            Player player = Main.player[Projectile.owner];

            if (charge >= 0.98f)
            {
                SpawnBigWhiteFlash();
                for (int i = 0; i < 26; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(8.5f, 8.5f), 20, Color.White, Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.9f, Pitch = -0.4f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f }, Projectile.Center);
                if (player.active)
                    player.GetModPlayer<ScreenShakePlayer>().TriggerShake(26, 2.6f);
            }
            else
            {
                SpawnShortFlash();
                for (int i = 0; i < 7; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(3f, 3f), 50, Color.White, 1f);
                    d.noGravity = true;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 8, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            float charge = Projectile.ai[0];
            float a = (255 - Projectile.alpha) / 255f;

            if (Projectile.ai[1] == 1)
            {
                for (int i = 0; i < trailPos.Length; i++)
                {
                    int idx = (trailIndex - 1 - i + trailPos.Length) % trailPos.Length;
                    if (trailPos[idx] == Vector2.Zero) continue;
                    float ta = (1f - i / (float)trailPos.Length) * 0.4f * a;
                    float ts = Projectile.scale * (1f - i * 0.09f);
                    Main.EntitySpriteDraw(texture, trailPos[idx] - Main.screenPosition, frame, Color.White * ta, trailRot[idx], origin, ts, SpriteEffects.None, 0);
                }
            }

            if (Projectile.ai[1] == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = i switch
                    {
                        0 => new Vector2(2.2f, 0),
                        1 => new Vector2(-2.2f, 0),
                        2 => new Vector2(0, 2.2f),
                        _ => new Vector2(0, -2.2f)
                    };
                    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + off, frame,
                        Color.White * (0.3f + charge * 0.35f) * a, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
                }

                Texture2D ray = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
                Vector2 rayOrig = new Vector2(ray.Width / 2f, ray.Height);
                float rot = Main.GlobalTimeWrappedHourly * (0.55f + charge * 0.85f);
                for (int i = 0; i < 5; i++)
                {
                    float ang = MathHelper.TwoPi / 5f * i + rot;
                    float intens = (0.2f + charge * 0.5f) * a;
                    float pulse = 0.88f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + i) * 0.08f;
                    Main.EntitySpriteDraw(ray, Projectile.Center - Main.screenPosition, null, Color.White * (intens * 0.5f),
                        ang, rayOrig, new Vector2((0.3f + charge * 0.2f) * pulse, (0.85f + charge * 0.65f) * pulse), SpriteEffects.None, 0);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1f + charge * 0.6f + i * 0.25f, 0).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 1.4f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + off, frame,
                    Color.White * (0.25f - i * 0.04f) * a, Projectile.rotation, origin, Projectile.scale * 1.03f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * a, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class TesseractLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.5f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 17f;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile o = Main.projectile[i];
                if (o.active && o.owner == Projectile.owner && o.type == ModContent.ProjectileType<TesseractProj1>() && o.Hitbox.Intersects(Projectile.Hitbox))
                    o.Kill();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.85f, 0.85f, 0.95f);

            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 4; i++) 
                {
                    float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(8f, 18f);
                    Vector2 spawnPos = Projectile.Center + trailOffset;
                    Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.6f, 1.6f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        trailAngle,
                        new System.Numerics.Vector2(Main.rand.NextFloat(22f, 34f)),
                        new Color(255, 255, 255, 180) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(16, 26)
                    ));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color core = Color.White;
            Color bloom = new Color(230, 240, 255, 70);

            for (int i = 0; i < 5; i++)
            {
                float s = Projectile.scale * (1.55f - i * 0.17f);
                Color c = (i == 0 ? core : bloom) * (1f - i * 0.11f);
                Main.EntitySpriteDraw(tex, pos, null, c, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2f, new Vector2(s * 0.65f, s * 2.7f), SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(tex, pos, null, core, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2f, new Vector2(Projectile.scale * 0.5f, Projectile.scale * 3.1f), SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(3.5f, 3.5f), 40, Color.White, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
}