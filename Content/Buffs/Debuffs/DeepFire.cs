using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Buffs.Debuffs
{
	public class DeepFire : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Fire");
			// Description.SetDefault("Slowly losing life");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true;
			BuffID.Sets.LongerExpertDebuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<deepFireNPC>().deepFire = true;

			if (Main.rand.NextBool(2))
			{
				int num1 = Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<DeepMagicDust>());
				Main.dust[num1].scale = Main.rand.NextFloat(1f, 1.5f);
				Main.dust[num1].alpha = 50;
				Main.dust[num1].velocity *= 0.75f;
				Main.dust[num1].noGravity = false;
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.lifeRegen > 0)
				player.lifeRegen = 0;

			player.lifeRegen = -20;

			if (Main.rand.NextBool(5))
			{
				int num1 = Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<DeepMagicDust>());
				Main.dust[num1].scale = Main.rand.NextFloat(1.5f, 2f);
				Main.dust[num1].alpha = 50;
				Main.dust[num1].velocity *= 0.5f;
				Main.dust[num1].noGravity = false;
			}
			if (Main.rand.NextBool(2))
			{
				int num1 = Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<DeepMagicDust>());
				Main.dust[num1].scale = Main.rand.NextFloat(2f, 3f);
				Main.dust[num1].alpha = 50;
				Main.dust[num1].velocity *= 0.75f;
				Main.dust[num1].noGravity = true;
			}
		}
	}

	public class deepFireNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool deepFire;

		public override void ResetEffects(NPC npc)
		{
			deepFire = false;
		}


		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (deepFire)
			{
				npc.lifeRegen -= 30;

				if (damage < 5)
				{
					damage = 5;
				}
			}
		}
	}
}