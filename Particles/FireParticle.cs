using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;

namespace Waybound.Particles
{
    public class FireParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/FireParticle";

        private float wobbleSeed;
        private float flickerSeed;
        private float riseSpeed;
        private float rotSpeed;
        private float stretchMax;
        private float sizeMultiplier;
        private int variant;
        private Color colorHot, colorMid, colorCold;

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(255, 140, 40, 255);

            wobbleSeed = Main.rand.NextFloat(MathHelper.TwoPi);
            flickerSeed = Main.rand.NextFloat(MathHelper.TwoPi);
            rotSpeed = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? 1f : -1f);

            float temp = Main.rand.NextFloat();
            if (temp < 0.3f)
            {
                colorHot = new Color(255, 190, 80);
                colorMid = new Color(220, 80, 20);
                colorCold = new Color(120, 15, 5);
            }
            else if (temp > 0.7f)
            {
                colorHot = new Color(255, 255, 230);
                colorMid = new Color(255, 210, 120);
                colorCold = new Color(255, 140, 50);
            }
            else
            {
                colorHot = new Color(255, 240, 180);
                colorMid = new Color(255, 140, 40);
                colorCold = new Color(180, 40, 20);
            }

            variant = Main.rand.Next(3);

            if (variant == 0)
            {
                riseSpeed = Main.rand.NextFloat(0.03f, 0.07f);
                stretchMax = 1.6f;
                sizeMultiplier = 1.35f;
            }
            else if (variant == 1)
            {
                riseSpeed = Main.rand.NextFloat(0.12f, 0.20f);
                stretchMax = 1.15f;
                sizeMultiplier = 0.6f;
            }
            else
            {
                riseSpeed = Main.rand.NextFloat(0.06f, 0.10f);
                stretchMax = 1.35f;
                sizeMultiplier = 1f;
            }
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)Math.Max(info.Duration, 1);
            float progress = 1f - life;

            info.Velocity.X *= 0.97f;
            info.Velocity.Y *= 0.97f;
            info.Velocity.Y -= riseSpeed;

            float wobbleX = MathF.Sin(progress * 6f + wobbleSeed) * 0.08f * life;
            float wobbleY = MathF.Cos(progress * 5f + wobbleSeed * 1.7f) * 0.05f * life;
            info.Velocity.X += wobbleX;
            info.Velocity.Y += wobbleY;

            info.Position += info.Velocity;
            info.Rotation += rotSpeed * (0.5f + life * 0.5f);

            float baseScale = info.InitialScale.X * sizeMultiplier;
            if (baseScale < 1f)
                baseScale = 24f;

            float shrink = MathHelper.Lerp(1.15f, 0.35f, progress * progress);
            float fade = life > 0.15f ? 1f : life / 0.15f;
            float size = baseScale * shrink * fade;

            float stretchX = MathHelper.Lerp(1f, 0.85f, progress);
            float stretchY = MathHelper.Lerp(1f, stretchMax, progress);
            info.Scale = new System.Numerics.Vector2(size * stretchX, size * stretchY);

            Color current;
            if (progress < 0.35f)
            {
                float t = progress / 0.35f;
                current = Color.Lerp(colorHot, colorMid, t);
            }
            else
            {
                float t = (progress - 0.35f) / 0.65f;
                current = Color.Lerp(colorMid, colorCold, t);
            }

            float flicker = 0.85f
                + MathF.Sin(progress * 38f + flickerSeed) * 0.12f
                + MathF.Sin(progress * 71f + flickerSeed * 2.3f) * 0.08f;

            float appear = MathHelper.Clamp(progress * 10f, 0f, 1f);
            float alpha = (float)Math.Pow(MathHelper.Clamp(life, 0f, 1f), 0.5f) * flicker * appear;
            info.Color = current * alpha;

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                1.5f * alpha,
                0.7f * alpha,
                0.25f * alpha
            );

            if (progress < 0.5f && Main.rand.NextBool(9) && ParticleSystem.FireBuffer != null)
            {
                Vector2 velocityXna = new Vector2(info.Velocity.X, info.Velocity.Y);
                Vector2 sparkVel = velocityXna * 0.4f + Main.rand.NextVector2Circular(2f, 2f);

                ParticleSystem.FireBuffer.Create(new ParticleInfo(
                    info.Position,
                    sparkVel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(info.InitialScale.X * 0.35f),
                    new Color(255, 220, 140),
                    Main.rand.Next(8, 14)
                ));
            }

            info.Time--;
        }
    }
}