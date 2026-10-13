using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Particles;

namespace Waybound.Content.NPCs.CrystalGrove
{
    public class StoneDefender : ModNPC
    {
        private const int FrameCount = 13;
        private const int WalkFrames = 9;
        private const int FrameJump = 9;
        private const int FrameWindup = 10;
        private const int FrameStrike = 11;
        private const int FrameRecover = 12;

        private const int WindupTicks = 9;
        private const int StrikeTicks = 5;
        private const int RecoverTicks = 14;
        private const int AttackTotal = WindupTicks + StrikeTicks + RecoverTicks;
        private const int AttackCooldown = 70;
        private const float AttackReachX = 75f;
        private const float AttackReachY = 70f;
        private const float StrikeDamageMul = 1.5f;

        private const bool FacesRight = true;

        private const float WalkSpeed = 1.7f;
        private const float WalkAccel = 0.12f;
        private const float Gravity = 0.3f;
        private const float MaxFall = 12f;
        private const float HopPower = 7f;
        private const float HighHopPower = 9.5f;
        private const int LeapCooldown = 120;
        private const float LeapRange = 280f;
        private const float LeapMinRange = 90f;
        private const int LandTime = 8;

        private static readonly Color Peach = new Color(255, 168, 135, 0);
        private static readonly Color Hot = new Color(255, 218, 185, 0);
        private static readonly Color SparkPeach = new Color(255, 168, 135, 255);
        private static readonly Color SparkHot = new Color(255, 218, 185, 255);

        private int currentFrame;
        private float walkTimer;
        private int walkFrame;
        private int landTimer;
        private bool wasGrounded = true;
        private float flash;
        private int hopCooldown;

        private bool Attacking => NPC.ai[2] > 0f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = FrameCount;

            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Position = new Vector2(0f, 2f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 52;
            NPC.damage = 28;
            NPC.defense = 14;
            NPC.lifeMax = 520;
            NPC.value = 350f;
            NPC.DeathSound = SoundID.Item27;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.knockBackResist = 0.15f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.aiStyle = -1;
            NPC.buffImmune[BuffID.Confused] = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.InModBiome<CrystalDepthsBiome>())
                return 0f;

            return NPC.CountNPCS(Type) < 3 ? 0.22f : 0f;
        }

        private bool OnGround()
        {
            return NPC.velocity.Y >= 0f && Collision.SolidCollision(new Vector2(NPC.position.X, NPC.Bottom.Y), NPC.width, 2, true);
        }

        private static void Spark(Vector2 pos, Vector2 vel, Color color, float size, int life)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size),
                color,
                life));
        }

        private static void SpawnLine(Vector2 pos, Vector2 vel, float rotation, float length, float thickness, int life)
        {
            if (Main.dedServ || ParticleSystem.LineBuffer == null)
                return;

            ParticleSystem.LineBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                rotation,
                new System.Numerics.Vector2(length, thickness),
                Peach,
                life));
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            bool hasTarget = target.active && !target.dead;
            bool onGround = OnGround();

            float dx = hasTarget ? target.Center.X - NPC.Center.X : 0f;
            float dy = hasTarget ? target.Center.Y - NPC.Center.Y : 0f;
            float dist = hasTarget ? NPC.Distance(target.Center) : float.MaxValue;
            int dir = dx >= 0f ? 1 : -1;

            if (hasTarget && !Attacking)
            {
                NPC.direction = dir;
                NPC.spriteDirection = dir;
            }

            if (NPC.ai[1] < LeapCooldown)
                NPC.ai[1]++;
            if (NPC.ai[3] < AttackCooldown)
                NPC.ai[3]++;
            if (hopCooldown > 0)
                hopCooldown--;
            if (landTimer > 0)
                landTimer--;

            if (Attacking)
            {
                NPC.ai[2]++;
                int t = (int)NPC.ai[2];

                if (t == WindupTicks + 1)
                    StrikeHit();

                if (t > AttackTotal)
                    NPC.ai[2] = 0f;
            }

            if (onGround)
            {
                if (Attacking)
                    NPC.velocity.X *= 0.7f;
                else if (hasTarget && Math.Abs(dx) > 24f && landTimer <= 0)
                {
                    NPC.velocity.X += dir * WalkAccel;
                    if (Math.Abs(NPC.velocity.X) > WalkSpeed)
                        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(NPC.velocity.X) * WalkSpeed, 0.2f);
                }
                else
                    NPC.velocity.X *= 0.8f;

                if (landTimer > 0)
                    NPC.velocity.X *= 0.6f;
            }
            else
            {
                if (hasTarget)
                    NPC.velocity.X += dir * 0.04f;

                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8f, 8f);
                NPC.velocity.Y = Math.Min(NPC.velocity.Y + Gravity, MaxFall);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && onGround && hasTarget && !Attacking)
            {
                bool inReach = Math.Abs(dx) < AttackReachX && Math.Abs(dy) < AttackReachY;

                if (inReach && NPC.ai[3] >= AttackCooldown && landTimer <= 0
                    && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1))
                {
                    NPC.ai[2] = 1f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item23 with { Volume = 0.5f, Pitch = -0.5f }, NPC.Center);
                }
                else if (landTimer <= 0)
                {
                    bool blocked = NPC.collideX && Math.Abs(dx) > 30f;

                    if (blocked && hopCooldown <= 0)
                    {
                        NPC.velocity.Y = dy < -80f ? -HighHopPower : -HopPower;
                        hopCooldown = 20;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] >= LeapCooldown && dist < LeapRange && dist > LeapMinRange
                        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1))
                    {
                        NPC.velocity.X = MathHelper.Clamp(dx / 55f, -7f, 7f);
                        NPC.velocity.Y = dy < -60f ? -10f : -8.5f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.6f, Pitch = -0.2f }, NPC.Center);
                    }
                }
            }

            if (!wasGrounded && onGround)
                Land();

            UpdateFrame(onGround);

            wasGrounded = onGround;
            flash = Math.Max(0f, flash - 0.08f);

            if (!Main.dedServ)
                Effects();
        }

        private void UpdateFrame(bool onGround)
        {
            if (!onGround)
            {
                currentFrame = FrameJump;
                return;
            }

            if (Attacking)
            {
                int t = (int)NPC.ai[2];
                currentFrame = t <= WindupTicks ? FrameWindup : t <= WindupTicks + StrikeTicks ? FrameStrike : FrameRecover;
                return;
            }

            if (Math.Abs(NPC.velocity.X) > 0.2f)
            {
                walkTimer += Math.Abs(NPC.velocity.X) * 0.9f;
                if (walkTimer >= 5f)
                {
                    walkTimer = 0f;
                    walkFrame = (walkFrame + 1) % WalkFrames;
                    Footstep();
                }
                currentFrame = walkFrame;
            }
            else
            {
                walkFrame = 0;
                walkTimer = 0f;
                currentFrame = 0;
            }
        }

        private void StrikeHit()
        {
            flash = 1f;
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);

            Vector2 hitPos = NPC.Bottom + new Vector2(NPC.spriteDirection * 38f, -4f);

            if (!Main.dedServ)
            {
                int dust = ModContent.DustType<DeepMagicDust>();
                for (int i = 0; i < 12; i++)
                {
                    Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(14f, 4f), dust,
                        new Vector2(NPC.spriteDirection * Main.rand.NextFloat(0.5f, 4f), -Main.rand.NextFloat(0.5f, 3f)), 0, default, Main.rand.NextFloat(1f, 1.6f));
                    d.noGravity = true;
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = new Vector2(NPC.spriteDirection * (3f + i * 1.3f), 0f);
                    SpawnLine(hitPos, vel, MathHelper.PiOver2, 16f, LineParticle.CoreThickness, 22);
                }

                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = new Vector2(NPC.spriteDirection * Main.rand.NextFloat(0f, 6f) + Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(1f, 4f));
                    Spark(hitPos, vel, Main.rand.NextBool(3) ? SparkHot : SparkPeach, Main.rand.NextFloat(7f, 12f), Main.rand.Next(14, 24));
                }

                Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(hitPos, Vector2.UnitY, 4f, 6f, 10, 900f, "StoneDefenderStrike"));
            }

            Player lp = Main.LocalPlayer;
            if (lp.active && !lp.dead && !lp.immune)
            {
                float rx = (lp.Center.X - NPC.Center.X) * NPC.spriteDirection;
                float ry = Math.Abs(lp.Center.Y - NPC.Center.Y);

                if (rx > -10f && rx < AttackReachX + 30f && ry < AttackReachY)
                {
                    int dmg = (int)(NPC.damage * StrikeDamageMul);
                    lp.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), dmg, NPC.spriteDirection);
                    lp.velocity.X += NPC.spriteDirection * 4f;
                    lp.velocity.Y = Math.Min(lp.velocity.Y, -3f);
                }
            }
        }

        private void Land()
        {
            landTimer = LandTime;

            if (Main.dedServ)
                return;

            Vector2 feet = NPC.Bottom;
            int dust = ModContent.DustType<DeepMagicDust>();

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(feet + new Vector2(Main.rand.NextFloat(-NPC.width / 2f, NPC.width / 2f), -2f), dust,
                    new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(0.5f, 2.5f)), 0, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.5f, Pitch = -0.4f }, NPC.Center);
        }

        private void Footstep()
        {
            if (Main.dedServ || (walkFrame != 2 && walkFrame != 6))
                return;

            Vector2 feet = NPC.Bottom + new Vector2(NPC.spriteDirection * -6f, -2f);
            Dust d = Dust.NewDustPerfect(feet, ModContent.DustType<DeepMagicDust>(), new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -Main.rand.NextFloat(0.3f, 1f)), 0, default, 1f);
            d.noGravity = true;

            Spark(feet, new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f)), SparkPeach, Main.rand.NextFloat(5f, 8f), 16);
        }

        private void Effects()
        {
            float time = Main.GlobalTimeWrappedHourly;

            if (Main.rand.NextBool(10))
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.7f, NPC.height * 0.6f);
                Spark(pos, new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -Main.rand.NextFloat(0.4f, 1f)), Main.rand.NextBool(3) ? SparkHot : SparkPeach, Main.rand.NextFloat(4f, 7f), 26);
            }

            if ((int)Main.GameUpdateCount % 3 == 0)
            {
                float a = time * 2.2f + NPC.whoAmI;
                Vector2 pos = NPC.Center + new Vector2(MathF.Cos(a) * 34f, MathF.Sin(a) * 22f - 6f);
                Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<DeepMagicDust>(), Vector2.Zero, 0, default, 1f);
                d.noGravity = true;
            }

            if (Attacking && NPC.ai[2] <= WindupTicks && Main.rand.NextBool(2))
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(22f, 40f);
                Spark(NPC.Center + off, -off * 0.1f, SparkHot, Main.rand.NextFloat(5f, 8f), 12);
            }

            if (!OnGround() && NPC.velocity.Length() > 4f && Main.rand.NextBool(2))
                Spark(NPC.Center + Main.rand.NextVector2Circular(10f, 14f), -NPC.velocity * 0.1f, SparkPeach, Main.rand.NextFloat(6f, 10f), 14);

            float glow = 0.5f + 0.4f * flash;
            Lighting.AddLight(NPC.Center, 1f * glow, 0.5f * glow, 0.3f * glow);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            flash = 1f;
            int dust = ModContent.DustType<DeepMagicDust>();
            bool dead = NPC.life <= 0;
            int count = dead ? 38 : 5;

            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dust, 0f, 0f, 0, default, Main.rand.NextFloat(1f, 1.7f));
                d.velocity = Main.rand.NextVector2Circular(dead ? 6f : 3f, dead ? 6f : 3f);
                d.noGravity = true;
            }

            for (int i = 0; i < (dead ? 18 : 4); i++)
                Spark(NPC.Center + Main.rand.NextVector2Circular(14f, 18f), Main.rand.NextVector2Circular(dead ? 7f : 3f, dead ? 7f : 3f), Main.rand.NextBool(3) ? SparkHot : SparkPeach, Main.rand.NextFloat(7f, 12f), Main.rand.Next(14, 26));

            if (dead)
            {
                float off = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 16; i++)
                {
                    float a = off + MathHelper.TwoPi * i / 16f;
                    Vector2 dir = a.ToRotationVector2();
                    SpawnLine(NPC.Center + dir * 20f, dir * 3.5f, a + MathHelper.PiOver2, 18f, LineParticle.CoreThickness, 24);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height);
            float time = Main.GlobalTimeWrappedHourly;

            Vector2 pos = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY + 2f);
            bool flip = FacesRight ? NPC.spriteDirection == -1 : NPC.spriteDirection == 1;
            SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float pulse = 0.5f + 0.5f * MathF.Sin(time * 3f + NPC.whoAmI);
            Color outline = Color.Lerp(Peach, Hot, flash) * (0.5f + 0.3f * pulse + 0.3f * flash);
            float spread = 1.5f + 1.1f * pulse + 1.5f * flash;

            Main.EntitySpriteDraw(tex, pos, frame, Peach * (0.16f + 0.1f * pulse), NPC.rotation, origin, NPC.scale * 1.08f, effects, 0);

            for (int i = 0; i < 8; i++)
            {
                Vector2 off = (MathHelper.PiOver4 * i + time * 0.6f).ToRotationVector2() * spread;
                Main.EntitySpriteDraw(tex, pos + off, frame, outline, NPC.rotation, origin, NPC.scale, effects, 0);
            }

            Color body = Color.Lerp(drawColor, Color.White, 0.25f + 0.2f * pulse);
            Main.EntitySpriteDraw(tex, pos, frame, body, NPC.rotation, origin, NPC.scale, effects, 0);

            if (flash > 0.02f)
                Main.EntitySpriteDraw(tex, pos, frame, Hot * (flash * 0.5f), NPC.rotation, origin, NPC.scale, effects, 0);

            return false;
        }
    }
}