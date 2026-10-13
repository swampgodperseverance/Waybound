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
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Particles;

namespace Waybound.Content.NPCs.CrystalGrove
{

    public class CrystalElemental : ModNPC
    {
        private const int FrameCount = 3;

        private const float ZoneRadius = 230f;
        private const float CloseRange = 150f;
        private const float OpenRange = 210f;
        private const int MinClosedTime = 50;
        private const float TransitionSpeed = 1f / 12f;
        private const float ClosedDamageMul = 0.3f;

        private const int BarrierSegments = 56;
        private const int BuildTicks = BarrierSegments;
        private const float SegmentLength = 30f;

        private const string GlyphPath = "Waybound/Content/NPCs/CrystalGrove/ElementalGlyphs";
        private const int GlyphFrames = 4;
        private const int GlyphCount = 10;
        private const float GlyphScale = 1f;
        private const float GlyphInset = 16f;

        private static Asset<Texture2D> glyphs;

        private static readonly Color Peach = new Color(255, 168, 135, 0);
        private static readonly Color Hot = new Color(255, 218, 185, 0);

        private float close;
        private float zoneGrow;
        private int age;
        private bool wasClosed;
        private bool initialized;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = FrameCount;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

            if (!Main.dedServ)
                glyphs = ModContent.Request<Texture2D>(GlyphPath, AssetRequestMode.ImmediateLoad);

            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Position = new Vector2(0f, 2f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void Unload()
        {
            glyphs = null;
        }

        public override void SetDefaults()
        {
            NPC.width = 44;
            NPC.height = 60;
            NPC.damage = 20;
            NPC.defense = 8;
            NPC.lifeMax = 420;
            NPC.value = 300f;
            NPC.DeathSound = SoundID.Item27;
            NPC.HitSound = SoundID.Item27;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = false;
            NPC.friendly = false;
            NPC.aiStyle = -1;
            NPC.buffImmune[BuffID.Confused] = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.InModBiome<CrystalDepthsBiome>())
                return 0f;

            return NPC.CountNPCS(Type) < 4 ? 0.28f : 0f;
        }

        public override void AI()
        {
            NPC.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.TargetClosest(false);
                Player nearest = Main.player[NPC.target];
                float dist = nearest.active && !nearest.dead ? NPC.Distance(nearest.Center) : float.MaxValue;

                if (NPC.ai[0] == 0f)
                {
                    if (dist < CloseRange)
                    {
                        NPC.ai[0] = 1f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    NPC.ai[1]++;
                    if (dist > OpenRange && NPC.ai[1] > MinClosedTime)
                    {
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;
                    }
                }
            }

            bool closed = NPC.ai[0] == 1f;

            if (!initialized)
            {
                initialized = true;
                wasClosed = closed;
                close = closed ? 1f : 0f;
            }
            else if (closed != wasClosed)
            {
                wasClosed = closed;
                Transition(closed);
            }

            close = MathHelper.Clamp(close + (closed ? TransitionSpeed : -TransitionSpeed), 0f, 1f);
            age++;

            float t = MathHelper.Clamp(age / (float)BuildTicks, 0f, 1f);
            zoneGrow = 1f - MathF.Pow(1f - t, 3f);

            NPC.HitSound = close > 0.5f ? SoundID.NPCHit4 : SoundID.Item27;

            if (Main.dedServ)
                return;

            Effects();
            AffectPlayer();
        }

        private void Transition(bool closed)
        {
            if (Main.dedServ)
                return;

            Burst(closed ? 28 : 16, closed ? 6.5f : 3.5f, closed ? 1.7f : 1.3f);
            Wave(closed);
            SoundEngine.PlaySound((closed ? SoundID.Item28 : SoundID.Item29) with { Volume = 0.8f, Pitch = closed ? -0.3f : 0.3f }, NPC.Center);
        }

        private void Burst(int count, float speed, float scale)
        {
            int dust = ModContent.DustType<DeepMagicDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / count).ToRotationVector2();
                Dust d = Dust.NewDustPerfect(NPC.Center + v * 18f, dust, v * speed * Main.rand.NextFloat(0.8f, 1.2f), 0, default, scale);
                d.noGravity = true;
            }
        }

        private static void SpawnLine(Vector2 pos, Vector2 vel, float rotation, float length, float thickness, int life)
        {
            if (ParticleSystem.LineBuffer == null)
                return;

            ParticleSystem.LineBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                rotation,
                new System.Numerics.Vector2(length, thickness),
                Peach,
                life
            ));
        }

        private void Wave(bool outward)
        {
            const int count = 24;
            const int life = 24;
            const float inner = 34f;

            float travel = (ZoneRadius - inner) / life;
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            float startRadius = outward ? inner : ZoneRadius;

            for (int i = 0; i < count; i++)
            {
                float a = offset + MathHelper.TwoPi * i / count;
                Vector2 dir = a.ToRotationVector2();
                SpawnLine(NPC.Center + dir * startRadius, dir * (outward ? travel : -travel), a + MathHelper.PiOver2, 18f, LineParticle.CoreThickness * 1.2f, life);
            }
        }

        private void SpawnBarrier(Vector2 center)
        {
            int dust = ModContent.DustType<DeepMagicDust>();

            for (int i = 0; i < BarrierSegments; i++)
            {
                int local = age - i;
                if (local < 0 || local % LineParticle.RingPeriod != 0)
                    continue;

                float a = MathHelper.TwoPi * i / BarrierSegments;
                Vector2 pos = center + a.ToRotationVector2() * ZoneRadius;
                float rotation = a + MathHelper.PiOver2;

                SpawnLine(pos, Vector2.Zero, rotation, SegmentLength, LineParticle.CoreThickness, LineParticle.RingDuration);
                SpawnLine(pos, Vector2.Zero, rotation, SegmentLength * 1.1f, LineParticle.GlowThickness, LineParticle.RingDuration);

                if (local == 0)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Dust d = Dust.NewDustPerfect(pos, dust, Main.rand.NextVector2Circular(2f, 2f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                        d.noGravity = true;
                    }
                }
            }

            if (age >= BuildTicks && Main.rand.NextBool(close > 0.5f ? 1 : 2))
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = center + a.ToRotationVector2() * ZoneRadius;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.15f, 0.15f), -Main.rand.NextFloat(0.5f, 1.1f));
                SpawnLine(pos, vel, a + MathHelper.PiOver2, Main.rand.NextFloat(14f, 30f), LineParticle.CoreThickness, 40);
            }
        }

        private void AffectPlayer()
        {
            if (age < BuildTicks)
                return;

            Player player = Main.LocalPlayer;
            if (!player.active || player.dead)
                return;

            if (Vector2.DistanceSquared(player.Center, NPC.Center) > ZoneRadius * ZoneRadius)
                return;

            player.AddBuff(BuffID.Slow, 10);
            player.GetModPlayer<CrystalZonePlayer>().zoneTimer = 4;

            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(player.width, player.height),
                    ModContent.DustType<DeepMagicDust>(), new Vector2(0f, -Main.rand.NextFloat(0.4f, 1.2f)), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }
        }

        private void Effects()
        {
            float time = Main.GlobalTimeWrappedHourly;
            int dust = ModContent.DustType<DeepMagicDust>();
            Vector2 center = NPC.Center;
            bool built = age >= BuildTicks;

            SpawnBarrier(center);

            if (built)
            {
                if (Main.rand.NextBool(3))
                {
                    float a = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = center + a.ToRotationVector2() * ZoneRadius;
                    Vector2 tangent = new Vector2(-MathF.Sin(a), MathF.Cos(a)) * Main.rand.NextFloat(0.3f, 0.9f);
                    Dust d = Dust.NewDustPerfect(pos, dust, tangent + new Vector2(0f, -0.8f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                    d.noGravity = true;
                }

                if (Main.rand.NextBool(5))
                {
                    Vector2 pos = center + Main.rand.NextVector2Circular(ZoneRadius, ZoneRadius);
                    Dust d = Dust.NewDustPerfect(pos, dust, new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -Main.rand.NextFloat(0.3f, 0.8f)), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                    d.noGravity = true;
                }
            }

            if ((int)Main.GameUpdateCount % 2 == 0)
            {
                int sparks = close > 0.5f ? 4 : 2;
                float orbit = MathHelper.Lerp(54f, 38f, close);
                float speed = MathHelper.Lerp(2.4f, 5f, close);

                for (int i = 0; i < sparks; i++)
                {
                    float a = time * speed + MathHelper.TwoPi * i / sparks;
                    Vector2 pos = center + new Vector2(MathF.Cos(a) * orbit, MathF.Sin(a) * orbit * 0.7f);
                    Dust d = Dust.NewDustPerfect(pos, dust, Vector2.Zero, 0, default, 1.1f);
                    d.noGravity = true;
                }
            }

            float glow = 0.55f + 0.2f * close;
            Lighting.AddLight(center, 1f * glow, 0.5f * glow, 0.3f * glow);

            if (built)
            {
                for (int i = 0; i < 10; i++)
                {
                    float a = MathHelper.TwoPi * i / 10f + time * 0.3f;
                    Lighting.AddLight(center + a.ToRotationVector2() * ZoneRadius, 0.45f, 0.22f, 0.12f);
                }
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (close > 0.5f)
                modifiers.FinalDamage *= ClosedDamageMul;
        }

        public override void FindFrame(int frameHeight)
        {
            int frame = close <= 0.001f ? 0 : close >= 0.999f ? 2 : 1;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            int dust = ModContent.DustType<DeepMagicDust>();
            bool dead = NPC.life <= 0;
            int count = dead ? 34 : close > 0.5f ? 7 : 3;

            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dust, 0f, 0f, 0, default, Main.rand.NextFloat(1f, 1.6f));
                d.velocity = Main.rand.NextVector2Circular(dead ? 6f : 3.5f, dead ? 6f : 3.5f);
                d.noGravity = true;
            }
        }

        private void DrawGlyphs(Vector2 center, float intensity)
        {
            if (glyphs == null || !glyphs.IsLoaded || zoneGrow <= 0.01f)
                return;

            Texture2D tex = glyphs.Value;
            int frameHeight = tex.Height / GlyphFrames;
            float time = Main.GlobalTimeWrappedHourly;
            float ringRadius = ZoneRadius - GlyphInset;
            float scale = GlyphScale * zoneGrow;

            for (int i = 0; i < GlyphCount; i++)
            {
                float a = MathHelper.TwoPi * i / GlyphCount + time * 0.18f;
                Rectangle src = new Rectangle(0, frameHeight * (i % GlyphFrames), tex.Width, frameHeight);
                Vector2 origin = src.Size() / 2f;
                Vector2 pos = center + a.ToRotationVector2() * ringRadius - Main.screenPosition;
                float flicker = 0.65f + 0.35f * MathF.Sin(time * 2.6f + i * 1.9f);
                float rotation = a + MathHelper.PiOver2;

                Main.EntitySpriteDraw(tex, pos, src, Peach * (0.35f * flicker * intensity), rotation, origin, scale * 1.35f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, pos, src, Hot * (0.9f * flicker * intensity), rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            DrawGlyphs(NPC.Center, 1f + 0.35f * close);

            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly;

            float bob = MathF.Sin(time * 1.7f + NPC.whoAmI) * 3.5f * (1f - close * 0.8f);
            Vector2 pos = NPC.Center - screenPos + new Vector2(0f, bob);

            float pulse = 0.5f + 0.5f * MathF.Sin(time * 3f);
            Color outline = Color.Lerp(Peach, Hot, close) * (0.55f + 0.3f * pulse + 0.25f * close);
            float spread = 1.6f + 1.2f * pulse + 1.2f * close;

            Main.EntitySpriteDraw(tex, pos, frame, Peach * (0.18f + 0.12f * pulse), NPC.rotation, origin, NPC.scale * 1.1f, SpriteEffects.None, 0);

            for (int i = 0; i < 8; i++)
            {
                Vector2 off = (MathHelper.PiOver4 * i + time * 0.6f).ToRotationVector2() * spread;
                Main.EntitySpriteDraw(tex, pos + off, frame, outline, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
            }

            Color body = Color.Lerp(drawColor, Color.White, 0.25f + 0.2f * pulse);
            Main.EntitySpriteDraw(tex, pos, frame, body, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);

            return false;
        }
    }
    public class CrystalZonePlayer : ModPlayer
    {
        public const int LifeRegenLoss = 8;

        public int zoneTimer;

        public override void ResetEffects()
        {
            if (zoneTimer > 0)
                zoneTimer--;
        }

        public override void UpdateBadLifeRegen()
        {
            if (zoneTimer <= 0)
                return;

            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            Player.lifeRegenTime = 0;
            Player.lifeRegen -= LifeRegenLoss;
        }
    }
}