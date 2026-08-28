using Microsoft.Xna.Framework;
using Terraria;

namespace Waybound.Helpers
{
    public class PlayerHelpers
    {
        // Для одного раза
        public static void StartTransforms(Player player, NPC npc, int item, int need, ref bool isSingleUse, int projType)
        {
            if (isSingleUse == false)
            {
                if (CheckItem(player, item, need))
                {
                    Projectile.NewProjectile(npc.GetSource_Death(), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
                    isSingleUse = true;
                }
            }
        }
        // Для многоразового использования
        public static void StartTransforms(Player player, NPC npc, int item, int need, int projType)
        {
            if (CheckItem(player, item, need))
            {
                Projectile.NewProjectile(npc.GetSource_Death(), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
            }
        }
        // need - количество, которое должно быть в стаке я не знаю если книга без стака это 1 или 0 из-за этого need2
        public static bool CheckItem(Player player, int item, int need = 0, int need2 = 1)
        {
            if (player.HasItem(item) && player.inventory[player.FindItem(item)].stack >= need)
            {
                player.inventory[player.FindItem(item)].stack -= need2;
                return true;
            }
            else return false;
        }
        public static bool CheckItem(Player player, int itemType) {
            for (int k = 0; k < player.inventory.Length; k++) {
                if (player.inventory[k].type == itemType) {
                    return true;
                }
            }
            return false;
        }
        public static Item GetLocalItem(Player player) {
            Item held = player.HeldItem;
            return held;
        }
    }
}