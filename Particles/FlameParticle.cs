using System;
using System.Buffers;
using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using Terraria;

namespace Waybound.Particles
{
    public class FlameParticle : Behavior<ParticleInfo>
    {
        public override string Texture => "Waybound/Particles/FlameParticle";

        public override void Initialize(ref ParticleInfo info)
        {
            info.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            info.Color = new Color(255, 140, 40, 255);
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
            info.Color = new Color(255, 140, 40) * alpha;

            Lighting.AddLight(
                new Vector2(info.Position.X, info.Position.Y),
                0.4f * alpha,
                0.2f * alpha,
                0.05f * alpha
            );

            info.Time--;
        }
    }

        public class FlameParticleOld : Particle
        {
            public override void SetDefaults()
            {
                width = 34;
                height = 34;
                Scale = 15f;
                timeLeft = 35;
                SpawnAction = Spawn;
            }

            public override void AI()
            {
                rotation += Utils.Clamp(velocity.X * 0.02f, -0.15f, 0.15f);
                velocity *= 0.98f;
                if (Scale <= 0f)
                    active = false;
            }

            public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
            {
                Texture2D tex = ModContent.Request<Texture2D>("Waybound/Particles/FlameParticle").Value;

                float alpha = timeLeft <= 20 ? timeLeft / 20f : 1f;
                if (alpha < 0f) alpha = 0f;

                Color color = Color.Multiply(new Color(3f, 3f, 3f, 0f), alpha);
                float rot = MathHelper.ToRadians(ai[0]).AngleLerp(MathHelper.ToRadians(ai[0] * 180f), (120f - timeLeft) / 120f);

                spriteBatch.Draw(
                    tex,
                    position - Main.screenPosition,
                    null,
                    color,
                    rot,
                    tex.Size() * 0.5f,
                    0.1f * scale,
                    SpriteEffects.None,
                    0f
                );
                return false;
            }

            public void Spawn()
            {
                ai[0] = Main.rand.NextFloat(-2f, 2f);
                ai[1] = Main.rand.NextFloat(2f, 8f);
                timeLeft = (int)ai[4] > 0 ? (int)ai[4] : timeLeft;
            }
        }
    
}