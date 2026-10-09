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
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Systems;
using Waybound.Content.Items.Accessories.Shields;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Bosses;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Items.Vanity.BossMasks;
using Waybound.Content.Items.Weapons.Melee.Flails;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Helpers;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
    [AutoloadBossHead]
    public class DeepStoneGolem : ModNPC
    {
        private const int WS = 0; //Walk Start 
        private const int WE = 4; //Walk End
        private const int GS = 5; //Guard Start
        private const int GE = 6; // End lol
        private const int SC = 7; // Spike Charg
        private const int SF = 8; // Spike fire 
        private const int TPS = 9; // tp start
        private const int TPE = 12; //tp end
        private bool hasBounced;
        private bool canDoubleLunge;
        private float previousVelocityX;
        private const int FrameWidth = 56;
        private const int FrameHeight = 74;

        private const int LW = 20; // Windup
        private const int TFT = 30; // TeleportFallback
        private static readonly Color CrystalColor = new Color(255, 168, 135, 0);

        private static int LungeDuration => Main.masterMode ? 42 : (Main.expertMode ? 38 : 35);
        private static float LungeSpeed => Main.masterMode ? 13f : (Main.expertMode ? 10.5f : 8.5f);

        private enum GolemState
        {
            Walking,
            Guarding,
            SpikeCharge,
            SpikeFire,
            Teleporting
        }

        private GolemState state = GolemState.Walking;
        private int stateTimer;
        private bool frameInitialized;
        private bool didTeleport;
        private Vector2 teleportTarget;

        private int baseDefense;

        private bool lungeChosen;
        private float lungeDirection;
        private int lungeTimer;

        private bool crystalBallChosen;
        private int crystalBallTimer;

        private static Asset<Texture2D> bodyTexture;
        private static Asset<Texture2D> glowTexture;

        private int CurrentFrame
        {
            get => NPC.frame.Y / FrameHeight;
            set
            {
                NPC.frame.Y = value * FrameHeight;
                NPC.frame.X = 0;
            }
        }

        public override void Load()
        {
            if (Main.dedServ) return;
            bodyTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolem");
            glowTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolem_Glow");
        }

        public override void Unload()
        {
            bodyTexture = null;
            glowTexture = null;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 13;
            NPCID.Sets.TrailCacheLength[NPC.type] = 12;
            NPCID.Sets.TrailingMode[NPC.type] = 2;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                CustomTexturePath = "Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolem_Bestiary",
                Position = new Vector2(0f, 0),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 1400;
            NPC.defense = 8;
            NPC.damage = 21;
            NPC.width = FrameWidth;
            NPC.height = FrameHeight;
            NPC.value = 750;
            NPC.knockBackResist = 0f;
            NPC.DeathSound = SoundID.Tink;
            NPC.HitSound = SoundID.Tink;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.boss = true;
            NPC.dontTakeDamage = false;
            NPC.netAlways = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<Common.Biome.CrystalDepthsBiome>().Type };
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrystalDepths");
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.825f * balance);
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = 600;
            stateTimer = 0;
            CurrentFrame = WS;
            frameInitialized = false;
            baseDefense = NPC.defense;
            base.OnSpawn(source);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)state);
            writer.Write((short)stateTimer);
            writer.Write((short)lungeTimer);
            writer.Write((short)crystalBallTimer);
            writer.Write(lungeDirection);
            writer.Write(lungeChosen);
            writer.Write(crystalBallChosen);
            writer.Write(didTeleport);
            writer.WriteVector2(teleportTarget);
            writer.Write(frameInitialized);
            writer.Write(hasBounced);
            writer.Write(canDoubleLunge);
            writer.Write(previousVelocityX);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            state = (GolemState)reader.ReadByte();
            stateTimer = reader.ReadInt16();
            lungeTimer = reader.ReadInt16();
            crystalBallTimer = reader.ReadInt16();
            lungeDirection = reader.ReadSingle();
            lungeChosen = reader.ReadBoolean();
            crystalBallChosen = reader.ReadBoolean();
            didTeleport = reader.ReadBoolean();
            teleportTarget = reader.ReadVector2();
            frameInitialized = reader.ReadBoolean();
            hasBounced = reader.ReadBoolean();
            canDoubleLunge = reader.ReadBoolean();
            previousVelocityX = reader.ReadSingle();
        }

        // =====================================================================
        //  Визуальные хелперы
        // =====================================================================

        private static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Ровное кольцо кристальных частиц (ударная волна).</summary>
        private void SpawnRing(Vector2 center, int count, float speed, float size)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float angle = offset + MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.9f, 1.1f);

                ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                    center.ToNumerics(),
                    vel.ToNumerics(),
                    angle,
                    new SystemVector2(size, size * 0.8f),
                    CrystalColor * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(18, 26)
                ));
            }
        }

        /// <summary>Короткая яркая вспышка в точке.</summary>
        private static void SpawnFlash(Vector2 center, float size)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.FlashBuffer == null)
                return;

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                center.ToNumerics(),
                SystemVector2.Zero,
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SystemVector2(size),
                new Color(255, 190, 160, 220),
                10
            ));
        }


        public override void AI()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.TargetClosest(true);
            }

            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                int closest = -1;
                float closestDist = float.MaxValue;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (!p.active || p.dead) continue;
                    float d = Vector2.DistanceSquared(p.Center, NPC.Center);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closest = i;
                    }
                }

                if (closest == -1)
                {
                    SpawnBurst(NPC.Center, 60, 5f, 1.4f);
                    NPC.netUpdate = true;
                    NPC.active = false;
                    return;
                }

                NPC.target = closest;
                player = Main.player[closest];
            }

            Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);

            if (!frameInitialized)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.Center = player.Center + new Vector2(0, -180);
                    CurrentFrame = WS;
                    frameInitialized = true;
                    NPC.netUpdate = true;
                }
                return;
            }
            if (state != GolemState.Teleporting && NPC.alpha > 0)
                NPC.alpha = Math.Max(0, NPC.alpha - 15);

            stateTimer++;

            switch (state)
            {
                case GolemState.Walking:
                    UpdateWalking(player);
                    break;
                case GolemState.Guarding:
                    UpdateGuarding(player);
                    break;
                case GolemState.SpikeCharge:
                    UpdateSpikeCharge(player);
                    break;
                case GolemState.SpikeFire:
                    UpdateSpikeFire(player);
                    break;
                case GolemState.Teleporting:
                    UpdateTeleporting(player);
                    break;
                    //the same as themis but not the same am i tripping
            }

            if (Main.expertMode && state == GolemState.Walking)
            {
                ExpertAttack(player);
            }

            if (state == GolemState.Walking && NPC.life <= NPC.lifeMax * 0.5f)
            {
                UpdateCrystalBallAttack(player);
            }

            if (state == GolemState.Walking && stateTimer > 120 && Main.rand.NextBool(180))
            {
                EnterGuarding();
            }

            if (state == GolemState.Walking && (NPC.ai[1] >= 600 || Vector2.Distance(NPC.Center, player.Center) > 450) && Vector2.Distance(NPC.Center, player.Center) < 900)
            {
                EnterTeleporting(player);
            }

            if (NPC.ai[1] < 600)
            {
                NPC.ai[1]++;
            }
        }

        private void ChangeState(GolemState newState)
        {
            state = newState;
            stateTimer = 0;
            NPC.netUpdate = true;
        }

        private void SpawnBurst(Vector2 center, int count, float speed, float sizeMul)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, speed);
                float size = Main.rand.NextFloat(18f, 32f) * sizeMul;

                ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                    center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                    CrystalColor * Main.rand.NextFloat(0.85f, 1.2f),
                    Main.rand.Next(18, 32)
                ));
            }
        }

        private void SpawnTrail(Vector2 center, Vector2 velocity, int count, float sizeMul)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = -velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float size = Main.rand.NextFloat(16f, 28f) * sizeMul;

                ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                    (center + Main.rand.NextVector2Circular(12f, 12f)).ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                    CrystalColor * Main.rand.NextFloat(0.85f, 1.2f),
                    Main.rand.Next(16, 28)
                ));
            }
        }

        private void UpdateWalking(Player player)
        {
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.dontTakeDamage = false;
            NPC.defense = baseDefense;

            Helpers.BaseHelper.FighterAI(NPC, 0.05f, 1.5f, 10, 0.15f, false);

            if (NPC.frameCounter > 6)
            {
                int f = CurrentFrame;
                if (!NPC.HasTileOnSide(4, new Vector2(0, 1f)))
                {
                    CurrentFrame = TPS;
                }
                else
                {
                    if (f < WS || f > WE)
                        CurrentFrame = WS;
                    else
                    {
                        f++;
                        if (f > WE)
                            f = WS;
                        CurrentFrame = f;
                    }
                }
                NPC.frameCounter = 0;
            }
        }

        private void EnterGuarding()
        {
            ChangeState(GolemState.Guarding);
            CurrentFrame = GS;
            NPC.velocity.X = 0f;
            NPC.velocity.Y = 0f;
            NPC.frameCounter = 0;
            NPC.defense = baseDefense * 2;
            lungeChosen = false;
            hasBounced = false;
            canDoubleLunge = false;
            SpawnRing(NPC.Center, 14, -2.5f, 18f);
        }

        private void UpdateGuarding(Player player)
        {
            NPC.velocity.X = 0f;
            NPC.velocity.Y += 0.15f;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.defense = baseDefense * 2;

            if (NPC.frameCounter > 8)
            {
                int f = CurrentFrame + 1;
                if (f > GE)
                    f = GE;
                CurrentFrame = f;
                NPC.frameCounter = 0;
            }

            if (stateTimer % 6 == 0)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                SpawnTrail(NPC.Center + dir * 55f, dir * 3f, 1, 0.8f);
            }

            if (stateTimer > 90)
            {
                ChangeState(GolemState.SpikeCharge);
                CurrentFrame = SC;
                NPC.frameCounter = 0;
                lungeChosen = false;
                lungeTimer = 0;
                lungeDirection = 0f;
            }
        }

        private void UpdateSpikeCharge(Player player)
        {
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.defense = baseDefense * 2;

            if (!lungeChosen)
            {
                lungeChosen = true;
                lungeDirection = player.Center.X > NPC.Center.X ? 1f : -1f;
                lungeTimer = 0;
                hasBounced = false;
                NPC.netUpdate = true;
            }

            lungeTimer++;

            float lungeSpeed = LungeSpeed;
            int lungeDuration = LungeDuration;

            if (lungeTimer < LW)
            {
                NPC.velocity.X *= 0.85f;
                NPC.velocity.Y += 0.2f;

                if (Main.rand.NextBool(2))
                    SpawnTrail(NPC.Center, new Vector2(lungeDirection * 2f, 0f), 1, 0.9f);
            }
            else if (lungeTimer < lungeDuration)
            {
                if (lungeTimer == LW)
                    NPC.netUpdate = true;

                NPC.velocity.X = lungeDirection * lungeSpeed;
                NPC.velocity.Y *= 0.9f;

                bool justHitWall = false;

                if (!hasBounced && Math.Abs(NPC.velocity.X) < 1.5f && Math.Abs(previousVelocityX) > lungeSpeed * 0.7f)
                {
                    justHitWall = true;
                }

                if (!hasBounced && Collision.SolidCollision(
                    NPC.position + new Vector2(NPC.velocity.X > 0 ? NPC.width : -8, 8),
                    8, NPC.height - 16))
                {
                    justHitWall = true;
                }

                if (justHitWall)
                {
                    hasBounced = true;
                    lungeDirection *= -1f;
                    NPC.velocity.X = lungeDirection * lungeSpeed * 0.9f;

                    if (Main.netMode != NetmodeID.Server)
                        Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>().TriggerShake(18, 1.6f);

                    SoundEngine.PlaySound(SoundID.Tink with { Volume = 1.2f, Pitch = -0.35f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.9f, Pitch = -0.2f }, NPC.Center);

                    SpawnCeilingStalactites(NPC.Center, Main.masterMode ? 9 : (Main.expertMode ? 7 : 5));

                    SpawnBurst(NPC.Center, 22, 5f, 1.3f);

                    Vector2 wallPos = NPC.Center + new Vector2(-lungeDirection * NPC.width * 0.5f, 0f);
                    SpawnFlash(wallPos, 70f);
                    SpawnRing(wallPos, 16, 6f, 22f);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 v = new Vector2(lungeDirection * Main.rand.NextFloat(1f, 5f), Main.rand.NextFloat(-4f, 1f));
                            Dust d = Dust.NewDustPerfect(wallPos, DustID.Stone, v, 0, default, Main.rand.NextFloat(1f, 1.8f));
                            d.noGravity = false;
                        }
                    }

                    float doubleChance = Main.masterMode ? 0.55f : (Main.expertMode ? 0.4f : 0.25f);
                    if (Main.rand.NextFloat() < doubleChance)
                    {
                        canDoubleLunge = true;
                        lungeTimer = 18;
                        hasBounced = false;
                    }

                    NPC.netUpdate = true;
                }

                int count = Main.masterMode ? 4 : (Main.expertMode ? 3 : 2);
                SpawnTrail(NPC.Center, NPC.velocity, count, Main.masterMode ? 1.2f : 1f);
            }
            else
            {
                NPC.velocity.X *= 0.85f;
                NPC.velocity.Y += 0.15f;
            }

            previousVelocityX = NPC.velocity.X;

            Lighting.AddLight(NPC.Center, 1.2f, 0.5f, 0.4f);

            if (stateTimer % 4 == 0)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                SpawnTrail(NPC.Center + speed * 30f, speed * 2f, 1, 0.8f);
            }

            if (stateTimer > 75)
            {
                ChangeState(GolemState.SpikeFire);
                CurrentFrame = SF;
                NPC.frameCounter = 0;
                FireSpikes(player);
            }
        }

        private void UpdateSpikeFire(Player player)
        {
            NPC.velocity.X *= 0.9f;
            NPC.velocity.Y += 0.15f;
            NPC.defense = baseDefense * 2;

            if (stateTimer < 30)
            {
                Lighting.AddLight(NPC.Center, 1.4f, 0.7f, 0.5f);

                if (stateTimer % 3 == 0)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(3f, 3f);
                    SpawnTrail(NPC.Center + speed * 40f, speed * 2f, 1, 1f);
                }
            }

            if (stateTimer > 50)
            {
                ChangeState(GolemState.Walking);
                CurrentFrame = WS;
                NPC.frameCounter = 0;
                NPC.ai[1] = 0;
                NPC.defense = baseDefense;
            }
        }

        private void SpawnCeilingStalactites(Vector2 origin, int count)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int spawned = 0;
            int attempts = 0;

            while (spawned < count && attempts < count * 12)
            {
                attempts++;

                int tileX = Math.Clamp((int)(origin.X / 16f) + Main.rand.Next(-18, 19), 10, Main.maxTilesX - 10);
                int tileY = Math.Clamp((int)(origin.Y / 16f) - Main.rand.Next(4, 28), 10, Main.maxTilesY - 10);

                for (int y = tileY; y > tileY - 40 && y > 10; y--)
                {
                    if (WorldGen.SolidTile(tileX, y) && !WorldGen.SolidTile(tileX, y + 1))
                    {
                        Vector2 spawnPos = new Vector2(tileX * 16 + 8, (y + 1) * 16 + 4);

                        int type = ModContent.ProjectileType<DeepStalactite>();
                        int damage = Main.masterMode ? 22 : (Main.expertMode ? 18 : 14);

                        int p = Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            spawnPos,
                            new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(0.8f, 2.2f)),
                            type,
                            damage,
                            2.5f,
                            Main.myPlayer);

                        Main.projectile[p].friendly = false;
                        Main.projectile[p].hostile = true;
                        Main.projectile[p].tileCollide = true;

                        for (int i = 0; i < 5; i++)
                        {
                            Dust d = Dust.NewDustPerfect(spawnPos + Main.rand.NextVector2Circular(8f, 4f), DustID.Stone,
                                new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 2f)), 0, default, Main.rand.NextFloat(1f, 1.6f));
                            d.noGravity = false;
                        }

                        spawned++;
                        break;
                    }
                }
            }
        }

        private void FireSpikes(Player player)
        {
            SpawnBurst(NPC.Center, 26, 5f, 1.3f);
            SpawnFlash(NPC.Center, 90f);
            SpawnRing(NPC.Center, 20, 7f, 24f);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int spikeCount = Main.masterMode ? 10 : (Main.expertMode ? 9 : 7);
            float halfSpread = MathHelper.Pi * 0.55f;
            float baseAngle = (player.Center - NPC.Center).ToRotation();

            float speedMin = 4.5f;
            float speedMax = 6.5f;

            for (int i = 0; i < spikeCount; i++)
            {
                float angle = baseAngle + MathHelper.Lerp(-halfSpread, halfSpread, i / (float)(spikeCount - 1));
                Vector2 dir = angle.ToRotationVector2() * Main.rand.NextFloat(speedMin, speedMax);
                int type = ModContent.ProjectileType<DeepGolemCrystalProj>();
                int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir, type, 13, 3f, Main.myPlayer);
                Main.projectile[proj].tileCollide = true;
                Main.projectile[proj].timeLeft = 240;
                Main.projectile[proj].friendly = false;
                Main.projectile[proj].hostile = true;
            }

            bool doubleShot = Main.masterMode ? Main.rand.NextBool(3) : Main.rand.NextBool(2);
            if (doubleShot)
            {
                stateTimer = 20;
                CurrentFrame = SC;
                NPC.frameCounter = 0;
            }
        }

        private void EnterTeleporting(Player player)
        {
            ChangeState(GolemState.Teleporting);
            CurrentFrame = TPS;
            NPC.frameCounter = 0;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.dontTakeDamage = true;
            NPC.velocity = Vector2.Zero;
            didTeleport = false;

            teleportTarget = player.Center + new Vector2(0, -180);
            NPC.netUpdate = true;
        }

        private void UpdateTeleporting(Player player)
        {
            NPC.velocity = Vector2.Zero;
            NPC.defense = baseDefense;

            int particleCount = Main.masterMode ? 4 : (Main.expertMode ? 3 : 2);
            SpawnTrail(NPC.Center, new Vector2(0f, -1f), particleCount, 1.1f);

            if (!didTeleport)
                NPC.alpha = (int)(200f * MathHelper.Clamp(stateTimer / (float)TFT, 0f, 1f));
            else
                NPC.alpha = (int)(200f * (1f - MathHelper.Clamp(stateTimer / 20f, 0f, 1f)));

            if (NPC.frameCounter > 7)
            {
                int f = CurrentFrame;
                if (f < TPE)
                {
                    f++;
                    CurrentFrame = f;
                }
                NPC.frameCounter = 0;
            }
            bool teleportReady = CurrentFrame >= TPE || stateTimer >= TFT;

            if (!didTeleport && teleportReady)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SpawnBurst(NPC.Center, Main.masterMode ? 60 : 45, 5f, 1.3f);
                    SpawnFlash(NPC.Center, 110f);
                    SpawnRing(NPC.Center, 18, 7f, 24f);

                    NPC.Center = teleportTarget;

                    SpawnBurst(NPC.Center, Main.masterMode ? 60 : 45, 5f, 1.3f);
                    SpawnFlash(NPC.Center, 110f);
                    SpawnRing(NPC.Center, 18, 7f, 24f);

                    NPC.netUpdate = true;
                    didTeleport = true;
                    stateTimer = 0;
                }
            }

            if (didTeleport && stateTimer > 20)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    ChangeState(GolemState.Walking);
                    CurrentFrame = WS;
                    NPC.frameCounter = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                }
            }
        }

        private void UpdateCrystalBallAttack(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!crystalBallChosen)
            {
                crystalBallChosen = true;
                crystalBallTimer = Main.rand.Next(300, 540);
            }

            crystalBallTimer--;

            if (crystalBallTimer <= 0)
            {
                crystalBallTimer = Main.rand.Next(360, 600);

                Vector2 spawnPos = NPC.Center + new Vector2(0f, -NPC.height * 0.5f - 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -12f);
                int type = ModContent.ProjectileType<DeepCrystalBall>();
                int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, vel, type, 18, 1f, Main.myPlayer);
                Main.projectile[p].friendly = false;
                Main.projectile[p].hostile = true;

                SpawnBurst(spawnPos, 24, 4f, 1.3f);
                SpawnFlash(spawnPos, 60f);
            }
        }

        private void ExpertAttack(Player player)
        {
            NPC.localAI[0]++;

            if (NPC.localAI[0] > 180)
            {
                if (state == GolemState.Walking && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(300, 350), 0).RotatedByRandom(MathHelper.ToRadians(360));
                    Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.UnitX) * 5f;
                    int type = ModContent.ProjectileType<DeepStoneGolemProj>();
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, type, 18, 3, Main.myPlayer);
                    Main.projectile[proj].tileCollide = false;
                    Main.projectile[proj].friendly = false;
                    Main.projectile[proj].hostile = true;

                    SpawnFlash(pos, 50f);
                }
                NPC.localAI[0] = 0;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter++;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (NPC.life > 0)
            {
                int shards = state == GolemState.Walking ? 3 : 2;
                SpawnBurst(NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.4f, NPC.height * 0.4f), shards, 3f, 0.6f);
            }
            else
            {
                SpawnBurst(NPC.Center, 70, 8f, 1.6f);
                SpawnFlash(NPC.Center, 160f);
                SpawnRing(NPC.Center, 28, 9f, 30f);
                SpawnRing(NPC.Center, 18, 5f, 22f);
                Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>().TriggerShake(30, 2.5f);
            }
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownedBossSystem.DownedDeepStoneGolem, -1);
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = bodyTexture.Value;
            Texture2D textureG = glowTexture.Value;
            Vector2 drawOrigin = new Vector2(FrameWidth * 0.5f, FrameHeight * 0.5f);
            Vector2 offset = new Vector2(-10, 2);
            Vector2 pos = (NPC.position - Main.screenPosition) + drawOrigin + offset;

            bool lunging = state == GolemState.SpikeCharge && lungeChosen && lungeTimer >= LW && lungeTimer < LungeDuration;

            // it should've been better
            BeginAdditive(spriteBatch);

            if (lunging)
            {
                for (int k = 1; k < NPC.oldPos.Length; k++)
                {
                    float t = (NPC.oldPos[k].Length() == 0f) ? 0f : (NPC.oldPos.Length - k) / (float)NPC.oldPos.Length;
                    Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + offset;
                    Color color = NPC.GetAlpha(CrystalColor) * 0.55f * t * t;
                    Main.EntitySpriteDraw(textureG, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale * (0.9f + t * 0.1f), effects, 0);
                }
            }

            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + offset;
                Color color = NPC.GetAlpha(CrystalColor) * 0.65f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(textureG, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.05f, effects, 0);
            }

            EndAdditive(spriteBatch);
            Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            float strength = 0.35f + 0.12f * pulse;

            if (NPC.life <= NPC.lifeMax * 0.5f)
                strength += 0.15f;

            if (state == GolemState.Guarding)
                strength += 0.2f * MathHelper.Clamp(stateTimer / 20f, 0f, 1f);
            else if (state == GolemState.SpikeCharge)
                strength += 0.25f * (lungeTimer < LW ? lungeTimer / (float)LW : 1f);
            else if (state == GolemState.SpikeFire && stateTimer < 30)
                strength += 0.45f * (1f - stateTimer / 30f);

            BeginAdditive(spriteBatch);
            Main.EntitySpriteDraw(textureG, pos, NPC.frame, NPC.GetAlpha(CrystalColor) * strength, NPC.rotation, drawOrigin, NPC.scale, effects, 0);

            if (state == GolemState.Guarding || (state == GolemState.SpikeCharge && lungeTimer < LW))
            {
                float halo = state == GolemState.Guarding
                    ? MathHelper.Clamp(stateTimer / 30f, 0f, 1f)
                    : lungeTimer / (float)LW;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 o = new Vector2(2.5f, 0f).RotatedBy(MathHelper.PiOver2 * i + Main.GlobalTimeWrappedHourly * 2f);
                    Main.EntitySpriteDraw(textureG, pos + o, NPC.frame, NPC.GetAlpha(CrystalColor) * (0.3f * halo), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
                }
            }
            EndAdditive(spriteBatch);

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D px = TextureAssets.MagicPixel.Value;
            Rectangle src = new Rectangle(0, 0, 1, 1);

            if (state == GolemState.SpikeCharge && lungeChosen && lungeTimer < LW)
            {
                float t = lungeTimer / (float)LW;
                Vector2 start = new Vector2(NPC.Center.X + lungeDirection * NPC.width * 0.5f, NPC.Bottom.Y - 8f);
                int segs = 10;
                float segLen = 24f;
                Vector2 origin = new Vector2(lungeDirection > 0f ? 0f : 1f, 0.5f);

                BeginAdditive(spriteBatch);
                for (int s = 0; s < segs; s++)
                {
                    float f = 1f - s / (float)segs;
                    Color c = CrystalColor * (0.5f * t * f);
                    Vector2 p = start + new Vector2(lungeDirection * segLen * s, 0f) - Main.screenPosition;
                    spriteBatch.Draw(px, p, src, c, 0f, origin, new Vector2(segLen, 3f + 6f * f * (1f - t)), SpriteEffects.None, 0f);
                }
                EndAdditive(spriteBatch);
            }

            if (state == GolemState.Teleporting && !didTeleport && teleportTarget != Vector2.Zero)
            {
                float t = MathHelper.Clamp(stateTimer / (float)TFT, 0f, 1f);
                float radius = MathHelper.Lerp(110f, 24f, t);
                int points = 28;

                BeginAdditive(spriteBatch);
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + Main.GlobalTimeWrappedHourly * 2f;
                    Vector2 p = teleportTarget + angle.ToRotationVector2() * radius - Main.screenPosition;
                    Color c = CrystalColor * (0.2f + 0.5f * t);
                    spriteBatch.Draw(px, p, src, c, angle + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(3f, 10f), SpriteEffects.None, 0f);
                }
                EndAdditive(spriteBatch);

                Lighting.AddLight(teleportTarget, 0.6f * t, 0.3f * t, 0.25f * t);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            int DeepStone = ModContent.ItemType<DeepStoneItem>();
            var DeepStoneParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 5,
                MaximumStackPerChunkBase = 10,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            npcLoot.Add(new DropOneByOne(DeepStone, DeepStoneParameters));

            int DeepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
            var DeepCrystalParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 2,
                MinimumStackPerChunkBase = 1,
                MaximumStackPerChunkBase = 5,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            npcLoot.Add(new DropOneByOne(DeepCrystal, DeepCrystalParameters));

            int core = ModContent.ItemType<DeepGolemCore>();
            var coreParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 1,
                MaximumStackPerChunkBase = 1,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            npcLoot.Add(new DropOneByOne(core, coreParameters));

            int DeepShield = ModContent.ItemType<DeepStoneShield>();
            var DeepShieldParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 3,
                MinimumStackPerChunkBase = 1,
                MaximumStackPerChunkBase = 1,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            npcLoot.Add(new DropOneByOne(DeepShield, DeepShieldParameters));

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeepStoneGolemMask>(), 7));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeepStoneGolemTrophyI>(), 10));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<DeepStoneGolemRelicI>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<DeepFlail>(), 10));
        }
    }
}