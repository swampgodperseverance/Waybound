using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Dusts.DeepDusts
{
	public class DeepMagicDust : ModDust
	{
		public override void SetStaticDefaults()
		{
			UpdateType = 60;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return new Color(255, 168, 135, 0);
		}
	}
}