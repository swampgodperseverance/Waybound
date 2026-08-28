using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;

namespace Waybound.Content.Buffs.Minions
{
    public class OssuaryBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summon.OssuaryStaffProj>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    Projectile.NewProjectile(
                        player.GetSource_Buff(buffIndex),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<Projectiles.Summon.OssuaryStaffProj>(),
                        50,
                        2f,
                        player.whoAmI
                    );
                }
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}