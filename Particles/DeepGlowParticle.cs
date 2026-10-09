using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using System;
using Terraria;

namespace Waybound.Particles
{
    public class DeepGlowParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/DeepGlowParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(255, 168, 175, 255);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)Math.Max(info.Duration, 1);
            float progress = 1f - life;

            info.Velocity *= 1.004f;
            info.Position += info.Velocity;

            float baseScale = info.InitialScale.X;
            if (baseScale < 1f)
                baseScale = 18f;

            float expand = MathHelper.Lerp(0.85f, 1.2f, progress);
            float fadeScale = life > 0.3f ? 1f : life / 0.3f;
            float size = baseScale * expand * fadeScale;
            info.Scale = new System.Numerics.Vector2(size, size);

            float alpha = (float)Math.Pow(MathHelper.Clamp(life, 0f, 1f), 0.75f);
            info.Color = new Color(255, 168, 175) * alpha;

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                0.40f * alpha,
                0.25f * alpha,
                0.28f * alpha
            );

            info.Time--;
        }
    }
}