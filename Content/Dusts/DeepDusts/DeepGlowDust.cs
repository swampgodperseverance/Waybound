using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Dusts.DeepDusts
{
	public class DeepGlowDust : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 32, 32);
		}

		public override bool Update(Dust dust)
		{
			dust.rotation = 0;
			dust.scale -= 0.025f;
			dust.alpha -= 2;
			if (dust.scale < 0.1f)
				dust.active = false;
			dust.position += dust.velocity;
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return new Color(255, 168, 135, 0);
		}
	}
}