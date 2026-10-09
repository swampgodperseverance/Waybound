using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVec2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.CrystalGrove
{
    public class CrystalHermit : ModNPC
    {
        private const int StateWalk = 0;
        private const int StateShell = 1;

        private const int ShellTime = 150;
        private const int TelegraphTime = 40;
        private const int ShellCooldown = 360;
        private const int MaxCharge = 8;
        private const float TriggerRange = 130f;

        private const int SDB = 14;//defense bonus
        private const float SDM = 0.5f; //damage multiplayer
        private const float KR = 0.35f; //knockback resist

        private const bool SpriteFacesRight = false;
        private const float DrawOffsetY = 2f;

        public static readonly Color CrystalColor = new Color(255, 168, 135, 0);
        public static readonly Color RoseColor = new Color(255, 120, 170, 0);

        private int state;
        private int stateTimer;
        private int charge;
        private int cooldown = 120;

        private int lastState;
        private int prevCharge;
        private int walkFrame;
        private float squash;
        private float squashVel;
        private float flash;
        private float tele;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundHallow,
                new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.CrystalHermit")
            });
        }

        public override void SetDefaults()
        {
            NPC.width = 44;
            NPC.height = 34;
            NPC.damage = 36;
            NPC.defense = 14;
            NPC.lifeMax = 170;
            NPC.value = 800f;
            NPC.knockBackResist = KR;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Skeleton;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.CrystalShard, 1, 1, 3));
        }

        private static bool InHomeBiome(Player player)
            => player.ZoneHallow && (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight);

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!InHomeBiome(spawnInfo.Player) || spawnInfo.Water || spawnInfo.PlayerInTown)
                return 0f;

            if (NPC.CountNPCS(Type) >= 6)
                return 0f;

            return 0.35f;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)state);
            writer.Write((short)stateTimer);
            writer.Write((byte)charge);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            state = reader.ReadByte();
            stateTimer = reader.ReadInt16();
            charge = reader.ReadByte();
        }

        private bool Authority => Main.netMode != NetmodeID.MultiplayerClient;

        public override bool PreAI()
        {
            if (state == StateWalk)
            {
                if (cooldown > 0)
                    cooldown--;

                if (Authority && cooldown <= 0)
                {
                    NPC.TargetClosest(false);
                    Player target = Main.player[NPC.target];

                    if (target.active && !target.dead
                        && Vector2.DistanceSquared(target.Center, NPC.Center) < TriggerRange * TriggerRange
                        && Collision.CanHit(NPC, target))
                    {
                        EnterShell();
                    }
                }

                return true;
            }

            if (stateTimer > 0)
                stateTimer--;

            if (Authority && stateTimer <= 0)
            {
                ExitShell();
                return true;
            }

            NPC.velocity = Vector2.Zero;
            NPC.oldVelocity = Vector2.Zero;
            NPC.oldPosition = NPC.position;
            NPC.direction = NPC.spriteDirection = NPC.Center.X < Main.player[NPC.target].Center.X ? 1 : -1;
            NPC.frameCounter = 0;
            walkFrame = 0;

            return false;
        }

        public override void PostAI()
        {
            if (state == StateShell)
            {
                NPC.velocity = Vector2.Zero;
                NPC.oldVelocity = Vector2.Zero;
                walkFrame = 0;
            }
        }

        public override void AI()
        {
            if (state != lastState)
            {
                OnStateChanged(lastState, state);
                lastState = state;
            }

            NPC.defense = NPC.defDefense + (state == StateShell ? SDB : 0);
            NPC.knockBackResist = state == StateShell ? 0f : KR;

            tele = state == StateShell ? Smooth(1f - stateTimer / (float)TelegraphTime) : 0f;

            squashVel += -0.18f * squash;
            squashVel *= 0.78f;
            squash += squashVel;
            flash = MathF.Max(0f, flash - 0.05f);

            if (!Main.dedServ)
                SpawnAmbientFx();

            float chargeFrac = charge / (float)MaxCharge;
            float l = 0.35f + 0.5f * chargeFrac + 0.6f * tele + flash;
            Lighting.AddLight(NPC.Center, 0.9f * l, 0.5f * l, 0.55f * l);

            prevCharge = charge;
        }

        private void EnterShell()
        {
            state = StateShell;
            stateTimer = ShellTime;
            charge = 0;
            NPC.velocity = Vector2.Zero;
            NPC.oldVelocity = Vector2.Zero;
            NPC.frameCounter = 0;
            walkFrame = 0;
            NPC.netUpdate = true;
        }

        private void ExitShell()
        {
            int released = charge;

            state = StateWalk;
            cooldown = ShellCooldown;
            charge = 0;
            NPC.netUpdate = true;

            FireNova(released);
        }

        private void FireNova(int released)
        {
            int type = ModContent.ProjectileType<CrystalHermitShard>();
            int damage = NPC.GetAttackDamage_ForProjectiles(NPC.damage * 0.55f, NPC.damage * 0.42f);

            int count = 6 + Math.Min(released, 6);
            float start = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = (start + MathHelper.TwoPi * i / count).ToRotationVector2();
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 5.5f, type, damage, 1f, Main.myPlayer);
            }

            if (released >= 3)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 dir = (start + MathHelper.TwoPi * (i + 0.5f) / count).ToRotationVector2();
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 3.2f, type, damage, 1f, Main.myPlayer);
                }
            }
        }

        private void OnStateChanged(int from, int to)
        {
            if (Main.dedServ)
                return;

            if (to == StateShell)
            {
                squashVel = 0.5f;
                Burst(NPC.Center, 10, 1.5f, 4f, 6f, 12f);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, Pitch = -0.3f }, NPC.Center);
            }
            else
            {
                squashVel = -0.55f;
                flash = 1f;

                int count = 16 + prevCharge * 4;
                for (int i = 0; i < count; i++)
                {
                    Vector2 dir = (MathHelper.TwoPi * i / count).ToRotationVector2();
                    Shard(NPC.Center + dir * 8f, dir * Main.rand.NextFloat(3f, 7f),
                        Main.rand.NextFloat(10f, 20f), Main.rand.Next(24, 40));
                }

                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.9f, Pitch = 0.15f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.2f }, NPC.Center);
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (state == StateShell)
            {
                modifiers.FinalDamage *= SDM;
                modifiers.DisableCrit();
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => state != StateShell;

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                if (!Main.dedServ)
                {
                    Burst(NPC.Center, 30, 2f, 7f, 8f, 20f, 26, 46);
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.9f, PitchVariance = 0.2f }, NPC.Center);
                }
                return;
            }

            if (state == StateShell)
            {
                if (Authority && charge < MaxCharge)
                {
                    charge++;
                    NPC.netUpdate = true;
                }

                if (!Main.dedServ)
                {
                    Burst(NPC.Center + Main.rand.NextVector2Circular(14f, 10f), 5, 1.5f, 5f, 6f, 12f);
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.3f, PitchVariance = 0.2f }, NPC.Center);
                }
            }
            else if (Authority && cooldown <= 0 && Main.rand.NextBool(3))
            {
                EnterShell();
            }
        }

        private void SpawnAmbientFx()
        {
            float chargeFrac = charge / (float)MaxCharge;

            int chance = state == StateShell ? Math.Max(2, 8 - charge) : 18;
            if (Main.rand.NextBool(chance))
            {
                Vector2 pos = NPC.Center + new Vector2(Main.rand.NextFloat(-14f, 14f), Main.rand.NextFloat(-16f, 2f));
                Shard(pos, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.2f, 0.9f)),
                    Main.rand.NextFloat(5f, 10f) * (1f + chargeFrac), Main.rand.Next(20, 34));
            }

            if (tele > 0.05f && Main.rand.NextFloat() < 0.35f + 0.6f * tele)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Vector2 pos = NPC.Center + dir * Main.rand.NextFloat(45f, 90f);
                Shard(pos, -dir * Main.rand.NextFloat(2.5f, 4.5f), Main.rand.NextFloat(6f, 12f), Main.rand.Next(14, 22));
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int frame;

            if (state == StateShell)
            {
                frame = 4;
                NPC.frameCounter = 0;
                walkFrame = 0;
            }
            else
            {
                float speed = Math.Abs(NPC.velocity.X);

                if (Math.Abs(NPC.velocity.Y) > 0.6f)
                {
                    frame = 1;
                }
                else if (speed < 0.15f)
                {
                    NPC.frameCounter = 0;
                    walkFrame = 0;
                    frame = 0;
                }
                else
                {
                    NPC.frameCounter += 0.6f + speed * 0.6f;
                    if (NPC.frameCounter >= 6f)
                    {
                        NPC.frameCounter = 0;
                        walkFrame = (walkFrame + 1) % 4;
                    }
                    frame = walkFrame;
                }
            }

            NPC.frame.Y = frame * frameHeight;
        }

        private float ShellGlow(float time, float chargeFrac)
        {
            float g = 0.12f + 0.06f * MathF.Sin(time * 2.5f + NPC.whoAmI);
            if (state == StateShell)
                g = 0.3f + 0.45f * chargeFrac + 0.5f * tele + 0.1f * MathF.Sin(time * 12f);
            return g + flash * 0.8f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(frame.Width * 0.5f, frame.Height);
            Vector2 pos = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

            bool flip = SpriteFacesRight ? NPC.spriteDirection == -1 : NPC.spriteDirection == 1;
            SpriteEffects fx = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float time = Main.GlobalTimeWrappedHourly;
            float chargeFrac = charge / (float)MaxCharge;
            float glow = ShellGlow(time, chargeFrac);

            Vector2 shake = Vector2.Zero;
            float rot = 0f;
            if (state == StateShell)
            {
                float amp = 0.3f + 1.4f * chargeFrac + 1.8f * tele;
                shake = new Vector2(0f, Main.rand.NextFloat(-amp, amp) * 0.4f);
                rot = MathF.Sin(time * 45f) * 0.015f * (1f + 3f * chargeFrac + 3f * tele);
            }

            Vector2 scale = new Vector2(1f - squash * 0.1f, 1f + squash * 0.1f) * NPC.scale;
            Vector2 p = pos + shake;

            Color halo = Add(RoseColor, 0.35f * glow);
            for (int i = 0; i < 6; i++)
            {
                float a = MathHelper.TwoPi * i / 6f + time;
                spriteBatch.Draw(tex, p + a.ToRotationVector2() * (2.5f + 2f * glow), frame, halo, rot, origin, scale * 1.04f, fx, 0f);
            }

            spriteBatch.Draw(tex, p, frame, Color.Lerp(drawColor, Color.White, 0.25f + 0.35f * glow), rot, origin, scale, fx, 0f);
            spriteBatch.Draw(tex, p, frame, Add(CrystalColor, glow * 0.7f), rot, origin, scale, fx, 0f);

            if (flash > 0.01f)
                spriteBatch.Draw(tex, p, frame, Add(Color.White, flash * 0.8f), rot, origin, scale, fx, 0f);

            return false;
        }

        public static float Smooth(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        public static Color Add(Color c, float a) => new Color((int)(c.R * a), (int)(c.G * a), (int)(c.B * a), 0);

        public static void Shard(Vector2 pos, Vector2 vel, float size, int life, Color? color = null, float rot = -1f, float alpha = 1f)
        {
            if (Main.dedServ || ParticleSystem.CrystalBuffer == null)
                return;

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                rot < 0f ? Main.rand.NextFloat(MathHelper.TwoPi) : rot,
                new SysVec2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                (color ?? CrystalColor) * Main.rand.NextFloat(0.85f, 1.15f) * alpha,
                life
            ));
        }

        public static void Burst(Vector2 center, int count, float minSpeed, float maxSpeed,
            float minSize, float maxSize, int minLife = 20, int maxLife = 36)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Shard(center + dir * Main.rand.NextFloat(0f, 6f),
                    dir * Main.rand.NextFloat(minSpeed, maxSpeed),
                    Main.rand.NextFloat(minSize, maxSize),
                    Main.rand.Next(minLife, maxLife));
            }
        }
    }

    public class CrystalHermitShard : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            Projectile.localAI[1] += 0.14f;
            Projectile.velocity *= 0.983f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            float fade = MathHelper.Clamp(Projectile.timeLeft / 25f, 0f, 1f);

            if (!Main.dedServ)
            {
                CrystalHermit.Shard(Projectile.Center, Vector2.Zero, 16f, 4, null, Projectile.localAI[1], fade);

                if (Main.rand.NextBool(2))
                {
                    CrystalHermit.Shard(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                        -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                        Main.rand.NextFloat(5f, 9f), Main.rand.Next(14, 22), null, -1f, fade);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.5f * fade, 0.3f * fade, 0.35f * fade);
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            CrystalHermit.Burst(Projectile.Center, 8, 1.5f, 4.5f, 6f, 12f);
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.4f, Pitch = 0.3f, PitchVariance = 0.2f }, Projectile.Center);
        }
    }
}