using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.NPCs.Bosses.Themis
{
    public class ThemisDroneMinion : ModNPC
    {
        public int frame;

        private float outlinePulse;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 30;
            NPC.damage = 20;
            NPC.defense = 3;
            NPC.lifeMax = 15;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = false;
            NPC.friendly = false;
            NPC.aiStyle = -1;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest(true);
            NPC.ai[3] = Main.rand.NextBool() ? 1f : -1f;
            base.OnSpawn(source);
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(true);
                player = Main.player[NPC.target];
            }

            NPC.rotation = NPC.velocity.X * 0.08f;
            NPC.spriteDirection = NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
            NPC.ai[0]++;

            if (NPC.ai[0] < 90)
            {
                Vector2 hoverTarget = player.Center + new Vector2(NPC.ai[3] * 280f, -60f + (float)Math.Sin(NPC.ai[0] * 0.08f) * 40f);
                Vector2 toTarget = hoverTarget - NPC.Center;
                float dist = toTarget.Length();
                float speed = dist > 400f ? 16f : 7f;
                NPC.velocity = (NPC.velocity * 19f + toTarget.SafeNormalize(Vector2.Zero) * speed) / 20f;

                if (NPC.ai[0] >= 55)
                {
                    float t = (NPC.ai[0] - 55f) / 35f;
                    outlinePulse = (float)Math.Sin(NPC.ai[0] * 0.35f) * 0.5f + 0.5f;
                    outlinePulse *= MathHelper.Clamp(t, 0f, 1f);
                }
                else
                {
                    outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.12f);
                }
            }
            else if (NPC.ai[0] == 90)
            {
                Vector2 dashDir = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = dashDir * 24f;
                outlinePulse = 0f;
                NPC.netUpdate = true;
            }
            else if (NPC.ai[0] > 90 && NPC.ai[0] < 125)
            {
                NPC.velocity *= 0.97f;
                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.15f);
            }
            else if (NPC.ai[0] >= 125)
            {
                NPC.ai[0] = 0;
                NPC.ai[3] = Main.rand.NextBool() ? 1f : -1f;
                outlinePulse = 0f;
                NPC.netUpdate = true;
            }

            Lighting.AddLight(NPC.position, 1.2f, 0.6f, 0.3f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (outlinePulse > 0.05f)
            {
                Color glow = new Color(255, 40, 30, 0) * outlinePulse * 0.75f;
                float scaleAdd = 1f + outlinePulse * 0.18f;

                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = new Vector2(3.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 6f + Main.GlobalTimeWrappedHourly * 2f);
                    spriteBatch.Draw(texture, drawPos + offset, frame, glow, NPC.rotation, origin, NPC.scale * scaleAdd, effects, 0f);
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(6.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    spriteBatch.Draw(texture, drawPos + offset, frame, glow * 0.45f, NPC.rotation, origin, NPC.scale * (scaleAdd + 0.06f), effects, 0f);
                }
            }

            spriteBatch.Draw(texture, drawPos, frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frame * frameHeight;
            NPC.frameCounter++;
            if (NPC.frameCounter > 3)
            {
                frame++;
                if (frame >= 4)
                    frame = 0;
                NPC.frameCounter = 0;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.TargetClosest(true);
            base.HitEffect(hit);
        }

        public override void OnKill()
        {
            Player player = Main.player[NPC.target];
            player.PlayerScreen().ScreenShakeIntensity = 8;

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 flameVel = Main.rand.NextVector2Circular(5f, 5f);
                    ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                        NPC.Center.ToNumerics(),
                        flameVel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(2f, 2f),
                        new Color(255, 140, 25, 0),
                        60
                    ));
                }

                for (int i = 0; i < 12; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(8f, 8f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        NPC.Center.ToNumerics(),
                        sparkVel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(1f, 1f),
                        new Color(255, 120, 30, 210),
                        Main.rand.Next(15, 30)
                    ));
                }
            }

            for (int i = 0; i < Main.rand.Next(2, 5); i++)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f)), Main.rand.Next(61, 64));
            }
        }
    }
}