using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

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
			if(dust.color.A == 0 || dust.color.A == 2)
				dust.velocity = dust.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-1, 2.5f)));
            if (dust.color.A == 1 || dust.color.A == 3)
                dust.velocity = dust.velocity.RotatedBy(MathHelper.ToRadians(-Main.rand.NextFloat(-1, 2.5f)));
            dust.position += dust.velocity;

            Lighting.AddLight(dust.position, 
				0.40f * dust.scale,
				0.25f * dust.scale,
				0.28f * dust.scale);

            return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return new Color(255 * (255 - dust.alpha) / 255, 168 * (255 - dust.alpha) / 255, 175 * (255 - dust.alpha) / 255, 255 - dust.alpha);
		}
	}
}