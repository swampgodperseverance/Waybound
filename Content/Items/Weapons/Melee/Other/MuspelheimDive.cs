using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using static Waybound.Content.Items.Weapons.Melee.Other.Muspelheim;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class MuspelheimDive : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Other/MuspelheimSpear";

        private const float GripAlong = 0.25f;
        public const float MaxTeleportRange = 1200f;
        private const int WindupTime = 28;
        private const int RiseTime = 18;
        private const int RainStart = 6;
        private const int RainInterval = 3;
        private const int MinMeteors = 7;
        private const int MaxMeteors = 10;
        private const float MeteorSpread = 190f;
        private const float TeleportHeightOffset = -220f;
        private const float RiseSpeed = 6f;

        private const float TiltWindup = -0.48f; 
        private const float GripLift = 10f;     //Versatile tho
        private const int SwingDownTime = 8;     

        private int phase;
        private int timer;
        private int dir = 1;
        private int meteorsLeft;
        private Vector2 impactPos;
        private Vector2 prevTip;
        private bool hasPrevTip;
        private float tilt;
        private float lift;
        private float diveStartTilt;
        private SpearTrail trail;

        private float SpearLength => MuspelheimFx.SpriteLength(Type, Projectile.scale);

        public static bool TryGetDestination(Player player, out Vector2 dest)
        {
            dest = default;
            Vector2 target = Main.MouseWorld + new Vector2(0f, TeleportHeightOffset);
            Vector2 delta = target - player.Center;
            if (delta.Length() > MaxTeleportRange)
                target = player.Center + delta.SafeNormalize(Vector2.UnitY) * MaxTeleportRange;

            Vector2 half = new Vector2(player.width, player.height) * 0.5f;
            for (int i = 0; i <= 24; i++)
            {
                Vector2 c = target + new Vector2(0f, -i * 8f);
                if (c.X < 400f || c.Y < 400f || c.X > Main.maxTilesX * 16f - 400f || c.Y > Main.maxTilesY * 16f - 400f)
                    continue;
                if (!Collision.SolidCollision(c - half, player.width, player.height))
                {
                    dest = c;
                    return true;
                }
            }
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.timeLeft = 600;
            Projectile.scale = 1.2f;
            trail = new SpearTrail(14);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            var mp = player.GetModPlayer<MuspelheimPlayer>();
            bool owner = Projectile.owner == Main.myPlayer;
            Projectile.timeLeft = 5;
            timer++;

            if (!Main.dedServ)
                trail.Step();

            float length = SpearLength;
            Vector2 down = Vector2.UnitY;

            if (phase == 0)
            {
                dir = player.direction == 0 ? 1 : player.direction;
                Vector2 dest = new Vector2(Projectile.ai[1], Projectile.ai[2]);
                Vector2 from = player.Center;

                if (owner)
                {
                    player.Center = dest;
                    player.velocity = Vector2.Zero;
                    player.oldPosition = player.position;
                    player.fallStart = (int)(player.position.Y / 16f);
                    mp.UltFreeze = true;
                }

                if (!Main.dedServ)
                {
                    MuspelheimFx.Burst(from, 18, 5f, 22f, 38f);
                    MuspelheimFx.Burst(dest, 18, 5f, 22f, 38f);
                }

                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.8f, Pitch = 0.2f }, from);
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.8f, Pitch = 0.2f }, dest);

                tilt = 0f;
                lift = 0f;
                phase = 1;
                timer = 0;
            }

            if (phase == 1)
            {
                float totalTime = RiseTime + WindupTime;
                float riseProgress = MathHelper.Clamp(timer / (float)RiseTime, 0f, 1f);
                float total = MathHelper.Clamp(timer / totalTime, 0f, 1f);

                if (owner)
                {
                    if (timer < RiseTime)
                    {
                        mp.UltFreeze = false;
                        float liftCurve = MathF.Sin(riseProgress * MathHelper.PiOver2);
                        player.velocity = new Vector2(0f, -RiseSpeed * liftCurve);
                    }
                    else
                    {
                        mp.UltFreeze = true;
                        player.velocity = Vector2.Zero;
                    }
                }

                float tiltProgress;
                if (timer < RiseTime)
                    tiltProgress = riseProgress * 0.45f;
                else
                {
                    float t = MathHelper.Clamp((timer - RiseTime) / (float)WindupTime, 0f, 1f);
                    tiltProgress = 0.45f + 0.55f * Smooth(t);
                }

                float targetTilt = MathHelper.Lerp(0f, TiltWindup, tiltProgress);
                if (timer >= RiseTime)
                    targetTilt += MathF.Sin(timer * 1.7f) * 0.015f * tiltProgress;

                tilt = MathHelper.Lerp(tilt, targetTilt, 0.35f);
                lift = GripLift * Smooth(total);
                ApplyPose(player);

                if (!Main.dedServ)
                {
                    Vector2 spearDir = Projectile.rotation.ToRotationVector2();
                    Vector2 tip = Projectile.Center + spearDir * (1f - GripAlong) * length;

                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(50f, 110f);
                        Vector2 vel = -off.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 5f) * (0.5f + tiltProgress);
                        MuspelheimFx.SpawnFire(tip + off, vel, Main.rand.NextFloat(14f, 24f), Main.rand.Next(14, 22));
                    }

                    if (timer >= RiseTime && timer % 6 == 0)
                        SpawnFireFlash(tip, 0.5f + 0.6f * tiltProgress, 10);
                }

                if (timer >= totalTime)
                {
                    phase = 2;
                    timer = 0;
                    diveStartTilt = tilt;

                    if (owner)
                    {
                        mp.UltFreeze = false;
                        mp.UltDive = true;
                        player.velocity = new Vector2(0f, MuspelheimPlayer.DiveSpeed);
                    }

                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.5f, Volume = 1.2f }, player.Center);

                    SoundEngine.PlaySound(new SoundStyle("Waybound/Assets/Sounds/Weapons/DragonRoar")
                    {
                        Volume = 0.95f,
                        PitchVariance = 0.05f
                    }, player.Center);

                    if (!Main.dedServ)
                    {
                        Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * (1f - GripAlong) * length;

                        SpawnFireFlash(tip, 2.2f, 16);
                        MuspelheimFx.Burst(tip, 16, 7f, 18f, 30f);
                    }
                }
            }
            else if (phase == 2)
            {
                if (owner)
                {
                    player.velocity.X = 0f;
                    player.velocity.Y = MuspelheimPlayer.DiveSpeed;
                }

                float sp = MathHelper.Clamp(timer / (float)SwingDownTime, 0f, 1f);
                tilt = MathHelper.Lerp(diveStartTilt, 0f, EaseOutCubic(sp));
                lift = GripLift * (1f - EaseOutCubic(sp));
                ApplyPose(player);

                Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * (1f - GripAlong) * length;
                Lighting.AddLight(tip, 1.6f, 0.75f, 0.25f);

                if (!Main.dedServ)
                {
                    if (timer % 2 == 0)
                        SpawnFireFlash(tip, 1.0f, 10);
                    for (int i = 0; i < 3; i++)
                    {
                        float u = MathHelper.Lerp(0.15f, 1f, Main.rand.NextFloat());
                        Vector2 pos = Projectile.Center + down * (u - GripAlong) * length + Main.rand.NextVector2Circular(6f, 6f);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -Main.rand.NextFloat(1f, 4f));
                        MuspelheimFx.SpawnFire(pos, vel, Main.rand.NextFloat(18f, 32f), Main.rand.Next(18, 30));
                    }
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(tip + Main.rand.NextVector2Circular(8f, 8f), DustID.Torch,
                            new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(3f, 8f)),
                            100, default, Main.rand.NextFloat(1.2f, 1.8f));
                        d.noGravity = true;
                    }
                }

                bool landed = timer > 6 && HasSolidBelow(player);
                if (landed || timer > 120)
                    Impact(player, mp, owner);
            }
            else if (phase == 3)
            {
                Projectile.Center = impactPos;

                if (owner && meteorsLeft > 0 && timer >= RainStart && (timer - RainStart) % RainInterval == 0)
                {
                    SpawnMeteor(player);
                    meteorsLeft--;
                }

                if (timer > RainStart + RainInterval * MaxMeteors + 30)
                    Projectile.Kill();
            }

            if (!Main.dedServ)
            {
                if (phase < 3)
                {
                    Vector2 spearDir = Projectile.rotation.ToRotationVector2();
                    Vector2 tip = Projectile.Center + spearDir * (1f - GripAlong) * length;

                    if (!hasPrevTip)
                    {
                        prevTip = tip;
                        hasPrevTip = true;
                    }

                    for (int s = 0; s < SpearTrail.Substeps; s++)
                    {
                        float t = (s + 1f) / SpearTrail.Substeps;
                        trail.Add(Vector2.Lerp(prevTip, tip, t), spearDir, (SpearTrail.Substeps - 1 - s) / (float)SpearTrail.Substeps);
                    }
                    prevTip = tip;
                }
                trail.Prune();
            }
        }
        private void ApplyPose(Player player)
        {
            Projectile.rotation = MathHelper.PiOver2 + tilt * dir;
            Projectile.Center = player.MountedCenter + new Vector2(0f, player.gfxOffY + 4f - lift);

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.ChangeDir(dir);
            player.itemRotation = MathF.Atan2(dir, 0f);

            float armRot = Projectile.rotation - MathHelper.PiOver2;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
        }
        private static void SpawnFireFlash(Vector2 pos, float scale, int baseDuration)
        {
            if (Main.dedServ || ParticleSystem.FlashBuffer == null)
                return;

            Color[] colors =
            {
                new Color(255, 235, 170, 255),
                new Color(255, 150, 50, 220),
                new Color(255, 80, 20, 160)
            };

            for (int i = 0; i < 3; i++)
            {
                float s = (40f + i * 16f) * scale;

                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new SysVector2(s + Main.rand.NextFloat(-3f, 3f)),
                    color: colors[i],
                    duration: baseDuration + i * 2
                ));
            }
        }
        private static void SpawnBoom(Vector2 pos, Vector2 vel, float sizeX, float sizeY, int duration, bool randomRotation)
        {
            if (Main.dedServ || ParticleSystem.FlameBoomBuffer == null)
                return;

            ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                position: pos.ToNumerics(),
                velocity: vel.ToNumerics(),
                rotation: randomRotation ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f,
                scale: new SysVector2(sizeX, sizeY),
                color: new Color(255, 140, 40),
                duration: duration
            ));
        }

        private static void SpawnImpactBoom(Vector2 pos)
        {
            if (Main.dedServ || ParticleSystem.FlameBoomBuffer == null)
                return;

            Vector2 center = pos + new Vector2(0f, -24f);
            SpawnBoom(center, Vector2.Zero, 170f, 170f, 22, true);
            SpawnBoom(center, Vector2.Zero, 320f, 320f, 34, true);
            SpawnBoom(center, Vector2.Zero, 480f, 480f, 46, true);
            Vector2 ground = pos + new Vector2(0f, -6f);
            SpawnBoom(ground, Vector2.Zero, 520f, 90f, 30, false);
            SpawnBoom(ground + new Vector2(-30f, 0f), new Vector2(-6f, 0f), 300f, 70f, 28, false);
            SpawnBoom(ground + new Vector2(30f, 0f), new Vector2(6f, 0f), 300f, 70f, 28, false);
        }

        private static bool HasSolidBelow(Player player)
        {
            Vector2 checkPos = player.Bottom + new Vector2(0f, 6f);
            int tileX = (int)(checkPos.X / 16f);
            int tileY = (int)(checkPos.Y / 16f);

            for (int x = -1; x <= 1; x++)
            {
                int cx = tileX + x;
                if (cx < 0 || cx >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[cx, tileY];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    return true;
            }
            return false;
        }

        private void Impact(Player player, MuspelheimPlayer mp, bool owner)
        {
            phase = 3;
            timer = 0;
            impactPos = player.Bottom + new Vector2(0f, -4f);

            if (owner)
            {
                mp.UltDive = false;
                player.velocity = Vector2.Zero;
                meteorsLeft = Main.rand.Next(MinMeteors, MaxMeteors + 1);
            }

            if (Main.dedServ)
                return;

            MuspelheimFx.Burst(impactPos, 22, 6f, 24f, 44f);
            SpawnImpactBoom(impactPos);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 pos = impactPos + new Vector2(side * Main.rand.NextFloat(0f, 30f), 0f);
                    Vector2 vel = new Vector2(side * Main.rand.NextFloat(3f, 10f), -Main.rand.NextFloat(0f, 1.6f));
                    MuspelheimFx.SpawnFire(pos, vel, Main.rand.NextFloat(22f, 38f), Main.rand.Next(22, 36));
                }
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.1f, Pitch = -0.2f }, impactPos);
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.8f }, impactPos);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(impactPos, Vector2.UnitY, 14f, 8f, 26, 1400f, "MuspelheimImpact"));
        }

        private void SpawnMeteor(Player player)
        {
            Vector2 target = player.Center + new Vector2(Main.rand.NextFloat(-MeteorSpread, MeteorSpread), Main.rand.NextFloat(-10f, 40f));
            Vector2 start = target + new Vector2(Main.rand.NextFloat(-320f, 320f), -Main.rand.NextFloat(780f, 980f));
            Vector2 vel = (target - start).SafeNormalize(Vector2.UnitY) * 24f;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), start, vel,
                ModContent.ProjectileType<MuspelheimMeteor>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack,
                Projectile.owner, target.Y);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (phase == 2)
            {
                float point = 0f;
                float length = SpearLength;
                Vector2 fwd = Projectile.rotation.ToRotationVector2();
                return Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    Projectile.Center - fwd * length * GripAlong,
                    Projectile.Center + fwd * length * (1f - GripAlong),
                    40f,
                    ref point);
            }

            if (phase == 3 && timer < 10)
            {
                Rectangle blast = new Rectangle((int)impactPos.X - 170, (int)impactPos.Y - 170, 340, 340);
                return blast.Intersects(targetHitbox);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            var mp = player.GetModPlayer<MuspelheimPlayer>();
            mp.UltActive = false;
            mp.UltFreeze = false;
            mp.UltDive = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float length = SpearLength;
            trail.Draw(phase == 2 ? 1f : 0.6f, true, length);

            if (phase >= 3)
                return false;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            bool flip = dir == -1;
            SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 origin = new Vector2(
                texture.Width * (flip ? 1f - GripAlong : GripAlong),
                texture.Height * (1f - GripAlong));

            float rot = Projectile.rotation + (flip ? MathHelper.PiOver4 * 3f : MathHelper.PiOver4);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rot, origin, Projectile.scale, effects, 0);

            Color glowColor = new Color(255, 140, 50, 0) * 0.55f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, rot, origin, Projectile.scale * 1.05f, effects, 0);

            if (phase == 1)
            {
                float totalTime = RiseTime + WindupTime;
                float prog = MathHelper.Clamp(timer / totalTime, 0f, 1f);
                float pulse = 0.5f + 0.5f * MathF.Sin(timer * 0.6f);
                Color windup = new Color(255, 120, 30, 0) * (0.25f + 0.5f * prog * pulse);

                Main.EntitySpriteDraw(texture, drawPos, null, windup, rot, origin,
                    Projectile.scale * (1.1f + 0.2f * prog), effects, 0);
            }

            return false;
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
        private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
       // I will never get used to math fields so idk why im always making separate smooths and messing up the code
    }
}