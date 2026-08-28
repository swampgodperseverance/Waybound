using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;

namespace Waybound.Common.Rarities
{
    public class CoreburnedRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float t = (float)(Main.GameUpdateCount % 240) / 240f;

                float basePulse = (float)(Math.Sin(t * MathHelper.TwoPi) * 0.5f + 0.5f);
                float microPulse = (float)(Math.Sin(t * MathHelper.TwoPi * 3f) * 0.2f + 0.8f);
                float combined = MathHelper.Clamp(basePulse * microPulse, 0f, 1f);

                Color emberDim = new Color(200, 100, 80);
                Color emberGlow = new Color(255, 180, 120);
                Color emberBlend = Color.Lerp(emberDim, emberGlow, (float)Math.Pow(combined, 1.5f));

                Color warmGlow = Color.Lerp(emberBlend, Color.White, combined * 0.3f);

                float brightness = 0.9f + 0.1f * (float)Math.Sin(t * MathHelper.TwoPi * 0.5f + 1f);
                Color final = new Color(
                    (int)(warmGlow.R * brightness),
                    (int)(warmGlow.G * brightness),
                    (int)(warmGlow.B * brightness)
                );

                return final;
            }
        }
    }
}