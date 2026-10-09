using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Dusts.DeepDusts
{
    public class DeepGlowDustScreen : ModDust
    {
        public override string Texture => "Waybound/Content/Dusts/DeepDusts/DeepGlowDust";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.frame = new Rectangle(0, 0, 32, 32);
            dust.color.A = (byte)Main.rand.Next(0, 2);
            dust.alpha = 255;
        }

        public override bool Update(Dust dust)
        {
            if (dust.color.A == 0 || dust.color.A == 1)
            {
                dust.alpha -= (int)(Main.rand.NextFloat(2, 6) * Math.Abs(dust.velocity.Length()));
                dust.scale += 0.01f * Math.Abs(dust.velocity.Length()) * dust.alpha / 255;
                if (dust.alpha <= 75)
                    dust.color.A = (byte)Main.rand.Next(2, 4);
                dust.velocity *= 1.005f;
            }
            if (dust.color.A == 2 || dust.color.A == 3)
            {
                dust.alpha += (int)(Main.rand.NextFloat(2, 6) * Math.Abs(dust.velocity.Length()));
                dust.scale -= 0.01f * Math.Abs(dust.velocity.Length()) * dust.alpha / 255;
                if (dust.alpha >= 255)
                    dust.active = false;
                dust.velocity /= 1.005f;
            }
            if (dust.color.A == 0 || dust.color.A == 2)
                dust.velocity = dust.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-1, 2.5f)));
            if (dust.color.A == 1 || dust.color.A == 3)
                dust.velocity = dust.velocity.RotatedBy(MathHelper.ToRadians(-Main.rand.NextFloat(-1, 2.5f)));

            dust.position += dust.velocity;

            if (Main.netMode != NetmodeID.Server && ParticleSystem.DeepGlowBuffer != null)
            {
                int roll = Main.rand.Next(100);

                if (roll < 50)
                {
                    ParticleSystem.DeepGlowBuffer.Create(new ParticleInfo(
                        dust.position.ToNumerics(),
                        dust.velocity.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(dust.scale * 22f),
                        new Color(255, 168, 175, 255),
                        Main.rand.Next(50, 90)
                    ));
                }
                else if (roll < 85)
                {
                    int count = Main.rand.Next(1, 3);
                    for (int i = 0; i < count; i++)
                    {
                        ParticleSystem.DeepGlowBuffer.Create(new ParticleInfo(
                            (dust.position + Main.rand.NextVector2Circular(14f, 14f)).ToNumerics(),
                            (dust.velocity + Main.rand.NextVector2Circular(1.2f, 1.2f)).ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(14f, 26f)),
                            new Color(255, 168, 175, 255),
                            Main.rand.Next(50, 100)
                        ));
                    }
                }
            }

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.Transparent;
        }
    }
}