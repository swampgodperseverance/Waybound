using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.NPCs.Ice
{//I reworked it three fucking times
    public class Hogsheep : ModNPC
    {
        private enum State : byte { Walk, Curl, Roll, Dizzy }

        private const int WalkFrames = 5;
        private const int BallFrame = 5;
        private const bool RequireSkeletron = true;
        private const int CurlTime = 40;
        private const int CurlBallTime = 14;
        private const int RollTime = 150;
        private const int DizzyTime = 90;
        private const int MaxBounces = 2;
        private const float RollSpeed = 9f;
        private const float RDM = 1.6f;//damage mulitplier while roll
        private const int RDB = 6;//Defense mulitplier while roll
        private const float DDM = 1.35f;//damage mulitplier while Dizzy
        private const float TriggerMin = 100f;
        private const float TriggerMax = 520f;
        private static readonly Color IceColor = new(130, 205, 255);

        private State state;
        private int timer;
        private int rollDir = 1;
        private int bounces;
        private int cooldown;
        private int bounceCd;
        private int walkFrame;
        private float spin;
        private bool wasGrounded;
        private State lastState = (State)255;

        private bool Authority => Main.netMode != NetmodeID.MultiplayerClient;
        private bool IsBall => state == State.Roll || (state == State.Curl && timer <= CurlBallTime);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.Hogsheep")
            });
        }

        public override void SetDefaults()
        {
            NPC.width = 42;
            NPC.height = 32;
            NPC.damage = 22;
            NPC.defense = 5;
            NPC.lifeMax = 70;
            NPC.value = 150f;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.Chilled] = true;
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Skeleton;
        }

        public override void OnSpawn(IEntitySource source) => cooldown = Main.rand.Next(60, 180);

        public override void ModifyNPCLoot(NPCLoot npcLoot)
            => npcLoot.Add(ItemDropRule.Common(ItemID.IceBlock, 2, 2, 5));

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.ZoneSnow || spawnInfo.Water || spawnInfo.PlayerInTown)
                return 0f;
            return NPC.CountNPCS(Type) >= 5 ? 0f : 0.5f;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)state);
            writer.Write((short)timer);
            writer.Write((sbyte)rollDir);
            writer.Write((byte)bounces);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            state = (State)reader.ReadByte();
            timer = reader.ReadInt16();
            rollDir = reader.ReadSByte();
            bounces = reader.ReadByte();
        }

        public override bool PreAI()
        {
            bool roll = state == State.Roll;
            NPC.defense = NPC.defDefense + (roll ? RDB : 0);
            NPC.damage = (int)(NPC.defDamage * (roll ? RDM : 1f));
            NPC.knockBackResist = roll ? 0f : state == State.Dizzy ? 0.7f : 0.4f;

            return state switch
            {
                State.Walk => UpdateWalk(),
                State.Curl => UpdateCurl(),
                State.Roll => UpdateRoll(),
                State.Dizzy => UpdateDizzy(),
                _ => true
            };
        }

        private bool UpdateWalk()
        {
            if (cooldown > 0) cooldown--;
            if (!Authority || cooldown > 0 || (RequireSkeletron && !NPC.downedBoss3))
                return true;

            NPC.TargetClosest(false);
            Player t = Main.player[NPC.target];
            if (!t.active || t.dead || !NPC.collideY) return true;

            float dist = Vector2.Distance(t.Center, NPC.Center);
            if (dist > TriggerMin && dist < TriggerMax && Collision.CanHit(NPC, t))
                SetState(State.Curl, CurlTime);
            return true;
        }

        private bool UpdateCurl()
        {
            timer--;
            Gravity();
            NPC.velocity.X *= 0.8f;

            Player t = Main.player[NPC.target];
            if (t.active)
                NPC.direction = NPC.spriteDirection = t.Center.X >= NPC.Center.X ? 1 : -1;

            if (Authority && timer <= 0)
            {
                rollDir = NPC.direction;
                bounces = 0;
                NPC.velocity = new Vector2(rollDir * RollSpeed * 0.5f, -3f);
                SetState(State.Roll, RollTime);
            }
            return false;
        }

        private bool UpdateRoll()
        {
            timer--;
            Gravity();
            bounceCd--;

            bool hitWall = NPC.collideX && bounceCd <= 0;
            float vx = NPC.velocity.X;

            if (hitWall && bounces < MaxBounces)
            {
                bounces++;
                bounceCd = 8;
                rollDir = -rollDir;
                vx = rollDir * RollSpeed * 0.8f;
                NPC.velocity.Y = -4.5f;
                OnBounce();
                if (Authority) NPC.netUpdate = true;

                if (Authority && bounces >= MaxBounces)
                {
                    NPC.velocity.X *= 0.4f;
                    SetState(State.Dizzy, DizzyTime);
                    return false;
                }
            }
            else
            {
                vx = MathHelper.Lerp(vx, rollDir * RollSpeed, 0.1f);
            }

            NPC.velocity.X = vx;
            NPC.direction = NPC.spriteDirection = rollDir;
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

            if (Authority && timer <= 0)
            {
                NPC.velocity.X *= 0.4f;
                cooldown = Main.rand.Next(150, 260);
                SetState(State.Walk, 0);
            }
            return false;
        }

        private bool UpdateDizzy()
        {
            timer--;
            Gravity();
            NPC.velocity.X *= 0.85f;

            if (Authority && timer <= 0)
            {
                cooldown = Main.rand.Next(150, 260);
                SetState(State.Walk, 0);
            }
            return false;
        }

        private void Gravity() => NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.3f, 10f);

        private void SetState(State s, int t)
        {
            state = s;
            timer = t;
            NPC.netUpdate = true;
        }

        public override void PostAI()
        {
            if (state != lastState)
            {
                OnStateChanged();
                lastState = state;
            }

            if (state == State.Walk)
                NPC.spriteDirection = NPC.direction;

            spin = state == State.Roll
                ? spin + rollDir * (0.15f + MathF.Abs(NPC.velocity.X) * 0.03f)
                : 0f;

            bool grounded = NPC.collideY && NPC.velocity.Y >= 0f;
            if (grounded && !wasGrounded && NPC.oldVelocity.Y > 2.5f)
                SnowPuff(NPC.Bottom, 4, 1.5f);
            wasGrounded = grounded;

            if (Main.dedServ) return;

            SpawnStateFx();
            if (state == State.Roll)
                Lighting.AddLight(NPC.Center, 0.2f, 0.45f, 0.7f);
        }

        private void OnStateChanged()
        {
            if (Main.dedServ) return;
            switch (state)
            {
                case State.Curl:
                    SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.6f, Pitch = 0.2f }, NPC.Center);
                    break;
                case State.Roll:
                    SnowPuff(NPC.Bottom, 14, 3.5f);
                    SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f, Pitch = -0.4f }, NPC.Center);
                    break;
                case State.Dizzy:
                    SnowPuff(NPC.Bottom, 8, 2f);
                    break;
            }
        }

        private void OnBounce()
        {
            if (Main.dedServ) return;
            Vector2 wall = NPC.Center + new Vector2(-rollDir * NPC.width * 0.5f, 0f);
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(wall,
                    i % 2 == 0 ? DustID.Ice : DustID.IceTorch,
                    Main.rand.NextVector2Circular(3.5f, 3.5f) + new Vector2(rollDir * 2f, 0f),
                    100, default, 1.25f);
                d.noGravity = i % 2 == 1;
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = -0.2f, PitchVariance = 0.2f }, NPC.Center);
        }

        private void SpawnStateFx()
        {
            switch (state)
            {
                case State.Curl:
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(NPC.Center + dir * Main.rand.NextFloat(28f, 50f),
                            DustID.Snow, -dir * Main.rand.NextFloat(1.4f, 2.8f), 100, default, 1.1f);
                        d.noGravity = true;
                    }
                    break;
                case State.Roll:
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            NPC.Bottom + new Vector2(Main.rand.NextFloat(-10f, 10f), -2f),
                            Main.rand.NextBool(3) ? DustID.IceTorch : DustID.Snow,
                            new Vector2(-NPC.velocity.X * 0.18f, -Main.rand.NextFloat(0.4f, 1.4f)),
                            100, default, 1.15f);
                        d.noGravity = true;
                    }
                    break;
                case State.Dizzy:
                    if (Main.GameUpdateCount % 5 == 0)
                    {
                        float a = Main.GameUpdateCount * 0.25f;
                        Dust d = Dust.NewDustPerfect(
                            NPC.Top + new Vector2(MathF.Cos(a) * 12f, -3f + MathF.Sin(a) * 3f),
                            DustID.IceTorch, Vector2.Zero, 100, default, 0.95f);
                        d.noGravity = true;
                    }
                    break;
            }
        }

        private static void SnowPuff(Vector2 pos, int count, float speed)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    pos + new Vector2(Main.rand.NextFloat(-12f, 12f), -2f),
                    DustID.Snow,
                    new Vector2(Main.rand.NextFloat(-speed, speed), -Main.rand.NextFloat(0.4f, speed)),
                    100, default, 1.15f);
                d.noGravity = Main.rand.NextBool();
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
            => state is State.Walk or State.Roll;

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (state == State.Dizzy)
                modifiers.FinalDamage *= DDM;
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (state == State.Roll)
                modifiers.Knockback *= 1.8f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
            => target.AddBuff(BuffID.Chilled, state == State.Roll ? 240 : 90);

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;
            int count = NPC.life <= 0 ? 20 : 4;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    i % 3 == 0 ? DustID.Ice : DustID.Snow,
                    hit.HitDirection * 1.8f, -1.8f, 100, default, 1.15f);
                d.velocity *= 0.75f;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int frame;
            if (IsBall)
                frame = BallFrame;
            else if (state != State.Walk)
                frame = 0;
            else
            {
                float speed = Math.Abs(NPC.velocity.X);
                if (speed < 0.2f)
                {
                    NPC.frameCounter = 0;
                    walkFrame = 0;
                }
                else
                {
                    NPC.frameCounter += 0.5f + speed * 0.35f;
                    if (NPC.frameCounter >= 5f)
                    {
                        NPC.frameCounter = 0;
                        walkFrame = (walkFrame + 1) % WalkFrames;
                    }
                }
                frame = walkFrame;
            }
            NPC.frame.Y = frame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            SpriteEffects fx = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float scale = NPC.scale;

            if (IsBall)
            {
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 offset = new(0f, NPC.gfxOffY + 2f - frame.Height * 0.5f);
                Vector2 pos = NPC.Bottom - screenPos + offset;

                if (state == State.Roll)
                {
                    for (int k = 2; k < NPC.oldPos.Length; k += 2)
                    {
                        if (NPC.oldPos[k] == Vector2.Zero) continue;
                        float t = 1f - k / (float)NPC.oldPos.Length;
                        Vector2 oldBottom = NPC.oldPos[k] + new Vector2(NPC.width * 0.5f, NPC.height);
                        Color trail = IceColor * (t * 0.4f);
                        trail.A = 0;
                        spriteBatch.Draw(tex, oldBottom - screenPos + offset, frame, trail,
                            spin - k * 0.1f * rollDir, origin, scale * (0.85f + t * 0.15f), fx, 0f);
                    }
                    Color glow = IceColor * 0.3f;
                    glow.A = 0;
                    spriteBatch.Draw(tex, pos, frame, glow, spin, origin, scale * 1.08f, fx, 0f);
                }
                spriteBatch.Draw(tex, pos, frame, drawColor, spin, origin, scale, fx, 0f);
                return false;
            }

            Vector2 origin2 = new(frame.Width * 0.5f, frame.Height);
            Vector2 pos2 = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY + 2f);
            Vector2 shake = Vector2.Zero;
            float tilt = 0f;
            float charge = 0f;

            if (state == State.Curl)
            {
                charge = 1f - timer / (float)CurlTime;
                shake.X = Main.rand.NextFloat(-(0.4f + 1.8f * charge), 0.4f + 1.8f * charge);
            }
            else if (state == State.Dizzy)
                tilt = MathF.Sin(Main.GlobalTimeWrappedHourly * 9f) * 0.12f;

            spriteBatch.Draw(tex, pos2 + shake, frame, drawColor, tilt, origin2, scale, fx, 0f);
            if (charge > 0f)
            {
                Color overlay = IceColor * (charge * 0.45f);
                overlay.A = 0;
                spriteBatch.Draw(tex, pos2 + shake, frame, overlay, tilt, origin2, scale, fx, 0f);
            }
            return false;
        }
    }
}