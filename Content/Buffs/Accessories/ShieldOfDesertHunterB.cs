using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Shields;

namespace Waybound.Content.Buffs.Accessories
{
	public class ShieldOfDesertHunterB : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Minotaur Shield");
			// Description.SetDefault("You've been protected by minotaur shield");
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.statDefense += 35;
			if (player.ownedProjectileCounts[ModContent.ProjectileType<ShieldOfDesertHunterP>()] == 0)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, new Vector2(0, 0), ModContent.ProjectileType<ShieldOfDesertHunterP>(), 0, 0f, player.whoAmI);
			}
		}
	}
}