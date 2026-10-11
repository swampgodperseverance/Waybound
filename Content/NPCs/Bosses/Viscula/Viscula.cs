using System;
using System.Collections.Generic;
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
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.NPCs.Bosses.Viscula
{
    //Assets library
    public static class HeartTextures
    {
        public const string Bolt = "Waybound/Content/NPCs/Bosses/Viscula/VisculaBolt";
        public const string Diamond = "Waybound/Content/NPCs/Bosses/Viscula/VisculaRomb";
        public const string Wheel = "Waybound/Content/NPCs/Bosses/Viscula/VisculaWheel";
        public const string Spike = "Waybound/Content/NPCs/Bosses/Viscula/VisculaSpike";
        public const string Wire = "Waybound/Content/NPCs/Bosses/Viscula/VisculaWire";
        public const string Glow = "Waybound/Content/NPCs/Bosses/Viscula/Viscula_Glow";
        public const string Ray = "Waybound/Assets/Textures/Ray";
        public const string Heartbeat = "Waybound/Assets/Sounds/Bosses/Heartbeat";

        public const float SpriteForward = MathHelper.PiOver2;
        public static bool RayAlongHeight = true;

        private static Asset<Texture2D> ray;
        private static Asset<Texture2D> wire;
        private static Asset<Texture2D> glow;

        private static Texture2D Load(ref Asset<Texture2D> asset, string path)
        {
            if (asset == null)
            {
                if (!ModContent.HasAsset(path))
                    return null;

                asset = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
            }
            return asset.Value;
        }

        public static Texture2D RayTex => Load(ref ray, Ray);
        public static Texture2D WireTex => Load(ref wire, Wire);
        public static Texture2D GlowTex => Load(ref glow, Glow);

        public static void Unload()
        {
            ray = null;
            wire = null;
            glow = null;
        }
    }

    public class HeartTexturesSystem : ModSystem
    {
        public override void Unload()
        {
            HeartTextures.Unload();
            Heartfx.DisposeGlow();
        }
    }

    public static class VisculaState
    {
        public const int Dormant = 0;
        public const int Phase1 = 1;
        public const int Trans12 = 2;
        public const int Phase2 = 3;
        public const int Trans23 = 4;
        public const int Phase3 = 5;
        public const int Dying = 6;
    }
    //Heart functions that i made just for my time
    public static class HeartMisc
    {
        public static int Pick(int normal, int expert, int master)
        {
            return Main.masterMode ? master : Main.expertMode ? expert : normal;
        }

        public static float Mult => Main.masterMode ? 1.15f : Main.expertMode ? 1.08f : 1f;

        public static bool Solid(Vector2 p, bool platforms = false)
        {
            int i = (int)(p.X / 16f);
            int j = (int)(p.Y / 16f);
            if (!WorldGen.InWorld(i, j, 2))
                return true;

            if (WorldGen.SolidTile(i, j))
                return true;

            if (!platforms)
                return false;

            Tile tile = Main.tile[i, j];
            return tile.HasUnactuatedTile && Main.tileSolidTop[tile.TileType];
        }

        public static float Raycast(Vector2 start, Vector2 dir, float max, out bool hit, bool platforms = false)
        {
            hit = false;
            dir = dir.SafeNormalize(Vector2.UnitX);
            float d = 0f;

            while (d < max)
            {
                d += 8f;
                if (!Solid(start + dir * d, platforms))
                    continue;

                for (float k = d - 8f; k <= d; k += 1f)
                {
                    if (Solid(start + dir * k, platforms))
                    {
                        hit = true;
                        return k;
                    }
                }

                hit = true;
                return d;
            }

            return max;
        }

        public static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float u)
        {
            float v = 1f - u;
            return v * v * a + 2f * v * u * c + u * u * b;
        }

        public static Vector2 BezierTangent(Vector2 a, Vector2 c, Vector2 b, float u)
        {
            return 2f * (1f - u) * (c - a) + 2f * u * (b - c);
        }

        public static void DrawRay(Vector2 start, float rotation, float length, float width, Color color)
        {
            Texture2D tex = HeartTextures.RayTex;
            if (tex == null || length <= 0f)
                return;

            if (HeartTextures.RayAlongHeight)
            {
                Main.EntitySpriteDraw(tex, start - Main.screenPosition, null, color, rotation + MathHelper.PiOver2,
                    new Vector2(tex.Width / 2f, tex.Height), new Vector2(width / tex.Width, length / tex.Height), SpriteEffects.None, 0);
            }
            else
            {
                Main.EntitySpriteDraw(tex, start - Main.screenPosition, null, color, rotation,
                    new Vector2(0f, tex.Height / 2f), new Vector2(length / tex.Width, width / tex.Height), SpriteEffects.None, 0);
            }
        }
    }
    //As i said FX is separated again
    public static class Heartfx
    {
        public static readonly Color Purple = new Color(120, 42, 190);
        public static readonly Color Violet = new Color(175, 105, 245);
        public static readonly Color Blood = new Color(235, 45, 90);

        public static Color Tint(int state)
        {
            return state switch
            {
                VisculaState.Dormant => new Color(80, 25, 125),
                VisculaState.Phase1 => new Color(115, 40, 180),
                VisculaState.Phase2 => new Color(135, 48, 205),
                VisculaState.Phase3 => new Color(160, 60, 235),
                _ => new Color(145, 55, 220)
            };
        }

        public static Color Alpha0(Color c) => new Color(c.R, c.G, c.B, 0);

        public static Color Dark(Color c, float k) => new Color((int)(c.R * k), (int)(c.G * k), (int)(c.B * k), c.A);

        public static void Spark(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                color,
                life
            ));
        }

        public static void Burst(Vector2 pos, int count, float speed, float size, Color color)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Spark(pos, vel, size * Main.rand.NextFloat(0.8f, 1.3f), color, Main.rand.Next(16, 28));
            }
        }

        public static void SparkRing(Vector2 pos, int count, float speed, float size, Color color, int life)
        {
            if (Main.dedServ)
                return;

            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = (off + MathHelper.TwoPi * i / count).ToRotationVector2();
                Spark(pos, dir * speed, size, color, life);
            }
        }

        public static void Star(Vector2 world, float rot, Vector2 scale, Color color)
        {
            Texture2D t = TextureAssets.Extra[98].Value;
            Main.EntitySpriteDraw(t, world - Main.screenPosition, null, color, rot, t.Size() / 2f, scale, SpriteEffects.None, 0);
        }

        private static Texture2D softGlow;

        public static Texture2D SoftGlow
        {
            get
            {
                if (softGlow == null || softGlow.IsDisposed)
                {
                    const int size = 128;
                    softGlow = new Texture2D(Main.instance.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - 63.5f) / 63.5f;
                            float dy = (y - 63.5f) / 63.5f;
                            float d = MathF.Sqrt(dx * dx + dy * dy);
                            float a = MathF.Pow(MathHelper.Clamp(1f - d, 0f, 1f), 2.2f);
                            data[y * size + x] = new Color(a, a, a, a);
                        }
                    }
                    softGlow.SetData(data);
                }
                return softGlow;
            }
        }

        public static void DisposeGlow()
        {
            Texture2D tex = softGlow;
            softGlow = null;
            if (tex != null)
                Main.QueueMainThreadAction(() => tex.Dispose());
        }

        public static void DrawSoft(Vector2 worldPos, Color color, float diameter)
        {
            Main.EntitySpriteDraw(SoftGlow, worldPos - Main.screenPosition, null, color, 0f, new Vector2(64f), diameter / 128f, SpriteEffects.None, 0);
        }

        public static void Flash(Vector2 pos, float size, Color color, int duration)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: pos.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(size),
                color: color,
                duration: duration));
        }

        public static void DrawGlow(Texture2D tex, Vector2 worldPos, Rectangle? src, float rot, Vector2 origin, float scale, Color tint,
            float fade = 1f, float outline = 0.35f, float bloom = 0.14f, float thickness = 1.6f)
        {
            Vector2 pos = worldPos - Main.screenPosition;

            if (bloom > 0f)
                Main.EntitySpriteDraw(tex, pos, src, Alpha0(tint) * (bloom * fade), rot, origin, scale * 1.28f, SpriteEffects.None, 0);

            if (outline <= 0f)
                return;

            Color c = Alpha0(tint) * (outline * fade);
            for (int i = 0; i < 8; i++)
            {
                Vector2 off = new Vector2(thickness, 0f).RotatedBy(MathHelper.PiOver4 * i);
                Main.EntitySpriteDraw(tex, pos + off, src, c, rot, origin, scale, SpriteEffects.None, 0);
            }
        }

        public static void DrawTrail(Projectile proj, Texture2D tex, Color tint, float intensity, float rotOffset, bool useOldRot)
        {
            Vector2 origin = tex.Size() / 2f;
            int len = proj.oldPos.Length;

            for (int k = 1; k < len; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero)
                    continue;

                float f = 1f - k / (float)len;
                float rot = (useOldRot ? proj.oldRot[k] : proj.rotation) + rotOffset;
                Main.EntitySpriteDraw(tex, proj.oldPos[k] + proj.Size / 2f - Main.screenPosition, null,
                    Alpha0(tint) * (f * intensity), rot, origin, proj.scale * (0.55f + 0.45f * f), SpriteEffects.None, 0);
            }
        }
    }

    public class Viscula : ModNPC
    {
        private struct Pending
        {
            public int Tick;
            public Vector2 Pos;
            public float Angle;
        }

        private const int DeathLength = 200;
        private static readonly int[] DeathBeats = { 1, 26, 48, 68, 86, 102, 116, 128, 138, 146, 153, 159, 165, 171, 177, 183, 189, 195 }; //Is that too much? i do not care also it's easiest way to get rid of coop issues shit, static fields with specific ticks

        private readonly List<Vector2> anchors = new List<Vector2>();
        private readonly List<Vector2> anchorSpawns = new List<Vector2>();
        private readonly List<Pending> pending = new List<Pending>();

        private int anchorTick;
        private int beatTimer;
        private int lastAttack;
        private int lastBeatSeen = -1;
        private int lastState = -1;
        private int secondBeat;
        private float pulse;
        private float wirePulse = 2f;
        private float appear = -1f;
        private bool deathBurst;

        private int State
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private int Attack
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private int Timer
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float Appear => appear < 0f ? (State == VisculaState.Dormant ? 0f : 1f) : appear;

        private float Bright
        {
            get
            {
                float a = Appear;
                return MathHelper.Lerp(0.28f, 1f, a * a * (3f - 2f * a));
            }
        }

        private static int BoltType => ModContent.ProjectileType<VisculaBolt>();
        private static int OrbType => ModContent.ProjectileType<VisculaOrb>();
        private static int DiamondType => ModContent.ProjectileType<VisculaDiamond>();
        private static int ThornType => ModContent.ProjectileType<VisculaThorn>();
        private static int FloorSpikeType => ModContent.ProjectileType<VisculaFloorSpike>();
        private static int WheelType => ModContent.ProjectileType<VisculaWheel>();
        private static int RayType => ModContent.ProjectileType<VisculaRay>();
        private static int SlashType => ModContent.ProjectileType<VisculaSlash>();

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            //Put the bestiary stuff here
        }

        public override void SetDefaults()
        {
            NPC.width = 96;
            NPC.height = 96;
            NPC.damage = 30;
            NPC.defense = 14;
            NPC.lifeMax = 6800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.npcSlots = 10f;
            NPC.scale = 1f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.lavaImmune = true;

            for (int i = 0; i < NPC.buffImmune.Length; i++)
                NPC.buffImmune[i] = true;
            //Please make the immunities list I'll forget
        }

        public override bool CheckActive() => false;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => State != VisculaState.Dormant && State != VisculaState.Dying;

        public override bool CheckDead()
        {
            if (State == VisculaState.Dying)
                return Timer >= DeathLength;

            State = VisculaState.Dying;
            Attack = 0;
            Timer = 0;
            pending.Clear();
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            NPC.netUpdate = true;
            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 4; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 2f, -1f, 0, default, 1.1f);

            if (NPC.life <= 0)
                Heartfx.Burst(NPC.Center, 40, 7f, 24f, Heartfx.Tint(VisculaState.Phase3));
        }

        public override void FindFrame(int frameHeight)
        {
            int frame;

            switch (State)
            {
                case VisculaState.Dormant:
                case VisculaState.Phase1:
                    frame = 0;
                    break;
                case VisculaState.Trans12:
                    frame = Timer >= 45 ? 1 : 0;
                    break;
                case VisculaState.Phase2:
                    frame = 1 + (int)(Main.GameUpdateCount / 7 % 3);
                    break;
                case VisculaState.Trans23:
                    float p = MathHelper.Clamp((Timer - 20f) / 130f, 0f, 0.999f);
                    frame = 4 + (int)(p * 4f);
                    break;
                default:
                    frame = 7;
                    break;
            }

            NPC.frame.Height = frameHeight;
            NPC.frame.Y = frame * frameHeight;
        }

        private int Dmg(float value) => (int)NPC.GetAttackDamage_ForProjectiles(value, value * 0.75f);

        private void Spawn(int type, Vector2 pos, Vector2 vel, float dmg, float a0 = 0f, float a1 = 0f, float a2 = 0f)
        {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, type, Dmg(dmg), 0f, Main.myPlayer, a0, a1, a2);
        }

        private void Ring(int count, float speed, float offset, float dmg, float accel = 0f, float max = 0f)
        {
            for (int i = 0; i < count; i++)
                Spawn(BoltType, NPC.Center, (offset + MathHelper.TwoPi * i / count).ToRotationVector2() * speed, dmg, accel > 0f ? 1f : 0f, accel, max);
        }

        private void Beat()
        {
            NPC.ai[3]++;
            beatTimer = 0;
            NPC.netUpdate = true;
        }

        private int BeatInterval()
        {
            return State switch
            {
                VisculaState.Dormant => 110,
                VisculaState.Phase1 => 62,
                VisculaState.Phase2 => 44,
                VisculaState.Phase3 => 30,
                _ => int.MaxValue
            };
        }

        private void BuildAnchors()
        {
            anchors.Clear();
            anchorSpawns.Clear();

            float[] baseAngles = { -2.356f, -0.785f, 0.785f, 2.356f };
            Random rng = new Random(((int)(NPC.Center.X / 16f) * 73856093) ^ ((int)(NPC.Center.Y / 16f) * 19349663));

            foreach (float baseAngle in baseAngles)
            {
                for (int tries = 0; tries < 14; tries++)
                {
                    float angle = baseAngle + (tries == 0 ? 0f : (float)(rng.NextDouble() - 0.5) * 0.9f);
                    Vector2 dir = angle.ToRotationVector2();
                    float d = HeartMisc.Raycast(NPC.Center, dir, 560f, out bool hit);

                    if (hit && d > 90f)
                    {
                        anchors.Add(NPC.Center + dir * d);
                        anchorSpawns.Add(NPC.Center + dir * (d - 16f));
                        break;
                    }
                }
            }
        }

        public override void AI()
        {
            NPC.velocity = Vector2.Zero;
            NPC.ai[2]++;
            NPC.dontTakeDamage = State == VisculaState.Trans12 || State == VisculaState.Trans23 || State == VisculaState.Dying;
            NPC.boss = State != VisculaState.Dormant;

            if (anchors.Count < 3 && anchorTick++ % 45 == 0)
                BuildAnchors();

            if (Main.netMode != NetmodeID.MultiplayerClient)
                ServerAI();

            ClientVisuals();
        }

        private void StartFight()
        {
            State = VisculaState.Phase1;
            Attack = 0;
            Timer = 0;
            lastAttack = 0;
            pending.Clear();
            Beat();
        }

        private void ResetFight()
        {
            State = VisculaState.Dormant;
            Attack = 0;
            Timer = 0;
            lastAttack = 0;
            beatTimer = 0;
            pending.Clear();
            NPC.life = NPC.lifeMax;
            NPC.netUpdate = true;
        }

        private void BeginTransition(int state)
        {
            State = state;
            Attack = 0;
            Timer = 0;
            pending.Clear();
            NPC.netUpdate = true;
        }

        private void DyingTick()
        {
            int t = Timer;

            if (Array.IndexOf(DeathBeats, t) >= 0)
                Beat();

            if (t >= DeathLength)
            {
                NPC.dontTakeDamage = false;
                NPC.life = 0;
                NPC.checkDead();
            }
        }

        private void ServerAI()
        {
            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];

            if (State == VisculaState.Dying)
            {
                DyingTick();
                return;
            }

            if (State == VisculaState.Dormant)
            {
                beatTimer++;
                if (beatTimer >= BeatInterval())
                    Beat();

                if (NPC.life < NPC.lifeMax)
                    StartFight();

                return;
            }

            if (!target.active || target.dead || Vector2.Distance(target.Center, NPC.Center) > 3600f)
            {
                ResetFight();
                return;
            }

            beatTimer++;
            if (beatTimer >= BeatInterval())
                Beat();

            // states
            switch (State)
            {
                case VisculaState.Trans12:
                    Transition12Tick();
                    break;
                case VisculaState.Trans23:
                    Transition23Tick();
                    break;
                default:
                    FightTick(target);
                    break;
            }
        }

        private void FightTick(Player target)
        {
            float frac = NPC.life / (float)NPC.lifeMax;

            if (State == VisculaState.Phase1 && frac <= 0.65f)
            {
                BeginTransition(VisculaState.Trans12);
                return;
            }

            if (State == VisculaState.Phase2 && frac <= 0.30f)
            {
                BeginTransition(VisculaState.Trans23);
                return;
            }

            if (Attack == 0)
            {
                int rest = (State == VisculaState.Phase1 ? 100 : State == VisculaState.Phase2 ? 60 : 40) - HeartMisc.Pick(0, 8, 14);
                if (Timer >= rest)
                {
                    int pool = Main.masterMode && State != VisculaState.Phase2 ? 6 : 5;
                    int next;
                    do
                    {
                        next = Main.rand.Next(1, pool);
                    }
                    while (next == lastAttack);

                    lastAttack = next;
                    Attack = next;
                    Timer = 0;
                    NPC.netUpdate = true;
                }
                return;
            }

            switch (State)
            {
                case VisculaState.Phase1:
                    RunPhase1Attack(target, Attack);
                    break;
                case VisculaState.Phase2:
                    RunPhase2Attack(target, Attack);
                    break;
                default:
                    RunPhase3Attack(target, Attack);
                    break;
            }
        }

        //1
        private void RunPhase1Attack(Player player, int atk)
        {
            bool done;

            switch (atk)
            {
                case 1:
                    done = BoltRings(player);
                    break;
                case 2:
                    done = DiamondFan(player);
                    break;
                case 3:
                    done = AimedBeam(player);
                    break;
                case 5:
                    done = SpiralBolts(player);
                    break;
                default:
                    done = SideDiamonds(player);
                    break;
            }

            FinishAttack(done);
        }
        //2
        private void RunPhase2Attack(Player player, int atk)
        {
            bool done;

            switch (atk)
            {
                case 1:
                    done = CeilingRays(player);
                    break;
                case 2:
                    done = FloorSpikes(player);
                    break;
                case 3:
                    done = AnchorRays(player);
                    break;
                default:
                    done = ThornRing(player);
                    break;
            }

            FinishAttack(done);
        }
        //3

        private void RunPhase3Attack(Player player, int atk)
        {
            bool done;

            switch (atk)
            {
                case 1:
                    done = WheelsAndRing(player);
                    break;
                case 2:
                    done = CrossRays(player);
                    break;
                case 3:
                    done = RotatingRays(player);
                    break;
                case 5:
                    done = SweepingSlashes(player);
                    break;
                default:
                    done = OrbsAndRays(player);
                    break;
            }

            FinishAttack(done);
        }

        private void FinishAttack(bool done)
        {
            if (!done)
                return;

            Attack = 0;
            Timer = 0;
            pending.Clear();
            NPC.netUpdate = true;
        }
        //1
        private bool BoltRings(Player target)
        {
            int t = Timer;

            if (t == 20 || t == 85 || t == 150)
            {
                Beat();
                int n = (t == 150 ? 12 : 10) + HeartMisc.Pick(0, 2, 2);
                float speed = (t == 20 ? 3f : t == 85 ? 3.4f : 3.8f) * HeartMisc.Mult;
                float offset = t == 85 ? NPC.localAI[2] + MathHelper.Pi / n : Main.rand.NextFloat(MathHelper.TwoPi);
                NPC.localAI[2] = offset;

                Ring(n, speed, offset, 16f);
            }

            if (t == 110 && Main.masterMode)
            {
                Beat();
                float aimOff = (target.Center - NPC.Center).ToRotation();
                Ring(8, 1.6f, aimOff + MathHelper.Pi / 8f, 16f, 1.045f, 8f);
            }

            return t >= 215;
        }

        private bool DiamondFan(Player target)
        {
            int t = Timer;

            if (t == 30 || t == 100 || t == 170)
            {
                Beat();
                float aim = (target.Center - NPC.Center).ToRotation();
                int count = (t == 100 ? 4 : 3) + HeartMisc.Pick(0, 1, 1);

                for (int i = 0; i < count; i++)
                {
                    float spread = MathHelper.Lerp(-0.4f, 0.4f, i / (float)(count - 1));
                    Spawn(DiamondType, NPC.Center, (aim + spread).ToRotationVector2() * 2f, 20f, 0f, 1.035f, 9.5f * HeartMisc.Mult);
                }
            }

            return t >= 225;
        }

        private bool AimedBeam(Player target)
        {
            int t = Timer;
            int ct = t % 100;

            if (ct == 2)
            {
                float aim = (target.Center - NPC.Center).ToRotation();
                NPC.localAI[2] = aim;
                Spawn(RayType, NPC.Center, aim.ToRotationVector2(), 0f, 0f, 52f, 0f);
            }

            if (ct == 54)
                Beat();

            if (ct >= 54 && ct < 84 && (ct - 54) % 6 == 0)
                Spawn(BoltType, NPC.Center, NPC.localAI[2].ToRotationVector2() * 10f, 16f);

            return t >= 190;
        }

        private bool SpiralBolts(Player target)
        {
            int t = Timer;

            if (t == 2)
            {
                NPC.localAI[2] = Main.rand.NextBool() ? 1f : -1f;
                NPC.localAI[3] = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            if (t >= 12 && t < 108 && t % 4 == 0)
            {
                if ((t - 12) % 32 == 0)
                    Beat();

                float a = NPC.localAI[3] + NPC.localAI[2] * (t - 12) * 0.065f;
                for (int arm = 0; arm < 2; arm++)
                    Spawn(BoltType, NPC.Center, (a + arm * MathHelper.Pi).ToRotationVector2() * 3.4f, 16f);
            }

            if (t == 130)
            {
                Beat();
                float aimD = (target.Center - NPC.Center).ToRotation();
                for (int i = -1; i <= 1; i++)
                    Spawn(DiamondType, NPC.Center, (aimD + i * 0.35f).ToRotationVector2() * 2.2f, 20f, 0f, 1.035f, 10f);
            }

            return t >= 190;
        }

        private bool SideDiamonds(Player target)
        {
            int t = Timer;

            for (int i = 0; i < 6; i++)
            {
                if (t != 20 + i * 22)
                    continue;

                int side = i % 2 == 0 ? -1 : 1;
                Vector2 start = new Vector2(target.Center.X, target.Center.Y + Main.rand.NextFloat(-70f, 70f));
                if (HeartMisc.Solid(start))
                    continue;

                float d = HeartMisc.Raycast(start, new Vector2(side, 0f), 1000f, out bool hit);
                if (!hit || d < 120f)
                    continue;

                Vector2 from = start + new Vector2(side * (d - 18f), 0f);
                float ang = (target.Center - from).ToRotation();
                Spawn(RayType, from, ang.ToRotationVector2(), 0f, 0f, 26f, 0f);
                pending.Add(new Pending { Tick = t + 26, Pos = from, Angle = ang });
            }

            for (int p = pending.Count - 1; p >= 0; p--)
            {
                if (t < pending[p].Tick)
                    continue;

                Beat();
                Spawn(DiamondType, pending[p].Pos, pending[p].Angle.ToRotationVector2() * 2.4f, 20f, 2f, 1.045f, 10.5f);
                pending.RemoveAt(p);
            }

            return t >= 205;
        }

        //2
        private bool CeilingRays(Player target)
        {
            int t = Timer;

            if (t == 20 || t == 72 || t == 124)
            {
                Beat();
                int wave = (t - 20) / 52;
                float shift = wave % 2 == 0 ? 0f : 45f;

                for (int k = -2; k <= 2; k++)
                {
                    float x = target.Center.X + k * 90f + shift + Main.rand.NextFloat(-14f, 14f);
                    Vector2 start = new Vector2(x, target.Center.Y - 20f);
                    if (HeartMisc.Solid(start))
                        continue;

                    float d = HeartMisc.Raycast(start, -Vector2.UnitY, 700f, out bool hit);
                    float ceilY = start.Y - (hit ? d : 420f);

                    Spawn(RayType, new Vector2(x, ceilY + 4f), Vector2.UnitY, 0f, 0f, 34f, 0f);
                    Spawn(DiamondType, new Vector2(x, ceilY + 16f), Vector2.Zero, 26f, 1f, 34f, 0f);
                }
            }

            return t >= 195;
        }

        private bool FloorSpikes(Player target)
        {
            int t = Timer;

            if (t == 20 || t == 95)
            {
                Beat();
                bool outward = t == 20;
                float spacing = outward ? 68f : 84f;

                for (int side = -1; side <= 1; side += 2)
                {
                    for (int k = 0; k <= 5; k++)
                    {
                        if (k == 0 && side == 1)
                            continue;

                        Vector2 start = new Vector2(target.Center.X + side * k * spacing, target.Center.Y - 24f);
                        if (HeartMisc.Solid(start))
                            continue;

                        float d = HeartMisc.Raycast(start, Vector2.UnitY, 700f, out bool hit, true);
                        if (!hit)
                            continue;

                        int delay = 34 + (outward ? k : 5 - k) * 6;
                        Spawn(FloorSpikeType, start + Vector2.UnitY * (d + 2f), Vector2.Zero, 26f, delay, 88f);
                    }
                }
            }

            return t >= 170;
        }

        private bool AnchorRays(Player target)
        {
            int t = Timer;

            if (anchorSpawns.Count == 0)
                return true;

            for (int round = 0; round < 3; round++)
            {
                if (t == 50 + round * 62)
                    Beat();

                for (int i = 0; i < anchorSpawns.Count; i++)
                {
                    if (t != 20 + round * 62 + i * 9)
                        continue;

                    Vector2 from = anchorSpawns[i];
                    float ang = (target.Center - from).ToRotation();
                    Spawn(RayType, from, ang.ToRotationVector2(), 0f, 0f, 30f, 0f);
                    pending.Add(new Pending { Tick = t + 30, Pos = from, Angle = ang });
                }
            }

            for (int p = pending.Count - 1; p >= 0; p--)
            {
                if (t < pending[p].Tick)
                    continue;

                Spawn(DiamondType, pending[p].Pos, pending[p].Angle.ToRotationVector2() * 5f, 24f, 0f, 1.04f, 14f);
                pending.RemoveAt(p);
            }

            return t >= 20 + 3 * 62 + 50;
        }

        private bool ThornRing(Player target)
        {
            int t = Timer;

            if (t == 12 || t == 78)
            {
                Beat();
                float off = t == 12 ? Main.rand.NextFloat(MathHelper.TwoPi) : NPC.localAI[2] + MathHelper.Pi / 12f;
                NPC.localAI[2] = off;
                float radius = Math.Max(NPC.width, NPC.height) * 0.5f + 28f;

                for (int i = 0; i < 12; i++)
                {
                    Vector2 dir = (off + MathHelper.TwoPi * i / 12f).ToRotationVector2();
                    Spawn(ThornType, NPC.Center + dir * radius, dir * 4.5f, 26f, 40f, 1.035f, 10f);
                }
            }

            return t >= 175;
        }

        // 3

        private int CountWheels()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == WheelType)
                    count++;
            }
            return count;
        }

        private bool WheelsAndRing(Player target)
        {
            int t = Timer;

            if (t == 20)
            {
                Beat();
                float aim = (target.Center - NPC.Center).ToRotation();

                for (int s = -1; s <= 1; s += 2)
                {
                    if (CountWheels() >= HeartMisc.Pick(2, 3, 4))
                        break;

                    Spawn(WheelType, NPC.Center, (aim + s * 0.7f).ToRotationVector2() * 6f, 30f);
                }
            }

            if (t == 64)
            {
                Beat();
                Ring(HeartMisc.Pick(10, 12, 14), 5f * HeartMisc.Mult, Main.rand.NextFloat(MathHelper.TwoPi), 20f);
            }

            return t >= 110;
        }

        private bool CrossRays(Player target)
        {
            int t = Timer;
            int n2 = HeartMisc.Pick(8, 10, 12);

            if (t == 8 || t == 100)
            {
                Beat();
                float off = t == 8 ? Main.rand.NextFloat(MathHelper.TwoPi) : NPC.localAI[2] + MathHelper.Pi / n2;
                NPC.localAI[2] = off;

                for (int i = 0; i < n2; i++)
                    Spawn(RayType, NPC.Center, (off + MathHelper.TwoPi * i / n2).ToRotationVector2(), 0f, 0f, 42f, 0f);
            }

            if (t == 50 || t == 142)
            {
                Beat();
                float off = NPC.localAI[2];

                for (int i = 0; i < n2; i++)
                    Spawn(ThornType, NPC.Center, (off + MathHelper.TwoPi * i / n2).ToRotationVector2() * 4f, 28f, 0f, 1.05f, 15f);
            }

            return t >= 190;
        }

        private bool RotatingRays(Player target)
        {
            int t = Timer;
            int rc = HeartMisc.Pick(2, 3, 3);

            if (t == 2)
            {
                Beat();
                float sign = Main.rand.NextBool() ? 1f : -1f;
                float aim = (target.Center - NPC.Center).ToRotation();

                for (int i = 0; i < rc; i++)
                    Spawn(RayType, NPC.Center, (aim + MathHelper.TwoPi * i / rc).ToRotationVector2(), 38f, 0.0165f * sign, 55f, 190f);
            }

            if (t == 90 || t == 150 || t == 210)
            {
                Beat();
                Ring(HeartMisc.Pick(6, 8, 10), 3.6f * HeartMisc.Mult, Main.rand.NextFloat(MathHelper.TwoPi), 20f);
            }

            return t >= 270;
        }

        private bool SweepingSlashes(Player target)
        {
            int t = Timer;

            for (int v = 0; v < 3; v++)
            {
                if (t != 16 + v * 80)
                    continue;

                Beat();
                int cnt = v == 2 ? 4 : 3;
                float side = Main.rand.NextFloat(MathHelper.TwoPi);
                if (v == 1)
                    side = NPC.localAI[2] + MathHelper.PiOver2;
                NPC.localAI[2] = side;

                Vector2 travel = -side.ToRotationVector2();
                Vector2 perp = new Vector2(-travel.Y, travel.X);
                float hold = 46f - v * 4f;

                for (int i = 0; i < cnt; i++)
                {
                    float lane = (i - (cnt - 1) / 2f) * 130f + 65f;
                    Vector2 pos = target.Center + side.ToRotationVector2() * 520f + perp * lane;
                    Spawn(SlashType, pos, travel * 4f, 24f, hold, 1.04f, 13f);
                }
            }

            return t >= 280;
        }

        private bool OrbsAndRays(Player target)
        {
            int t = Timer;

            if (t == 2)
            {
                Beat();
                float sign = Main.rand.NextBool() ? 1f : -1f;
                Spawn(RayType, NPC.Center, -Vector2.UnitY, 36f, 0.0065f * sign, 50f, 190f);
                Spawn(RayType, NPC.Center, Vector2.UnitY, 36f, 0.0065f * sign, 50f, 190f);
            }

            if (t == 90 || t == 150)
            {
                Beat();
                float aim = (target.Center - NPC.Center).ToRotation();
                int oc = HeartMisc.Pick(1, 3, 3);

                for (int i = 0; i < oc; i++)
                    Spawn(OrbType, NPC.Center, (aim + (i - (oc - 1) / 2f) * 0.3f).ToRotationVector2() * 6f * HeartMisc.Mult, 26f);
            }

            return t >= 270;
        }

        private void Transition12Tick()
        {
            int t = Timer;

            if (t == 1 || t == 30 || t == 54 || t == 74 || t == 90)
                Beat();

            if (t >= 100)
            {
                State = VisculaState.Phase2;
                Attack = 0;
                Timer = 0;
                lastAttack = 0;
                beatTimer = 0;
                Beat();
            }
        }

        private void Transition23Tick()
        {
            int t = Timer;

            if (t == 1 || t == 28 || t == 52 || t == 72 || t == 90 || t == 106 || t == 120 || t == 132 || t == 142 || t == 150)
                Beat();

            if (t == 150)
                Ring(16, 5f, Main.rand.NextFloat(MathHelper.TwoPi), 22f);

            if (t >= 185)
            {
                State = VisculaState.Phase3;
                Attack = 0;
                Timer = 0;
                lastAttack = 0;
                beatTimer = 0;
                Beat();
            }
        }
        //Coop stuff in there
        private void ClientVisuals()
        {
            if (Main.dedServ)
                return;

            int seen = (int)NPC.ai[3];
            if (lastBeatSeen == -1)
                lastBeatSeen = seen;
            else if (seen != lastBeatSeen)
            {
                lastBeatSeen = seen;
                OnBeat();
            }

            if (lastState == -1)
                lastState = State;
            else if (State != lastState)
            {
                lastState = State;
                OnStateChanged();
            }

            if (appear < 0f)
                appear = State == VisculaState.Dormant ? 0f : 1f;
            else if (State == VisculaState.Dormant)
                appear = Math.Max(0f, appear - 0.03f);
            else if (appear < 1f)
            {
                appear = Math.Min(1f, appear + 0.0125f);
                if ((int)(appear / 0.0125f) % 14 == 0)
                    Heartfx.SparkRing(NPC.Center, 14, 3f + 4f * appear, 14f, Heartfx.Tint(VisculaState.Phase1), 24);
            }

            if (secondBeat > 0 && --secondBeat == 0)
            {
                pulse = Math.Max(pulse, 0.6f);
                PlayThump(0.55f, 0.15f);
            }

            pulse = Math.Max(0f, pulse - 0.06f);
            if (wirePulse <= 1.2f)
                wirePulse += 0.045f;

            if (State == VisculaState.Dying)
                DyingVisuals();
            else
                deathBurst = false;

            Color tint = Heartfx.Tint(State);
            Lighting.AddLight(NPC.Center, tint.ToVector3() * (0.25f + 0.9f * pulse) * (0.3f + 0.7f * Appear));
        }

        private void DyingVisuals()
        {
            float k = MathHelper.Clamp(Timer / (float)DeathLength, 0f, 1f);
            float dist = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);
            float prox = 1f - MathHelper.Clamp(dist / 1600f, 0f, 1f);

            if (Timer % 3 == 0)
            {
                PlayerScreen ps = Main.LocalPlayer.PlayerScreen();
                ps.ScreenShakeIntensity = Math.Max(ps.ScreenShakeIntensity, (2f + 7f * k) * prox);
            }

            if (Main.rand.NextFloat() < 0.35f + 0.6f * k)
            {
                Vector2 p = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 dir = (p - NPC.Center).SafeNormalize(Vector2.UnitY);
                Color c = Main.rand.NextBool() ? Heartfx.Tint(VisculaState.Phase3) : Heartfx.Blood;
                Heartfx.Spark(p, dir * Main.rand.NextFloat(2f, 6f + 6f * k), 12f + 8f * k, c, 22);
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f + 4f * k, 2f + 4f * k);
                Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), DustID.Blood, vel, 100, default, 1.1f + k * 0.8f);
            }

            if (Timer >= DeathLength - 2 && !deathBurst)
            {
                deathBurst = true;

                Heartfx.Flash(NPC.Center, 520f, Color.White, 26);
                Heartfx.Flash(NPC.Center, 340f, new Color(190, 160, 255, 240), 30);
                Heartfx.SparkRing(NPC.Center, 56, 10f, 22f, Heartfx.Tint(VisculaState.Phase3), 34);
                Heartfx.SparkRing(NPC.Center, 40, 5f, 26f, Heartfx.Blood, 40);
                Heartfx.Burst(NPC.Center, 70, 12f, 26f, Heartfx.Violet);

                for (int i = 0; i < 90; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Blood, vel, 100, default, Main.rand.NextFloat(1.4f, 2.4f));
                    d.noGravity = Main.rand.NextBool(3);
                }

                PlayerScreen ps = Main.LocalPlayer.PlayerScreen();
                ps.ScreenShakeIntensity = Math.Max(ps.ScreenShakeIntensity, 16f * Math.Max(prox, 0.3f));
                if (dist < 2400f)
                    ps.Rumble(60, 10);

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1f, Pitch = -0.4f }, NPC.Center);
                PlayThump(1.4f, -0.3f);
            }
        }

        private void PlayThump(float volume, float pitchOffset)
        {
            float pitch = State == VisculaState.Phase2 ? 0.1f : State == VisculaState.Phase3 ? 0.2f : State == VisculaState.Dying ? 0.25f : 0f;
            float vol = State == VisculaState.Dormant ? volume * 0.5f : volume;

            SoundEngine.PlaySound(new SoundStyle(HeartTextures.Heartbeat)
            {
                Volume = vol,
                Pitch = pitch + pitchOffset,
                PitchVariance = 0.04f,
                MaxInstances = 6
            }, NPC.Center);
        }

        private void OnBeat()
        {
            pulse = 1f;
            secondBeat = State == VisculaState.Phase3 || State == VisculaState.Dying ? 7 : 11;
            wirePulse = 0f;
            PlayThump(1f, 0f);

            float shake = State switch
            {
                VisculaState.Dormant => 0f,
                VisculaState.Phase1 => 1.6f,
                VisculaState.Phase2 => 2.6f,
                VisculaState.Phase3 => 3.8f,
                _ => 5f
            };

            if (shake > 0f)
            {
                float dist = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);
                float prox = 1f - MathHelper.Clamp(dist / 1600f, 0f, 1f);
                PlayerScreen ps = Main.LocalPlayer.PlayerScreen();
                ps.ScreenShakeIntensity = Math.Max(ps.ScreenShakeIntensity, shake * prox);
            }

            int count = State == VisculaState.Dormant ? 4 : State == VisculaState.Phase3 || State == VisculaState.Dying ? 12 : 8;
            Heartfx.Burst(NPC.Center, count, 4f, 14f, Heartfx.Tint(State));

            if (State == VisculaState.Dying)
                Heartfx.Flash(NPC.Center, 160f, new Color(235, 45, 90, 200), 14);
        }

        private void OnStateChanged()
        {
            float dist = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);
            if (dist > 2000f)
                return;

            PlayerScreen ps = Main.LocalPlayer.PlayerScreen();

            if (State == VisculaState.Phase1)
            {
                ps.Rumble(50, 6);
                Heartfx.SparkRing(NPC.Center, 40, 7f, 20f, Heartfx.Tint(VisculaState.Phase1), 30);
                Heartfx.Burst(NPC.Center, 40, 6f, 20f, Heartfx.Violet);
                SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.6f, Pitch = -0.5f }, NPC.Center);
            }
            else if (State == VisculaState.Trans12)
                ps.Rumble(40, 4);
            else if (State == VisculaState.Trans23)
                ps.Rumble(80, 5);
            else if (State == VisculaState.Dying)
                ps.Rumble(180, 4);
            else if (State == VisculaState.Phase3)
            {
                ps.Rumble(30, 8);
                Heartfx.Burst(NPC.Center, 40, 8f, 22f, Heartfx.Tint(VisculaState.Phase3));
            }
        }

        private void DrawWires(Color drawColor)
        {
            Texture2D tex = HeartTextures.WireTex;
            if (tex == null || anchors.Count == 0)
                return;

            float tension = State switch
            {
                VisculaState.Dormant => 0.16f,
                VisculaState.Phase1 => 0.16f,
                VisculaState.Trans12 => 0.10f,
                VisculaState.Phase2 => 0.10f,
                _ => 0.035f
            };

            float bright = Bright;
            float time = Main.GlobalTimeWrappedHourly;
            Vector2 half = tex.Size() / 2f;
            float spacing = Math.Max(tex.Width * 0.8f, 4f);

            for (int a = 0; a < anchors.Count; a++)
            {
                Vector2 end = anchors[a];
                Vector2 dir = (end - NPC.Center).SafeNormalize(Vector2.UnitY);
                Vector2 start = NPC.Center + dir * 24f;
                float len = Vector2.Distance(start, end);
                float sag = len * tension + (float)Math.Sin(time * 1.6f + a * 1.7f) * 2f - pulse * len * 0.02f;
                Vector2 ctrl = (start + end) / 2f + new Vector2(0f, sag);
                int steps = Math.Max((int)(len / spacing), 2);

                for (int s = 0; s <= steps; s++)
                {
                    float u = s / (float)steps;
                    Vector2 p = HeartMisc.Bezier(start, ctrl, end, u);
                    float rot = HeartMisc.BezierTangent(start, ctrl, end, u).ToRotation();
                    Color light = Heartfx.Dark(Color.Lerp(Lighting.GetColor(p.ToTileCoordinates()), Color.White, 0.2f), bright);

                    Main.EntitySpriteDraw(tex, p - Main.screenPosition, null, light, rot, half, 1f, SpriteEffects.None, 0);

                    if (wirePulse <= 1.2f)
                    {
                        float g = Math.Max(0f, 1f - Math.Abs(u - wirePulse) * 6f) * (0.3f + 0.7f * Appear);
                        if (g > 0.01f)
                            Main.EntitySpriteDraw(tex, p - Main.screenPosition, null, Heartfx.Alpha0(Heartfx.Tint(State)) * g, rot, half, 1.15f, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawWires(drawColor);

            bool dying = State == VisculaState.Dying;
            float k = dying ? MathHelper.Clamp(Timer / (float)DeathLength, 0f, 1f) : 0f;
            float bright = Bright;

            Texture2D tex = TextureAssets.Npc[Type].Value;
            Texture2D glow = HeartTextures.GlowTex;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            if (dying)
                pos += Main.rand.NextVector2Circular(1f, 1f) * (1f + 5f * k * k);

            float scale = NPC.scale * (1f + pulse * 0.05f * (1f + k));
            Color tint = Heartfx.Tint(State);

            spriteBatch.Draw(tex, pos, frame, Heartfx.Dark(Color.Lerp(drawColor, Color.White, 0.25f), bright), 0f, origin, scale, SpriteEffects.None, 0f);

            if (glow != null)
            {
                bool dormant = State == VisculaState.Dormant;
                float baseGlow = dormant ? 0.04f : 0.2f;
                float amp = dormant ? 0.35f : 1f;
                float g = baseGlow + pulse * amp * (1f - baseGlow);

                spriteBatch.Draw(glow, pos, frame, Color.White * g, 0f, origin, scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, pos, frame, Heartfx.Alpha0(tint) * (pulse * 0.6f * amp), 0f, origin, scale * (1f + pulse * 0.08f), SpriteEffects.None, 0f);

                if (State != VisculaState.Dormant && Appear < 1f)
                {
                    float flash = (float)Math.Sin(Appear * MathHelper.Pi);
                    spriteBatch.Draw(glow, pos, frame, Heartfx.Alpha0(Heartfx.Violet) * (flash * 0.8f), 0f, origin, scale * (1f + flash * 0.12f), SpriteEffects.None, 0f);
                }

                if (dying)
                    spriteBatch.Draw(glow, pos, frame, Heartfx.Alpha0(Heartfx.Blood) * (k * 0.7f * (0.6f + pulse * 0.4f)), 0f, origin, scale * (1f + k * 0.1f), SpriteEffects.None, 0f);
            }

            if (dying)
            {
                Vector2 center = NPC.Center;
                float flick = 0.5f + pulse * 0.5f;

                for (int i = 0; i < 12; i++)
                {
                    float ang = i * MathHelper.TwoPi / 12f + i * 0.37f;
                    float lenK = 0.6f + 0.4f * (float)Math.Sin(i * 2.1f + 1f);
                    float len = 50f + 280f * k * lenK;

                    HeartMisc.DrawRay(center, ang, len, 3f + 7f * k, Heartfx.Alpha0(Heartfx.Blood) * (0.55f * k * flick));
                    HeartMisc.DrawRay(center, ang, len * 0.8f, 1.4f + 2f * k, Heartfx.Alpha0(Heartfx.Violet) * (0.8f * k * flick));
                }
            }

            return false;
        }
    }
}