using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Particles
{
    public class BubbleParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/BubbleParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(25, 50, 150, 210) * Main.rand.NextFloat(0.85f, 1.1f);
       
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)info.Duration;
            float time = info.Duration - info.Time;

            info.Velocity *= 0.97f;
            info.Position += info.Velocity;

            
            float amp = 0.6f + info.InitialScale.X * 0.4f;
            float freq = 0.07f + life * 0.03f;

            info.Position.X += (float)Math.Sin(time * freq + info.Rotation) * amp * 0.3f;
            info.Position.Y += (float)Math.Cos(time * freq * 0.75f + info.Rotation * 1.3f) * amp * 0.4f;

            info.Position.Y -= 0.35f + life * 0.25f;

            info.Rotation += 0.012f;

            float baseScale = MathHelper.Lerp(1.1f, 0.25f, 1f - life);
            info.Scale = info.InitialScale * baseScale;

            float alpha = (float)Math.Sin(life * MathHelper.Pi);
            Color c = info.InitialColor;
            info.Color = new Color(c.R, c.G, c.B, (byte)(c.A * alpha));

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                0.06f * alpha,
                0.14f * alpha,
                0.28f * alpha
            );

            info.Time--;
        }
    }
}