using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.Ocean
{
    public class TheCoreOfTheOceans : ModNPC
    {
        private const int MaxOrbsPerRing = 5;
        private const int MaxRingCount = 3;
        private const int MaxOrbCount = MaxOrbsPerRing * MaxRingCount;
        private const int TrailLength = 5;

        private float ringYaw;
        private float ringPitch;
        private float ringRoll;

        private float[] orbBobPhase = new float[MaxOrbCount];
        private float[] rayRot = new float[MaxOrbCount];
        private float[] rayRotSpeed = new float[MaxOrbCount];
        private bool raysInited;

        private Vector2[][] orbTrails = new Vector2[MaxOrbCount][];
        private bool trailsInited;

        private static readonly float[] RingRadii = new float[] { 140f, 235f, 345f };

        private const float RotationSpeed = 0.0085f;
        private const float TiltSpeed = 0.0034f;
        private const float BobAmplitude = 4.8f;

        private float orbitExpandTimer;
        private float orbitExpandProgress;
        private float orbitExpandCooldown;
        private const float OrbitExpandDuration = 90f;
        private const float OrbitExpandCooldownMin = 420f;
        private const float OrbitExpandCooldownMax = 780f;
        private const float OrbitExpandMaxScale = 1.35f;

        private bool PostLunar => NPC.downedMoonlord;
        private int ActiveRingCount => PostLunar ? MaxRingCount : 1;
        private int ActiveOrbCount => PostLunar ? MaxOrbCount : MaxOrbsPerRing;

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

            for (int i = 0; i < MaxOrbCount; i++)
                orbBobPhase[i] = i * 1.3f;

            orbitExpandCooldown = Main.rand.NextFloat(OrbitExpandCooldownMin, OrbitExpandCooldownMax);
        }

        public override void AI()
        {
            if (!raysInited)
            {
                for (int i = 0; i < MaxOrbCount; i++)
                {
                    rayRot[i] = Main.rand.NextFloat(MathHelper.TwoPi);
                    rayRotSpeed[i] = Main.rand.NextFloat(-0.04f, 0.04f);
                }
                raysInited = true;
            }

            if (!trailsInited)
            {
                for (int i = 0; i < MaxOrbCount; i++)
                {
                    orbTrails[i] = new Vector2[TrailLength];
                    for (int j = 0; j < TrailLength; j++)
                        orbTrails[i][j] = NPC.Center;
                }
                trailsInited = true;
            }

            ringYaw += RotationSpeed;
            float t = Main.GlobalTimeWrappedHourly;
            ringPitch = MathF.Sin(t * TiltSpeed * 16f) * 0.48f;
            ringRoll = MathF.Cos(t * TiltSpeed * 12f) * 0.32f;

            for (int i = 0; i < MaxOrbCount; i++)
            {
                orbBobPhase[i] += 0.034f;
                rayRot[i] += rayRotSpeed[i];
            }

            UpdateOrbitExpand();

            UpdateOrbTrails();

            if (Main.rand.NextBool(5) && ParticleSystem.BubbleBuffer != null)
            {
                int ring = Main.rand.Next(ActiveRingCount);
                float angle = ringYaw + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = RingRadii[ring];
                Vector3 local = new Vector3(
                    MathF.Cos(angle) * radius,
                    0f,
                    MathF.Sin(angle) * radius
                );
                local = Vector3.Transform(local, Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll));
                Vector2 spawnPos = NPC.Center + new Vector2(local.X, local.Y);

                ParticleSystem.BubbleBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    Main.rand.NextVector2Circular(0.6f, 0.6f).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(Main.rand.NextFloat(1.8f, 3.2f)),
                    new Color(40, 90, 180, 200),
                    Main.rand.Next(50, 80)
                ));
            }

            if (PostLunar && Main.rand.NextBool(3) && ParticleSystem.BubbleBuffer != null)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float spawnRadius = Main.rand.NextFloat(20f, 60f);
                Vector2 spawnPos = NPC.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * spawnRadius;
                Vector2 vel = (NPC.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.4f);

                ParticleSystem.BubbleBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SystemVector2(Main.rand.NextFloat(2.0f, 3.5f)),
                    new Color(50, 110, 200, 210),
                    Main.rand.Next(55, 85)
                ));
            }

            if (!Main.dedServ && ParticleSystem.MegasparkBuffer != null)
            {
                for (int i = 0; i < ActiveOrbCount; i++)
                {
                    Vector2 orbCenter = GetOrbWorldPosition(i);
                    int ring = i / MaxOrbsPerRing;
                    float distanceFactor = RingRadii[ring] / RingRadii[0];
                    int particleChance = Math.Max(1, (int)(3 / distanceFactor));

                    if (Main.rand.NextBool(particleChance))
                    {
                        Vector2 vel = new Vector2(
                            Main.rand.NextFloat(-0.35f, 0.35f),
                            Main.rand.NextFloat(1.4f, 2.4f) * distanceFactor
                        );
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            orbCenter.ToNumerics(),
                            vel.ToNumerics(),
                            0f,
                            new SystemVector2(Main.rand.NextFloat(28f, 40f) * MathHelper.Lerp(1f, 1.35f, distanceFactor - 1f)),
                            new Color(12, 28, 75, 190),
                            Main.rand.Next(40, 65)
                        ));
                    }

                    if (Main.rand.NextBool(particleChance + 1))
                    {
                        float outlineAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float outlineRadius = Main.rand.NextFloat(18f, 28f) * distanceFactor;
                        Vector2 offset = new Vector2(MathF.Cos(outlineAngle), MathF.Sin(outlineAngle)) * outlineRadius;
                        Vector2 spawnPos = orbCenter + offset;
                        Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.15f, 0.55f) + Main.rand.NextVector2Circular(0.25f, 0.25f);
                        float size = Main.rand.NextFloat(10f, 18f) * MathHelper.Lerp(1f, 1.3f, distanceFactor - 1f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            spawnPos.ToNumerics(),
                            vel.ToNumerics(),
                            outlineAngle + Main.rand.NextFloat(-0.4f, 0.4f),
                            new SystemVector2(size, size * Main.rand.NextFloat(0.6f, 1.1f)),
                            new Color(8, 22, 55, 170),
                            Main.rand.Next(18, 32)
                        ));
                    }

                    if (Main.rand.NextBool(particleChance + 2))
                    {
                        float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(8f, 16f) * distanceFactor;
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

        private void UpdateOrbitExpand()
        {
            if (!PostLunar)
            {
                orbitExpandProgress = MathHelper.Lerp(orbitExpandProgress, 0f, 0.08f);
                return;
            }

            if (orbitExpandTimer > 0f)
            {
                orbitExpandTimer -= 1f;
                float raw = 1f - orbitExpandTimer / OrbitExpandDuration;
                float smooth = MathF.Sin(raw * MathF.PI);
                orbitExpandProgress = smooth;
            }
            else
            {
                orbitExpandProgress = MathHelper.Lerp(orbitExpandProgress, 0f, 0.06f);
                orbitExpandCooldown -= 1f;
                if (orbitExpandCooldown <= 0f)
                {
                    orbitExpandTimer = OrbitExpandDuration;
                    orbitExpandCooldown = Main.rand.NextFloat(OrbitExpandCooldownMin, OrbitExpandCooldownMax);
                    SpawnExpandFlash();
                }
            }
        }

        private void SpawnExpandFlash()
        {
            if (Main.dedServ)
                return;

            if (ParticleSystem.BubbleBuffer != null)
            {
                for (int i = 0; i < 70; i++)
                {
                    float angle = MathHelper.TwoPi * i / 70f + Main.rand.NextFloat(-0.08f, 0.08f);
                    Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    Vector2 spawnPos = NPC.Center + dir * Main.rand.NextFloat(25f, 55f);
                    Vector2 vel = dir * Main.rand.NextFloat(2.5f, 6.5f);

                    ParticleSystem.BubbleBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(Main.rand.NextFloat(2.2f, 4.0f)),
                        new Color(35, 80, 170, 220),
                        Main.rand.Next(50, 85)
                    ));
                }
            }

            if (ParticleSystem.MegasparkBuffer != null)
            {
                for (int i = 0; i < 30; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    Vector2 spawnPos = NPC.Center + dir * Main.rand.NextFloat(30f, 80f);
                    Vector2 vel = dir * Main.rand.NextFloat(4f, 9f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        angle,
                        new SystemVector2(Main.rand.NextFloat(20f, 40f)),
                        new Color(140, 200, 255, 200),
                        Main.rand.Next(30, 55)
                    ));
                }
            }
        }

        private float GetOrbitExpandFactor()
        {
            return 1f + orbitExpandProgress * (OrbitExpandMaxScale - 1f);
        }

        private void UpdateOrbTrails()
        {
            for (int i = 0; i < ActiveOrbCount; i++)
            {
                Vector2 pos = GetOrbWorldPosition(i);
                for (int j = TrailLength - 1; j > 0; j--)
                    orbTrails[i][j] = orbTrails[i][j - 1];
                orbTrails[i][0] = pos;
            }
        }

        private Vector2 GetOrbWorldPosition(int i)
        {
            int ring = i / MaxOrbsPerRing;
            int indexInRing = i % MaxOrbsPerRing;
            float angle = ringYaw + MathHelper.TwoPi / MaxOrbsPerRing * indexInRing + ring * 0.4f;
            float radius = RingRadii[ring] * GetOrbitExpandFactor();

            Vector3 local = new Vector3(
                MathF.Cos(angle) * radius,
                0f,
                MathF.Sin(angle) * radius
            );
            local = Vector3.Transform(local, Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll));
            local.Y += MathF.Sin(orbBobPhase[i]) * BobAmplitude;
            return NPC.Center + new Vector2(local.X, local.Y);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D crystalTex = TextureAssets.Npc[Type].Value;
            Vector2 origin = new Vector2(50f, 72.5f);
            Vector2 basePos = NPC.Center - screenPos;
            float time = Main.GlobalTimeWrappedHourly;

            Matrix planeRotation = Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll);

            float[] depths = new float[MaxOrbCount];
            Vector2[] screenPositions = new Vector2[MaxOrbCount];

            for (int i = 0; i < ActiveOrbCount; i++)
            {
                int ring = i / MaxOrbsPerRing;
                int indexInRing = i % MaxOrbsPerRing;
                float angle = ringYaw + MathHelper.TwoPi / MaxOrbsPerRing * indexInRing + ring * 0.4f;
                float radius = RingRadii[ring] * GetOrbitExpandFactor();

                Vector3 local = new Vector3(
                    MathF.Cos(angle) * radius,
                    0f,
                    MathF.Sin(angle) * radius
                );
                local = Vector3.Transform(local, planeRotation);
                local.Y += MathF.Sin(orbBobPhase[i]) * BobAmplitude;
                depths[i] = local.Z;
                screenPositions[i] = NPC.Center + new Vector2(local.X, local.Y) - screenPos;
            }
            DrawOrbitRings(spriteBatch, screenPos, time);
            DrawOrbRays(spriteBatch, screenPositions, depths, time);
            if (PostLunar)
                DrawArcs(spriteBatch, depths, time, screenPos);
            DrawOrbs(spriteBatch, screenPositions, depths, true);

            if (PostLunar)
                DrawCrystalMerged(spriteBatch, crystalTex, basePos, origin, drawColor, time);
            else
                DrawCrystalBroken(spriteBatch, crystalTex, basePos, origin, drawColor, time);

            DrawOrbs(spriteBatch, screenPositions, depths, false);
            return false;
        }
        private void DrawOrbitRings(SpriteBatch spriteBatch, Vector2 screenPos, float time)
        {
            Texture2D rayTex = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray").Value;
            Vector2 origin = new Vector2(0f, rayTex.Height * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float expand = GetOrbitExpandFactor();
            Matrix plane = Matrix.CreateFromYawPitchRoll(0f, ringPitch, ringRoll);

            for (int ring = 0; ring < ActiveRingCount; ring++)
            {
                float radius = RingRadii[ring] * expand;
                float pulse = 0.65f + MathF.Sin(time * 1.7f + ring * 1.3f) * 0.35f;
                float alpha = MathHelper.Lerp(0.35f, 0.7f, pulse);

                Color colCore = new Color(60, 150, 255) * alpha;
                Color colMid = new Color(40, 110, 220) * (alpha * 0.7f);
                Color colSoft = new Color(90, 180, 255) * (alpha * 0.35f);

                const int segments = 72;
                Vector2 prev = Vector2.Zero;

                for (int i = 0; i <= segments; i++)
                {
                    float a = MathHelper.TwoPi * i / segments + ringYaw * 0.12f;
                    Vector3 local = new Vector3(MathF.Cos(a) * radius, 0f, MathF.Sin(a) * radius);
                    local = Vector3.Transform(local, plane);
                    Vector2 pos = NPC.Center + new Vector2(local.X, local.Y) - screenPos;

                    if (i > 0)
                    {
                        Vector2 delta = pos - prev;
                        float len = delta.Length();
                        if (len > 0.5f)
                        {
                            float rot = delta.ToRotation();
                            float scaleX = len / rayTex.Width;

                            // мягкое внешнее свечение
                            spriteBatch.Draw(rayTex, prev, null, colSoft, rot, origin,
                                new Vector2(scaleX, 0.18f), SpriteEffects.None, 0f);

                            // средняя полоса
                            spriteBatch.Draw(rayTex, prev, null, colMid, rot, origin,
                                new Vector2(scaleX, 0.09f), SpriteEffects.None, 0f);

                            // яркое ядро
                            spriteBatch.Draw(rayTex, prev, null, colCore, rot, origin,
                                new Vector2(scaleX, 0.04f), SpriteEffects.None, 0f);
                        }
                    }
                    prev = pos;
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawOrbRays(SpriteBatch spriteBatch, Vector2[] screenPos, float[] depths, float time)
        {
            Texture2D rayTex = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray").Value;
            Vector2 rayOrigin = new Vector2(rayTex.Width * 0.5f, rayTex.Height * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < ActiveOrbCount; i++)
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
                spriteBatch.Draw(rayTex, screenPos[i], null, rayColor, rot, rayOrigin, new Vector2(scale * 0.9f, scale * 0.55f), SpriteEffects.None, 0f);
                spriteBatch.Draw(rayTex, screenPos[i], null, rayColor * 0.55f, rot + 0.12f, rayOrigin, new Vector2(scale * 0.7f, scale * 0.4f), SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawArcs(SpriteBatch spriteBatch, float[] depths, float time, Vector2 screenPos)
        {
            Texture2D rayTex = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray").Value;
            Vector2 rayOrigin = new Vector2(rayTex.Width * 0.5f, rayTex.Height * 0.5f);
            Texture2D sparkTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 sparkOrigin = sparkTex.Size() * 0.5f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int ring = 0; ring < MaxRingCount; ring++)
            {
                for (int j = 0; j < MaxOrbsPerRing; j++)
                {
                    int a = ring * MaxOrbsPerRing + j;
                    int b = ring * MaxOrbsPerRing + (j + 1) % MaxOrbsPerRing;
                    DrawLightningArc(spriteBatch, rayTex, rayOrigin, sparkTex, sparkOrigin,
                        orbTrails[a][0], orbTrails[b][0], depths[a], depths[b], time, a * 7 + b * 13);
                }
            }

            for (int j = 0; j < MaxOrbsPerRing; j++)
            {
                int a = 0 * MaxOrbsPerRing + j;
                int b = 2 * MaxOrbsPerRing + j;
                DrawLightningArc(spriteBatch, rayTex, rayOrigin, sparkTex, sparkOrigin,
                    orbTrails[a][0], orbTrails[b][0], depths[a], depths[b], time, a * 5 + b * 11);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawLightningArc(SpriteBatch spriteBatch, Texture2D rayTex, Vector2 rayOrigin,
            Texture2D sparkTex, Vector2 sparkOrigin, Vector2 start, Vector2 end,
            float depthA, float depthB, float time, int seed)
        {
            float avgDepth = (depthA + depthB) * 0.5f;
            float depthFactor = MathHelper.Clamp(avgDepth / 105f, -1f, 1f);
            float visibility = MathHelper.Lerp(0.45f, 1f, (depthFactor + 1f) * 0.5f);

            float flicker = MathF.Sin(time * 9.5f + seed * 2.3f) * 0.5f + 0.5f;
            float flicker2 = MathF.Sin(time * 17.3f + seed * 1.1f) * 0.5f + 0.5f;
            float intensity = MathHelper.Lerp(0.25f, 1f, flicker * 0.6f + flicker2 * 0.4f);

            if (intensity < 0.3f)
                return;

            Vector2 delta = end - start;
            float length = delta.Length();
            if (length < 1f)
                return;

            const int segments = 8;
            Vector2 prev = start;

            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)segments;
                Vector2 point = Vector2.Lerp(start, end, t);

                if (s < segments)
                {
                    Vector2 perp = new Vector2(-delta.Y, delta.X) / length;
                    float taper = MathF.Sin(t * MathF.PI);
                    float offset = MathF.Sin(time * 13f + seed + s * 1.7f) * 14f * taper;
                    offset += MathF.Sin(time * 27f + seed * 0.7f + s * 3.1f) * 6f * taper;
                    point += perp * offset;
                }

                Vector2 segDelta = point - prev;
                float segLen = segDelta.Length();
                if (segLen > 0.5f)
                {
                    float rot = segDelta.ToRotation();
                    float scaleX = segLen / rayTex.Width;
                    float scaleY = 0.07f * intensity;

                    Color core = new Color(180, 230, 255) * (0.85f * intensity * visibility);
                    Color mid = new Color(80, 160, 255) * (0.55f * intensity * visibility);
                    Color outer = new Color(40, 90, 200) * (0.3f * intensity * visibility);

                    spriteBatch.Draw(rayTex, prev, null, outer, rot, new Vector2(0f, rayTex.Height * 0.5f), new Vector2(scaleX, scaleY * 3.2f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(rayTex, prev, null, mid, rot, new Vector2(0f, rayTex.Height * 0.5f), new Vector2(scaleX, scaleY * 1.9f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(rayTex, prev, null, core, rot, new Vector2(0f, rayTex.Height * 0.5f), new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                }

                if (s < segments)
                {
                    float nodeAlpha = intensity * visibility * 0.7f;
                    Color nodeColor = new Color(160, 220, 255) * nodeAlpha;
                    float nodeScale = Main.rand.NextFloat(0.12f, 0.22f) * intensity;
                    spriteBatch.Draw(sparkTex, point, null, nodeColor, 0f, sparkOrigin, nodeScale, SpriteEffects.None, 0f);
                }

                prev = point;
            }

            float endPulse = 0.85f + MathF.Sin(time * 12f + seed) * 0.15f;
            Color endColor = new Color(200, 240, 255) * (0.6f * intensity * visibility * endPulse);
            spriteBatch.Draw(sparkTex, start, null, endColor, 0f, sparkOrigin, 0.18f * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(sparkTex, end, null, endColor, 0f, sparkOrigin, 0.18f * intensity, SpriteEffects.None, 0f);
        }

        private void DrawCrystalBroken(SpriteBatch spriteBatch, Texture2D tex, Vector2 basePos,
            Vector2 origin, Color drawColor, float time)
        {
            Rectangle upperFrame = new Rectangle(0, 0, 100, 145);
            Rectangle lowerFrame = new Rectangle(0, 145, 100, 145);

            float upperFloat = MathF.Sin(time * 0.9f) * 2.4f;
            float upperTilt = MathF.Sin(time * 0.7f) * 0.06f;
            Vector2 upperPos = basePos + new Vector2(MathF.Sin(time * 0.5f) * 2f, upperFloat - 8f);

            float lowerFloat = MathF.Sin(time * 1.1f + 1.7f) * 1.6f;
            float lowerTilt = MathF.Sin(time * 0.85f + 2.3f) * -0.05f;
            Vector2 lowerPos = basePos + new Vector2(MathF.Cos(time * 0.6f) * 1.4f, lowerFloat + 6f);

            Color softGlow = new Color(80, 140, 190) * 0.22f;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + time * 0.4f;
                Vector2 glowPos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 4.5f;
                spriteBatch.Draw(tex, upperPos + glowPos, upperFrame, softGlow, upperTilt, origin, NPC.scale * 1.08f, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, lowerPos + glowPos, lowerFrame, softGlow * 0.7f, lowerTilt, origin, NPC.scale * 1.06f, SpriteEffects.None, 0f);
            }

            Color lowerGlow = new Color(70, 130, 180) * 0.4f;
            for (int i = 0; i < 3; i++)
            {
                Vector2 off = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f));
                spriteBatch.Draw(tex, lowerPos + off, lowerFrame, lowerGlow, lowerTilt, origin, NPC.scale * 1.02f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, lowerPos, lowerFrame, drawColor, lowerTilt, origin, NPC.scale, SpriteEffects.None, 0f);

            Color upperGlow = new Color(110, 170, 210) * 0.75f;
            for (int i = 0; i < 5; i++)
            {
                Vector2 off = new Vector2(Main.rand.NextFloat(-1.8f, 1.8f), Main.rand.NextFloat(-1.8f, 1.8f));
                spriteBatch.Draw(tex, upperPos + off, upperFrame, upperGlow, upperTilt, origin, NPC.scale * 1.035f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, upperPos, upperFrame, drawColor, upperTilt, origin, NPC.scale, SpriteEffects.None, 0f);

            float crackPulse = 0.55f + MathF.Sin(time * 3.6f) * 0.45f;
            Color crackGlow = new Color(120, 200, 255) * (0.35f * crackPulse);
            Texture2D sparkTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 sparkOrigin = sparkTex.Size() * 0.5f;
            Vector2 crackCenter = (upperPos + lowerPos) * 0.5f;
            spriteBatch.Draw(sparkTex, crackCenter, null, crackGlow, 0f, sparkOrigin, new Vector2(0.6f, 0.25f), SpriteEffects.None, 0f);
        }

        private void DrawCrystalMerged(SpriteBatch spriteBatch, Texture2D tex, Vector2 basePos,
    Vector2 origin, Color drawColor, float time)
        {
            float breath = 1f + MathF.Sin(time * 1.6f) * 0.035f;
            float scale = NPC.scale * breath;

            Color outlineColor = new Color(50, 130, 220) * 0.5f;
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + time * 0.3f;
                Vector2 outlineOff = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 6.5f;
                spriteBatch.Draw(tex, basePos + outlineOff, new Rectangle(0, 0, 100, 145), outlineColor, 0f, origin, scale * 1.08f, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, basePos + outlineOff, new Rectangle(0, 145, 100, 145), outlineColor * 0.9f, 0f, origin, scale * 1.08f, SpriteEffects.None, 0f);
            }

            Color innerGlow = new Color(90, 170, 255) * 0.4f;
            for (int i = 0; i < 5; i++)
            {
                Vector2 gOff = Main.rand.NextVector2Circular(2.2f, 2.2f);
                spriteBatch.Draw(tex, basePos + gOff, new Rectangle(0, 0, 100, 145), innerGlow, 0f, origin, scale * 1.03f, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, basePos + gOff, new Rectangle(0, 145, 100, 145), innerGlow * 0.85f, 0f, origin, scale * 1.03f, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(tex, basePos, new Rectangle(0, 145, 100, 145), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, basePos, new Rectangle(0, 0, 100, 145), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            Texture2D sparkTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 sparkOrigin = sparkTex.Size() * 0.5f;
            float corePulse = 0.7f + MathF.Sin(time * 2.8f) * 0.3f;
            Color coreColor = new Color(160, 220, 255) * (0.55f * corePulse);
            spriteBatch.Draw(sparkTex, basePos, null, coreColor, 0f, sparkOrigin, 0.55f * breath, SpriteEffects.None, 0f);
        }

        private void DrawOrbs(SpriteBatch spriteBatch, Vector2[] screenPos, float[] depths, bool behind)
        {
            Texture2D orbTex = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Ocean/Waterkiller").Value;
            Texture2D megaspark = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;

            Vector2 orbOrigin = orbTex.Size() / 2f;
            Vector2 megaOrigin = megaspark.Size() * 0.5f;
            float aspect = megaspark.Width / (float)megaspark.Height;

            float time = Main.GlobalTimeWrappedHourly * 2.5f;

            for (int i = 0; i < ActiveOrbCount; i++)
            {
                bool isBehind = depths[i] < 0f;
                if (isBehind != behind) continue;

                int ring = i / MaxOrbsPerRing;
                float depthFactor = MathHelper.Clamp(depths[i] / 105f, -1f, 1f);
                float scale = MathHelper.Lerp(0.65f, 1.28f, (depthFactor + 1f) * 0.5f);
                float alpha = MathHelper.Lerp(0.45f, 1f, (depthFactor + 1f) * 0.5f);

                float radiusScale = MathHelper.Lerp(1f, 1.4f, (RingRadii[ring] - RingRadii[0]) / (RingRadii[2] - RingRadii[0]));
                scale *= radiusScale;

                float expandPulse = 1f + orbitExpandProgress * 0.25f;
                scale *= expandPulse;

                Vector2 drawPos = screenPos[i];

                DrawMotionBlur(spriteBatch, orbTex, orbOrigin, i, depths[i], scale, alpha);

                float pulse = 0.85f + MathF.Sin(time + i * 1.3f) * 0.15f;
                float baseScale = scale * 0.11f * pulse;
                Vector2 scaleVec = new Vector2(baseScale / aspect, baseScale);

                Main.EntitySpriteDraw(megaspark, drawPos, null,
                    new Color(60, 130, 235) * (0.55f * alpha),
                    time * 0.15f + i, megaOrigin, scaleVec, SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(megaspark, drawPos, null,
                    new Color(110, 185, 255) * (0.7f * alpha),
                    -time * 0.25f + i * 0.7f, megaOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(megaspark, drawPos, null,
                    new Color(175, 220, 255) * (0.85f * alpha),
                    time * 0.4f + i * 1.1f, megaOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(megaspark, drawPos, null,
                    new Color(235, 248, 255) * (0.95f * alpha),
                    -time * 0.6f + i * 0.4f, megaOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);

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

        private void DrawMotionBlur(SpriteBatch spriteBatch, Texture2D orbTex, Vector2 orbOrigin,
            int orbIndex, float depth, float scale, float alpha)
        {
            Vector2[] trail = orbTrails[orbIndex];
            if (trail == null)
                return;

            float depthFactor = MathHelper.Clamp(depth / 105f, -1f, 1f);
            Color baseTint = new Color(70, 140, 220);
            if (depthFactor < 0)
                baseTint = Color.Lerp(baseTint, new Color(50, 100, 170), -depthFactor * 0.5f);

            for (int t = 1; t < TrailLength; t++)
            {
                float trailT = t / (float)TrailLength;
                float trailAlpha = (1f - trailT) * 0.32f * alpha;
                float trailScale = scale * (1f - trailT * 0.28f);

                Vector2 pos = trail[t] - Main.screenPosition;
                spriteBatch.Draw(orbTex, pos, null, baseTint * trailAlpha,
                    0f, orbOrigin, trailScale, SpriteEffects.None, 0f);
            }
        }
    }
}   