using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Buffs.Debuffs
{
	public class Caught : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Caught!");
			// Description.SetDefault("Don't move - it will hurts!");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true;
			BuffID.Sets.LongerExpertDebuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.lifeRegen -= 5;
			if (!npc.boss && !npc.dontTakeDamage)
			{
				npc.velocity *= 0.85f;
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.lifeRegen > 0)
				player.lifeRegen = 0;
			player.lifeRegenTime = 0;
			player.lifeRegen -= (int)(player.velocity.Length() * 25);
			player.velocity *= 0.65f;
		}
	}
}