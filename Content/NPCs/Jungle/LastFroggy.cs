using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.NPCs.Jungle
{
    public class LastFroggy : ModNPC
    {
        private const int StateIdle = 0;   
        private const int StateCrouch = 1; 
        private const int StateAir = 2;    

        private const int CrouchTime = 20;

        private ref float State => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float IdleDuration => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4; 

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers
            {
                CustomTexturePath = Texture + "Bestiary",
                Position = new Vector2(0f, -5f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = -20f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.LastFroggy")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 38;
            NPC.height = 26;
            NPC.lifeMax = 100;
            NPC.defense = 0;
            NPC.damage = 30;
            NPC.value = 1000f;
            NPC.knockBackResist = 0.2f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => State == StateAir;

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            bool grounded = NPC.velocity.Y == 0f;
            bool authority = Main.netMode != NetmodeID.MultiplayerClient; 

            NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            switch ((int)State)
            {
                case StateIdle:
                    NPC.velocity.X *= 0.8f; 
                    Timer++;

                    if (authority)
                    {
                        if (IdleDuration <= 0f)
                        {
                            IdleDuration = Main.rand.Next(40, 91);
                            NPC.netUpdate = true;
                        }

                        if (Timer >= IdleDuration && grounded)
                        {
                            State = StateCrouch;
                            Timer = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                    break;

                case StateCrouch:
                    NPC.velocity.X = 0f;
                    Timer++;

                    if (authority && Timer >= CrouchTime)
                    {
                        Jump(player);
                        State = StateAir;
                        Timer = 0f;
                        NPC.netUpdate = true;
                    }
                    break;

                case StateAir:
                    Timer++;

                    if (NPC.collideX)
                        NPC.velocity.X *= -0.3f;

                    if (authority && Timer > 5f && grounded)
                    {
                        NPC.velocity.X = 0f;
                        State = StateIdle;
                        Timer = 0f;
                        IdleDuration = 0f; 
                        NPC.netUpdate = true;
                    }
                    break;
            }
        }

        private void Jump(Player player)
        {
            float dx = player.Center.X - NPC.Center.X;
            float dir = Math.Sign(dx);
            if (dir == 0f)
                dir = NPC.direction;

            float speedX = MathHelper.Clamp(Math.Abs(dx) / 40f, 1.5f, 6f);
            float speedY = -6f;
            //Dynamical jump height 
            if (player.Center.Y < NPC.Center.Y - 80f)
                speedY = -8.5f;

            Vector2 ahead = new Vector2(NPC.position.X + (dir > 0f ? NPC.width : -12f), NPC.position.Y);
            if (Collision.SolidCollision(ahead, 12, NPC.height - 4))
                speedY = -8.5f;

            NPC.velocity = new Vector2(dir * speedX, speedY);

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(new Vector2(NPC.position.X, NPC.Bottom.Y - 4f), NPC.width, 4, DustID.GreenMoss, 0f, -1f);
                d.velocity *= 0.5f;
                d.noGravity = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int frame;
            switch ((int)State)
            {
                case StateCrouch:
                    frame = 2;
                    break;
                case StateAir:
                    frame = 3;
                    break;
                default:
                    frame = Timer % 60f < 8f ? 1 : 0;
                    break;
            }

            NPC.frame.Y = frame * frameHeight;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int count = NPC.life <= 0 ? 20 : 4;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 2f, -2f, 20, default, 1.4f);
                d.velocity *= 0.75f;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.AnyNPCs(Type))
                return 0f;

            if (!spawnInfo.Player.ZoneJungle || !spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Water)
                return 0f;

            return Main.raining ? 0.01f : 0.02f;
        }
    }
}