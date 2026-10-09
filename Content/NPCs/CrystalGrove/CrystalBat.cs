using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.CrystalGrove
{
    public class CrystalBat : ModNPC
    {
        private const int StateIdle = 0;
        private const int StateDashing = 1;
        private const int StateTeleporting = 2;

        private const int DashWindup = 15;
        private const int DashHold = 50;
        private const int DashEnd = 90;

        private int State
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private int StateTimer
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private int AttackCounter
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float DashDirection
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        private int FireTimer
        {
            get => (int)NPC.localAI[0];
            set => NPC.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailCacheLength[NPC.type] = 12;
            NPCID.Sets.TrailingMode[NPC.type] = 2;

            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 14;
            NPC.lifeMax = 100;
            NPC.defense = 10;
            NPC.damage = 30;
            NPC.width = 54;
            NPC.height = 40;
            NPC.value = 300;
            NPC.DeathSound = SoundID.Item27;
            NPC.HitSound = SoundID.Item27;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.knockBackResist = 0;
            AnimationType = NPCID.CaveBat;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 240);
            }
        }

        private void SpawnCrystal(Vector2 pos, Vector2 vel, float sizeMin, float sizeMax, float lifeMul = 1f)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            float size = Main.rand.NextFloat(sizeMin, sizeMax);

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                new Color(255, 168, 135, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                (int)(Main.rand.Next(14, 24) * lifeMul)
            ));
        }

        private void Burst(Vector2 center, int count, float speed)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, speed);
                SpawnCrystal(center, vel, 16f, 28f);
            }
        }

        private void Trail(Vector2 center, Vector2 velocity, int count)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = -velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                SpawnCrystal(center + Main.rand.NextVector2Circular(8f, 8f), vel, 12f, 22f);
            }
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
                NPC.velocity.Y -= 0.1f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }

            Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                StateTimer++;

                switch (State)
                {
                    case StateIdle:
                        ServerUpdateIdle(player);
                        break;
                    case StateDashing:
                        ServerUpdateDashing(player);
                        break;
                    case StateTeleporting:
                        ServerUpdateTeleporting(player);
                        break;
                }
            }

            ClientVisualUpdate(player);
        }

        private void ServerUpdateIdle(Player player)
        {
            if (AttackCounter > 0)
                AttackCounter--;

            if (AttackCounter > 0)
                return;

            float distX = Math.Abs(player.Center.X - NPC.Center.X);
            float distY = Math.Abs(player.Center.Y - NPC.Center.Y);
            float totalDist = Vector2.Distance(NPC.Center, player.Center);

            if (totalDist > 700 && Main.rand.NextBool(4))
            {
                EnterTeleporting(player);
                return;
            }

            if (totalDist < 500 && distY < 140)
            {
                EnterDashing(player);
                return;
            }

            if (totalDist > 900 && Main.rand.NextBool(3))
            {
                EnterTeleporting(player);
                return;
            }

            EnterDashing(player);
        }

        private void EnterDashing(Player player)
        {
            State = StateDashing;
            StateTimer = 0;
            FireTimer = 0;
            DashDirection = player.Center.X > NPC.Center.X ? 1f : -1f;

            NPC.velocity = new Vector2(DashDirection * 2f, -5f);

            if (Main.netMode != NetmodeID.Server)
                Burst(NPC.Center, 12, 4f);

            NPC.netUpdate = true;
        }

        private void ServerUpdateDashing(Player player)
        {
            if (StateTimer < DashWindup)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, DashDirection * 3f, 0.15f);
                NPC.velocity.Y *= 0.92f;
            }
            else if (StateTimer < DashHold)
            {
                if (StateTimer == DashWindup)
                    NPC.netUpdate = true;

                NPC.velocity.X = DashDirection * 9f;
                NPC.velocity.Y *= 0.9f;

                FireTimer++;
                if (FireTimer >= 10)
                {
                    FireTimer = 0;
                    NPC.netUpdate = true;

                    Vector2 aim = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    aim = aim.RotatedByRandom(0.18f);
                    Vector2 dir = aim * Main.rand.NextFloat(4.5f, 6f);

                    int type = ModContent.ProjectileType<DeepCrystalProj>();
                    int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir, type, 12, 2f, Main.myPlayer);
                    Main.projectile[p].friendly = false;
                    Main.projectile[p].hostile = true;
                    Main.projectile[p].tileCollide = false;
                    Main.projectile[p].timeLeft = 180;
                    Main.projectile[p].scale = 0.9f;
                }
            }
            else
            {
                NPC.velocity.X *= 0.85f;
                NPC.velocity.Y += 0.15f;

                if (StateTimer > DashEnd)
                {
                    State = StateIdle;
                    StateTimer = 0;
                    AttackCounter = Main.rand.Next(60, 110);
                    NPC.netUpdate = true;
                }
            }
        }

        private void EnterTeleporting(Player player)
        {
            State = StateTeleporting;
            StateTimer = 0;

            if (Main.netMode != NetmodeID.Server)
                Burst(NPC.Center, 20, 4.5f);

            Vector2 targetPos;
            float side = Main.rand.NextBool() ? 1f : -1f;
            targetPos = player.Center + new Vector2(side * Main.rand.NextFloat(180f, 260f), -Main.rand.NextFloat(80f, 160f));

            if (Collision.SolidCollision(targetPos - NPC.Size * 0.5f, NPC.width, NPC.height))
            {
                targetPos = player.Center + new Vector2(0, -200f);
            }

            NPC.Center = targetPos;
            NPC.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.Server)
                Burst(NPC.Center, 20, 4.5f);

            NPC.netUpdate = true;
        }

        private void ServerUpdateTeleporting(Player player)
        {
            NPC.velocity *= 0.85f;

            if (StateTimer > 15)
            {
                State = StateIdle;
                StateTimer = 0;
                AttackCounter = Main.rand.Next(80, 140);
                NPC.netUpdate = true;
            }
        }

        private void ClientVisualUpdate(Player player)
        {
            int state = State;
            int timer = StateTimer;

            if (state == StateDashing)
            {
                if (timer >= DashWindup && timer < DashHold)
                {
                    Trail(NPC.Center, NPC.velocity, 2);

                    int clientFireTimer = FireTimer;
                    if (clientFireTimer == 0 && timer == DashWindup)
                    {
                        Burst(NPC.Center, 14, 4.5f);
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);

            bool dashing = State == StateDashing && StateTimer >= DashWindup && StateTimer < DashEnd;

            if (dashing)
            {
                for (int k = 1; k < NPC.oldPos.Length; k++)
                {
                    float t = (NPC.oldPos.Length - k) / (float)NPC.oldPos.Length;
                    Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin;
                    Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.55f * t * t;
                    Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale * (0.85f + t * 0.15f), effects, 0);
                }
            }
            else
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin;
                    Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.05f, effects, 0);
                }
            }

            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            Vector2 pos = (NPC.position - Main.screenPosition) + drawOrigin;
            Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                    Burst(NPC.Center, 26, 5.5f);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome(ModContent.GetInstance<CrystalDepthsBiome>()))
            {
                return 0.05f;
            }
            else
            {
                return 0f;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            int deepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
            var deepCrystalParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 2,
                MinimumStackPerChunkBase = 1,
                MaximumStackPerChunkBase = 3,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };

            npcLoot.Add(new DropOneByOne(deepCrystal, deepCrystalParameters));
        }
    }
}