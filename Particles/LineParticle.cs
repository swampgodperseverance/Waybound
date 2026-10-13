using System;
using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using Terraria;

namespace Waybound.Particles
{
    public class LineParticle : Behavior<ParticleInfo>
    {
        public const int RingDuration = 60;
        public const int RingPeriod = 48;
        public const float CoreThickness = 10f;
        public const float GlowThickness = 26f;
        public const float GlowThreshold = 16f;

        private static readonly Color CoreColor = new Color(255, 214, 180);
        private static readonly Color GlowColor = new Color(255, 168, 135);

        public override string Texture => "Waybound/Particles/LineParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Color = new Color(255, 168, 135, 0);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = MathHelper.Clamp(info.Time / (float)Math.Max(info.Duration, 1), 0f, 1f);
            float age = 1f - life;
            bool ring = info.Duration >= RingDuration;
            bool glow = info.Scale.Y >= GlowThreshold;

            info.Position += info.Velocity;

            float alpha;
            if (ring)
            {
                float fadeIn = MathHelper.Clamp(age / 0.2f, 0f, 1f);
                float fadeOut = MathHelper.Clamp(life / 0.2f, 0f, 1f);
                float angle = info.Rotation - MathHelper.PiOver2;
                float flow = 0.6f + 0.4f * MathF.Sin(angle * 4f - Main.GlobalTimeWrappedHourly * 2.2f);
                float breath = 0.92f + 0.08f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.4f);

                alpha = Math.Min(fadeIn, fadeOut) * flow * breath;
            }
            else
            {
                alpha = MathHelper.Clamp(age / 0.15f, 0f, 1f) * life;
            }

            Color baseColor = glow ? GlowColor : CoreColor;
            info.Color = baseColor * (alpha * (glow ? 0.35f : 1f));

            info.Time--;
        }
    }
}