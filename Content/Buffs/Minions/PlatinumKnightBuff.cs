using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Summon.PreHM;

namespace Waybound.Content.Buffs.Minions
{
	public class PlatinumKnightBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Platinum Knight");
			// Description.SetDefault("Platinum knight fighting for you!");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<KingsPlatinumKnight>()] > 0)
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