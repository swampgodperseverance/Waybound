using System;
using System.Buffers;
using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using Terraria;

namespace Waybound.Particles
{
    public class CrystalParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/CrystalParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(255, 168, 135, 0);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)Math.Max(info.Duration, 1);
            float progress = 1f - life;

            info.Velocity *= 0.96f;
            info.Velocity.Y -= 0.03f;
            info.Position += info.Velocity;
            info.Rotation += 0.05f * life;

            float baseScale = info.InitialScale.X;
            if (baseScale < 1f)
                baseScale = 28f;

            float expand = MathHelper.Lerp(0.8f, 1.3f, progress);
            float fadeScale = life > 0.25f ? 1f : life / 0.25f;
            float size = baseScale * expand * fadeScale;
            info.Scale = new System.Numerics.Vector2(size, size);

            float alpha = (float)Math.Pow(MathHelper.Clamp(life, 0f, 1f), 0.7f);
            info.Color = new Color(255, 168, 135) * alpha;

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                0.6f * alpha,
                0.35f * alpha,
                0.2f * alpha
            );

            info.Time--;
        }
    }
}