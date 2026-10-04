using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Dusts.DeepDusts
{
	public class DeepLeafDust : ModDust
	{
		public override void SetStaticDefaults()
		{
			UpdateType = 3;
		}

		public override bool Update(Dust dust)
		{
			float strength = dust.scale;
			/*if (strength > 0.75f)
            {
                strength = 0.75f;
            }*/
			Lighting.AddLight(dust.position, 0.075f * strength, 0.55f * strength, 0.75f * strength);
			return true;
		}
	}
}