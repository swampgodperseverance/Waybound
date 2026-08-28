//using System;
//using Microsoft.Xna.Framework;
//using Synergia.Content.Items.Misc;
//using Synergia.Content.Projectiles.Other;
//using Terraria;
//using Terraria.ModLoader;

//namespace  Synergia.Content.Buffs
//{
//    // Token: 0x020005EF RID: 1519
//    public class HotEggBuff : ModBuff
//    {
//        // Token: 0x060018E2 RID: 6370 RVA: 0x000B1F82 File Offset: 0x000B0182
//        public override void SetStaticDefaults()
//        {
//            Main.buffNoSave[base.Type] = true;
//            Main.buffNoTimeDisplay[base.Type] = true;
//            Main.vanityPet[base.Type] = true;
//        }

//        // Token: 0x060018E3 RID: 6371 RVA: 0x000B263C File Offset: 0x000B083C
//        public override void Update(Player player, ref int buffIndex)
//        {
//            player.buffTime[buffIndex] = 18000;
//            int num = ModContent.ProjectileType<HotEggProj>();
//            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[num] <= 0)
//            {
//                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, num, 0, 0f, player.whoAmI, 0f, 0f, 0f);
//            }
//        }
//    }
//}