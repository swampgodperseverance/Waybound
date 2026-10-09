using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using static Waybound.Content.Items.Weapons.Melee.Other.Muspelheim;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class MuspelheimChainProj : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Other/ChainProj1";

        private const string RopePath = "Waybound/Content/Items/Weapons/Melee/Other/ChainProj2";
        private const string HandlePath = "Waybound/Content/Items/Weapons/Melee/Other/ChainProj3";

        private const float MaxOmega = 0.30f;
        private const int SpinUpTime = 80;
        private const int ReleaseTime = 40;
        private const float MinReach = 34f;
        private const float MaxReach = 150f;
        private const float ArmReach = 12f;
        private const int TrailLife = 14;

        private static readonly float BladeAxis = -MathHelper.PiOver4;
        private static readonly float HandleAxis = -MathHelper.PiOver4;
        private static readonly float RopeAxis = -MathHelper.PiOver2;

        private static readonly Vector2 BladeOrigin = new Vector2(0.12f, 0.88f);
        private static readonly Vector2 HandleOrigin = new Vector2(0.15f, 0.85f);

        private static Asset<Texture2D> ropeTex;
        private static Asset<Texture2D> handleTex;

        private SpearTrail trail;

        private float angle = MathHelper.PiOver2;
        private float omega;
        private float reach = MinReach;
        private int spinTime;
        private int releaseTimer;
        private bool released;
        private float omegaAtRelease;
        private float reachAtRelease;

        private float speedFrac;
        private float drawAlpha;

        private bool hasPrev;
        private float prevAngle;
        private Vector2 prevHand;

        private Vector2 handPos;
        private Vector2 ropeStart;
        private Vector2 bladeBase;
        private Vector2 bladeTip;
        private Vector2 sweepFrom;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            ropeTex = ModContent.Request<Texture2D>(RopePath, AssetRequestMode.ImmediateLoad);
            handleTex = ModContent.Request<Texture2D>(HandlePath, AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            ropeTex = null;
            handleTex = null;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 5;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.1f;

            trail = new SpearTrail(TrailLife);
        }

        private static float AxisLength(Texture2D tex, Vector2 originFrac)
        {
            float dx = (1f - originFrac.X) * tex.Width;
            float dy = originFrac.Y * tex.Height;
            return MathF.Sqrt(dx * dx + dy * dy) * 0.92f;
        }

        private float BladeLength
        {
            get
            {
                if (Main.dedServ)
                    return 60f * Projectile.scale;

                Main.instance.LoadProjectile(Type);
                return AxisLength(TextureAssets.Projectile[Type].Value, BladeOrigin) * Projectile.scale;
            }
        }

        private float HandleLength
        {
            get
            {
                if (Main.dedServ || handleTex == null)
                    return 30f * Projectile.scale;

                return AxisLength(handleTex.Value, HandleOrigin) * Projectile.scale;
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 5;
            float spinDir = Projectile.ai[1] >= 0f ? 1f : -1f;

            if (Projectile.owner == Main.myPlayer && Projectile.ai[0] == 0f &&
                (!player.channel || player.CCed || player.noItems))
            {
                Projectile.ai[0] = 1f;
                Projectile.netUpdate = true;
            }

            if (Projectile.ai[0] == 1f && !released)
            {
                released = true;
                releaseTimer = 0;
                omegaAtRelease = omega;
                reachAtRelease = reach;
            }

            spinTime++;

            if (!released)
            {
                float f = MathHelper.Clamp(spinTime / (float)SpinUpTime, 0f, 1f);
                omega = MaxOmega * Smooth(f);
                reach = MathHelper.Lerp(MinReach, MaxReach, EaseOutCubic(f));
                drawAlpha = MathHelper.Clamp(spinTime / 8f, 0f, 1f);
            }
            else
            {
                releaseTimer++;
                float f = MathHelper.Clamp(releaseTimer / (float)ReleaseTime, 0f, 1f);
                omega = omegaAtRelease * (1f - Smooth(f));
                reach = MathHelper.Lerp(reachAtRelease, reachAtRelease * 0.45f, Smooth(f));
                drawAlpha = 1f - Smooth(MathHelper.Clamp((f - 0.35f) / 0.65f, 0f, 1f));

                if (releaseTimer >= ReleaseTime)
                {
                    Projectile.Kill();
                    return;
                }
            }

            angle += spinDir * omega;
            speedFrac = MathHelper.Clamp(omega / MaxOmega, 0f, 1f);

            Vector2 oldTip = bladeTip;

            float handleLen = HandleLength;
            float bladeLen = BladeLength;
            Vector2 center = player.MountedCenter + new Vector2(0f, player.gfxOffY);
            Vector2 dirV = angle.ToRotationVector2();

            handPos = center + dirV * ArmReach;
            ropeStart = handPos + dirV * handleLen * 0.85f;
            bladeBase = ropeStart + dirV * reach;
            bladeTip = bladeBase + dirV * bladeLen;

            Projectile.Center = center;

            if (!hasPrev)
            {
                prevHand = handPos;
                prevAngle = angle;
                oldTip = bladeTip;
                hasPrev = true;
            }
            sweepFrom = oldTip;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.ChangeDir((int)spinDir);
            player.itemRotation = MathF.Atan2(dirV.Y * player.direction, dirV.X * player.direction);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, angle - spinDir * 0.55f - MathHelper.PiOver2);

            trail.Step();

            if (!Main.dedServ)
            {
                if (speedFrac > 0.12f && drawAlpha > 0.05f)
                {
                    float diff = MathHelper.WrapAngle(angle - prevAngle);

                    for (int s = 0; s < SpearTrail.Substeps; s++)
                    {
                        float t = (s + 1f) / SpearTrail.Substeps;
                        float a = prevAngle + diff * t;
                        Vector2 d = a.ToRotationVector2();
                        Vector2 h = Vector2.Lerp(prevHand, handPos, t);

                        Vector2 tip = h + d * (handleLen * 0.85f + reach + bladeLen);
                        trail.Add(tip, d, (SpearTrail.Substeps - 1 - s) / (float)SpearTrail.Substeps);
                    }
                }

                trail.Prune();

                SpawnEffects(dirV, spinDir, bladeLen);
                PlaySounds(player);
            }

            prevAngle = angle;
            prevHand = handPos;
        }

        private void SpawnEffects(Vector2 dirV, float spinDir, float bladeLen)
        {
            if (speedFrac < 0.1f || drawAlpha < 0.2f)
                return;

            Vector2 tangent = new Vector2(-dirV.Y, dirV.X) * spinDir;

            float rate = speedFrac * 2.2f;
            int n = (int)rate;
            if (Main.rand.NextFloat() < rate - n)
                n++;

            for (int i = 0; i < n; i++)
            {
                float u = Main.rand.NextFloat(0.15f, 1f);
                Vector2 pos = bladeBase + dirV * bladeLen * u + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 vel = -tangent * Main.rand.NextFloat(0.5f, 2.5f) + dirV * Main.rand.NextFloat(0f, 1f);
                MuspelheimFx.SpawnFire(pos, vel,
                    Main.rand.NextFloat(16f, 28f) * MathHelper.Lerp(0.7f, 1.2f, u), Main.rand.Next(16, 26));
            }

            if (Main.rand.NextFloat() < speedFrac * 0.5f)
            {
                Vector2 pos = bladeBase + dirV * bladeLen * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = -tangent * Main.rand.NextFloat(2f, 5f) + dirV * Main.rand.NextFloat(0f, 2f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }

            Lighting.AddLight(bladeTip, 1.1f * speedFrac, 0.5f * speedFrac, 0.15f * speedFrac);
        }

        private void PlaySounds(Player player)
        {
            if (spinTime == 1)
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f, Volume = 0.7f }, player.Center);

            if (!released && speedFrac > 0.12f && spinTime % 14 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item7 with
                {
                    Volume = 0.45f,
                    Pitch = -0.3f + 0.7f * speedFrac,
                    MaxInstances = 6
                }, player.Center);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (omega < 0.01f)
                return false;

            float point = 0f;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    bladeBase, bladeTip, 36f, ref point))
                return true;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    sweepFrom, bladeTip, 36f, ref point))
                return true;

            Vector2 ropeMid = Vector2.Lerp(ropeStart, bladeBase, 0.5f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                ropeMid, bladeBase, 24f, ref point);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X >= player.Center.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 100, default, 1.5f).noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                MuspelheimFx.SpawnFire(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(20f, 30f), Main.rand.Next(20, 32));
            }

            SpawnBladeFlash(bladeTip);
        }

        private static void SpawnBladeFlash(Vector2 pos)
        {
            if (Main.dedServ)
                return;

            if (ParticleSystem.FlashBuffer != null)
            {
                Color[] colors =
                {
                    new Color(255, 235, 170, 255),
                    new Color(255, 150, 50, 220),
                    new Color(255, 80, 20, 160)
                };

                for (int i = 0; i < 3; i++)
                {
                    float s = 26f + i * 10f;

                    ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                        position: pos.ToNumerics(),
                        velocity: SysVector2.Zero,
                        rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                        scale: new SysVector2(s + Main.rand.NextFloat(-2f, 2f)),
                        color: colors[i],
                        duration: 10 + i * 2
                    ));
                }
            }

            if (ParticleSystem.FlameBoomBuffer != null)
            {
                ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new SysVector2(80f, 80f),
                    color: new Color(255, 140, 40),
                    duration: 16
                ));

                ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new SysVector2(140f, 140f),
                    color: new Color(255, 140, 40),
                    duration: 22
                ));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || ropeTex == null || handleTex == null)
                return false;

            Main.instance.LoadProjectile(Type);
            Texture2D blade = TextureAssets.Projectile[Type].Value;
            Texture2D rope = ropeTex.Value;
            Texture2D handle = handleTex.Value;

            float spinDir = Projectile.ai[1] >= 0f ? 1f : -1f;
            bool flip = spinDir < 0f;
            Vector2 dirV = angle.ToRotationVector2();

            trail.Draw(MathHelper.Clamp(speedFrac * 1.3f, 0f, 1f) * drawAlpha, false, BladeLength);

            DrawRope(rope, spinDir, dirV);

            Color bladeLight = Color.Lerp(LightAt(bladeBase), Color.White, 0.35f) * drawAlpha;
            DrawOriented(blade, bladeBase, angle, BladeOrigin, BladeAxis, Projectile.scale, bladeLight, flip);

            Color glow = new Color(255, 140, 50, 0) * (0.55f * drawAlpha * (0.4f + 0.6f * speedFrac));
            DrawOriented(blade, bladeBase, angle, BladeOrigin, BladeAxis, Projectile.scale * 1.06f, glow, flip);

            Color handleLight = lightColor * drawAlpha;
            DrawOriented(handle, handPos, angle, HandleOrigin, HandleAxis, Projectile.scale, handleLight, flip);

            return false;
        }

        private void DrawRope(Texture2D rope, float spinDir, Vector2 dirV)
        {
            Vector2 p0 = ropeStart;
            Vector2 p2 = bladeBase;
            Vector2 chordMid = (p0 + p2) * 0.5f;

            Vector2 perp = new Vector2(-dirV.Y, dirV.X);
            Vector2 midOffset = -spinDir * perp * (22f * speedFrac)
                              + Vector2.UnitY * (26f * (1f - speedFrac) * (reach / MaxReach));
            Vector2 p1 = chordMid + midOffset * 2f;

            float spacing = Math.Max(4f, rope.Height * Projectile.scale * 0.85f);
            float approxLen = Vector2.Distance(p0, p2) + midOffset.Length() * 0.8f;
            int count = Math.Max(2, (int)MathF.Ceiling(approxLen / spacing));

            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                float u = 1f - t;

                Vector2 pos = u * u * p0 + 2f * u * t * p1 + t * t * p2;
                Vector2 tan = 2f * u * (p1 - p0) + 2f * t * (p2 - p1);
                float segAngle = tan.ToRotation();

                Color c = LightAt(pos) * drawAlpha;
                Vector2 origin = rope.Size() * 0.5f;
                float rot = segAngle - RopeAxis;

                Main.EntitySpriteDraw(rope, pos - Main.screenPosition, null, c, rot, origin, Projectile.scale, SpriteEffects.None, 0);

                if (speedFrac > 0.15f)
                {
                    Color g = new Color(255, 110, 30, 0) * (0.28f * speedFrac * drawAlpha);
                    Main.EntitySpriteDraw(rope, pos - Main.screenPosition, null, g, rot, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);
                }
            }
        }

        private static Color LightAt(Vector2 worldPos)
        {
            return Lighting.GetColor((int)(worldPos.X / 16f), (int)(worldPos.Y / 16f));
        }

        private static void DrawOriented(Texture2D tex, Vector2 worldPos, float theta, Vector2 originFrac, float axis,
            float scale, Color color, bool flip)
        {
            Vector2 origin = new Vector2(
                tex.Width * (flip ? 1f - originFrac.X : originFrac.X),
                tex.Height * originFrac.Y);

            float rot = flip ? theta - (MathHelper.Pi - axis) : theta - axis;
            SpriteEffects fx = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, worldPos - Main.screenPosition, null, color, rot, origin, scale, fx, 0);
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
        private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    }
    public class MuspelheimHook : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Other/ChainProj1";

        private const string RopePath = "Waybound/Content/Items/Weapons/Melee/Other/ChainProj2";

        public const float FlySpeed = 26f;
        private const float MaxRange = 640f;
        private const float RetractSpeed = 40f;
        private const float PullSpeed = 30f;
        private const int MaxPullTime = 50;

        private const int StateFly = 0;
        private const int StatePull = 1;
        private const int StateRetract = 2;

        private static Asset<Texture2D> ropeTex;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            ropeTex = ModContent.Request<Texture2D>(RopePath, AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            ropeTex = null;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 600;
            Projectile.scale = 1.1f;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.ai[0] == StateFly ? null : false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X >= player.Center.X ? 1 : -1;
            modifiers.Knockback *= 0f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] != StateFly)
                return;

            Projectile.ai[0] = StatePull;
            Projectile.ai[1] = target.whoAmI;
            Projectile.localAI[1] = 0f;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;

            MuspelheimFx.Burst(target.Center, 10, 3.5f, 18f, 30f);
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.2f, Volume = 0.8f }, target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == StateFly)
            {
                SoundEngine.PlaySound(SoundID.Item10 with { Pitch = -0.3f }, Projectile.Center);
                StartRetract();
            }
            return false;
        }

        private void StartRetract()
        {
            Projectile.ai[0] = StateRetract;
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            MuspelheimPlayer mp = player.GetModPlayer<MuspelheimPlayer>();
            Vector2 start = player.MountedCenter + new Vector2(0f, player.gfxOffY);
            Vector2 toProj = Projectile.Center - start;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.ChangeDir(toProj.X >= 0f ? 1 : -1);
            player.itemRotation = MathF.Atan2(toProj.Y * player.direction, toProj.X * player.direction);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, toProj.ToRotation() - MathHelper.PiOver2);

            switch ((int)Projectile.ai[0])
            {
                case StateFly:
                    UpdateFly();
                    break;
                case StatePull:
                    UpdatePull(player, mp);
                    break;
                default:
                    UpdateRetract(player, start);
                    break;
            }

            Projectile.rotation = (Projectile.Center - start).ToRotation();

            if (Main.dedServ)
                return;

            if (Projectile.ai[0] == StateFly && Main.rand.NextBool(2))
            {
                MuspelheimFx.SpawnFire(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(14f, 22f), Main.rand.Next(14, 22));
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.4f, 0.12f);
        }

        private void UpdateFly()
        {
            if (Projectile.localAI[0] == 0f)
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.Center);

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] * FlySpeed >= MaxRange)
                StartRetract();
        }

        private void UpdatePull(Player player, MuspelheimPlayer mp)
        {
            int index = (int)Projectile.ai[1];
            NPC target = index >= 0 && index < Main.maxNPCs ? Main.npc[index] : null;

            if (target == null || !target.active || target.life <= 0)
            {
                mp.HookPull = false;
                if (Projectile.owner == Main.myPlayer)
                    StartRetract();
                return;
            }

            Projectile.localAI[1]++;
            Projectile.Center = target.Center;
            Projectile.velocity = Vector2.Zero;

            Rectangle reach = target.Hitbox;
            reach.Inflate(28, 28);
            bool arrived = player.Hitbox.Intersects(reach);

            if (Projectile.owner != Main.myPlayer)
                return;

            if (arrived || Projectile.localAI[1] > MaxPullTime)
            {
                mp.HookPull = false;
                MuspelheimFx.Burst(target.Center, 8, 3f, 14f, 24f);
                StartRetract();
            }
            else
            {
                mp.HookPull = true;
                mp.HookPullVelocity = (target.Center - player.Center).SafeNormalize(Vector2.UnitX) * PullSpeed;
            }
        }

        private void UpdateRetract(Player player, Vector2 start)
        {
            Vector2 to = start - Projectile.Center;
            float dist = to.Length();

            if (dist < 40f)
            {
                if (Projectile.owner == Main.myPlayer)
                    Projectile.Kill();
                return;
            }

            Projectile.velocity = to / dist * RetractSpeed;
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            MuspelheimPlayer mp = player.GetModPlayer<MuspelheimPlayer>();
            mp.HookPull = false;

            if (Projectile.owner == Main.myPlayer)
                mp.HookCooldown = 45;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || ropeTex == null)
                return false;

            Player player = Main.player[Projectile.owner];
            Main.instance.LoadProjectile(Type);
            Texture2D head = TextureAssets.Projectile[Type].Value;
            Texture2D rope = ropeTex.Value;

            Vector2 start = player.MountedCenter + new Vector2(0f, player.gfxOffY);
            Vector2 diff = Projectile.Center - start;
            float len = diff.Length();

            if (len > 1f)
            {
                Vector2 dir = diff / len;
                float rot = dir.ToRotation() + MathHelper.PiOver2;
                float spacing = Math.Max(4f, rope.Height * Projectile.scale * 0.85f);
                Vector2 origin = rope.Size() * 0.5f;

                for (float d = spacing * 0.5f; d < len; d += spacing)
                {
                    Vector2 pos = start + dir * d;
                    Color c = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f));
                    Main.EntitySpriteDraw(rope, pos - Main.screenPosition, null, c, rot, origin, Projectile.scale, SpriteEffects.None, 0);

                    Color g = new Color(255, 110, 30, 0) * 0.3f;
                    Main.EntitySpriteDraw(rope, pos - Main.screenPosition, null, g, rot, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);
                }
            }

            Vector2 headPos = Projectile.Center - Main.screenPosition;
            float headRot = Projectile.rotation + MathHelper.PiOver4;
            Vector2 headOrigin = head.Size() * 0.5f;
            Color headLight = Color.Lerp(lightColor, Color.White, 0.35f);

            Main.EntitySpriteDraw(head, headPos, null, headLight, headRot, headOrigin, Projectile.scale, SpriteEffects.None, 0);

            Color glow = new Color(255, 140, 50, 0) * 0.55f;
            Main.EntitySpriteDraw(head, headPos, null, glow, headRot, headOrigin, Projectile.scale * 1.06f, SpriteEffects.None, 0);

            return false;
        }
    }
}