using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using System;
using Terraria;

namespace Waybound.Particles
{
    public class SnowFlakeParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/SnowFlake"; 

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = Color.White * Main.rand.NextFloat(0.75f, 1.05f);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)info.Duration;

            info.Velocity *= 0.98f;
            info.Position += info.Velocity;

            info.Rotation += 0.028f * (info.InitialScale.X > 1f ? 1f : -1f);

            float baseScale = MathHelper.Lerp(1.1f, 0.15f, 1f - life);
            info.Scale = info.InitialScale * baseScale;

            float alpha = (float)Math.Sin(life * MathHelper.Pi);
            Color c = info.InitialColor;
            info.Color = new Color(c.R, c.G, c.B, (byte)(c.A * alpha));

            info.Time--;
        }
    }
}