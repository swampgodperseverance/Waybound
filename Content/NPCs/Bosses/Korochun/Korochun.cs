using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.NetCodeUtil;
using Waybound.Common.NetCodeUtil.Packets;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Mounts;
using Waybound.Content.Items.Placeable.Bosses;
using Waybound.Content.Items.Vanity.BossMasks;
using Waybound.Particles;

namespace Waybound.Content.NPCs.Bosses.Korochun
{
    internal static class KorochunFx
    {

        //As i said in the muspelheim fx, i found that sometimes it's easier to make fx as a separate class, since code sometimes' messing up
        public static readonly Color IceDeep = new Color(40, 120, 255);
        public static readonly Color Ice = new Color(130, 210, 255);
        public static readonly Color Frost = new Color(220, 242, 255);
        public static readonly Color Violet = new Color(170, 130, 255);

        private static Texture2D glow;
        private static Asset<Texture2D> laserBeam;
        private static Asset<Texture2D> body2;
        private static Asset<Texture2D> body2Glow;

        public static Texture2D Glow
        {
            get
            {
                if (glow == null || glow.IsDisposed)
                {
                    const int size = 128;
                    glow = new Texture2D(Main.instance.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - 63.5f) / 63.5f;
                            float dy = (y - 63.5f) / 63.5f;
                            float d = MathF.Sqrt(dx * dx + dy * dy);
                            float a = MathF.Pow(MathHelper.Clamp(1f - d, 0f, 1f), 2.2f);
                            data[y * size + x] = new Color(a, a, a, a);
                        }
                    glow.SetData(data);
                }
                return glow;
            }
        }

        public static Texture2D LaserBeam
            => (laserBeam ??= ModContent.Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad)).Value;

        public static Texture2D Body2
            => (body2 ??= ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/Korochun/Korochun2", AssetRequestMode.ImmediateLoad)).Value;

        public static Texture2D Body2Glow
            => (body2Glow ??= ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/Korochun/Korochun2_Glow", AssetRequestMode.ImmediateLoad)).Value;

        public static void UnloadAssets()
        {
            laserBeam = null;
            body2 = null;
            body2Glow = null;
        }

        public static void DisposeGlow()
        {
            Texture2D tex = glow;
            glow = null;
            if (tex != null) Main.QueueMainThreadAction(() => tex.Dispose());
        }

        public static void DrawGlow(Vector2 screenPos, Color color, float diameter)
            => Main.spriteBatch.Draw(Glow, screenPos, null, color, 0f, new Vector2(64f), diameter / 128f, SpriteEffects.None, 0f);

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

        public static Color Tint(Color c, int alpha = 235) => new Color(c.R, c.G, c.B, alpha);

        public static Color RandIce() => Tint(Color.Lerp(Ice, Frost, Main.rand.NextFloat()));

        private static Color Pick(Color? tint) => tint.HasValue ? Tint(tint.Value) : RandIce();

        public static void Snow(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.SnowFlakeBuffer == null) return;

            ParticleSystem.SnowFlakeBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size),
                color,
                life));
        }

        public static void Ring(Vector2 center, int count, float speed, float scale, Color? tint = null)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count);
                Snow(center + dir * 6f, dir * speed * Main.rand.NextFloat(0.85f, 1.15f),
                    scale * 14f * Main.rand.NextFloat(0.8f, 1.2f), Pick(tint), Main.rand.Next(22, 34));
            }
        }

        public static void Converge(Vector2 target, float minR, float maxR, int count, float scale, int ticks, Color? tint = null)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(minR, maxR);
                Snow(target + off, -off / (ticks * 0.9f), scale * 12f * Main.rand.NextFloat(0.8f, 1.2f), Pick(tint), ticks + 6);
            }
        }

        public static void Spray(Vector2 pos, Vector2 dir, int count, float speedMin, float speedMax, float spread, float scale, Color? tint = null)
        {
            dir = dir.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < count; i++)
            {
                Vector2 v = dir.RotatedByRandom(spread) * Main.rand.NextFloat(speedMin, speedMax);
                Snow(pos, v, scale * 13f * Main.rand.NextFloat(0.8f, 1.3f), Pick(tint), Main.rand.Next(20, 32));
            }
        }

        public static void Steam(Vector2 pos, Vector2 vel, float scale = 1.5f)
        {
            Snow(pos, vel * 0.8f, scale * 18f * Main.rand.NextFloat(0.8f, 1.2f),
                new Color(225, 240, 255, 170), Main.rand.Next(30, 46));
        }

        private static Color? TintFor(int dustType)
        {
            if (dustType == DustID.PurpleTorch) return Color.Lerp(Violet, Ice, Main.rand.NextFloat(0.4f));
            if (dustType == DustID.WhiteTorch) return Color.Lerp(Color.White, Frost, Main.rand.NextFloat());
            return null;
        }

        public static void Ring(Vector2 center, int count, float speed, int dustType, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count);
                Snow(center + dir * 6f, dir * speed * Main.rand.NextFloat(0.85f, 1.15f),
                    scale * 14f * Main.rand.NextFloat(0.8f, 1.2f), Pick(TintFor(dustType)), Main.rand.Next(22, 34));
            }
        }

        public static void Converge(Vector2 target, float minR, float maxR, int count, int dustType, float scale, int ticks)
            => Converge(target, minR, maxR, count, scale, ticks, TintFor(dustType));

        public static void Shake(int duration, float power)
        {
            if (Main.netMode == NetmodeID.Server || Main.LocalPlayer == null) return;
            Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>().TriggerShake(duration, power);
        }
    }

    public class KorochunFxSystem : ModSystem
    {
        public override void Unload()
        {
            KorochunFx.DisposeGlow();
            KorochunFx.UnloadAssets();
        }
    }

    public class Korochun : ModNPC
    {
        private const float SideOffset = 520f;
        private const int LCT = 420;   //las charge time
        private const int LLT = 312; //las lock time
        private const int LFT = 70; //las fire time
        private const float BeamLength = 2600f;
        private static readonly Vector2 MouthOffset = new Vector2(48f, 8f);
        private const bool SpriteFacesRight = false;

        private int DashCount => phase2 ? 3 : 2;
        private float DashSpeed => phase2 ? 20f : 16f;
        private int ShotsPerAttack => phase2 ? 6 : 4;

        private const float Phase2LifeRatio = 0.5f;
        private const int TransformSwap = 100;
        private const int TransformTotal = 160;
        private const int SpiritRingCount = 4;
        private const int WaveSlots = 10;
        /// <summary>
        /// This section contains misc attacks so idk
        /// </summary>
        private const int NVB = 70; // First birst
        private const int NVG = 60; //Gap between
        private const int NCT = 50;//meh charge time
        private int NBursts => phase2 ? 3 : 2;
        private int NShots => phase2 ? 16 : 12;
        /// <summary>
        /// Nova section, attack when the boss is bursting a lot of projectiles
        /// </summary>
        private enum State
        {
            Intro = 0, Idle = 1, Dash = 2, Shoot = 3, Laser = 4,
            SpiritRing = 5, SpiritWave = 6, Transform = 7, FrostNova = 8
        }

        private State CurrentState { get => (State)(int)NPC.ai[0]; set => NPC.ai[0] = (int)value; }
        private ref float Timer => ref NPC.ai[1];
        private ref float Sub => ref NPC.ai[2];
        private ref float Side => ref NPC.ai[3];

        private float LaserAngle { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }

        private static bool Authority => Main.netMode != NetmodeID.MultiplayerClient;

        private bool phase2;
        public bool Phase2 => phase2;

        private bool DrawsPhase2 => phase2 || (CurrentState == State.Transform && Timer >= TransformSwap);

        public bool MouthOpen { get; private set; }
        private Vector2 drawShake;
        private int animFrame;

        private float smoothSide;
        private bool sideInit;

        private Vector2 tc;
        private Vector2 syncTarget;
        private Vector2 syncTargetVel;
        private bool syncTargetInit;
        private Vector2 visualOffset;
        private Vector2 expectedNext;
        private bool hasExpected;

        private float auraCharge;
        private float auraFlash;

        public Vector2 Mouth => NPC.Center + new Vector2(NPC.direction * MouthOffset.X, MouthOffset.Y);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 96;
            NPC.damage = 42;
            NPC.defense = 14;
            NPC.lifeMax = 1500;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.scale = 1.4f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lavaImmune = true;
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.alpha = 255;

            NPC.buffImmune[BuffID.Frozen] = true;
            NPC.buffImmune[BuffID.Chilled] = true;
            NPC.buffImmune[BuffID.Confused] = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(phase2);
            writer.Write(smoothSide);
            for (int i = 0; i < NPC.localAI.Length; i++) writer.Write(NPC.localAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            phase2 = reader.ReadBoolean();
            smoothSide = reader.ReadSingle();
            sideInit = true;
            for (int i = 0; i < NPC.localAI.Length; i++) NPC.localAI[i] = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            phase2 = false;
            if (!Authority) return;
            Side = Main.rand.NextBool() ? 1f : -1f;
            NPC.localAI[0] = -1f;
            CurrentState = State.Intro;
            Timer = 0; Sub = 0;
            NPC.netUpdate = true;

        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.alpha < 150;

        public void ApplySync(Vector2 center, Vector2 velocity, Vector2 target, Vector2 targetVelocity, float side)
        {
            NPC.Center = center;
            NPC.velocity = velocity;
            syncTarget = target;
            syncTargetVel = targetVelocity;
            syncTargetInit = true;
            smoothSide = side;
            sideInit = true;
        }

        public override bool PreAI()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && hasExpected)
            {
                Vector2 jump = NPC.Center - expectedNext;
                float sq = jump.LengthSquared();
                if (sq > 9f)
                {
                    if (sq < 640000f) visualOffset -= jump;
                    else visualOffset = Vector2.Zero;
                }
            }
            return true;
        }

        public override void PostAI()
        {
            expectedNext = NPC.Center + NPC.velocity;
            hasExpected = true;
            visualOffset *= 0.9f;
            if (visualOffset.LengthSquared() < 0.25f) visualOffset = Vector2.Zero;
        }

        public override void AI()
        {
            MouthOpen = false;
            drawShake = Vector2.Zero;
            NPC.dontTakeDamage = false;
            auraCharge = 0f;
            auraFlash = Math.Max(0f, auraFlash - 0.05f);

            if (!NPC.HasValidTarget) NPC.TargetClosest(false);
            Player t = Main.player[NPC.target];
            if (!t.active || t.dead) { LeaveAI(); return; }

            NPC.timeLeft = 600;

            if (!Authority && syncTargetInit) syncTarget += syncTargetVel;
            tc = Authority || !syncTargetInit ? t.Center : syncTarget;

            if (CurrentState != State.Intro && CurrentState != State.Dash)
                NPC.alpha = Math.Max(0, NPC.alpha - 25);

            switch (CurrentState)
            {
                case State.Intro:
                    IntroAI(t);
                    break;
                case State.Idle:
                    IdleAI(t);
                    break;
                case State.Dash:
                    DashAI(t);
                    break;
                case State.Shoot:
                    ShootAI(t);
                    break;
                case State.Laser:
                    LaserAI(t);
                    break;
                case State.SpiritRing:
                    SpiritRingAI(t);
                    break;
                case State.SpiritWave:
                    SpiritWaveAI(t);
                    break;
                case State.Transform:
                    TransformAI(t);
                    break;
                case State.FrostNova:
                    FrostNovaAI(t);
                    break;

                    //so sigma
            }

            if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 3 == 0)
                MultiplayerSystem.SendPacket(new KorochunSyncPacket(NPC.whoAmI, NPC.Center, NPC.velocity, tc, t.velocity, smoothSide));

            if (phase2)
            {
                Lighting.AddLight(NPC.Center, 0.45f, 0.35f, 0.85f);

                if (Main.netMode != NetmodeID.Server && NPC.alpha < 100 && Main.rand.NextBool(6))
                {
                    Color c = Main.rand.NextBool() ? KorochunFx.Tint(KorochunFx.Violet) : KorochunFx.RandIce();
                    KorochunFx.Snow(NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f),
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.4f, 1.3f)),
                        Main.rand.NextFloat(12f, 20f), c, Main.rand.Next(30, 46));
                }
            }
            else
            {
                Lighting.AddLight(NPC.Center, 0.25f, 0.5f, 0.8f);
            }
        }

        private void LeaveAI()
        {
            NPC.velocity.Y -= 0.35f;
            NPC.alpha = Math.Min(255, NPC.alpha + 4);
            NPC.dontTakeDamage = true;
            NPC.EncourageDespawn(40);
        }

        private void Face(Player t)
        {
            float dx = tc.X - NPC.Center.X;
            if (NPC.direction == 0)
            {
                NPC.direction = NPC.spriteDirection = dx >= 0f ? 1 : -1;
                return;
            }
            if (Math.Abs(dx) < 48f) return;
            NPC.direction = NPC.spriteDirection = dx > 0f ? 1 : -1;
        }

        private void SmoothMove(Vector2 dest, float maxSpeed, float accel)
        {
            Vector2 desired = (dest - NPC.Center) * 0.06f;
            float len = desired.Length();
            if (len > maxSpeed) desired *= maxSpeed / len;

            Vector2 dv = desired - NPC.velocity;
            float dl = dv.Length();
            if (dl > accel) dv *= accel / dl;
            NPC.velocity += dv;
        }

        private void HoverAtSide(Player t, float offsetX, float offsetY, float maxSpeed, float accel = 0.55f)
        {
            if (!sideInit)
            {
                smoothSide = Side;
                sideInit = true;
            }
            smoothSide = MathHelper.Lerp(smoothSide, Side, 0.045f);

            float crossing = 1f - Math.Abs(smoothSide);
            float arcLift = -MathF.Sin(crossing * MathHelper.PiOver2) * 190f;

            Vector2 dest = tc + new Vector2(smoothSide * offsetX, offsetY + arcLift);
            SmoothMove(dest, maxSpeed, accel);

            float tilt = MathHelper.Clamp(NPC.velocity.Y * 0.015f * NPC.direction, -0.35f, 0.35f);
            NPC.rotation = MathHelper.Lerp(NPC.rotation, tilt, 0.08f);
        }

        private void SetState(State s)
        {
            CurrentState = s;
            Timer = 0; Sub = 0;
            NPC.netUpdate = true;
        }

        private void IntroAI(Player t)
        {
            Timer++;
            NPC.alpha = (int)MathHelper.Lerp(255f, 0f, Math.Min(Timer / 70f, 1f));
            NPC.dontTakeDamage = true;
            Face(t);
            HoverAtSide(t, SideOffset, 0f, 8f);

            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = -0.4f }, NPC.Center);
                KorochunFx.Flash(NPC.Center, 260f, new Color(190, 225, 255, 200), 20);
                KorochunFx.Ring(NPC.Center, 24, 6f, 1.5f);
            }
            if (Timer % 4 == 0)
                KorochunFx.Steam(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), new Vector2(0f, -Main.rand.NextFloat(1f, 2.5f)));
            //are we deadass (Dried rose sticker)
            if (Timer >= 70) SetState(State.Idle);
        }

        private void IdleAI(Player t)
        {
            Timer++;
            Face(t);
            float wobble = MathF.Sin(Main.GameUpdateCount * 0.04f) * 34f;
            HoverAtSide(t, SideOffset, wobble, 8f);

            if (!phase2 && NPC.life < NPC.lifeMax * Phase2LifeRatio)
            {
                SetState(State.Transform);
                return;
            }

            if (Timer >= (phase2 ? 70 : 90) && Authority) ChooseAttack();
        }

        private void ChooseAttack()
        {
            int[] pool = phase2 ? new[] { 0, 1, 2, 3, 4, 5 } : new[] { 0, 1, 5 };
            int next;
            do { next = pool[Main.rand.Next(pool.Length)]; } while (next == (int)NPC.localAI[0]);
            NPC.localAI[0] = next;
            if (Main.rand.NextBool()) Side = -Side;
            SetState(next switch
            {
                0 => State.Dash,
                1 => State.Shoot,
                2 => State.Laser,
                3 => State.SpiritRing,
                4 => State.SpiritWave,
                _ => State.FrostNova
            });
        }

        private void TransformAI(Player t)
        {
            Timer++;
            NPC.dontTakeDamage = true;
            NPC.alpha = 0;
            Face(t);
            HoverAtSide(t, SideOffset - 60f, -80f, 3f, 0.2f);

            if (Timer < TransformSwap)
            {
                float p = Timer / (float)TransformSwap;
                MouthOpen = Timer > 20;
                drawShake = Main.rand.NextVector2Circular(1f, 1f) * (1f + p * 9f);

                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.9f, Pitch = -0.4f }, NPC.Center);
                    KorochunFx.Flash(NPC.Center, 240f, new Color(200, 180, 255, 200), 18);
                }

                int n = 1 + (int)(p * 4f);
                for (int i = 0; i < n; i++)
                {
                    Color? tint = Main.rand.NextBool(2) ? KorochunFx.Violet : null;
                    KorochunFx.Converge(NPC.Center, 90f, 260f, 1, 1.2f + p * 0.5f, 14, tint);
                }
                if (Timer % 5 == 0)
                    KorochunFx.Steam(NPC.Center + Main.rand.NextVector2Circular(50f, 40f), new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f)), 1.4f);
                if (Timer % 12 == 0)
                    KorochunFx.Shake(6, 0.5f + p * 1.5f);

                Lighting.AddLight(NPC.Center, 0.3f + p, 0.3f + p * 0.6f, 0.6f + p);
            }

            if (Timer == TransformSwap)
            {
                phase2 = true;
                NPC.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1f, Pitch = -0.5f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = -0.4f }, NPC.Center);
                KorochunFx.Flash(NPC.Center, 520f, Color.White, 26);
                KorochunFx.Flash(NPC.Center, 340f, new Color(190, 160, 255, 240), 30);
                KorochunFx.Ring(NPC.Center, 44, 12f, 2f);
                KorochunFx.Ring(NPC.Center, 30, 7f, 1.7f, KorochunFx.Violet);
                KorochunFx.Ring(NPC.Center, 20, 3.5f, 1.5f, Color.White);
                for (int i = 0; i < 24; i++)
                    KorochunFx.Steam(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), Main.rand.NextVector2Circular(5f, 5f), 1.8f);
                KorochunFx.Shake(40, 4f);
                auraFlash = 1f;
            }

            if (Timer > TransformSwap)
            {
                MouthOpen = Timer < TransformSwap + 30;
                float fade = 1f - (Timer - TransformSwap) / (float)(TransformTotal - TransformSwap);
                drawShake = Main.rand.NextVector2Circular(1f, 1f) * (4f * fade);
                if (Timer % 4 == 0)
                    KorochunFx.Spray(NPC.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 3, 2f, 6f, 0.3f, 1.2f, KorochunFx.Violet);
            }

            if (Timer >= TransformTotal)
            {
                SetState(State.Idle);
                Timer = 30;
            }
        }

        private void DashAI(Player t)
        {
            int phase = (int)Sub % 10;
            int idx = (int)Sub / 10;

            switch (phase)
            {
                case 0:
                    {
                        Timer++;
                        NPC.alpha = 0;
                        Face(t);

                        int wind = (idx == 0 ? 72 : 52) - (phase2 ? 14 : 0);
                        float p = Math.Min(Timer / wind, 1f);

                        HoverAtSide(t, SideOffset + p * 60f, 0f, 7f);
                        drawShake = Main.rand.NextVector2Circular(1f, 1f) * (1f + p * 5f);

                        if (Timer == 1)
                            SoundEngine.PlaySound(SoundID.Item34 with { Volume = 0.8f, Pitch = -0.2f }, NPC.Center);

                        int puffs = 1 + (int)(p * 2f);
                        for (int i = 0; i < puffs; i++)
                        {
                            Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.42f, NPC.height * 0.42f);
                            Vector2 vel = new Vector2(-NPC.direction * Main.rand.NextFloat(0.6f, 2.2f), Main.rand.NextFloat(-3f, -0.8f));
                            KorochunFx.Steam(pos, vel, 1.1f + p * 0.6f);
                        }
                        if (Timer % 12 == 0)
                            KorochunFx.Snow(NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.4f, NPC.height * 0.4f),
                                Main.rand.NextVector2Circular(1.5f, 1.5f), 16f, KorochunFx.RandIce(), 30);

                        if (Timer >= wind)
                        {
                            Vector2 dir = (tc - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction);
                            NPC.velocity = dir * 5f;
                            Sub = idx * 10 + 1;
                            Timer = 0;
                            NPC.netUpdate = true;
                        }
                        break;
                    }

                case 1:
                    {
                        Timer++;
                        NPC.alpha = 0;
                        MouthOpen = Timer < 12;

                        Vector2 dirV = NPC.velocity.SafeNormalize(Vector2.UnitX * NPC.direction);
                        float spd = Math.Min(DashSpeed, NPC.velocity.Length() + 2.6f);
                        NPC.velocity = dirV * spd;

                        NPC.direction = NPC.spriteDirection = NPC.velocity.X >= 0f ? 1 : -1;
                        NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.Y / DashSpeed * 0.4f * NPC.direction, 0.2f);

                        if (Timer == 1)
                        {
                            SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.6f, Pitch = 0.3f }, NPC.Center);
                            KorochunFx.Flash(NPC.Center, 180f, new Color(200, 230, 255, 220), 12);
                            KorochunFx.Ring(NPC.Center, 20, 7f, 1.4f);
                            KorochunFx.Shake(8, 1.2f);
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            KorochunFx.Snow(NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.35f, NPC.height * 0.35f),
                                -NPC.velocity * 0.06f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                                Main.rand.NextFloat(14f, 22f),
                                Main.rand.NextBool() ? KorochunFx.RandIce() : KorochunFx.Tint(phase2 ? KorochunFx.Violet : Color.White), 24);
                        }
                        if (Timer % 3 == 0)
                            KorochunFx.Steam(NPC.Center, -NPC.velocity * 0.04f + Main.rand.NextVector2Circular(0.8f, 0.8f), 1.4f);

                        Lighting.AddLight(NPC.Center, 0.5f, 0.9f, 1.3f);

                        float far = Math.Max(1200f, Main.screenWidth * 0.5f + 300f);
                        float d = Vector2.Distance(NPC.Center, tc);

                        float fadeStart = far - 260f;
                        if (d > fadeStart)
                            NPC.alpha = (int)MathHelper.Clamp((d - fadeStart) / 260f * 255f, 0f, 255f);

                        if (d > far || Timer > 110)
                        {
                            NPC.velocity = Vector2.Zero;
                            if (Authority) Side = Main.rand.NextBool() ? 1f : -1f;
                            Sub = idx * 10 + 2;
                            Timer = 0;
                            NPC.netUpdate = true;
                        }
                        break;
                    }

                case 2:
                    {
                        const int gone = 46;
                        Timer++;
                        NPC.alpha = 255;
                        NPC.dontTakeDamage = true;
                        NPC.velocity = Vector2.Zero;

                        Vector2 dest = tc + new Vector2(Side * SideOffset, 0f);
                        NPC.Center = dest;
                        NPC.direction = NPC.spriteDirection = dest.X < tc.X ? 1 : -1;

                        smoothSide = Side;
                        sideInit = true;

                        if (Timer > gone - 30 && Timer < gone)
                        {
                            float p = (Timer - (gone - 30)) / 30f;
                            float radius = 30f + 160f * (1f - p);
                            for (int i = 0; i < 3; i++)
                            {
                                float ang = Main.GameUpdateCount * 0.2f + MathHelper.TwoPi * i / 3f + p * 6f;
                                Vector2 pos = dest + Vector2.UnitX.RotatedBy(ang) * radius;
                                Color c = Main.rand.NextBool() ? KorochunFx.RandIce() : KorochunFx.Tint(KorochunFx.Violet);
                                KorochunFx.Snow(pos, (dest - pos) * 0.05f, Main.rand.NextFloat(13f, 20f), c, 22);
                            }
                            Lighting.AddLight(dest, 0.4f * p, 0.8f * p, 1.2f * p);
                        }

                        if (Timer == gone)
                        {
                            KorochunFx.Flash(dest, 300f, new Color(210, 235, 255, 240), 20);
                            KorochunFx.Flash(dest, 180f, Color.White, 14);
                            KorochunFx.Ring(dest, 32, 9f, 1.7f);
                            KorochunFx.Ring(dest, 22, 5f, 1.5f, KorochunFx.Violet);
                            for (int i = 0; i < 16; i++)
                                KorochunFx.Steam(dest + Main.rand.NextVector2Circular(28f, 28f), Main.rand.NextVector2Circular(3.5f, 3.5f), 1.6f);
                            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.2f }, dest);
                            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f }, dest);
                            KorochunFx.Shake(12, 1.6f);
                        }

                        if (Timer >= gone)
                        {
                            Array.Fill(NPC.oldPos, NPC.position);
                            Sub = idx * 10 + 3;
                            Timer = 0;
                            NPC.netUpdate = true;
                        }
                        break;
                    }

                default:
                    {
                        Timer++;
                        NPC.alpha = Math.Max(0, 255 - (int)Timer * 24);
                        NPC.dontTakeDamage = Timer < 10;
                        NPC.velocity = Vector2.Zero;
                        Face(t);

                        if (Timer < 10)
                            KorochunFx.Steam(NPC.Center + Main.rand.NextVector2Circular(36f, 36f), Main.rand.NextVector2Circular(2.5f, 2.5f), 1.4f);

                        if (Timer >= 18)
                        {
                            if (idx + 1 < DashCount)
                            {
                                Sub = (idx + 1) * 10;
                                Timer = 0;
                                NPC.netUpdate = true;
                            }
                            else
                            {
                                Side = -Side;
                                SetState(State.Idle);
                            }
                        }
                        break;
                    }
            }
        }

        private void ShootAI(Player t)
        {
            Timer++;
            Face(t);

            int idx = (int)Sub;
            float yOff = (idx % 2 == 0 ? -1f : 1f) * 130f;
            HoverAtSide(t, SideOffset - 40f, yOff, 12f, 0.7f);

            MouthOpen = Timer >= ShotPrep - 14 && Timer <= ShotPrep + 16;

            if (Timer == ShotPrep - 12)
                SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f, Pitch = 0.3f }, NPC.Center);

            if (Timer > ShotPrep - 16 && Timer < ShotPrep)
                KorochunFx.Converge(Mouth, 30f, 70f, 1, 1.1f, 12);

            if (Timer == ShotPrep)
            {
                Vector2 m = Mouth;
                Vector2 aim = (tc - m).SafeNormalize(Vector2.UnitX * NPC.direction);

                if (Authority)
                {
                    int count = (idx == ShotsPerAttack - 1 ? 5 : 3) + (phase2 ? 2 : 0);
                    int dmg = NPC.GetAttackDamage_ForProjectiles(24f, 20f);

                    float speed = phase2 ? 7.5f : 6.5f;

                    for (int i = 0; i < count; i++)
                    {
                        float off = (i - (count - 1) / 2f) * 0.2f;
                        Vector2 v = aim.RotatedBy(off) * speed;

                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(), m, v,
                            ModContent.ProjectileType<KorochunProj>(),
                            dmg, 0f, Main.myPlayer,
                            ai0: i * 0.7f);
                    }

                }

                NPC.velocity -= aim * 4.5f;
                NPC.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.8f, Pitch = 0.1f }, m);
                KorochunFx.Flash(m, 100f, new Color(200, 230, 255, 220), 10);
                KorochunFx.Spray(m, aim, 14, 2.5f, 8f, 0.4f, 1.1f);
                KorochunFx.Shake(5, 0.7f);
            }

            if (Timer >= ShotPrep + 28)
            {
                Sub++;
                Timer = 0;
                NPC.netUpdate = true;
                if ((int)Sub >= ShotsPerAttack) SetState(State.Idle);
            }
        }

        private const int ShotPrep = 44;

        private int NovaBurstTime(int k) => NVB + NVG * k;

        private void FrostNovaAI(Player t)
        {
            Timer++;
            Face(t);
            HoverAtSide(t, SideOffset - 80f, -30f, 6f, 0.45f);

            int bursts = NBursts;
            int lastBurst = NovaBurstTime(bursts - 1);
            Color? tint = phase2 ? KorochunFx.Violet : null;

            for (int k = 0; k < bursts; k++)
            {
                int bt = NovaBurstTime(k);
                if (Timer > bt) continue;

                float p = MathHelper.Clamp((Timer - (bt - NCT)) / NCT, 0f, 1f);
                auraCharge = p;

                if (p > 0f)
                {
                    MouthOpen = true;
                    drawShake = Main.rand.NextVector2Circular(1f, 1f) * (0.5f + p * 3f);

                    int n = 1 + (int)(p * 3f);
                    for (int i = 0; i < n; i++)
                        KorochunFx.Converge(NPC.Center, 110f, 230f, 1, 1.1f + p * 0.5f, 14, Main.rand.NextBool(3) ? tint : null);

                    if (Timer == bt - NCT + 1)
                        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.7f, Pitch = -0.2f + k * 0.1f }, NPC.Center);

                    Lighting.AddLight(NPC.Center, 0.3f + p * 0.6f, 0.6f + p * 0.6f, 1f + p * 0.5f);
                }
                break;
            }

            for (int k = 0; k < bursts; k++)
            {
                if (Timer != NovaBurstTime(k)) continue;

                Vector2 c = NPC.Center;
                auraFlash = 1f;

                if (Authority)
                {
                    int shots = NShots;
                    int dmg = NPC.GetAttackDamage_ForProjectiles(22f, 18f);
                    float speed = phase2 ? 7.5f : 6.5f;
                    float baseAng = (k % 2) * MathHelper.Pi / shots;
                    for (int i = 0; i < shots; i++)
                    {
                        Vector2 v = Vector2.UnitX.RotatedBy(baseAng + MathHelper.TwoPi * i / shots) * speed;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), c, v, ModContent.ProjectileType<KorochunIceShot>(), dmg, 0f, Main.myPlayer);
                    }
                    NPC.netUpdate = true;
                }

                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.1f }, c);
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.7f, Pitch = -0.1f }, c);
                KorochunFx.Flash(c, 340f, new Color(200, 230, 255, 230), 18);
                KorochunFx.Ring(c, 30, 9f, 1.6f, tint);
                KorochunFx.Ring(c, 18, 4.5f, 1.4f, Color.White);
                for (int i = 0; i < 8; i++)
                    KorochunFx.Steam(c + Main.rand.NextVector2Circular(30f, 30f), Main.rand.NextVector2Circular(3f, 3f), 1.5f);
                KorochunFx.Shake(10, 1.4f);
            }

            if (Timer >= lastBurst + 40)
            {
                SetState(State.Idle);
                Timer = 20;
            }
        }

        private void LaserAI(Player t)
        {
            if ((int)Sub == 0)
            {
                Timer++;
                float p = Math.Min(Timer / LCT, 1f);
                MouthOpen = Timer > 8;

                if (Timer < LLT)
                {
                    Face(t);
                    HoverAtSide(t, SideOffset + 40f, -10f, 5f);
                }
                else NPC.velocity *= 0.92f;

                drawShake = Main.rand.NextVector2Circular(1f, 1f) * (0.3f + p * 3.5f);

                Vector2 m = Mouth;
                float want = (tc - m).ToRotation();
                if (Timer == 1) LaserAngle = want;
                else if (Timer < LLT) LaserAngle = Utils.AngleLerp(LaserAngle, want, 0.08f);

                if (Timer == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("Waybound/Assets/Sounds/Bosses/SpiritDive") with
                    {
                        Volume = 0.5f,
                        Pitch = -0.15f
                    }, NPC.Center);

                    // SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = -0.3f }, NPC.Center);
                }

                int n = 1 + (int)(p * 3f);
                for (int i = 0; i < n; i++)
                {
                    Color? tint = Main.rand.NextBool(3) ? KorochunFx.Violet : null;
                    KorochunFx.Converge(m, 50f, 170f, 1, 1f + p, 12, tint);
                }

                if (Timer % 8 == 0)
                    KorochunFx.Steam(NPC.Center + Main.rand.NextVector2Circular(38f, 28f), new Vector2(-NPC.direction * 1.2f, -Main.rand.NextFloat(0.8f, 2.5f)), 1.2f);

                if (Timer == LLT)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.9f, Pitch = 0.2f }, NPC.Center);
                    KorochunFx.Flash(m, 140f, new Color(200, 230, 255, 220), 10);
                    KorochunFx.Ring(m, 18, 6f, 1.3f);
                }

                Lighting.AddLight(m, 0.4f + p, 0.7f + p, 1.1f + p);

                if (Timer >= LCT)
                {
                    if (Authority)
                    {
                        int dmg = NPC.GetAttackDamage_ForProjectiles(60f, 45f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), m, Vector2.Zero, ModContent.ProjectileType<KorochunLaser>(),
                            dmg, 0f, Main.myPlayer, NPC.whoAmI, LaserAngle);
                    }

                    NPC.velocity = -LaserAngle.ToRotationVector2() * 6f;
                    Sub = 1;
                    Timer = 0;
                    NPC.netUpdate = true;

                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1f, Pitch = -0.3f }, m);
                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.8f, Pitch = -0.2f }, m);
                    KorochunFx.Flash(m, 320f, Color.White, 18);
                    KorochunFx.Flash(m, 210f, new Color(170, 215, 255, 230), 22);
                    KorochunFx.Ring(m, 32, 10f, 1.8f);
                    KorochunFx.Ring(m, 22, 5f, 1.4f, Color.White);
                    KorochunFx.Shake(26, 2.4f);
                }
            }
            else
            {
                Timer++;
                MouthOpen = true;
                drawShake = Main.rand.NextVector2Circular(1f, 1f) * 2f;

                TrackLaser(t);
                HoverAtSide(t, SideOffset + 40f, -10f, 3.2f, 0.12f);

                Vector2 m = Mouth;
                if (Timer % 3 == 0) KorochunFx.Steam(m, Main.rand.NextVector2Circular(2.5f, 2.5f), 1.3f);
                KorochunFx.Spray(m + Main.rand.NextVector2Circular(12f, 12f), LaserAngle.ToRotationVector2(), 2, 1.5f, 4f, 1.2f, 1.1f);

                if (Timer >= LFT + 12)
                {
                    SetState(State.Idle);
                    Timer = 30;
                }
            }
        }

        private void TrackLaser(Player t)
        {
            int type = ModContent.ProjectileType<KorochunLaser>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != type || (int)p.ai[0] != NPC.whoAmI) continue;

                if (Authority)
                {
                    float target = (tc - Mouth).ToRotation();

                    float face = NPC.direction >= 0 ? 0f : MathHelper.Pi;
                    float rel = MathHelper.Clamp(MathHelper.WrapAngle(target - face), -1.3f, 1.3f);

                    float ease = MathHelper.Clamp(Timer / 20f, 0f, 1f);
                    float step = 0.003f + 0.009f * ease;

                    p.ai[1] = Utils.AngleTowards(p.ai[1], face + rel, step);
                    if (Timer % 2 == 0) p.netUpdate = true;
                }

                LaserAngle = p.ai[1];
                break;
            }
        }

        private void SpiritRingAI(Player t)
        {
            Timer++;
            Face(t);
            HoverAtSide(t, SideOffset + 40f, -60f, 5f, 0.4f);

            const int spawnGap = 12;                       // шаг между головами
            int summonEnd = spawnGap * (SpiritRingCount + 1);

            MouthOpen = Timer > 4 && Timer < summonEnd + 10;
            drawShake = Main.rand.NextVector2Circular(1f, 1f) * (Timer < summonEnd ? 2f : 0.5f);

            if (Timer % spawnGap == 0 && Timer >= spawnGap && Timer <= summonEnd - spawnGap)
            {
                int k = (int)Timer / spawnGap - 1;         // 0, 1, 2, 3 — ровно 4 головы
                if (k >= 0 && k < SpiritRingCount)
                {
                    Vector2 m = Mouth;
                    KorochunFx.Flash(m, 120f, new Color(190, 170, 255, 220), 10);
                    KorochunFx.Spray(m, Main.rand.NextVector2CircularEdge(1f, 1f), 8, 2f, 6f, 0.5f, 1.1f, KorochunFx.Violet);
                    SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.7f, Pitch = -0.2f + k * 0.07f }, m);

                    if (Authority)
                    {
                        float ang = MathHelper.TwoPi * k / SpiritRingCount;
                        float delay = 130f + 20f * k - Timer;
                        int dmg = NPC.GetAttackDamage_ForProjectiles(34f, 28f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<KorochunHeadProj>(), dmg, 0f, Main.myPlayer, 0f, delay, ang);
                    }
                }
            }

            if (Timer >= 270)
            {
                SetState(State.Idle);
                Timer = 20;
            }
        }

        private void SpiritWaveAI(Player t)
        {
            Timer++;
            Face(t);
            HoverAtSide(t, SideOffset, -40f, 5f, 0.4f);

            MouthOpen = (Timer > 8 && Timer < 70) || (Timer > 98 && Timer < 160);
            drawShake = Main.rand.NextVector2Circular(1f, 1f) * ((Timer > 14 && Timer < 40) || (Timer > 104 && Timer < 130) ? 3f : 0.5f);

            if (Timer == 20 || Timer == 110)
            {
                int w = Timer == 20 ? 0 : 1;
                Vector2 m = Mouth;
                KorochunFx.Flash(m, 220f, new Color(190, 170, 255, 230), 14);
                KorochunFx.Ring(m, 20, 7f, 1.5f, KorochunFx.Violet);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = 0.1f }, m);
                KorochunFx.Shake(10, 1.2f);

                if (Authority) SpawnWave(t, w);
            }

            if (Timer >= 330)
            {
                SetState(State.Idle);
                Timer = 20;
            }
        }

        private void SpawnWave(Player t, int w)
        {
            float dirSign = w == 0 ? -Side : Side;
            float spawnX = tc.X - dirSign * 860f;
            const float spacing = 108f;
            float top = tc.Y - spacing * (WaveSlots - 1) / 2f;
            int gap = Main.rand.Next(2, WaveSlots - 3);
            int dmg = NPC.GetAttackDamage_ForProjectiles(36f, 30f);
            float speed = dirSign * (8.5f + w * 1.5f);

            for (int i = 0; i < WaveSlots; i++)
            {
                if (i == gap || i == gap + 1) continue;
                Vector2 pos = new Vector2(spawnX, top + i * spacing);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero,
                    ModContent.ProjectileType<KorochunHeadProj>(), dmg, 0f, Main.myPlayer, 1f, 55f, speed);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int start = MouthOpen ? 4 : 0;
            bool dashing = CurrentState == State.Dash && (int)Sub % 10 == 1;
            bool charging = CurrentState == State.Laser && (int)Sub == 0;
            int rate = dashing ? 3 : charging ? 4 : 6;

            if (animFrame < start || animFrame > start + 3)
                animFrame = start + (animFrame % 4);

            NPC.frameCounter++;
            if (NPC.frameCounter >= rate)
            {
                NPC.frameCounter = 0;
                animFrame++;
                if (animFrame > start + 3) animFrame = start;
            }
            NPC.frame.Y = animFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float a = 1f - NPC.alpha / 255f;
            if (a <= 0.01f) return false;

            bool p2 = DrawsPhase2;
            Texture2D tex = p2 ? KorochunFx.Body2 : TextureAssets.Npc[Type].Value;

            int fh = tex.Height / 8;
            Rectangle frame = new Rectangle(0, animFrame * fh, tex.Width, fh);
            Vector2 origin = frame.Size() / 2f;
            bool flip = SpriteFacesRight ? NPC.spriteDirection == -1 : NPC.spriteDirection == 1;
            SpriteEffects fx = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            int phase = (int)Sub % 10;
            bool dashing = CurrentState == State.Dash && phase == 1;
            bool windup = CurrentState == State.Dash && phase == 0;
            bool charging = CurrentState == State.Laser && (int)Sub == 0;
            bool transforming = CurrentState == State.Transform && Timer < TransformSwap;

            Vector2 pos = NPC.Center - screenPos + drawShake + new Vector2(0f, NPC.gfxOffY) + visualOffset;

            DrawAura(pos, a, p2);

            KorochunFx.BeginAdditive();

            if (dashing)
            {
                for (int i = NPC.oldPos.Length - 1; i >= 1; i--)
                {
                    if (NPC.oldPos[i] == Vector2.Zero) continue;
                    float k = i / (float)NPC.oldPos.Length;
                    Color c = Color.Lerp(p2 ? KorochunFx.Violet : KorochunFx.Frost, KorochunFx.IceDeep, k) * ((1f - k) * 0.5f);
                    spriteBatch.Draw(tex, NPC.oldPos[i] + NPC.Size / 2f - screenPos, frame, c, NPC.oldRot[i], origin, NPC.scale * (1f - k * 0.1f), fx, 0f);
                }
            }

            if (windup || charging || transforming)
            {
                float p = transforming ? Timer / (float)TransformSwap
                        : windup ? Math.Min(Timer / 72f, 1f)
                        : Math.Min(Timer / LCT, 1f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = new Vector2(2f + p * 3f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 2.5f);
                    Color c = transforming ? Color.Lerp(KorochunFx.Ice, KorochunFx.Violet, p) : KorochunFx.Ice;
                    spriteBatch.Draw(tex, pos + off, frame, c * (0.22f + p * 0.35f) * a, NPC.rotation, origin, NPC.scale * (1.02f + p * 0.04f), fx, 0f);
                }
            }

            KorochunFx.EndAdditive();

            spriteBatch.Draw(tex, pos, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, fx, 0f);

            if (charging)
            {
                float p = Math.Min(Timer / LCT, 1f);
                KorochunFx.BeginAdditive();
                spriteBatch.Draw(tex, pos, frame, KorochunFx.Ice * (p * 0.5f * (0.85f + 0.15f * MathF.Sin(Timer * 0.4f))), NPC.rotation, origin, NPC.scale, fx, 0f);
                KorochunFx.EndAdditive();
            }

            if (p2)
                DrawPhase2Glow(spriteBatch, pos, fh, frame, origin, fx, a);

            return false;
        }

        private void DrawAura(Vector2 pos, float a, bool p2)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.8f + 0.2f * MathF.Sin(time * 2.4f);
            float baseSize = Math.Max(NPC.width, NPC.height) * NPC.scale * 1.9f;
            Color main = p2 ? KorochunFx.Violet : KorochunFx.Ice;

            float strength = 0.08f + auraCharge * 0.4f;
            float size = baseSize * (1f + auraCharge * 0.35f) * pulse;

            KorochunFx.BeginAdditive();

            KorochunFx.DrawGlow(pos, main * (strength * a), size);
            KorochunFx.DrawGlow(pos, KorochunFx.IceDeep * (strength * 0.7f * a), size * 0.65f);

            if (auraCharge > 0f)
            {
                float ringSize = baseSize * (2.4f - auraCharge * 1.2f);
                KorochunFx.DrawGlow(pos, KorochunFx.Frost * (auraCharge * 0.18f * a), ringSize);
                KorochunFx.DrawGlow(pos, Color.White * (auraCharge * auraCharge * 0.3f * a), baseSize * 0.7f);
            }

            if (auraFlash > 0f)
            {
                float k = 1f - auraFlash;
                KorochunFx.DrawGlow(pos, KorochunFx.Frost * (auraFlash * 0.5f * a), baseSize * (1.2f + k * 3.2f));
                KorochunFx.DrawGlow(pos, main * (auraFlash * 0.35f * a), baseSize * (0.9f + k * 2f));
            }

            KorochunFx.EndAdditive();
        }

        private void DrawPhase2Glow(SpriteBatch spriteBatch, Vector2 pos, int fh, Rectangle frame, Vector2 origin, SpriteEffects fx, float a)
        {
            Texture2D glowTex = KorochunFx.Body2Glow;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.75f + 0.25f * MathF.Sin(time * 3f);

            float boost = 1f;
            if (CurrentState == State.Laser && (int)Sub == 0)
                boost += Math.Min(Timer / LCT, 1f) * 1.2f;
            else if (CurrentState == State.Laser)
                boost += 1.2f;
            else if (CurrentState == State.SpiritRing || CurrentState == State.SpiritWave)
                boost += 0.6f + 0.2f * MathF.Sin(time * 8f);
            else if (CurrentState == State.FrostNova)
                boost += auraCharge * 0.8f + auraFlash * 0.8f;
            else if (CurrentState == State.Transform)
                boost += 1.5f * Math.Max(0f, 1f - (Timer - TransformSwap) / 50f);

            KorochunFx.BeginAdditive();

            float haloSize = Math.Max(NPC.width, NPC.height) * NPC.scale * 2.4f;
            KorochunFx.DrawGlow(pos, KorochunFx.Violet * (0.2f * pulse * boost * a), haloSize);
            KorochunFx.DrawGlow(pos, KorochunFx.IceDeep * (0.12f * pulse * boost * a), haloSize * 0.65f);

            for (int i = 0; i < 6; i++)
            {
                float ang = time * 1.6f + MathHelper.TwoPi * i / 6f;
                float radius = 3.5f + 1.5f * MathF.Sin(time * 2f + i);
                Vector2 off = new Vector2(radius, 0f).RotatedBy(ang);
                Color c = Color.Lerp(KorochunFx.Ice, KorochunFx.Violet, 0.5f + 0.5f * MathF.Sin(time * 1.3f + i)) * (0.26f * boost * a);
                spriteBatch.Draw(glowTex, pos + off, frame, c, NPC.rotation, origin, NPC.scale * 1.035f, fx, 0f);
            }

            int bandH = Math.Max(2, (int)(fh * 0.16f));
            float sweep = (time * 0.55f) % 1f;
            int bandY = (int)(sweep * (fh - bandH));
            Rectangle band = new Rectangle(frame.X, frame.Y + bandY, frame.Width, bandH);
            Vector2 bandOrigin = new Vector2(frame.Width / 2f, fh / 2f - bandY);
            spriteBatch.Draw(glowTex, pos, band, Color.White * (0.9f * a), NPC.rotation, bandOrigin, NPC.scale, fx, 0f);
            spriteBatch.Draw(glowTex, pos, band, KorochunFx.Frost * (0.5f * a), NPC.rotation, bandOrigin, NPC.scale * 1.04f, fx, 0f);

            spriteBatch.Draw(glowTex, pos, frame, Color.White * ((0.65f + 0.35f * pulse) * Math.Min(boost, 1.6f) * a), NPC.rotation, origin, NPC.scale, fx, 0f);

            KorochunFx.EndAdditive();
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (CurrentState != State.Laser || (int)Sub != 0 || Timer < 8) return;

            float p = Math.Min(Timer / LCT, 1f);
            bool locked = Timer >= LLT;
            Vector2 mouthWorld = Mouth + visualOffset;
            Vector2 m = mouthWorld - screenPos;
            float ang = LaserAngle;
            Texture2D ray = TextureAssets.Extra[98].Value;
            Vector2 rayOrigin = new Vector2(ray.Width / 2f, ray.Height);
            float time = Main.GlobalTimeWrappedHourly;

            float lp = locked ? (Timer - LLT) / (float)(LCT - LLT) : 0f;
            float flick = 0.7f + 0.3f * (((int)(Timer / 2f)) % 2);
            float telScale = locked ? (0.5f + 0.4f * lp) * flick : 0.12f + 0.3f * p;
            float telAlpha = locked ? 0.85f : 0.2f + 0.3f * p;
            KorochunLaser.DrawBeam(mouthWorld, ang, BeamLength, telScale, p, telAlpha);

            KorochunFx.BeginAdditive();

            float orb = MathHelper.Lerp(20f, 170f, p * p) * (1f + 0.06f * MathF.Sin(time * 12f));
            KorochunFx.DrawGlow(m, KorochunFx.IceDeep * (0.45f + 0.35f * p), orb * 1.5f);
            KorochunFx.DrawGlow(m, KorochunFx.Ice * (0.55f + 0.35f * p), orb);
            KorochunFx.DrawGlow(m, Color.White * p, orb * 0.5f);

            int rays = 6 + (int)(p * 4f);
            for (int i = 0; i < rays; i++)
            {
                float rAng = MathHelper.TwoPi / rays * i + time * (0.8f + p * 1.6f);
                float pulse = 0.85f + 0.15f * MathF.Sin(time * 8f + i * 1.7f);
                Main.EntitySpriteDraw(ray, m, null, KorochunFx.Ice * (0.18f + 0.4f * p), rAng, rayOrigin,
                    new Vector2(0.18f + 0.18f * p, (0.3f + 1f * p) * pulse), SpriteEffects.None, 0);
            }

            KorochunFx.EndAdditive();
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server) return;

            KorochunFx.Spray(NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.3f, NPC.height * 0.3f),
                new Vector2(hit.HitDirection, -0.3f), 5, 1.5f, 4.5f, 0.9f, 0.9f);

            if (NPC.life <= 0)
            {
                KorochunFx.Flash(NPC.Center, 400f, Color.White, 24);
                KorochunFx.Ring(NPC.Center, 40, 10f, 2f);
                KorochunFx.Ring(NPC.Center, 24, 5f, 1.6f, Color.White);
                for (int i = 0; i < 30; i++)
                {
                    KorochunFx.Snow(NPC.Center, Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextFloat(18f, 34f),
                        Main.rand.NextBool() ? KorochunFx.RandIce() : KorochunFx.Tint(Color.White), Main.rand.Next(26, 44));
                }
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KorochunTrophyItem>(), 10));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            int crumblePenny = ModContent.ItemType<CrumblePenny>();
            var crumblePennyParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 25,
                MaximumStackPerChunkBase = 35,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            notExpertRule.OnSuccess(new DropOneByOne(crumblePenny, crumblePennyParameters));


            //notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<KorochunMask>(), 7));

            npcLoot.Add(notExpertRule);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.Bags.KorochunTreasureBag>()));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<KorochunRelicItem>()));

            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<MotorcycleOfDesertHunter>(), 4));
        }

    }
}