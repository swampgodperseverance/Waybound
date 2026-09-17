using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.Ocean
{
    public class TheCoreOfTheOceans : ModNPC
    {
        private const int OrbCount = 5;
        private float ringYaw;
        private float ringPitch;
        private float ringRoll;
        private float[] orbBobPhase = new float[OrbCount];
        private float[] rayRot = new float[OrbCount];
        private float[] rayRotSpeed = new float[OrbCount];
        private bool raysInited;
        private const float OrbRadius = 118f;
        private const float RotationSpeed = 0.0085f;
        private const float TiltSpeed = 0.0034f;
        private const float BobAmplitude = 4.8f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 145;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0f;
            NPC.npcSlots = 1f;
            NPC.aiStyle = -1;
            for (int i = 0; i < OrbCount; i++)
                orbBobPhase[i] = i * 1.3f;
        }

        public override void AI()
        {
            if (!raysInited)
            {
                for (int i = 0; i < OrbCount; i++)
                {
                    rayRot[i] = Main.rand.NextFloat(MathHelper.TwoPi);
                    rayRotSpeed[i] = Main.rand.NextFloat(-0.04f, 0.04f);
                }
                raysInited = true;
            }

            ringYaw += RotationSpeed;
            float t = Main.GlobalTimeWrappedHourly;
            ringPitch = MathF.Sin(t * TiltSpeed * 16f) * 0.48f;
            ringRoll = MathF.Cos(t * TiltSpeed * 12f) * 0.32f;

            for (int i = 0; i < OrbCount; i++)
            {
                orbBobPhase[i] += 0.034f;
                rayRot[i] += rayRotSpeed[i];
            }

            if (Main.rand.NextBool(5))
            {
                float angle = ringYaw + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector3 local = new Vector3(
                    MathF.Cos(angle) * OrbRadius,
                    0f,
                    MathF.Sin(angle) * OrbRadius
                );
                local = Vector3.Transform(local, Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll));
                Vector2 spawnPos = NPC.Center + new Vector2(local.X, local.Y);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.BlueCrystalShard, Vector2.Zero, 160, new Color(90, 140, 190), 0.75f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                d.fadeIn = 0.85f;
            }

            if (!Main.dedServ && ParticleSystem.MegasparkBuffer != null)
            {
                for (int i = 0; i < OrbCount; i++)
                {
                    float angle = ringYaw + MathHelper.TwoPi / OrbCount * i;
                    float radius = OrbRadius * Main.rand.NextFloat(0.96f, 1.04f);
                    Vector3 local = new Vector3(
                        MathF.Cos(angle) * radius,
                        0f,
                        MathF.Sin(angle) * radius
                    );
                    local = Vector3.Transform(local, Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll));
                    local.Y += MathF.Sin(orbBobPhase[i]) * BobAmplitude * 0.6f;
                    Vector2 orbCenter = NPC.Center + new Vector2(local.X, local.Y);

                    if (Main.rand.NextBool(2))
                    {
                        Vector2 vel = new Vector2(
                            Main.rand.NextFloat(-0.35f, 0.35f),
                            Main.rand.NextFloat(1.4f, 2.4f)
                        );
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            orbCenter.ToNumerics(),
                            vel.ToNumerics(),
                            0f,
                            new SystemVector2(Main.rand.NextFloat(28f, 40f)),
                            new Color(12, 28, 75, 190),
                            Main.rand.Next(40, 65)
                        ));
                    }

                    if (Main.rand.NextBool(3))
                    {
                        float outlineAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float outlineRadius = Main.rand.NextFloat(18f, 28f);
                        Vector2 offset = new Vector2(MathF.Cos(outlineAngle), MathF.Sin(outlineAngle)) * outlineRadius;
                        Vector2 spawnPos = orbCenter + offset;
                        Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.15f, 0.55f) + Main.rand.NextVector2Circular(0.25f, 0.25f);
                        float size = Main.rand.NextFloat(10f, 18f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            spawnPos.ToNumerics(),
                            vel.ToNumerics(),
                            outlineAngle + Main.rand.NextFloat(-0.4f, 0.4f),
                            new SystemVector2(size, size * Main.rand.NextFloat(0.6f, 1.1f)),
                            new Color(8, 22, 55, 170),
                            Main.rand.Next(18, 32)
                        ));
                    }

                    if (Main.rand.NextBool(4))
                    {
                        float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(8f, 16f);
                        Vector2 spawnPos = orbCenter + trailOffset;
                        Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.1f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            spawnPos.ToNumerics(),
                            vel.ToNumerics(),
                            trailAngle,
                            new SystemVector2(Main.rand.NextFloat(6f, 12f)),
                            new Color(18, 40, 95, 140),
                            Main.rand.Next(12, 22)
                        ));
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D crystalTex = TextureAssets.Npc[Type].Value;
            Vector2 origin = new Vector2(50f, 72.5f);
            Vector2 basePos = NPC.Center - screenPos;
            float time = Main.GlobalTimeWrappedHourly;

            Vector2 upperOffset = new Vector2(
                MathF.Sin(time * 1.1f + NPC.whoAmI * 0.5f) * 1.6f + MathF.Cos(time * 1.6f) * 0.9f,
                MathF.Cos(time * 0.95f + NPC.whoAmI * 0.3f) * 1.4f + 2.5f
            );
            Vector2 lowerOffset = new Vector2(
                MathF.Sin(time * 0.9f + NPC.whoAmI * 0.8f) * 1.3f + MathF.Cos(time * 1.3f) * 0.7f,
                MathF.Sin(time * 1.2f + NPC.whoAmI * 0.4f) * 1.5f + 6f
            );

            Matrix planeRotation = Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll);
            float[] depths = new float[OrbCount];
            Vector2[] screenPositions = new Vector2[OrbCount];

            for (int i = 0; i < OrbCount; i++)
            {
                float angle = ringYaw + MathHelper.TwoPi / OrbCount * i;
                Vector3 local = new Vector3(
                    MathF.Cos(angle) * OrbRadius,
                    0f,
                    MathF.Sin(angle) * OrbRadius
                );
                local = Vector3.Transform(local, planeRotation);
                local.Y += MathF.Sin(orbBobPhase[i]) * BobAmplitude;
                depths[i] = local.Z;
                screenPositions[i] = NPC.Center + new Vector2(local.X, local.Y) - screenPos;
            }

            DrawOrbRays(spriteBatch, screenPositions, depths, time);
            DrawOrbs(spriteBatch, screenPositions, depths, true);
            DrawCrystal(spriteBatch, crystalTex, basePos, upperOffset, lowerOffset, origin, drawColor, time);
            DrawOrbs(spriteBatch, screenPositions, depths, false);
            return false;
        }

        private void DrawOrbRays(SpriteBatch spriteBatch, Vector2[] screenPos, float[] depths, float time)
        {
            Texture2D rayTex = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray").Value;
            Vector2 rayOrigin = new Vector2(rayTex.Width * 0.5f, rayTex.Height * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < OrbCount; i++)
            {
                float depthFactor = MathHelper.Clamp(depths[i] / 105f, -1f, 1f);
                float alpha = MathHelper.Lerp(0.4f, 0.95f, (depthFactor + 1f) * 0.5f);
                float pulse = 0.55f + MathF.Sin(time * 3.2f + i * 1.7f) * 0.45f;
                if (pulse < 0.15f)
                    continue;

                float scale = MathHelper.Lerp(0.35f, 0.65f, (depthFactor + 1f) * 0.5f) * pulse;
                Color rayColor = new Color(
                    (int)(18 * pulse * 3f),
                    (int)(35 * pulse * 2f),
                    (int)(110 * pulse * 2.5f),
                    0
                ) * alpha;
                float rot = rayRot[i] + MathHelper.Pi;

                spriteBatch.Draw(
                    rayTex,
                    screenPos[i],
                    null,
                    rayColor,
                    rot,
                    rayOrigin,
                    new Vector2(scale * 0.9f, scale * 0.55f),
                    SpriteEffects.None,
                    0f
                );
                spriteBatch.Draw(
                    rayTex,
                    screenPos[i],
                    null,
                    rayColor * 0.55f,
                    rot + 0.12f,
                    rayOrigin,
                    new Vector2(scale * 0.7f, scale * 0.4f),
                    SpriteEffects.None,
                    0f
                );
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCrystal(SpriteBatch spriteBatch, Texture2D tex, Vector2 basePos,
            Vector2 upperOffset, Vector2 lowerOffset, Vector2 origin, Color drawColor, float time)
        {
            Color softGlow = new Color(80, 140, 190) * 0.22f;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + time * 0.4f;
                Vector2 glowPos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 4.5f;
                spriteBatch.Draw(tex, basePos + upperOffset + glowPos,
                    new Rectangle(0, 0, 100, 145), softGlow, 0f, origin, NPC.scale * 1.08f, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, basePos + lowerOffset + glowPos,
                    new Rectangle(0, 145, 100, 145), softGlow * 0.7f, 0f, origin, NPC.scale * 1.06f, SpriteEffects.None, 0f);
            }

            Rectangle lowerFrame = new Rectangle(0, 145, 100, 145);
            Color lowerGlow = new Color(70, 130, 180) * 0.4f;
            for (int i = 0; i < 3; i++)
            {
                Vector2 off = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f));
                spriteBatch.Draw(tex, basePos + lowerOffset + off, lowerFrame, lowerGlow, 0f, origin, NPC.scale * 1.02f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, basePos + lowerOffset, lowerFrame, drawColor, 0f, origin, NPC.scale, SpriteEffects.None, 0f);

            Rectangle upperFrame = new Rectangle(0, 0, 100, 145);
            Color upperGlow = new Color(110, 170, 210) * 0.75f;
            for (int i = 0; i < 5; i++)
            {
                Vector2 off = new Vector2(Main.rand.NextFloat(-1.8f, 1.8f), Main.rand.NextFloat(-1.8f, 1.8f));
                spriteBatch.Draw(tex, basePos + upperOffset + off, upperFrame, upperGlow, 0f, origin, NPC.scale * 1.035f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, basePos + upperOffset, upperFrame, drawColor, 0f, origin, NPC.scale, SpriteEffects.None, 0f);
        }

        private void DrawOrbs(SpriteBatch spriteBatch, Vector2[] screenPos, float[] depths, bool behind)
        {
            Texture2D orbTex = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Ocean/Waterkiller").Value;
            Vector2 orbOrigin = orbTex.Size() / 2f;

            for (int i = 0; i < OrbCount; i++)
            {
                bool isBehind = depths[i] < 0f;
                if (isBehind != behind) continue;

                float depthFactor = MathHelper.Clamp(depths[i] / 105f, -1f, 1f);
                float scale = MathHelper.Lerp(0.65f, 1.28f, (depthFactor + 1f) * 0.5f);
                float alpha = MathHelper.Lerp(0.45f, 1f, (depthFactor + 1f) * 0.5f);
                Vector2 drawPos = screenPos[i];

                Color glowColor = new Color(70, 130, 190) * (0.28f * alpha);
                for (int g = 0; g < 2; g++)
                {
                    float gScale = scale * (1.12f + g * 0.08f);
                    spriteBatch.Draw(orbTex, drawPos, null, glowColor * (1f - g * 0.3f),
                        0f, orbOrigin, gScale, SpriteEffects.None, 0f);
                }

                Color orbColor = Color.White * alpha;
                if (depthFactor < 0)
                    orbColor = Color.Lerp(orbColor, new Color(120, 160, 210), -depthFactor * 0.5f);
                spriteBatch.Draw(orbTex, drawPos, null, orbColor, 0f, orbOrigin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}   