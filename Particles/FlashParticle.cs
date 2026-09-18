using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using System;
using Terraria;

namespace Waybound.Particles
{
    public class FlashParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/Megaspark";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(210, 235, 255, 255);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)info.Duration;   
            float progress = 1f - life;                   

            info.Velocity *= 0.88f;
            info.Position += info.Velocity;
            float rotSpeed = MathHelper.Lerp(0.09f, 0.018f, progress);
            info.Rotation += rotSpeed;

            float expand = MathHelper.Lerp(0.35f, 1.85f, EaseOutCubic(progress));
            float endShrink = MathHelper.Lerp(1f, 0.92f, MathHelper.Clamp((progress - 0.7f) / 0.3f, 0f, 1f));
            info.Scale = info.InitialScale * expand * endShrink;

            float alpha = (float)Math.Pow(life, 1.35f);
            alpha *= MathHelper.Lerp(1f, 0.15f, MathHelper.Clamp((progress - 0.75f) / 0.25f, 0f, 1f));

            Color c = info.InitialColor;
            info.Color = new Color(c.R, c.G, c.B, (byte)(c.A * alpha));

            info.Time--;
        }

        private static float EaseOutCubic(float t)
        {
            float f = t - 1f;
            return f * f * f + 1f;
        }
    }
}