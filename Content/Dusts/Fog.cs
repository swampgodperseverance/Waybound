using Microsoft.Xna.Framework;

using Waybound.Common.ModSystems;

using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Dusts;

sealed class Fog : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.frame = new Rectangle(0, 12 * Main.rand.Next(4), 42, 12);
        dust.alpha = 255;
        dust.noGravity = true;
        dust.noLight = true;

        dust.scale = 1.1f + Main.rand.NextFloat(-0.15f, 0.25f);

        if (Main.rand.NextBool(12))
        {
            dust.color = new Color(68, 28, 32, 190); 
        }
        else
        {
            dust.color = new Color(32, 34, 46, 190);
        }

        dust.customData = 2.6f + Main.rand.NextFloat(-0.5f, 0.7f);
    }

    public override bool Update(Dust dust)
    {
        float lifeTime = (float)dust.customData;
        dust.fadeIn += TimerSystem.LogicDeltaTime;

        if (dust.fadeIn >= lifeTime)
        {
            dust.active = false;
            return false;
        }

        float progress = dust.fadeIn / lifeTime;

        if (progress < 0.35f)
        {
            dust.alpha = (int)(255 * (1f - progress / 0.35f));
        }
        else if (progress > 0.7f)
        {
            float fadeOutProgress = (progress - 0.7f) / 0.3f;
            dust.alpha = (int)(255 * fadeOutProgress);
        }
        else
        {
            dust.alpha = 55;
        }

        dust.velocity.X = 0.18f * Main.WindForVisuals + Main.rand.NextFloat(-0.08f, 0.08f);
        dust.velocity.Y = -0.03f + Main.rand.NextFloat(-0.08f, 0.11f);

        dust.position += dust.velocity;

        dust.scale = 1.1f + (float)System.Math.Sin(progress * 6.283185f * 1.5f) * 0.1f;

        return false;
    }
}