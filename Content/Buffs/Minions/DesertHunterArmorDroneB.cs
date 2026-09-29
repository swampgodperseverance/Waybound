using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Armor;

namespace Waybound.Content.Buffs.Minions
{
	public class DesertHunterArmorDroneB : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Drone");
			// Description.SetDefault("Desert hunter's drone helps you!");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DesertHunterArmorDrone>()] > 0)
				player.buffTime[buffIndex] = 18000;
			else
                Projectile.NewProjectile(player.GetSource_Misc("ArmorOfDesertHunter"), player.Center, Vector2.Zero, ModContent.ProjectileType<DesertHunterArmorDrone>(), 1, 0f, player.whoAmI);
		}
	}
}