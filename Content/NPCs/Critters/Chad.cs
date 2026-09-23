using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Miscellaneous;

namespace Waybound.Content.NPCs.Critters
{
    public class Chad : ModNPC
    {
        private enum ChadState
        {
            Idle,
            Standing,
            Spinning
        }

        private ChadState State
        {
            get => (ChadState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private ref float Timer => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 11;

            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = false;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = -1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.width = 24;
            NPC.height = 24;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 30;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.friendly = false;
            NPC.chaseable = true;
            NPC.npcSlots = 0.1f;
            NPC.value = 0f;
            NPC.catchItem = (short)ModContent.ItemType<ChadItem>();
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(false);
                target = Main.player[NPC.target];
            }

            float distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);

            switch (State)
            {
                case ChadState.Idle:
                    NPC.velocity.X *= 0.9f;

                    if (distanceToPlayer < 120f)
                    {
                        State = ChadState.Standing;
                        Timer = 0f;
                    }
                    break;

                case ChadState.Standing:
                    NPC.velocity.X = 0f;
                    Timer++;

                    if (Timer >= 30f)
                    {
                        State = ChadState.Spinning;
                        Timer = 0f;
                    }
                    break;

                case ChadState.Spinning:
                    NPC.velocity.X = 0f;
                    Timer++;

                    if (Timer >= 600f)
                    {
                        Timer = 0f;
                    }
                    break;
            }

            NPC.velocity.Y += 0.4f;
            if (NPC.velocity.Y > 10f)
            {
                NPC.velocity.Y = 10f;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;

            switch (State)
            {
                case ChadState.Idle:
                    NPC.frameCounter++;

                    if (NPC.frameCounter < 30.0)
                    {
                        NPC.frame.Y = 0 * frameHeight;
                    }
                    else if (NPC.frameCounter < 40.0)
                    {
                        NPC.frame.Y = 1 * frameHeight;
                    }
                    else if (NPC.frameCounter < 70.0)
                    {
                        NPC.frame.Y = 0 * frameHeight;
                    }
                    else
                    {
                        NPC.frame.Y = 1 * frameHeight;
                        NPC.frameCounter = 0.0;
                    }
                    break;

                case ChadState.Standing:
                    NPC.frameCounter++;

                    if (NPC.frameCounter < 15.0)
                    {
                        NPC.frame.Y = 2 * frameHeight;
                    }
                    else if (NPC.frameCounter < 30.0)
                    {
                        NPC.frame.Y = 3 * frameHeight;
                    }
                    else if (NPC.frameCounter < 45.0)
                    {
                        NPC.frame.Y = 4 * frameHeight;
                    }
                    else if (NPC.frameCounter < 60.0)
                    {
                        NPC.frame.Y = 5 * frameHeight;
                    }
                    else if (NPC.frameCounter < 75.0)
                    {
                        NPC.frame.Y = 6 * frameHeight;
                    }
                    else
                    {
                        NPC.frameCounter = 0.0;
                    }
                    break;

                case ChadState.Spinning:
                    NPC.frameCounter++;

                    if (NPC.frameCounter < 12.0)
                    {
                        NPC.frame.Y = 6 * frameHeight;
                    }
                    else if (NPC.frameCounter < 24.0)
                    {
                        NPC.frame.Y = 7 * frameHeight;
                    }
                    else if (NPC.frameCounter < 36.0)
                    {
                        NPC.frame.Y = 8 * frameHeight;
                    }
                    else if (NPC.frameCounter < 48.0)
                    {
                        NPC.frame.Y = 9 * frameHeight;
                    }
                    else if (NPC.frameCounter < 60.0)
                    {
                        NPC.frame.Y = 10 * frameHeight;
                    }
                    else
                    {
                        NPC.frame.Y = 7 * frameHeight;
                        NPC.frameCounter = 12.0;
                    }
                    break;
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            float redness = 1f - healthPercent;

            return new Color(
                (byte)(255),
                (byte)(255 * (1f - redness * 0.7f)),
                (byte)(255 * (1f - redness * 0.7f)),
                (byte)(255 * (1f - redness * 0.3f))
            );
        }

        public override void OnKill()
        {
            Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.Wood, Main.rand.Next(3, 8));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.dayTime)
            {
                return 0f;
            }

            if (spawnInfo.Player.ZoneForest && spawnInfo.SpawnTileY < Main.worldSurface)
            {
                return 0.08f;
            }

            return 0f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement("")
            });
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f);
                }
            }
        }
    }
}