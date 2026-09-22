using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Particles
{
    public class MegasparkParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/Megaspark";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(25, 50, 150, 210) * Main.rand.NextFloat(0.85f, 1.1f);
        }

        public override void Update(ref ParticleInfo info)
        {
            float life = info.Time / (float)info.Duration;
            info.Velocity *= 0.97f;
            info.Position += info.Velocity;
            info.Rotation += 0.012f;
            float baseScale = MathHelper.Lerp(1.05f, 0.2f, 1f - life);
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