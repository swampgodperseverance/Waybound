using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Summon.PreHM;

namespace Waybound.Content.Buffs.Minions
{
	public class GoldKnightBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Gold Knight");
			// Description.SetDefault("Gold knight fighting for you!");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<KingsGoldKnight>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}