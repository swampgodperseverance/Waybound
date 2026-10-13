using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.NPCs.CrystalGrove
{
    public class DeepJellyfish : ModNPC
    {
        private const int FrameCount = 6;
        private const int PushFrame = 2;
        private const int PulseFrameTicks = 7;
        private const int ExhaustFrameTicks = 13;

        private const float DetectRange = 1100f;
        private const float ChargeRange = 300f;
        private const float ShockRadius = 240f;
        private const float ShockPower = 15f;

        private const int ChargeCooldown = 100;
        private const int ChargeTime = 48;
        private const int DischargeTime = 14;
        private const int ExhaustTime = 170;

        private enum Mode
        {
            Drift = 0,
            Charge = 1,
            Discharge = 2,
            Exhausted = 3
        }

        private Mode Current => (Mode)(int)NPC.ai[0];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = FrameCount;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 0;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Position = new Vector2(0f, 2f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 68;
            NPC.height = 82;
            NPC.damage = 30;
            NPC.defense = 4;
            NPC.lifeMax = 580;
            NPC.DeathSound = SoundID.Item27;
            NPC.HitSound = SoundID.Item27;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = false;
            NPC.friendly = false;
            NPC.aiStyle = -1;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[2] = Main.rand.Next(FrameCount * PulseFrameTicks);
        }

        private void SetMode(Mode mode)
        {
            NPC.ai[0] = (float)mode;
            NPC.ai[1] = 0f;
            NPC.localAI[0] = 0f;
            NPC.netUpdate = true;
        }

        private void Face(Vector2 dir)
        {
            float target = MathHelper.Clamp(MathHelper.WrapAngle(dir.ToRotation() + MathHelper.PiOver2), -0.9f, 0.9f);
            NPC.rotation = NPC.rotation.AngleLerp(target, 0.1f);
        }

        private void FaceUp()
        {
            NPC.rotation = NPC.rotation.AngleLerp(0f, 0.08f);
        }

        private void FacePlayerInstant(Player player)
        {
            Vector2 dir = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            NPC.rotation = MathHelper.Clamp(MathHelper.WrapAngle(dir.ToRotation() + MathHelper.PiOver2), -0.9f, 0.9f);
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            float dist = player.active && !player.dead ? NPC.Distance(player.Center) : float.MaxValue;
            bool hasTarget = dist < DetectRange;

            if (dist > 2600f)
                NPC.EncourageDespawn(60);

            switch (Current)
            {
                case Mode.Drift:
                    Drift(player, hasTarget, dist);
                    break;
                case Mode.Charge:
                    Charge(dist);
                    break;
                case Mode.Discharge:
                    Discharge(player);
                    break;
                case Mode.Exhausted:
                    Exhausted();
                    break;
            }

            float targetGlow = Current switch
            {
                Mode.Charge => 0.3f + 0.7f * (NPC.ai[1] / ChargeTime),
                Mode.Discharge => 1f,
                Mode.Exhausted => 0.04f,
                _ => 0.3f + 0.15f * MathF.Sin(NPC.ai[2] / (FrameCount * PulseFrameTicks) * MathHelper.TwoPi)
            };

            NPC.localAI[1] = MathHelper.Lerp(NPC.localAI[1], targetGlow, 0.12f);

            float g = NPC.localAI[1];
            Lighting.AddLight(NPC.Center, 0.1f * g, 0.45f * g, 0.8f * g);
        }

        private void Drift(Player player, bool hasTarget, float dist)
        {
            NPC.ai[1]++;
            float t = (float)Main.GameUpdateCount;

            int period = FrameCount * PulseFrameTicks;
            NPC.ai[2] = (NPC.ai[2] + 1f) % period;
            bool push = (int)NPC.ai[2] == PushFrame * PulseFrameTicks;

            Vector2 dir;
            if (hasTarget)
            {
                Vector2 aim = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float sway = MathF.Sin(t * 0.028f + NPC.whoAmI * 1.7f) * 0.55f;
                dir = aim.RotatedBy(sway);

                if (dist < ChargeRange && NPC.ai[1] > ChargeCooldown)
                {
                    SetMode(Mode.Charge);
                    return;
                }
            }
            else
            {
                float a = t * 0.011f + NPC.whoAmI * 2.3f;
                dir = new Vector2(MathF.Cos(a), MathF.Sin(a * 1.3f) * 0.8f - 0.35f).SafeNormalize(Vector2.UnitY);
            }

            if (push)
            {
                NPC.velocity += dir * (hasTarget ? 6.2f : 3.2f);
                PushEffects(dir);
                if (hasTarget)
                    FacePlayerInstant(player);
            }

            NPC.velocity *= 0.962f;
            NPC.velocity.Y += 0.035f;

            float max = hasTarget ? 10f : 6f;
            if (NPC.velocity.Length() > max)
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * max;

            Face(dir);
        }

        private void PushEffects(Vector2 dir)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(NPC.Center - dir * 24f + Main.rand.NextVector2Circular(16f, 16f), ModContent.DustType<DeepMagicDust>(),
                    -dir * Main.rand.NextFloat(1f, 3.2f) + Main.rand.NextVector2Circular(0.6f, 0.6f), 100, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.3f, Pitch = -0.6f }, NPC.Center);
        }

        private void Charge(float dist)
        {
            NPC.ai[1]++;
            NPC.velocity *= 0.92f;
            FaceUp();

            if (!Main.dedServ && (int)NPC.ai[1] % 2 == 0)
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(70f, 120f);
                Dust d = Dust.NewDustPerfect(NPC.Center + off, ModContent.DustType<DeepMagicDust>(), -off * 0.065f, 100, default, Main.rand.NextFloat(1.1f, 1.6f));
                d.noGravity = true;
            }

            if (dist > ChargeRange * 1.8f)
            {
                SetMode(Mode.Drift);
                return;
            }

            if (NPC.ai[1] >= ChargeTime)
                SetMode(Mode.Discharge);
        }

        private void Discharge(Player player)
        {
            NPC.ai[1]++;
            NPC.velocity *= 0.9f;
            FaceUp();

            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                Shockwave(player);
            }

            if (NPC.ai[1] >= DischargeTime)
            {
                NPC.ai[2] = 0f;
                SetMode(Mode.Exhausted);
            }
        }

        private void Shockwave(Player player)
        {
            Vector2 center = NPC.Center;
            Vector2 away = (center - player.Center).SafeNormalize(Vector2.UnitY);
            NPC.velocity = away * 8f;

            if (Main.dedServ)
                return;

            for (int i = 0; i < 44; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / 44f).ToRotationVector2();
                Dust d = Dust.NewDustPerfect(center + v * 20f, ModContent.DustType<DeepMagicDust>(), v * Main.rand.NextFloat(8f, 11f), 100, default, 1.8f);
                d.noGravity = true;
            }

            for (int i = 0; i < 28; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / 28f + 0.1f).ToRotationVector2();
                Dust d = Dust.NewDustPerfect(center + v * 12f, ModContent.DustType<DeepMagicDust>(), v * Main.rand.NextFloat(4f, 6.5f), 100, default, 1.4f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 1f, Pitch = -0.4f }, center);

            Player local = Main.LocalPlayer;
            if (!local.active || local.dead)
                return;

            Vector2 off = local.Center - center;
            float d2 = off.Length();
            if (d2 >= ShockRadius)
                return;

            float power = ShockPower * (1f - d2 / ShockRadius * 0.5f);
            local.RemoveAllGrapplingHooks();
            local.velocity = off.SafeNormalize(Vector2.UnitY) * power + new Vector2(0f, -3f);
            local.AddBuff(BuffID.Slow, 90);
        }

        private void Exhausted()
        {
            NPC.ai[1]++;

            int period = FrameCount * ExhaustFrameTicks;
            NPC.ai[2] = (NPC.ai[2] + 1f) % period;

            if ((int)NPC.ai[2] == PushFrame * ExhaustFrameTicks)
                NPC.velocity += new Vector2(MathF.Sin(NPC.whoAmI * 3.1f), -0.8f).SafeNormalize(Vector2.UnitY) * 2.2f;

            NPC.velocity *= 0.95f;
            NPC.velocity.Y += 0.05f;
            FaceUp();

            if (NPC.ai[1] >= ExhaustTime)
            {
                NPC.ai[2] = 0f;
                SetMode(Mode.Drift);
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (Current == Mode.Exhausted)
                modifiers.FinalDamage *= 1.35f;
        }

        public override void FindFrame(int frameHeight)
        {
            int frame;

            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
                frame = (int)(NPC.frameCounter / PulseFrameTicks) % FrameCount;
            }
            else
            {
                switch (Current)
                {
                    case Mode.Charge:
                        frame = PushFrame - 1;
                        break;
                    case Mode.Discharge:
                        frame = PushFrame;
                        break;
                    case Mode.Exhausted:
                        frame = (int)(NPC.ai[2] / ExhaustFrameTicks);
                        break;
                    default:
                        frame = (int)(NPC.ai[2] / PulseFrameTicks);
                        break;
                }
            }

            NPC.frame.Y = Math.Clamp(frame, 0, FrameCount - 1) * frameHeight;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            int count = NPC.life <= 0 ? 24 : 4;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<DeepMagicDust>(), 0f, 0f, 100, default, Main.rand.NextFloat(1f, 1.6f));
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() / 2f;
            Vector2 offsetY = new Vector2(0f, NPC.gfxOffY);
            Vector2 pos = NPC.Center - screenPos + offsetY;
            float time = Main.GlobalTimeWrappedHourly;

            float charge = Current == Mode.Charge ? NPC.ai[1] / ChargeTime : Current == Mode.Discharge ? 1f : 0f;
            float exhaust = Current == Mode.Exhausted ? 1f : 0f;
            float glow = NPC.localAI[1];

            Vector2 shake = charge > 0f
                ? new Vector2(MathF.Sin(time * 70f), MathF.Cos(time * 83f)) * 2.5f * charge
                : Vector2.Zero;

            float speed = NPC.velocity.Length();
            if (speed > 2.5f)
            {
                float strength = MathHelper.Clamp(speed / 6f, 0f, 1f);
                for (int k = 1; k < NPC.oldPos.Length; k++)
                {
                    if (NPC.oldPos[k] == Vector2.Zero)
                        continue;

                    float fade = 1f - k / (float)NPC.oldPos.Length;
                    Vector2 p = NPC.oldPos[k] + NPC.Size / 2f - screenPos + offsetY;
                    Main.EntitySpriteDraw(tex, p, frame, new Color(255, 168, 135, 0) * (fade * 0.25f * strength),
                        NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
                }
            }

            Color halo = new Color(255, 168, 135, 0) * (glow * 0.5f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = (MathHelper.PiOver2 * i + time * 1.5f).ToRotationVector2() * (2f + 4f * glow);
                Main.EntitySpriteDraw(tex, pos + shake + off, frame, halo, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
            }

            Color body = Color.Lerp(drawColor, Color.White, glow * 0.45f);
            body = Color.Lerp(body, new Color(80, 90, 120), exhaust * 0.45f);

            Main.EntitySpriteDraw(tex, pos + shake, frame, body, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}