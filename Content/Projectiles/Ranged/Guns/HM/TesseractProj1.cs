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
    internal static class TesseractFx
    {
        public static readonly Color Ice = new Color(120, 200, 255, 0);
        public static readonly Color Violet = new Color(175, 120, 255, 0);

        public static void Flash(Vector2 pos, float size, Color color, int duration)
        {
            if (Main.netMode == NetmodeID.Server) return;
            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: pos.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(size),
                color: color,
                duration: duration));
        }

        public static void Ring(Vector2 center, int count, float speed, int dustType, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count);
                Dust d = Dust.NewDustPerfect(center + dir * 6f, dustType, dir * speed, 20, Color.White, scale);
                d.noGravity = true;
            }
        }

        public static void BeginAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void EndAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public class TesseractProj1 : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/Ranged/Guns/HM/TesseractProj1";

        private const float MaxCharge = 1f;
        private const float ChargeRate = 0.014f;
        private const float MaxScale = 2.4f;
        private const int TrailLen = 10;

        private Vector2[] trailPos = new Vector2[TrailLen];
        private float[] trailRot = new float[TrailLen];
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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[1] == 0)
            {
                ChargingAI(player);
                return;
            }

            FlyingAI();
        }

        private void ChargingAI(Player player)
        {
            if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<Tesseract>() || !player.channel)
            {
                Launch(player);
                return;
            }

            bool wasFull = Projectile.ai[0] >= MaxCharge;
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

            Projectile.rotation += (0.028f + charge * 0.12f) * player.direction;

            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            float dist = 99f + charge * 14f;
            Projectile.Center = player.MountedCenter + dir * dist;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;

            if (charge > 0.1f && Main.rand.NextBool(Math.Max(1, 4 - (int)(charge * 3f))))
            {
                float radius = Main.rand.NextFloat(55f, 110f) * (0.6f + charge * 0.7f);
                Vector2 off = Main.rand.NextVector2CircularEdge(radius, radius);
                int type = Main.rand.NextBool(2) ? DustID.IceTorch : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + off, type, -off * 0.055f, 60, Color.White, 0.8f + charge * 0.7f);
                d.noGravity = true;
            }

            if (charge > 0.2f && Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f * Projectile.scale, 4f * Projectile.scale),
                    DustID.WhiteTorch, Vector2.Zero, 120, Color.White, 0.45f + charge * 0.35f);
                d.noGravity = true;
            }

            if (!wasFull && charge >= MaxCharge && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
                TesseractFx.Flash(Projectile.Center, 130f, new Color(190, 225, 255, 200), 14);
                TesseractFx.Ring(Projectile.Center, 28, 7f, DustID.IceTorch, 1.5f);
                player.GetModPlayer<ScreenShakePlayer>().TriggerShake(10, 1.2f);
            }

            if (charge >= MaxCharge && Main.GameUpdateCount % 18 == 0)
                TesseractFx.Ring(Projectile.Center, 10, 3.5f, DustID.PurpleTorch, 1.1f);

            Lighting.AddLight(Projectile.Center, 0.3f + charge * 0.6f, 0.4f + charge * 0.6f, 0.5f + charge * 0.8f);
        }

        private void FlyingAI()
        {
            trailPos[trailIndex] = Projectile.Center;
            trailRot[trailIndex] = Projectile.rotation;
            trailIndex = (trailIndex + 1) % trailPos.Length;

            float flyCharge = Projectile.ai[0];
            Projectile.rotation += 0.09f * Projectile.direction * (0.65f + flyCharge * 0.4f);

            if (flyCharge >= 0.5f)
            {
                NPC target = null;
                float best = 480f * 480f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.CanBeChasedBy(Projectile)) continue;
                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < best)
                    {
                        best = d;
                        target = n;
                    }
                }
                if (target != null)
                {
                    float spd = Math.Max(Projectile.velocity.Length(), 9f);
                    Vector2 want = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * spd;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, 0.05f);
                }
                else
                {
                    Projectile.velocity.Y += 0.06f;
                }
            }
            else
            {
                Projectile.velocity.Y += 0.06f;
            }
            Projectile.velocity *= 0.9985f;

            float anim = 0.28f + flyCharge * 0.55f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= Math.Max(1, (int)(5f / anim)))
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 8;
            }

            if (Main.rand.NextBool(3))
            {
                int type = Main.rand.NextBool(2) ? DustID.IceTorch : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f) * Projectile.scale,
                    type, -Projectile.velocity * 0.12f, 90, Color.White, 0.8f + flyCharge * 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.45f + flyCharge * 0.3f, 0.5f + flyCharge * 0.3f, 0.65f + flyCharge * 0.4f);
        }

        private void Launch(Player player)
        {
            float charge = Projectile.ai[0];
            Projectile.ai[1] = 1;

            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            float speed = MathHelper.Lerp(19f, 12f, charge);
            float dist = 99f + charge * 14f;
            Projectile.Center = player.MountedCenter + dir * dist;
            Projectile.velocity = dir * speed;

            Projectile.damage = (int)(Projectile.damage * (0.7f + charge * 1.8f));
            Projectile.knockBack *= 0.75f + charge * 0.85f;
            Projectile.scale = 1f + charge * (MaxScale - 1f);
            int size = (int)(24 * Projectile.scale);
            Projectile.Resize(size, size);
            Projectile.tileCollide = true;
            Projectile.timeLeft = 260;
            Projectile.penetrate = 2 + (int)(charge * 3);
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

            if (charge > 0.4f)
                player.GetModPlayer<ScreenShakePlayer>().TriggerShake((int)(8 + charge * 8), 0.8f + charge * 1.2f);

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 flashPos = player.MountedCenter + dir * 40f;
                TesseractFx.Flash(flashPos, Main.rand.NextFloat(28f, 40f) * (1f + charge), new Color(255, 255, 255, 220), 9);
                TesseractFx.Flash(flashPos, Main.rand.NextFloat(18f, 26f) * (1f + charge), new Color(160, 210, 255, 180), 8);

                for (int i = 0; i < 6 + (int)(charge * 14); i++)
                {
                    Vector2 v = dir.RotatedByRandom(0.45f) * Main.rand.NextFloat(3f, 9f + charge * 6f);
                    Dust d = Dust.NewDustPerfect(flashPos, Main.rand.NextBool(2) ? DustID.IceTorch : DustID.WhiteTorch, v, 40, Color.White, Main.rand.NextFloat(1f, 1.8f));
                    d.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] != 1) return;
            float charge = Projectile.ai[0];
            TesseractFx.Flash(target.Center, 50f + charge * 70f, new Color(190, 225, 255, 200), 10);
            for (int i = 0; i < 6 + (int)(charge * 8); i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.IceTorch, Main.rand.NextVector2Circular(5f, 5f), 40, Color.White, 1.3f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Projectile.ai[1] != 1) return; 

            float charge = Projectile.ai[0];
            bool combo = Projectile.ai[2] == 1f;
            bool full = charge >= 0.98f || combo;
            Player player = Main.player[Projectile.owner];

            if (full)
            {
                for (int i = 0; i < 5; i++)
                {
                    float s = 160f + i * 70f;
                    Color c = Color.Lerp(Color.White, TesseractFx.Violet, i / 5f);
                    c.A = (byte)(255 - i * 25);
                    TesseractFx.Flash(Projectile.Center, s + Main.rand.NextFloat(-15f, 15f), c, 18 + i * 3);
                }

                TesseractFx.Ring(Projectile.Center, 40, 12f, DustID.WhiteTorch, 2.2f);
                TesseractFx.Ring(Projectile.Center, 32, 8f, DustID.IceTorch, 2f);
                TesseractFx.Ring(Projectile.Center, 24, 4.5f, DustID.PurpleTorch, 1.8f);

                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(11f, 11f), 20, Color.White, Main.rand.NextFloat(1.5f, 2.8f));
                    d.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1f, Pitch = -0.5f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.8f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.Center);

                if (player.active)
                    player.GetModPlayer<ScreenShakePlayer>().TriggerShake(combo ? 40 : 30, combo ? 4f : 3f);

                if (Main.myPlayer == Projectile.owner)
                {
                    int dmg = (int)(Projectile.damage * (combo ? 2f : 1.4f));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<TesseractBlast>(), dmg, Projectile.knockBack * 1.5f, Projectile.owner);

                    int shards = combo ? 12 : 8;
                    float start = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < shards; i++)
                    {
                        Vector2 v = Vector2.UnitX.RotatedBy(start + MathHelper.TwoPi * i / shards) * 14f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, v,
                            ModContent.ProjectileType<TesseractLaser>(), (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.5f, Projectile.owner, 0f, 1f);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    float s = 70f + i * 25f;
                    TesseractFx.Flash(Projectile.Center, s + Main.rand.NextFloat(-8f, 8f), Color.White, 12 + i);
                }
                TesseractFx.Ring(Projectile.Center, 14, 4f, DustID.IceTorch, 1.2f);
                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(3f, 3f), 50, Color.White, 1f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
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
            Vector2 center = Projectile.Center - Main.screenPosition;

            TesseractFx.BeginAdditive();

            if (Projectile.ai[1] == 1)
            {
                for (int i = 0; i < trailPos.Length; i++)
                {
                    int idx = (trailIndex - 1 - i + trailPos.Length) % trailPos.Length;
                    if (trailPos[idx] == Vector2.Zero) continue;
                    float k = 1f - i / (float)trailPos.Length;
                    Color tc = Color.Lerp(TesseractFx.Violet, TesseractFx.Ice, k) * (k * 0.5f * a);
                    float ts = Projectile.scale * (0.5f + k * 0.55f);
                    Main.EntitySpriteDraw(texture, trailPos[idx] - Main.screenPosition, frame, tc, trailRot[idx], origin, ts, SpriteEffects.None, 0);
                }
            }

            if (Projectile.ai[1] == 0)
            {
                Texture2D ray = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
                Vector2 rayOrig = new Vector2(ray.Width / 2f, ray.Height);
                float rot = Main.GlobalTimeWrappedHourly * (0.55f + charge * 1.1f);
                int rays = 5 + (int)(charge * 5);
                for (int i = 0; i < rays; i++)
                {
                    float ang = MathHelper.TwoPi / rays * i + rot;
                    float intens = (0.25f + charge * 0.6f) * a;
                    float pulse = 0.88f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i) * 0.1f;
                    Color rc = Color.Lerp(TesseractFx.Ice, TesseractFx.Violet, (i % 2) * 0.7f) * (intens * 0.55f);
                    Main.EntitySpriteDraw(ray, center, null, rc, ang, rayOrig,
                        new Vector2((0.3f + charge * 0.25f) * pulse, (0.9f + charge * 1.1f) * pulse), SpriteEffects.None, 0);
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = i switch
                    {
                        0 => new Vector2(2.4f, 0),
                        1 => new Vector2(-2.4f, 0),
                        2 => new Vector2(0, 2.4f),
                        _ => new Vector2(0, -2.4f)
                    };
                    Main.EntitySpriteDraw(texture, center + off, frame, TesseractFx.Ice * (0.35f + charge * 0.4f) * a,
                        Projectile.rotation, origin, Projectile.scale * 1.12f, SpriteEffects.None, 0);
                }
            }

            float halo = 1.25f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.06f + charge * 0.25f;
            Main.EntitySpriteDraw(texture, center, frame, TesseractFx.Violet * (0.22f + charge * 0.25f) * a,
                -Projectile.rotation, origin, Projectile.scale * halo, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1f + charge * 0.9f + i * 0.35f, 0).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 1.6f);
                Main.EntitySpriteDraw(texture, center + off, frame, Color.White * (0.28f - i * 0.05f) * a,
                    Projectile.rotation, origin, Projectile.scale * 1.03f, SpriteEffects.None, 0);
            }

            TesseractFx.EndAdditive();

            Main.EntitySpriteDraw(texture, center, frame, Color.White * a, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class TesseractBlast : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        public override void SetDefaults()
        {
            Projectile.width = 340;
            Projectile.height = 340;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class TesseractLaser : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        private bool Shard => Projectile.ai[1] == 1f;

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
            Projectile.timeLeft = 100;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.7f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * (Shard ? 15f : 19f);
                if (Shard)
                    Projectile.scale = 1.0f;
            }

            if (!Shard)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile o = Main.projectile[i];
                    if (o.active && o.owner == Projectile.owner && o.type == ModContent.ProjectileType<TesseractProj1>()
                        && o.ai[1] == 1f && o.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        o.ai[2] = 1f;
                        o.netUpdate = true;
                        o.Kill();
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.8f, 0.9f, 1.1f);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                int count = Shard ? 2 : 5;
                for (int i = 0; i < count; i++)
                {
                    float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(8f, 20f);
                    Vector2 spawnPos = Projectile.Center + trailOffset;
                    Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.6f, 1.8f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        trailAngle,
                        new System.Numerics.Vector2(Main.rand.NextFloat(22f, 36f)),
                        new Color(255, 255, 255, 180) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(16, 28)
                    ));
                }

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(2) ? DustID.IceTorch : DustID.PurpleTorch,
                        Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), 60, Color.White, 1.2f);
                    d.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            TesseractFx.Flash(target.Center, Shard ? 40f : 80f, new Color(200, 230, 255, 200), 9);
            for (int i = 0; i < (Shard ? 3 : 8); i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(4.5f, 4.5f), 40, Color.White, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color core = Color.White;
            float rot = Projectile.rotation + MathHelper.PiOver2;
            float len = Shard ? 2.2f : 3.6f;

            TesseractFx.BeginAdditive();

            for (int i = 0; i < 6; i++)
            {
                float s = Projectile.scale * (1.8f - i * 0.2f);
                Color c = Color.Lerp(TesseractFx.Violet, TesseractFx.Ice, i / 5f) * (0.55f - i * 0.07f);
                Main.EntitySpriteDraw(tex, pos, null, c, rot, tex.Size() / 2f, new Vector2(s * 0.7f, s * len), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, core, rot, tex.Size() / 2f,
                new Vector2(Projectile.scale * 0.45f, Projectile.scale * (len + 0.5f)), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, core * 0.8f, rot, tex.Size() / 2f,
                new Vector2(Projectile.scale * 0.25f, Projectile.scale * (len + 1.2f)), SpriteEffects.None, 0);

            TesseractFx.EndAdditive();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;
            int n = Shard ? 6 : 16;
            for (int i = 0; i < n; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2Circular(4.5f, 4.5f), 40, Color.White, 1.4f);
                d.noGravity = true;
            }
            if (!Shard)
            {
                TesseractFx.Flash(Projectile.Center, 90f, new Color(200, 230, 255, 220), 12);
                TesseractFx.Ring(Projectile.Center, 14, 5f, DustID.IceTorch, 1.4f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
    }
}