using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using Terraria;

namespace Waybound.Particles
{
    public class FlameBoomParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/FlameBoomParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Color = new Color(255, 220, 150, 255);
            info.Scale = new System.Numerics.Vector2(0.01f, 0.01f);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)Math.Max(info.Duration, 1);
            float progress = 1f - life;

            float grow = 1f - MathF.Pow(1f - progress, 5f);
            float k = 0.15f + 0.85f * grow;
            info.Scale = new System.Numerics.Vector2(info.InitialScale.X * k, info.InitialScale.Y * k);

            info.Position += info.Velocity;
            info.Velocity *= 0.92f;
            info.Rotation += 0.004f * life;

            Color hot = new Color(255, 240, 190);
            Color mid = new Color(255, 140, 40);
            Color cold = new Color(170, 40, 15);

            Color current = progress < 0.25f
                ? Color.Lerp(hot, mid, progress / 0.25f)
                : Color.Lerp(mid, cold, (progress - 0.25f) / 0.75f);

            float fadeIn = MathHelper.Clamp(progress / 0.06f, 0f, 1f);
            float fadeOut = MathF.Pow(MathHelper.Clamp(life, 0f, 1f), 1.6f);
            float alpha = fadeIn * fadeOut;

            info.Color = current * alpha;

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                1.5f * alpha,
                0.6f * alpha,
                0.2f * alpha
            );

            info.Time--;
        }
    }
}